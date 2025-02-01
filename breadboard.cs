using UnityEngine;
using System.Collections.Generic;

public class BreadBoardManager : MonoBehaviour
{
    public static BreadBoardManager Instance;
    private Dictionary<Transform, GameObject> holeConnections = new Dictionary<Transform, GameObject>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void AddComponentToCircuit(GameObject component, Transform hole1, Transform hole2)
    {
        holeConnections[hole1] = component;
        holeConnections[hole2] = component;

        Debug.Log($"Component {component.name} connected to Hole1: {hole1.name}, Hole2: {hole2.name}");

        ValidateCircuit();
    }

    public void ValidateCircuit()
    {
        // Logic to validate if the circuit is open or closed
        Debug.Log("Validating circuit...");
        if (IsCircuitOpen())
        {
            Debug.Log("Circuit is open.");
        }
        else
        {
            Debug.Log("Circuit is closed. Calculating...");
            CalculateCircuit();
        }
    }

    private bool IsCircuitOpen()
    {
        // Check for any unconnected holes or disconnected paths
        return holeConnections.Count < 2; // Simplified check for demo purposes
    }

    public void CalculateCircuit()
    {
        // Example: Iterate through connections and compute voltages, currents, etc.
        Debug.Log("Calculating voltages and currents...");
        foreach (var connection in holeConnections)
        {
            Debug.Log($"Hole {connection.Key.name} connected to {connection.Value.name}");
        }
    }
}