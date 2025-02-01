using UnityEngine;

public class Wire : MonoBehaviour
{
    [Header("Connection Points")]
    public Transform hole1;
    public Transform hole2;

    private void Start()
    {
        // Register connection with NodeManager
        if (hole1 && hole2)
        {
            NodeManager.Instance.CreateConnection(
                hole1, 
                hole2,
                gameObject
            );
            
            // Add wire properties
            Properties wireProps = gameObject.AddComponent<Properties>();
            wireProps.itemObject = gameObject;
        }
    }

    private void OnDestroy()
    {
        NodeManager.Instance.RecalculateCircuit();
    }
}