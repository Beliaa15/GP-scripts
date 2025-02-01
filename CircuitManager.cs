using System.Collections.Generic;
using System.Linq; // Required for LINQ methods like Select
using UnityEngine;


public class CircuitManager : MonoBehaviour
{
    private Dictionary<Transform, List<Transform>> connections = new Dictionary<Transform, List<Transform>>();
    private Dictionary<Transform, ComponentType> components = new Dictionary<Transform, ComponentType>();
    private Dictionary<Transform, float> voltages = new Dictionary<Transform, float>();
    private Dictionary<Transform, float> currents = new Dictionary<Transform, float>();
    private HashSet<Transform> visited = new HashSet<Transform>();

    [SerializeField] private float batteryVoltage = 9.0f; // Default battery voltage

    public enum ComponentType { Resistor, LED, Battery, Wire }

    public static CircuitManager Instance;

    private void Start()
    {
        Debug.Log("Circuit Manager Initialized.");
        // Retrieve the "holes" parent object containing all rows
        Transform holesParent = GameObject.Find("holes").transform;

        // Retrieve the first two rows
        Transform firstRow = holesParent.GetChild(4);
        Transform secondRow = holesParent.GetChild(5);

        // Retrieve holes 1 and 5 from the first and second rows respectively
        Transform row1hole1 = firstRow.GetChild(0); // Hole 1 in the first row
        Transform row2hole1 = secondRow.GetChild(0); // Hole 1 in the first row
        Transform row1hole5 = firstRow.GetChild(4); 
        Transform row2hole5 = secondRow.GetChild(4); 

        // Connect hole1 and hole5
        Connect(row1hole1, row2hole1);
        Connect(row1hole5, row2hole5);


        //Connect(row1hole1, row1hole5);
        //Connect(row2hole1, row2hole5);

    }

    public void AddComponent(Transform component, ComponentType type)
    {
        if (!components.ContainsKey(component))
        {
            components.Add(component, type);
            //Debug.Log($"{type} added to the circuit at {component.name}");
        }
    }

    public void RemoveComponent(Transform component)
    {
        if (components.ContainsKey(component))
        {
            connections.Remove(component);
            components.Remove(component);
            Debug.Log($"{component.name} removed from the circuit.");
        }
    }

    public void Connect(Transform hole1, Transform hole2)
    {
        if (!connections.ContainsKey(hole1))
            connections[hole1] = new List<Transform>();
        if (!connections.ContainsKey(hole2))
            connections[hole2] = new List<Transform>();

        connections[hole1].Add(hole2);
        connections[hole2].Add(hole1);

        //Debug.Log($"Connected {hole1.name} to {hole2.name}");
        //foreach (var kvp in connections)
        //{
        //    string parentName = kvp.Key.parent != null ? kvp.Key.parent.name : "No Parent";
        //    string connectedHoles = string.Join(", ", kvp.Value.Select(v => $"{v.name} (Parent: {v.parent?.name ?? "No Parent"})"));

        //    Debug.Log($"Hole {kvp.Key.name} (Parent: {parentName}) connected to: {connectedHoles}");
        //}

        //Debug.Log($"================================================================");
        SimulateCircuit();
    }

    public bool IsCircuitClosed()
    {
        visited.Clear();
        if (components.Count == 0) return false;

        Transform startNode = connections.Keys.FirstOrDefault();
        if (startNode == null) return false;

        bool result = TraverseCircuit(startNode);
        Debug.Log(result ? "Circuit is closed." : "Circuit is open.");
        return result;
    }

    private bool TraverseCircuit(Transform current)
    {
        visited.Add(current);
        Debug.Log($"Visiting: {current.name} | Connections: {connections[current].Count}");

        foreach (var neighbor in connections[current])
        {
            if (!visited.Contains(neighbor))
            {
                if (TraverseCircuit(neighbor))
                {
                    return true;
                }
            }
            else if (neighbor == connections.Keys.First()) // Detect loop back to start
            {
                Debug.Log($"Detected loop back to start at {neighbor.name}");
                return true;
            }
        }

        return false;
    }


    public void SimulateCircuit()
    {
        if (!IsCircuitClosed())
        {
            //Debug.LogError("Circuit is open! Simulation aborted.");
            Debug.Log("Circuit is open! Simulation aborted.");
            ResetCircuitValues();
            return;
        }
        Debug.Log("Circuit is closed");

        float totalResistance = CalculateTotalResistance();
        float circuitCurrent = batteryVoltage / (totalResistance > 0 ? totalResistance : float.MaxValue);

        UpdateComponentVoltagesAndCurrents(circuitCurrent);

        Debug.Log($"Circuit simulated. Total Resistance: {totalResistance:F2}Ω, Current: {circuitCurrent:F3}A");
    }

    private float CalculateTotalResistance()
    {
        // Handle series and parallel configurations
        return CalculateResistanceForPath(components.Keys);
    }

    private float CalculateResistanceForPath(IEnumerable<Transform> path)
    {
        float totalResistance = 0f;
        foreach (var comp in path)
        {
            if (components[comp] == ComponentType.Resistor)
            {
                var resistance = comp.GetComponent<Resistance>();
                if (resistance != null)
                {
                    totalResistance += resistance.Value;
                }
            }
        }

        // Add logic for parallel configurations if applicable
        // Placeholder logic: actual logic depends on detecting parallel paths
        return totalResistance;
    }

    private void UpdateComponentVoltagesAndCurrents(float circuitCurrent)
    {
        float remainingVoltage = batteryVoltage;

        foreach (var kvp in components)
        {
            if (kvp.Value == ComponentType.Resistor)
            {
                var resistance = kvp.Key.GetComponent<Resistance>();
                if (resistance != null)
                {
                    float voltageDrop = circuitCurrent * resistance.Value;
                    voltages[kvp.Key] = voltageDrop;
                    currents[kvp.Key] = circuitCurrent;
                    remainingVoltage -= voltageDrop;
                }
            }
            else if (kvp.Value == ComponentType.LED)
            {
                var led = kvp.Key.GetComponent<LED>();
                if (led != null)
                {
                    float voltageDrop = Mathf.Min(remainingVoltage, led.ForwardVoltage);
                    voltages[kvp.Key] = voltageDrop;
                    currents[kvp.Key] = circuitCurrent;
                    led.UpdateLED(circuitCurrent);
                    remainingVoltage -= voltageDrop;
                }
            }
        }
    }

    private void ResetCircuitValues()
    {
        voltages.Clear();
        currents.Clear();

        foreach (var kvp in components)
        {
            if (kvp.Value == ComponentType.LED)
            {
                var led = kvp.Key.GetComponent<LED>();
                if (led != null)
                {
                    led.UpdateLED(0f);
                }
            }
        }
    }
}