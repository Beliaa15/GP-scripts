using UnityEngine;
[RequireComponent(typeof(Properties))]
public class Resistance : MonoBehaviour
{
    [SerializeField] public float resistanceValue = 100f; // Default resistance in ohms
    
     private Properties properties;

    private void Awake()
    {
        properties = GetComponent<Properties>();
        properties.resistance = resistanceValue;
        properties.itemObject = gameObject;
    }

    public void UpdateResistance(float newResistance)
    {
        resistanceValue = newResistance;
        properties.resistance = newResistance;
        NodeManager.Instance.RecalculateCircuit();
    }
} 