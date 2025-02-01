using UnityEngine;
using System.Collections.Generic;

public class Node
{
    // Node registry
    public static List<Node> _registry = new List<Node>();
    
    // Node properties
    public GameObject nodeObject;
    public List<Wire> attachedWires = new List<Wire>();

    public Node(GameObject nodeObject)
    {
        this.nodeObject = nodeObject;
        _registry.Add(this);
    }

    public List<Wire> GetAttachedWires()
    {
        // Refresh wire connections
        attachedWires.Clear();
        foreach (Wire wire in WireManager.allWires)
        {
            if (wire.hole1 == nodeObject.transform || wire.hole2 == nodeObject.transform)
                attachedWires.Add(wire);
        }
        return attachedWires;
    }

    public static void ClearRegistry()
    {
        _registry.Clear();
    }
}