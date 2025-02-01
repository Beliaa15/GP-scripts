using UnityEngine;
using System.Collections.Generic;


public class NodeConnection
{
    public static List<NodeConnection> _registry = new List<NodeConnection>();
    public static int shortcircuitAmount = 0;
    public static int ledAmount = 0;

    // Connection properties
    public Node node1;
    public Node node2;
    public Wire wire;
    public Component item;

    public NodeConnection(Node node1, Node node2, Component item = null)
    {
        this.node1 = node1;
        this.node2 = node2;
        this.item = item;
        
        _registry.Add(this);

        // Count special connection types
        if (item == null) shortcircuitAmount++;
        if (item is LED) ledAmount++;
    }

    public NodeConnection(Node node1, Node node2, Wire wire)
    {
        this.node1 = node1;
        this.node2 = node2;
        this.wire = wire;
        _registry.Add(this);
    }
}