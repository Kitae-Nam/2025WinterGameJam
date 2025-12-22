using UnityEngine;

public class NodeView : MonoBehaviour
{
    [SerializeField] int id;
    public int Id => id;

    [SerializeField] bool isProtected;
    public bool IsProtected => isProtected;

    public void Init(int newId) => id = newId;
}