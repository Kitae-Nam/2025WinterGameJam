using UnityEngine;

public class SnowSlide : MonoBehaviour
{
    [Header("Move")]
    [SerializeField] private float moveSpeed = 3f;

    [Header("Target")]
    [SerializeField] private Transform playerTransform;

    private float alpha = 0.5f;

    public float minDistance = 2f;   // 이 거리 이하면 alpha = 1
    public float maxDistance = 10f;  // 이 거리 이상이면 alpha = 0
    public SpriteRenderer SnowSlideSR;



    private void Update()
    {
        SnowSlideSR.color = new Color(1f, 1f, 1f, alpha);
        Move();
        UpdateAlpha();
    }

    private void Move()
    {
        transform.position += Vector3.right * moveSpeed * Time.deltaTime;
    }

    private void UpdateAlpha()
    {

        if (playerTransform == null) return;

        float distance = Vector2.Distance(
            transform.position,
            playerTransform.position
        );
        Debug.Log(distance);


        // 가까울수록 1, 멀수록 0
        alpha = Mathf.InverseLerp(maxDistance, minDistance, distance);

        // 안전 클램프 (필수)
        alpha = Mathf.Clamp01(alpha);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("Player Hit SnowSlide");

        }
    }
}
