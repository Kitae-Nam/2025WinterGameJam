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
        //_rigid.freezeRotation = true;
    }
    private void FixedUpdate()
    {
        MoveHandle();
        _rigid.linearVelocity = new Vector2(_playerSo.GetMoveSpeed(), _rigid.linearVelocityY);
    }
    private void Update()
    {
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
        float targetVelocity = 0f;

        if (Input.GetKey(KeyCode.D))
            dir = -1f;
        else if (Input.GetKey(KeyCode.A))
            dir = 1f;

        targetVelocity = dir * _playerSo.maxRotationSpeed;

        if (dir != 0f)//입력이 있을때 점점 빨리 회전
        {
            _velocity += dir * _playerSo.rotationAcceleration;
        }
        else//점점 느려짐
        {
            _velocity = Mathf.MoveTowards(_velocity, targetVelocity, _playerSo.rotationDeceleration);
        }

        _velocity = Mathf.Clamp(_velocity, -_playerSo.maxRotationSpeed, _playerSo.maxRotationSpeed);

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
        else if (absAngle <= 45f)
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
