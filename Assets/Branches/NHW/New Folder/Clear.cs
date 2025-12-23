using UnityEngine;
using UnityEngine.Events;

public class Clear : MonoBehaviour
{
    public  UnityEvent clear;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            clear.Invoke();
        }

    }
}
