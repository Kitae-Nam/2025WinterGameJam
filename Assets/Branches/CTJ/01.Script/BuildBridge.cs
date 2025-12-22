using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEditor.PlayerSettings;

public class BuildBridge : MonoBehaviour
{
    [Header("Node State")]
    [SerializeField] int activeNodeId = -1;
    public int ActiveNodeId
    {
        get 
        {
            return activeNodeId;
        }
        set
        {
            activeNodeId = value;
        }
    }

    [Header("Rules")]
    [SerializeField] float costPerUnit = 2.0f;
    [SerializeField] float minLineLength = 0.15f;
    [SerializeField] float maxLineLength = 3.0f;
    [SerializeField] LayerMask nodeLayer;
    [SerializeField] LayerMask edgeLayer;

    [Header("Angle")]
    [SerializeField, Range(0f, 180f)] float maxAngle = 60f;
    [SerializeField] bool useAbsoluteAngle = true;

    [Header("Budget")]
    [SerializeField] float total = 200.0f;
    [SerializeField] float spent = 0.0f;
    public string RemainingBudget => (total - spent).ToString("F2");
    public event Action<float, float> OnBudgetChanged;

    [Header("Zone")]
    [SerializeField] Collider2D buildZone;

    [Header("Prefabs")]
    [SerializeField] NodeView nodePrefab;
    [SerializeField] EdgeView edgePrefab;

    [Header("Preview")]
    [SerializeField] LineRenderer previewLine;
    [SerializeField] bool disablePreviewWhenInvalid = true;

    [Header("Group")]
    readonly Dictionary<int, NodeView> nodeDictionary = new();
    readonly List<EdgeView> edgeList = new();
    readonly Dictionary<int, Vector2> angleDir = new();

    readonly Dictionary<int, List<int>> childrenMap = new();
    readonly Dictionary<int, EdgeView> edgeByChild = new();

    private void Awake()
    {
        activeNodeId = -1;

        foreach (var n in FindObjectsByType<NodeView>(FindObjectsSortMode.None))
        {
            if (n == null) continue;

            if (n.Id == 0)
            {
                int id;
                do { id = UnityEngine.Random.Range(0, 2000000000); }
                while (nodeDictionary.ContainsKey(id));
                n.Init(id);
            }

            nodeDictionary[n.Id] = n;
        }
    }

    private void Update()
    {
        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            Collider2D nodeHit = Physics2D.OverlapPoint(GetMousePos(), nodeLayer);
            if (nodeHit != null)
            {
                NodeView node = nodeHit.GetComponent<NodeView>();
                if (node != null && !node.IsProtected)
                {
                    float refund = DeleteNode(node.Id);
                    spent = Mathf.Max(0f, spent - refund);
                    OnBudgetChanged?.Invoke(spent, Mathf.Max(0f, total - spent));
                    ActiveNodeId = -1;
                    return;
                }
            }

            float pickRadius = 0.08f;
            Collider2D edgeHit = Physics2D.OverlapCircle(GetMousePos(), pickRadius, edgeLayer);
            if (edgeHit != null)
            {
                EdgeView edge = edgeHit.GetComponent<EdgeView>();
                if (edge != null)
                {
                    float refund = DeleteNode(edge.ChildId);
                    spent = Mathf.Max(0f, spent - refund);
                    OnBudgetChanged?.Invoke(spent, Mathf.Max(0f, total - spent));
                    ActiveNodeId = -1;
                    return;
                }
            }

            ActiveNodeId = -1;
        }

