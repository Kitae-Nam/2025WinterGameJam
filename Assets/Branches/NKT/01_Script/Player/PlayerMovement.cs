using UnityEngine;
using UnityEngine.Rendering;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private PlayerSO _playerSo;

    private Rigidbody2D _rigid;
    [SerializeField] private bool _isOnGround;


    private void Awake()
    {
        _rigid = GetComponent<Rigidbody2D>();
    }
    private void FixedUpdate()
    {
        _rigid.linearVelocity = new Vector2(_playerSo.GetMoveSpeed(), _rigid.linearVelocityY);
    }
    private void Update()
    {
        MoveHandle();
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            Debug.Log($"Player Speed : {_playerSo.GetMoveSpeed()}");
            _isOnGround = true;
            _rigid.angularVelocity = 0f;
            Landing();
        }
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            _isOnGround = false;
        }
    }
    private void MoveHandle()
    {
        if (_isOnGround == false)
        {
            Spin();
        }
        else
        {
            SpeedUp();
        }
    }

    private void SpeedUp()
    {
        if (Input.GetKey(KeyCode.D))
        {
            _playerSo.Accelarte();
        }
        else if (Input.GetKey(KeyCode.A))
        {
            _playerSo.Decelerate();
        }
    }

    private void Spin()
    {
        if (Input.GetKey(KeyCode.D))
        {
            _rigid.angularVelocity = -_playerSo.rotationAcceleration;
        }
        else if (Input.GetKey(KeyCode.A))
        {
            _rigid.angularVelocity = _playerSo.rotationDeceleration;
        }
    }
    private void Landing()
    {
        int lap = Mathf.FloorToInt(Mathf.Abs(transform.eulerAngles.z) / 180);
        Debug.Log(lap);
        float remainder = transform.eulerAngles.z - (lap * 180);
        Debug.Log(remainder);

        if (remainder < 30)
        {
            Debug.Log("Landing Perfect Success!");
        }
        else if (remainder > 330)
        {
            Debug.Log("Landing Success!");
        }
        else
        {
            Debug.Log("Landing Fail!");
        }
    }
}
