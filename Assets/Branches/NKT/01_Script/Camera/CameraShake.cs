using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance;

    private CinemachineImpulseSource _cinemachine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Start()
    {
        _cinemachine = GetComponent<CinemachineImpulseSource>();
    }
    public void ShakeCamera()
    {
        _cinemachine.GenerateImpulse();
    }
}
