using UnityEngine;

public class Properties : MonoBehaviour
{
    [Header("Electrical Properties")]
    public double resistance;     // For resistors
    public double voltage;        // For batteries
    public double voltageDrop;    // For LEDs/diodes
    public double current;
    public double minCurrent;     // For LEDs
    public double maxCurrent;     // For LEDs
    
    [Header("References")]
    public GameObject itemObject; // Connected component
    public GameObject nodeObject; // Connected node
}