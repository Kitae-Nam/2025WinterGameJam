using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private Transform _visual;
    [SerializeField] private PlayerSO _playerSo;

    private Rigidbody2D _rigid;
    [SerializeField] private bool _isOnGround;

    private float _velocity;
    private float _totalRotation;

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

        if(Input.GetKeyDown(KeyCode.Space) && _isOnGround)
        {
            _rigid.AddForce(Vector2.up * _playerSo.jumpForce, ForceMode2D.Impulse);
            _isOnGround = false;
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
        float dir = 0f;

        if (Input.GetKey(KeyCode.D))
            dir = -1f;
        else if (Input.GetKey(KeyCode.A))
            dir = 1f;

        float targetVelocity = dir * _playerSo.maxRotationSpeed;

        _velocity = Mathf.MoveTowards(
            _velocity,
            targetVelocity,
            _playerSo.rotationAcceleration * Time.fixedDeltaTime
        );

        _rigid.angularVelocity = _velocity;
        _totalRotation += _velocity * Time.fixedDeltaTime;
    }
    private void Landing()
    {
        float angle = transform.eulerAngles.z;

        if (angle > 180f)
            angle -= 360f;

        float absAngle = Mathf.Abs(angle);

        if (absAngle <= 20f)
        {
            Debug.Log("Landing Perfect Success!");
        }
        else if (absAngle <= 35f)
        {
            Debug.Log("Landing Success!");
        }
        else
        {
            Debug.Log("Landing Fail!");
        }

        _velocity = 0f;
        _totalRotation = 0f;
        _rigid.angularVelocity = 0f;
    }
    private int GetSpinCount()
    {
        return Mathf.FloorToInt(Mathf.Abs(_totalRotation) / 360f);
    }
}
