using UnityEngine;

[CreateAssetMenu(fileName = "PlayerSO", menuName = "SO/Player/PlayerSO")]
public class PlayerSO : ScriptableObject
{
    public float maxSpeed = 20f;
    public float minSpeed = 3f;
    [SerializeField] private float moveSpeed;

    [Range(0.01f, 3f)]
    public float acceleration = 0.01f;
    [Range(0.01f, 3f)]
    public float deceleration = 0.01f;
    [Range(1f, 300f)]
    public float rotationAcceleration = 150f;
    [Range(1f, 300f)]
    public float rotationDeceleration = 100f;
    public float maxRotationSpeed = 200f;

    public float jumpForce = 7f;
    public float gravityForce = 20f;

    public float GetMoveSpeed()
    {
        return moveSpeed;
    }
    public void SetMoveSpeed(float value)
    {
        float temp = Mathf.Clamp(value, minSpeed, maxSpeed);
        moveSpeed = temp;
    }

    public void Accelarte()
    {
        SetMoveSpeed(GetMoveSpeed() + acceleration);
    }
    public void Decelerate()
    {
        SetMoveSpeed(GetMoveSpeed() - deceleration);
    }

    private void OnEnable()
    {
        Reset();
    }
    public void Reset()
    {
        SetMoveSpeed(5f);
    }
}
