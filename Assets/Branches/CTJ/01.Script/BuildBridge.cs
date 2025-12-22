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

    readonly Dictionary<int, List<int>> childrenMap = new();
    readonly Dictionary<int, EdgeView> edgeByChild = new();

    private void Awake()
    {
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

        if (buildZone != null && !buildZone.OverlapPoint(mousePos))
        {
            return;
        }

        float dist = Vector2.Distance(fromNode.transform.position, mousePos);
        if (dist < minLineLength || dist > maxLineLength)
            return;

        float cost = dist * costPerUnit;
        if (spent + cost > total)
        {
            return;
        }


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

        spent += cost;
        OnBudgetChanged?.Invoke(spent, Mathf.Max(0f, total - spent));

        ActiveNodeId = newId;
    }

    void UpdatePreview(Vector2 mousePos)
    {
        if (previewLine == null) return;

        if (ActiveNodeId < 0 || !nodeDictionary.TryGetValue(ActiveNodeId, out NodeView fromNode) || fromNode == null)
        {
            previewLine.enabled = false;
            return;
        }

        bool valid = true;

        if (buildZone != null && !buildZone.OverlapPoint(mousePos))
            valid = false;

        float dist = Vector2.Distance(fromNode.transform.position, mousePos);
        if (dist < minLineLength || dist > maxLineLength)
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
        previewLine.SetPosition(1, mousePos);
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

        if (nodeDictionary.TryGetValue(nodeId, out var node) && node != null)
        {
            nodeDictionary.Remove(nodeId);
            Destroy(node.gameObject);
        }

        childrenMap.Remove(nodeId);

        return refund;
    }

    Vector2 GetMousePos()
    {
        Vector3 p = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        return new Vector2(p.x, p.y);
    }
}
