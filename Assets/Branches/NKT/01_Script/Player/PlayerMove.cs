using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

public class PlayerMove : MonoBehaviour
{
    public PlayerSO _playerSo;
    private Rigidbody2D _rigid;
    private Animator _anime;

    private readonly int _jumpHash = Animator.StringToHash("IsJump");
    private readonly int _sitHash = Animator.StringToHash("IsSit");
    private readonly int _standHash = Animator.StringToHash("IsStand");

    [SerializeField] private bool _isOnGround;
    [SerializeField] private bool _canJump;
    [SerializeField] private bool _isJumping = false;
    private bool _isDead = false;

    [SerializeField] private float _rayLength = 1f;
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private float _stickDistance = 0.2f;

    private Vector2 _groundNormal = Vector2.up;

    private float _velocity;
    private float _totalRotation;

    [SerializeField] private float _extraGroundGravity = 30f;
    [SerializeField] private float _softStickRayLength = 0.3f;

    [SerializeField] private ParticleSystem _particle;

    public UnityEvent OnPlayerDead;

    private void Awake()
    {
        _rigid = GetComponent<Rigidbody2D>();
        _anime = GetComponentInChildren<Animator>();
    }
    private void Update()
    {
        if (_isDead) return;

        GroundCheck();

        Move();

        if(_isJumping == false)
            GroundPull();

        if (_isOnGround == true)
        {
            _particle.Play();
        }
        else
        {
            _particle.Stop();
        }

        MoveHandle();
    }
    private void MoveHandle()
    {
        if (_isOnGround == true)
        {
            if (Input.GetKey(KeyCode.D))
            {
                _playerSo.Accelarte();
                _anime.SetTrigger(_sitHash);
            }
            else if (Input.GetKey(KeyCode.A))
            {
                _playerSo.Decelerate();
                _anime.SetTrigger(_standHash);
            }
            if (Input.GetKey(KeyCode.Space) && _canJump == true)
            {
                Jump();
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

        if (_isOnGround == true && !_isJumping)
        {

            Vector2 alongGround = Vector2.Perpendicular(_groundNormal);//지금 바닥의 법선의 수직 구한거
            if (alongGround.x < 0)//오른쪽 방향이어야 하니까 음수면 양수로 바꿔줌
                alongGround = -alongGround;

            _rigid.linearVelocity = new Vector2(alongGround.normalized.x * speed, _rigid.linearVelocity.y);

            if (_rigid.linearVelocity.y > 0f)
            {
                var v = _rigid.linearVelocity;
                v.y *= 0.5f;
                _rigid.linearVelocity = v;
                _rigid.AddForce(-_groundNormal * _extraGroundGravity, ForceMode2D.Force);
            }
        }
        else
        {

            Vector2 vel = _rigid.linearVelocity;
            vel.x = speed;
            _rigid.linearVelocity = vel;
        }
    }
    private void GroundPull()
    {
        if (_isOnGround == true) return;
        if(_rigid.linearVelocity.y >= 0f) return;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, _rayLength + _stickDistance, _groundLayer);

        if (hit.collider != null)
        {
            _rigid.AddForce(-_groundNormal * _extraGroundGravity, ForceMode2D.Force);
        }
    }
    private void Jump()
    {
        _isJumping = true;
        _anime.SetTrigger(_jumpHash);
        _canJump = false;
        Vector2 vel = _rigid.linearVelocity;
        vel.y = 0;
        _rigid.linearVelocity = vel;
        _rigid.AddForce(_groundNormal * _playerSo.jumpForce, ForceMode2D.Impulse);
    }
    private void Spin(float dir)
    {
        _anime.SetTrigger(_sitHash);
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

        GroundPull();

        float angle = transform.eulerAngles.z;

        if (angle > 180f)
            angle -= 360f;

        float absAngle = Mathf.Abs(angle);

        Debug.Log($"Landing Angle : {absAngle}");
        if (absAngle <= 20f)
        {
            Debug.Log("Landing Perfect Success!");
            CameraShake.Instance.ShakeCamera();
        }
        else if (absAngle <= 100f)
        {
            Debug.Log("Landing Success!");
        }
        else
        {
            Debug.Log("Landing Fail!");
            OnPlayerDead.Invoke();
        }

        _isJumping = false;
    }
    public void Die()
    {
        Debug.Log("Player Dead");
        _isDead = true;
        _rigid.angularVelocity = 0f;
        _rigid.linearVelocity = Vector3.zero;
        Time.timeScale = 0f;

    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            Landing();
            _canJump = true;
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * _rayLength);
    }
}
