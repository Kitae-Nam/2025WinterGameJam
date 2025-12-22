using TMPro.EditorUtilities;
using UnityEngine;
namespace nkt
{
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private Transform _visual;
        [SerializeField] private PlayerSO _playerSo;

        private Rigidbody2D _rigid;
        private Collider2D _collider;
        [SerializeField] private bool _isOnGround;

        private float _velocity;
        private float _totalRotation;

        private bool _isJump;
        [SerializeField] private float _rayLength = 1f;

        private void Awake()
        {
            _rigid = GetComponent<Rigidbody2D>();
        }
        private void FixedUpdate()
        {
            Ray2D ray = new Ray2D(transform.position, Vector2.down * _rayLength);
            if (Physics2D.Raycast(ray.origin, ray.direction, _rayLength))
            {
                _isOnGround = true;
            }
            else
            {
                _isOnGround = false;
            }

            _rigid.linearVelocity = new Vector2(_playerSo.GetMoveSpeed(), _rigid.linearVelocityY);
            if (_isJump == false && _isOnGround == true)
            {
                _rigid.AddForce(Vector2.down * _playerSo.gravityForce, ForceMode2D.Force);
            }
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
                _isJump = false;
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

            if (Input.GetKeyDown(KeyCode.Space) && _isOnGround)
            {
                _rigid.AddForce(Vector2.up * _playerSo.jumpForce, ForceMode2D.Impulse);
                _isJump = true;
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

            _velocity = Mathf.MoveTowards(_velocity, targetVelocity, _playerSo.rotationAcceleration * Time.deltaTime);

            _rigid.angularVelocity = _velocity;
            _totalRotation += _velocity * Time.deltaTime;
        }
        private void Landing()
        {
            _velocity = 0f;
            _totalRotation = 0f;
            _rigid.angularVelocity = 0f;

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
        }
        private int GetSpinCount()
        {
            return Mathf.FloorToInt(Mathf.Abs(_totalRotation) / 360f);
        }
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, Vector2.down * _rayLength);
        }
    }
}