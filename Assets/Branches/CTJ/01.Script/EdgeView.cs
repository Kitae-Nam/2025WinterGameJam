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

    Transform aT;
    Transform bT;

    Joint2D simJoint;
    bool hadSimJoint;

    void Awake()
    {
        lr = GetComponent<LineRenderer>();
        col = GetComponent<EdgeCollider2D>();

        lr.positionCount = 2;
        lr.useWorldSpace = true;
    }

    void LateUpdate()
    {
        if (!aT || !bT)
        {
            gameObject.SetActive(false);
            return;
        }

        if (hadSimJoint && simJoint == null)
        {
            gameObject.SetActive(false);
            return;
        }

        UpdateShape();
    }

    void UpdateShape()
    {
        Vector3 a = aT.position;
        Vector3 b = bT.position;

        lr.SetPosition(0, a);
        lr.SetPosition(1, b);

        Vector2 p0 = transform.InverseTransformPoint(a);
        Vector2 p1 = transform.InverseTransformPoint(b);
        col.points = new[] { p0, p1 };
    }

    public void Init(int parentId, int childId, Transform a, Transform b, float cost)
    {
        ParentId = parentId;
        ChildId = childId;
        Cost = cost;

        aT = a;
        bT = b;

        UpdateShape();
    }

    public void BindSimJoint(Joint2D j)
    {
        simJoint = j;
        hadSimJoint = (j != null);
        gameObject.SetActive(true);
    }

    public void ResetSim()
    {
        simJoint = null;
        hadSimJoint = false;
        gameObject.SetActive(true);
    }
}