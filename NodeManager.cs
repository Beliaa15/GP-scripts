using System;
using System.Collections.Generic;
using UnityEngine;

public class NodeManager : MonoBehaviour
{
    // Singleton instance
    public static NodeManager Instance;

    // Matrix system from GitHub implementation
    private static List<Node> unknownNodes = new List<Node>();
    private static int matrixDimension;
    private static double[][] yMatrix;
    private static double[][] iMatrix;
    private static double[][] resultMatrix;
    private static Node groundNode;
    private static Node positiveNode;

    // Component tracking
    public enum ComponentType { Resistor, LED, Battery, Wire }
    private Dictionary<Transform, Node> holeToNodeMap = new Dictionary<Transform, Node>();
    private Dictionary<Transform, ComponentType> componentTypes = new Dictionary<Transform, ComponentType>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        ClearAllNodes();
    }

    // Called when components are added/removed
    public void RegisterComponent(Transform component, ComponentType type)
    {
        if (!componentTypes.ContainsKey(component))
        {
            componentTypes.Add(component, type);
            InitializeComponentProperties(component, type);
        }
    }

    public void UnregisterComponent(Transform component)
    {
        if (componentTypes.ContainsKey(component))
        {
            componentTypes.Remove(component);
            RecalculateCircuit();
        }
    }

    private void InitializeComponentProperties(Transform component, ComponentType type)
    {
        Properties props = component.GetComponent<Properties>();
        if (!props) props = component.gameObject.AddComponent<Properties>();

        switch (type)
        {
            case ComponentType.Resistor:
                props.resistance = component.GetComponent<Resistor>().resistance;
                break;
            case ComponentType.LED:
                LED led = component.GetComponent<LED>();
                props.voltageDrop = led.forwardVoltage;
                props.minCurrent = led.minCurrent;
                props.maxCurrent = led.maxCurrent;
                break;
            case ComponentType.Battery:
                props.voltage = component.GetComponent<BatteryComponent>().voltage;
                break;
        }
    }

    // Connection management
    public void CreateConnection(Transform hole1, Transform hole2, GameObject wireObject)
    {
        Node node1 = GetOrCreateNode(hole1.gameObject);
        Node node2 = GetOrCreateNode(hole2.gameObject());

        // Special node identification
        if (hole1.CompareTag("Ground")) groundNode = node1;
        if (hole2.CompareTag("Ground")) groundNode = node2;
        if (hole1.CompareTag("PositiveTerminal")) positiveNode = node1;
        if (hole2.CompareTag("PositiveTerminal")) positiveNode = node2;

        // Create wire connection
        Wire wire = wireObject.GetComponent<Wire>();
        new NodeConnection(node1, node2, wire?.GetComponent<Properties>());
        RecalculateCircuit();
    }

    public Node GetOrCreateNode(GameObject holeObject)
    {
        if (!holeToNodeMap.ContainsKey(holeObject.transform))
        {
            Node newNode = new Node(holeObject);
            Node._registry.Add(newNode);
            holeToNodeMap[holeObject.transform] = newNode;
        }
        return holeToNodeMap[holeObject.transform];
    }

    // Main calculation trigger
    public void RecalculateCircuit()
    {
        try
        {
            ClearMatrices();
            CalculateNodes();
            UpdateComponentVisuals();
        }
        catch (Exception e)
        {
            Debug.LogError($"Circuit calculation failed: {e.Message}");
            ResetAllComponents();
        }
    }

    private void CalculateNodes()
    {
        if (MakeConnectionsBetweenNodes())
        {
            CreateMatrices();
            AssignValuesToMatrices();
            CalculateInverseMatrix();
            CalculateResultMatrix();

            if (!AssignResultVoltagesToNodes())
            {
                ResetVoltages();
            }
            AssignCurrents();
        }
    }

    // Matrix calculations from GitHub implementation
    private static bool MakeConnectionsBetweenNodes()
    {
        if (positiveNode == null || groundNode == null)
        {
            UIManager.SendWarningNotification("Circuit incomplete - missing power connections!");
            return false;
        }
        new NodeConnection(positiveNode, groundNode);
        return true;
    }

    private static void CreateMatrices()
    {
        matrixDimension = Node._registry.Count - 1 + NodeConnection.shortcircuitAmount + NodeConnection.ledAmount + 1;
        yMatrix = new double[matrixDimension][];
        iMatrix = new double[matrixDimension][];
        
        for (int i = 0; i < matrixDimension; i++)
        {
            yMatrix[i] = new double[matrixDimension];
            iMatrix[i] = new double[1];
        }
    }

    private static void CalculateInverseMatrix() => yMatrix = Matrix.MatrixInverse(yMatrix);
    private static void CalculateResultMatrix() => resultMatrix = Matrix.MatrixProduct(yMatrix, iMatrix);

    private static bool AssignResultVoltagesToNodes()
    {
        foreach (Node node in unknownNodes)
        {
            if (double.IsNaN(resultMatrix[unknownNodes.IndexOf(node)][0]))
            {
                UIManager.SendWarningNotification("Unsolvable circuit configuration!");
                return false;
            }
            node.nodeObject.GetComponent<Properties>().voltage = resultMatrix[unknownNodes.IndexOf(node)][0];
        }
        return true;
    }

    // Visual updates
    private void UpdateComponentVisuals()
    {
        foreach (NodeConnection connection in NodeConnection._registry)
        {
            if (connection.item?.type == "LED")
            {
                LED led = connection.item.itemObject.GetComponent<LED>();
                double current = connection.item.itemObject.GetComponent<Properties>().current;
                led.UpdateLED((float)current);
            }
        }
    }

    private void ResetAllComponents()
    {
        foreach (var component in componentTypes)
        {
            Properties props = component.Key.GetComponent<Properties>();
            if (props)
            {
                props.voltage = 0;
                props.current = 0;
            }
        }
    }

    public static void ClearAllNodes()
    {
        Node._registry.Clear();
        NodeConnection._registry.Clear();
        unknownNodes.Clear();
        holeToNodeMap.Clear();
        groundNode = null;
        positiveNode = null;
        NodeConnection.shortcircuitAmount = 0;
        NodeConnection.ledAmount = 0;
    }

    private void ClearMatrices()
    {
        yMatrix = null;
        iMatrix = null;
        resultMatrix = null;
        unknownNodes.Clear();
    }

    // Helper methods
    public static bool IsGround(Node node) => node == groundNode;
    public static bool IsPositive(Node node) => node == positiveNode;
    public static void ResetVoltages() => unknownNodes.ForEach(node => node.nodeObject.GetComponent<Properties>().voltage = 0);
}