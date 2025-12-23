using UnityEngine;

public class DeadBoxCollision : MonoBehaviour
{
    private PlayerMove _playerMove;

    private void Awake()
    {
        _playerMove = GetComponent<PlayerMove>();
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("DeadBox"))
        {
            _playerMove.OnPlayerDead?.Invoke();
        }
    }
}
