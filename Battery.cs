using UnityEngine;

[RequireComponent(typeof(Properties))]
public class BatteryComponent : MonoBehaviour
{
    public float voltage = 9f;
    public Transform positiveTerminal;
    public Transform negativeTerminal;

    private Properties properties;

    private void Awake()
    {
        properties = GetComponent<Properties>();
        properties.voltage = voltage;
        properties.itemObject = gameObject;
        
        // Auto-tag terminals
        positiveTerminal.tag = "PositiveTerminal";
        negativeTerminal.tag = "Ground";
    }

    public void UpdateVoltage(float newVoltage)
    {
        voltage = newVoltage;
        properties.voltage = newVoltage;
        NodeManager.Instance.RecalculateCircuit();
    }
}