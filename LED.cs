using UnityEngine;

[RequireComponent(typeof(Properties))]
public class LED : MonoBehaviour
{
    [Header("LED Specifications")]
    public float forwardVoltage = 2.1f;
    public float maxSafeCurrent = 0.02f;
    public float minOperatingCurrent = 0.01f;

    [Header("Visuals")]
    public Color emissionColor = Color.green;
    
    private Properties properties;

    private void Awake()
    {
        properties = GetComponent<Properties>();
        properties.voltageDrop = forwardVoltage;
        properties.minCurrent = minOperatingCurrent;
        properties.maxCurrent = maxSafeCurrent;
        properties.itemObject = gameObject;
    }

    public void UpdateLED(float current)
    {
        // Update both physics and visual state
        properties.current = current;
        NodeManager.UpdateGlowIntensity(gameObject, current);
    }
}