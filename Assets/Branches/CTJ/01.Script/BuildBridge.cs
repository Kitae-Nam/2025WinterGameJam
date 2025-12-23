using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class BuildBridge : MonoSingleton<BuildBridge>
{
    [Header("Cam")]
    [SerializeField] Camera cam;

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
    [SerializeField] bool isLimitAngle = false;
    [SerializeField, Range(0f, 180f)] float maxAngle = 60f;

    [Header("Budget")]
    [SerializeField] float total = 200.0f;
    [SerializeField] float spent = 0.0f;
    [SerializeField] TextMeshProUGUI moneyText;
    public string RemainingBudgetUI => (total - spent).ToString("F2");
    public int RemainingBudget => (int)(total - spent);
    public event Action<float, float> OnBudgetChanged;

    [Header("Zone")]
    [SerializeField] Collider2D[] buildZone;

    [Header("Prefabs")]
    [SerializeField] NodeView nodePrefab;
    [SerializeField] EdgeView edgePrefab;

    [Header("Preview")]
    [SerializeField] LineRenderer previewLine;
    [SerializeField] bool disablePreviewWhenInvalid = true;

    [Header("Simulating")]
    [SerializeField] float nodeMass = 1f;
    [SerializeField] float gravityScale = 1f;
    [SerializeField] float jointBreakForce = 250f; // ���� ����, �̰� �̻� �þ�� �ڻ쳲 (�Ƹ�? �³�?)
    [SerializeField] bool isSimulating = false;

    [Header("UI")]
    [SerializeField] GameObject finBtn;
    [SerializeField] GameObject exitBtn;

    [Header("Sound")]
    [SerializeField] AudioClip buildSound;
    AudioSource soundManager;

    [Header("Group")]
    readonly Dictionary<int, NodeView> nodeDictionary = new();
    readonly List<EdgeView> edgeList = new();
    readonly Dictionary<int, Vector2> angleDir = new();

    // ���
    readonly Dictionary<int, List<int>> childrenMap = new();
    readonly Dictionary<int, EdgeView> edgeByChild = new();

    // ���� ��ġ
    readonly Dictionary<int, Vector2> savedPos = new();
    readonly Dictionary<int, float> savedRot = new();

    protected override void Awake()
    {
        base.Awake();

        exitBtn.SetActive(false);

        activeNodeId = -1;
        moneyText.text = RemainingBudgetUI;

        previewLine.useWorldSpace = true;
        previewLine.positionCount = 2;

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

    private void Start()
    {
        soundManager = GameObject.Find("SoundManager").GetComponent<AudioSource>();
    }

    private void Update()
    {
        /*
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            if (!isSimulating) StartSimulation();
            else StopSimulation();
            return;
        }
        */

        if (isSimulating) return;


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
                    moneyText.text = RemainingBudgetUI;
                    OnBudgetChanged?.Invoke(spent, Mathf.Max(0f, total - spent));
                    ActiveNodeId = -1;
                    return;
                }
            }

            float pickRadius = 0.8f;
            Collider2D edgeHit = Physics2D.OverlapCircle(GetMousePos(), pickRadius, edgeLayer);
            if (edgeHit != null)
            {
                EdgeView edge = edgeHit.GetComponent<EdgeView>();
                if (edge != null)
                {
                    bool isTreeEdge = edgeByChild.TryGetValue(edge.ChildId, out var stored) && stored == edge;
                    bool childIsProtected = nodeDictionary.TryGetValue(edge.ChildId, out var childNode) && childNode != null && childNode.IsProtected;

                    if (isTreeEdge && !childIsProtected)
                    {
                        float refund = DeleteNode(edge.ChildId);
                        spent = Mathf.Max(0f, spent - refund);
                    }
                    else
                    {
                        float refund = edge.Cost;
                        spent = Mathf.Max(0f, spent - refund);

                        if (childrenMap.TryGetValue(edge.ParentId, out var list))
                            list.Remove(edge.ChildId);
                        if (edgeByChild.TryGetValue(edge.ChildId, out var stored2) && stored2 == edge)
                            edgeByChild.Remove(edge.ChildId);

                        edgeList.Remove(edge);
                        Destroy(edge.gameObject);
                    }

                    moneyText.text = RemainingBudgetUI;
                    OnBudgetChanged?.Invoke(spent, Mathf.Max(0f, total - spent));
                    ActiveNodeId = -1;
                    return;
                }
            }

            ActiveNodeId = -1;
            moneyText.text = RemainingBudgetUI;
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

        if (buildZone != null)
        {
            bool count = false;
            for (int i = 0; i < buildZone.Length; i++)
            {
                if (buildZone[i] != null && buildZone[i].OverlapPoint(targetPos))
                {
                    count = true;
                }
            }    

            if (!count)
            {
                OnText("Outside Permitted Zone.");
                return;
            }
        }

        float dist = Vector2.Distance(fromNode.transform.position, targetPos);
        if (dist < minLineLength)
        {
            OnText("The Distance Is Too Short.");
            return;
        }
        else if (dist > maxLineLength)
        {
            OnText("The Distance Is Too Far.");
            return;
        }

        if (isLimitAngle)
        {
            Vector2 newDir = (targetPos - (Vector2)fromNode.transform.position).normalized;
            Vector2 baseDir = angleDir.TryGetValue(fromNode.Id, out var prevDir) ? prevDir : Vector2.right;

            if (Mathf.Abs(Vector2.SignedAngle(baseDir, newDir)) >= maxAngle)
            {
                OnText("The Angle Is Too Large.");
                return;
            }
        }

        float cost = dist * costPerUnit;
        if (spent + cost > total)
        {
            OnText("Budget Shortage.");
            return;
        }

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
        edge.Init(fromNode.Id, targetId, fromNode.transform, targetNode.transform, cost);
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
        moneyText.text = RemainingBudgetUI;
        OnBudgetChanged?.Invoke(spent, Mathf.Max(0f, total - spent));

        soundManager.PlayOneShot(buildSound);
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

        if (buildZone != null)
        {
            bool count = false;
            for (int i = 0; i < buildZone.Length; i++)
            {
                if (buildZone[i] != null && buildZone[i].OverlapPoint(targetPos))
                {
                    count = true;
                }
            }

            if (!count)
            {
                valid = false;
            }
        }

        float dist = Vector2.Distance(fromNode.transform.position, targetPos);
        if (dist < minLineLength || dist > maxLineLength)
            valid = false;

        if (isLimitAngle)
        {
            Vector2 newDir = (targetPos - (Vector2)fromNode.transform.position).normalized;
            Vector2 baseDir = angleDir.TryGetValue(fromNode.Id, out var prevDir) ? prevDir : Vector2.right;

            if (Mathf.Abs(Vector2.SignedAngle(baseDir, newDir)) >= maxAngle)
                valid = false;
        }

        float cost = dist * costPerUnit;
        if (spent + cost > total)
            valid = false;

        if (disablePreviewWhenInvalid && !valid)
        {
            previewLine.startColor = Color.red;
            previewLine.endColor = Color.red;
            return;
        }

        previewLine.enabled = true;
        previewLine.startColor = Color.white;
        previewLine.endColor = Color.white;
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

        // �̷� ���װ� ������ ���ݾ�?
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
        if (!cam) cam = Camera.main;

        Vector2 screen = Mouse.current.position.ReadValue();
        float z = -cam.transform.position.z;

        Vector3 world = cam.ScreenToWorldPoint(new Vector3(screen.x, screen.y, z));
        return new Vector2(world.x, world.y);
    }

    // ���� ������ �̰� �� �۵��ϴ����� �𸣰ڴ�(��� ������ �𸥴�, ��û�� ���� �Ƚ�!!)
    // �ٵ� �ϴ� ���ݾ� ������~
    public void StartSimulation()
    {
        if (isSimulating) return;

        finBtn.SetActive(false);
        exitBtn.SetActive(true);

        savedPos.Clear();
        savedRot.Clear();

        foreach (var kv in nodeDictionary)
        {
            if (kv.Value == null) continue;
            savedPos[kv.Key] = kv.Value.transform.position;
            savedRot[kv.Key] = kv.Value.transform.eulerAngles.z;
        }

        isSimulating = true;
        ActiveNodeId = -1;

        var adj = new Dictionary<int, List<int>>();
        void AddAdj(int a, int b)
        {
            if (!adj.TryGetValue(a, out var list)) { list = new List<int>(); adj[a] = list; }
            list.Add(b);
        }

        foreach (var e in edgeList)
        {
            if (e == null) continue;
            AddAdj(e.ParentId, e.ChildId);
            AddAdj(e.ChildId, e.ParentId);
        }

        var supported = new HashSet<int>();
        var q = new Queue<int>();

        foreach (var kv in nodeDictionary)
        {
            if (kv.Value != null && kv.Value.IsProtected)
            {
                supported.Add(kv.Key);
                q.Enqueue(kv.Key);
            }
        }

        while (q.Count > 0)
        {
            int cur = q.Dequeue();
            if (!adj.TryGetValue(cur, out var list)) continue;

            for (int i = 0; i < list.Count; i++)
            {
                int nxt = list[i];
                if (supported.Add(nxt)) q.Enqueue(nxt);
            }
        }

        foreach (var kv in nodeDictionary)
        {
            var node = kv.Value;
            if (node == null) continue;

            foreach (var j in node.GetComponents<Joint2D>()) Destroy(j);

            var rb = node.GetComponent<Rigidbody2D>();
            if (!rb) rb = node.gameObject.AddComponent<Rigidbody2D>();

            rb.mass = nodeMass;
            rb.gravityScale = gravityScale;

            if (node.IsProtected)
                rb.bodyType = RigidbodyType2D.Static;
            else
                rb.bodyType = RigidbodyType2D.Dynamic;
        }

        foreach (var e in edgeList)
        {
            if (e == null) continue;
            bool aSup = supported.Contains(e.ParentId);
            bool bSup = supported.Contains(e.ChildId);
            if (!aSup && !bSup) continue;

            if (!nodeDictionary.TryGetValue(e.ParentId, out var a) || a == null) continue;
            if (!nodeDictionary.TryGetValue(e.ChildId, out var b) || b == null) continue;

            var rbA = a.GetComponent<Rigidbody2D>();
            var rbB = b.GetComponent<Rigidbody2D>();
            if (!rbA || !rbB) continue;

            var joint = b.gameObject.AddComponent<DistanceJoint2D>();
            joint.connectedBody = rbA;
            joint.autoConfigureDistance = false;
            joint.distance = Vector2.Distance(a.transform.position, b.transform.position);
            joint.enableCollision = false;
            joint.breakForce = jointBreakForce;
            joint.breakTorque = jointBreakForce;
            e.BindSimJoint(joint);
        }

        if (previewLine) previewLine.enabled = false;
    }

    public void StopSimulation()
    {
        if (!isSimulating) return;

        finBtn.SetActive(true);
        exitBtn.SetActive(false);

        isSimulating = false;
        ActiveNodeId = -1;

        foreach (var kv in nodeDictionary)
        {
            var node = kv.Value;
            if (node == null) continue;

            foreach (var j in node.GetComponents<Joint2D>())
                Destroy(j);

            var rb = node.GetComponent<Rigidbody2D>();
            if (rb != null)
                Destroy(rb);

            if (savedPos.TryGetValue(kv.Key, out var p))
                node.transform.position = p;

            if (savedRot.TryGetValue(kv.Key, out var rz))
                node.transform.rotation = Quaternion.Euler(0f, 0f, rz);
        }

        for (int i = edgeList.Count - 1; i >= 0; i--)
        {
            var e = edgeList[i];
            if (e == null) { edgeList.RemoveAt(i); continue; }
            e.ResetSim();
        }

        if (previewLine) previewLine.enabled = true;
    }

    private void OnText(string message)
    {
        StopAllCoroutines();
        StartCoroutine(BuildEventManager.Instance.MessageOn(message));
    }
}
