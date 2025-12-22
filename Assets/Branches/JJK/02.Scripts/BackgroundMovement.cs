using System;
using UnityEngine;

public class BackgroundMovement : MonoBehaviour
{
    [SerializeField] private float speed = 2f;
    [SerializeField] private float tpPoint = 20f;

    private void FixedUpdate()
    {
        transform.position += transform.right * speed * Time.fixedDeltaTime;
        
        if (transform.position.x > tpPoint)
            transform.position = new Vector3(-tpPoint, transform.position.y, transform.position.z);
    }
}
