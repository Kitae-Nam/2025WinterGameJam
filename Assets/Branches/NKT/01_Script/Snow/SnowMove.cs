using UnityEngine;
using UnityEngine.Events;

public class SnowMove : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 0.1f;
    [SerializeField] private Transform _player;
    public UnityEvent OnSnowHitPlayer;

    private void Update()
    {
        transform.Translate((_player.position - transform.position) * _moveSpeed * Time.deltaTime);
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            OnSnowHitPlayer?.Invoke();
        }
    }
}
