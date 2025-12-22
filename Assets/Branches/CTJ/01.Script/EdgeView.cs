using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(EdgeCollider2D))]
public class EdgeView : MonoBehaviour
{
    public int ParentId { get; private set; }
    public int ChildId { get; private set; }
    public float Cost { get; private set; }

    LineRenderer lr;
    EdgeCollider2D col;

    void Awake()
    {
        lr = GetComponent<LineRenderer>();
        col = GetComponent<EdgeCollider2D>();

        lr.positionCount = 2;
        lr.useWorldSpace = true;
    }

    public void Init(int parentId, int childId, Vector2 a, Vector2 b, float cost)
    {
        ParentId = parentId;
        ChildId = childId;
        Cost = cost;

        lr.SetPosition(0, a);
        lr.SetPosition(1, b);

        Vector2 p0 = transform.InverseTransformPoint(a);
        Vector2 p1 = transform.InverseTransformPoint(b);
        col.points = new[] { p0, p1 };
    }
}