        UpdatePreview(GetMousePos());

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            HandleBuild(GetMousePos());
        }
    }

    private void HandleBuild(Vector2 mousePos)
    {
        if (ActiveNodeId < 0)
        {
            Collider2D hit = Physics2D.OverlapPoint(mousePos, nodeLayer);
            if (hit == null) return;

            NodeView clicked = hit.GetComponent<NodeView>();
            if (clicked == null) return;

            if (!nodeDictionary.ContainsKey(clicked.Id))
                nodeDictionary[clicked.Id] = clicked;

            ActiveNodeId = clicked.Id;
            return;
        }


        if (!nodeDictionary.TryGetValue(ActiveNodeId, out NodeView fromNode) || fromNode == null)
        {
            ActiveNodeId = -1;
            return;
        }

        #region New Instantiate
        NodeView targetNode = null;
        Collider2D hitNode = Physics2D.OverlapPoint(mousePos, nodeLayer);
        if (hitNode != null)
        {
            NodeView clickedNode = hitNode.GetComponent<NodeView>();
            if (clickedNode != null && clickedNode.Id != fromNode.Id)
                targetNode = clickedNode;
        }

        Vector2 targetPos = targetNode != null ? (Vector2)targetNode.transform.position : mousePos;

        if (buildZone != null && !buildZone.OverlapPoint(targetPos))
            return;

        float dist = Vector2.Distance(fromNode.transform.position, targetPos);
        if (dist < minLineLength || dist > maxLineLength)
            return;

        Vector2 newDir = (targetPos - (Vector2)fromNode.transform.position).normalized;
        Vector2 baseDir = angleDir.TryGetValue(fromNode.Id, out var prevDir) ? prevDir : Vector2.right;

        if (Mathf.Abs(Vector2.SignedAngle(baseDir, newDir)) >= maxAngle)
            return;

        float cost = dist * costPerUnit;
        if (spent + cost > total)
            return;

        bool createdNew = false;
        int targetId;
        if (targetNode == null)
        {
            int newId;
            do { newId = UnityEngine.Random.Range(0, 2000000000); }
            while (nodeDictionary.ContainsKey(newId));

            targetNode = Instantiate(nodePrefab, targetPos, Quaternion.identity);
            targetNode.Init(newId);
            nodeDictionary[newId] = targetNode;

            targetId = newId;
            createdNew = true;
        }
        else
        {
            if (!nodeDictionary.ContainsKey(targetNode.Id))
                nodeDictionary[targetNode.Id] = targetNode;

            targetId = targetNode.Id;
        }

        EdgeView edge = Instantiate(edgePrefab);
        edge.Init(fromNode.Id, targetId, fromNode.transform.position, targetNode.transform.position, cost);
        edgeList.Add(edge);

        if (createdNew)
        {
            if (!childrenMap.TryGetValue(fromNode.Id, out var childList))
            {
                childList = new List<int>();
                childrenMap[fromNode.Id] = childList;
            }
            childList.Add(targetId);
            edgeByChild[targetId] = edge;
        }

        angleDir[targetId] = ((Vector2)targetNode.transform.position - (Vector2)fromNode.transform.position).normalized;

        spent += cost;
        OnBudgetChanged?.Invoke(spent, Mathf.Max(0f, total - spent));

        ActiveNodeId = targetId;
        #endregion

        #region Old Instantiate
        /*
        int newId;
        do { newId = UnityEngine.Random.Range(0, 2000000000); }
        while (nodeDictionary.ContainsKey(newId));

        NodeView newNode = Instantiate(nodePrefab, mousePos, Quaternion.identity);
        newNode.Init(newId);
        nodeDictionary[newId] = newNode;

        EdgeView edge = Instantiate(edgePrefab);
        edge.Init(fromNode.Id, newNode.Id, fromNode.transform.position, newNode.transform.position, cost);
        edgeList.Add(edge);

        if (!childrenMap.TryGetValue(fromNode.Id, out var childList))
        {
            childList = new List<int>();
            childrenMap[fromNode.Id] = childList;
        }
        childList.Add(newId);
        edgeByChild[newId] = edge;
        angleDir[newId] = ((Vector2)newNode.transform.position - (Vector2)fromNode.transform.position).normalized;

        spent += cost;
        OnBudgetChanged?.Invoke(spent, Mathf.Max(0f, total - spent));

        ActiveNodeId = newId;
        */
        #endregion
    }

    void UpdatePreview(Vector2 mousePos)
    {
        if (previewLine == null) return;

        if (ActiveNodeId < 0 || !nodeDictionary.TryGetValue(ActiveNodeId, out NodeView fromNode) || fromNode == null)
        {
            previewLine.enabled = false;
            return;
        }

        Vector2 targetPos = mousePos;
        Collider2D hitNode = Physics2D.OverlapPoint(mousePos, nodeLayer);
        if (hitNode != null)
        {
            NodeView hovered = hitNode.GetComponent<NodeView>();
            if (hovered != null && hovered.Id != fromNode.Id)
                targetPos = hovered.transform.position;
        }

        bool valid = true;

        if (buildZone != null && !buildZone.OverlapPoint(targetPos))
            valid = false;

        float dist = Vector2.Distance(fromNode.transform.position, targetPos);
        if (dist < minLineLength || dist > maxLineLength)
            valid = false;

        Vector2 newDir = (targetPos - (Vector2)fromNode.transform.position).normalized;
        Vector2 baseDir = angleDir.TryGetValue(fromNode.Id, out var prevDir) ? prevDir : Vector2.right;

        if (Mathf.Abs(Vector2.SignedAngle(baseDir, newDir)) >= maxAngle)
            valid = false;

        float cost = dist * costPerUnit;
        if (spent + cost > total)
            valid = false;

        if (disablePreviewWhenInvalid && !valid)
        {
            previewLine.enabled = false;
            return;
        }

        previewLine.enabled = true;
        previewLine.SetPosition(0, fromNode.transform.position);
        previewLine.SetPosition(1, targetPos);
    }

    float DeleteNode(int nodeId)
    {
        float refund = 0f;

        if (childrenMap.TryGetValue(nodeId, out var children))
        {
            var copy = new List<int>(children);
            foreach (int childId in copy)
                refund += DeleteNode(childId);
        }

        for (int i = edgeList.Count - 1; i >= 0; i--)
        {
            EdgeView e = edgeList[i];
            if (e == null) { edgeList.RemoveAt(i); continue; }

            if (e.ParentId == nodeId || e.ChildId == nodeId)
            {
                refund += e.Cost;
                if (childrenMap.TryGetValue(e.ParentId, out var list))
                    list.Remove(e.ChildId);

                if (edgeByChild.TryGetValue(e.ChildId, out var stored) && stored == e)
                    edgeByChild.Remove(e.ChildId);

                Destroy(e.gameObject);
                edgeList.RemoveAt(i);
            }
        }

        // 이런 버그가 멈추지 않잖아?
        /*
        if (edgeByChild.TryGetValue(nodeId, out var incomingEdge) && incomingEdge != null)
        {
            refund += incomingEdge.Cost;

            int parentId = incomingEdge.ParentId;
            if (childrenMap.TryGetValue(parentId, out var siblings))
                siblings.Remove(nodeId);

            edgeByChild.Remove(nodeId);
            edgeList.Remove(incomingEdge);
            Destroy(incomingEdge.gameObject);
        }
        */

        if (nodeDictionary.TryGetValue(nodeId, out var node) && node != null)
        {
            nodeDictionary.Remove(nodeId);
            Destroy(node.gameObject);
        }

        childrenMap.Remove(nodeId);
        angleDir.Remove(nodeId);

        return refund;
    }

    Vector2 GetMousePos()
    {
        Vector3 p = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        return new Vector2(p.x, p.y);
    }
}
