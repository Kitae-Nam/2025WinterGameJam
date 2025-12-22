using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    [SerializeField] private PlayerSO _playerSo;
    private Rigidbody2D _rigid;

    [SerializeField] private bool _isOnGround;
    [SerializeField] private bool _canJump;
    [SerializeField] private float _rayLength = 1f;
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private float _stickDistance = 0.2f;

    private Vector2 _groundNormal = Vector2.up;

    private float _velocity;
    private float _totalRotation;

    private void Awake()
    {
        _rigid = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        GroundCheck();

        Move();

        //StickToGround();

        _rigid.linearVelocity = new Vector2(_playerSo.GetMoveSpeed(), _rigid.linearVelocity.y);
    }
    private void Update()
    {
        MoveHandle();
    }
    private void MoveHandle()
    {
        if (_isOnGround == true)
        {
            if (Input.GetKey(KeyCode.Space) && _canJump == true)
            {
                Jump();
            }
            if (Input.GetKey(KeyCode.D))
            {
                _playerSo.Accelarte();
            }
            else if (Input.GetKey(KeyCode.A))
            {
                _playerSo.Decelerate();
            }
        }
        else
        {
            if (Input.GetKey(KeyCode.D))
            {
                Spin(-1);
            }
            else if (Input.GetKey(KeyCode.A))
            {
                Spin(1);
            }
        }
    }
    private void GroundCheck()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, _rayLength, _groundLayer);

        if (hit.collider != null)
        {
            _isOnGround = true;
            _groundNormal = hit.normal;
        }
        else
        {
            _isOnGround = false;
            _groundNormal = Vector2.up;
        }
    }
    private void Move()//땅을 기준으로 이동 방향 싹싹김치
    {
        float speed = _playerSo.GetMoveSpeed();

        if (_isOnGround == true)
        {
            Vector2 alongGround = Vector2.Perpendicular(_groundNormal);//지금 바닥의 법선의 수직 구한거
            if (alongGround.x < 0)//오른쪽 방향이어야 하니까 음수면 양수로 바꿔줌
                alongGround = -alongGround;

            _rigid.linearVelocity = alongGround.normalized * speed;
        }
        else
        {
            Vector2 vel = _rigid.linearVelocity;
            vel.x = speed;
            _rigid.linearVelocity = vel;
        }
    }
    private void StickToGround()
    {
        RaycastHit2D ray = Physics2D.Raycast(transform.position + Vector3.up * 0.1f, Vector2.down, _stickDistance + _rayLength, _groundLayer);

        if(ray.collider != null)
        {
            Vector3 pos = transform.position;
            pos.y = ray.point.y + _stickDistance;
            transform.position = pos;

            Vector2 vel = _rigid.linearVelocity;
            if (vel.y > 0)
                vel.y = 0;
            _rigid.linearVelocity = vel;
        }
    }
    private void Jump()
    {
        Debug.Log("Jump");
        _canJump = false;
        Vector2 vel = _rigid.linearVelocity;
        vel.y = 0;
        _rigid.AddForce(Vector2.up * _playerSo.jumpForce, ForceMode2D.Impulse);
    }
    private void Spin(float dir)
    {
        float targetVelocity = dir * _playerSo.maxRotationSpeed;

        _velocity = Mathf.MoveTowards(_velocity, targetVelocity, _playerSo.rotationAcceleration * Time.deltaTime);

        _rigid.angularVelocity = _velocity;
        _totalRotation += _velocity * Time.deltaTime;
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            _canJump = true;
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * _rayLength);
    }
}
