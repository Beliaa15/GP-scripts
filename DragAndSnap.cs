using Unity.Mathematics;
using UnityEngine;

public class DragAndSnap3D : MonoBehaviour
{
    private Vector3 offset;
    private Camera mainCamera;
    private Quaternion originalRotation;
    [SerializeField] private float halfLength ;
    [SerializeField] private float yOffset ;
    private NodeManager crctManager;
    [SerializeField] private NodeManager.ComponentType componentType;

    private void Start()
    {
        mainCamera = Camera.main;
        originalRotation = transform.rotation;
        crctManager = FindObjectOfType<NodeManager>();
        // Store the original rotation explicitly
    }

    private void OnMouseDown()
    {
        offset = mainCamera.WorldToScreenPoint(transform.position) - Input.mousePosition;
    }

    private void OnMouseDrag()
    {
        Vector3 newScreenPoint = Input.mousePosition + offset;
        Vector3 newPosition = mainCamera.ScreenToWorldPoint(newScreenPoint);
        transform.position = new Vector3(newPosition.x, 0.8f, newPosition.z); // Maintain depth
    }

    private void OnMouseUp()
    {
        //originalRotation = Quaternion.Euler(0, -90, -90);
        // Calculate the start and end positions of the resistor
        Vector3 direction = transform.TransformDirection(Vector3.right); // Adjust for rotation
        // Adjust this based on the actual length of the resistor


        Transform row0 = GameObject.Find("row0").transform;

        // Get the first and last children of row0
        Transform firstHole = row0.GetChild(0); // First child
        Transform lastHole = row0.GetChild(row0.childCount - 1); // Last child

        // Check if the start position is between the first and last holes (on the same axis, e.g., x or z)
        

        Vector3 startPosition = transform.position - (Vector3.right * halfLength);
        Vector3 endPosition = transform.position + (Vector3.right * halfLength);

        if (startPosition.x < firstHole.position.x)
        {
            startPosition.x = firstHole.position.x;
            endPosition = startPosition + (Vector3.right * halfLength*2);
        }else if(endPosition.x > lastHole.position.x)
        {
            endPosition.x = lastHole.position.x;
            startPosition = endPosition - (Vector3.right * halfLength * 2);
        }

        //startPosition.x = Mathf.Max(startPosition.x, firstHole.position.x);
        //endPosition.x = Mathf.Min(endPosition.x, firstHole.position.x);

        Debug.Log($"=============================================================");
        Debug.Log($"Start: {startPosition}, End: {endPosition}");
        (Transform closestHole1, Transform closestHole2) = FindClosestPair(startPosition, endPosition);

        if (closestHole1 != null && closestHole2 != null)
        {
            // Log the selected holes and their parents
            Debug.Log($"Selected Hole 1: {closestHole1.name}, Parent: {closestHole1.parent.name}");
            Debug.Log($"Selected Hole 2: {closestHole2.name}, Parent: {closestHole2.parent.name}");

            // Align the resistor between the two holes
            Vector3 hole1Position = closestHole1.position;
            Vector3 hole2Position = closestHole2.position;
            hole1Position.y += yOffset;
            hole2Position.y += yOffset;
            transform.position = (hole1Position + hole2Position) / 2;

            // Align rotation based on holes
            //Vector3 alignmentDirection = (hole2Position - hole1Position).normalized;

            //Quaternion alignmentRotation = Quaternion.LookRotation(alignmentDirection, Vector3.up);
            //Debug.Log($"alignmentDirection: {alignmentDirection}, alignmentRotation: {alignmentRotation}");
            //Debug.Log($"Inverse: {Quaternion.Inverse(Quaternion.LookRotation(Vector3.forward, Vector3.right))}");
            // Combine alignment with original rotation
            //alignmentRotation* Quaternion.Inverse(Quaternion.LookRotation(Vector3.right, Vector3.up)) *
            transform.rotation = originalRotation;
            
            crctManager.RegisterComponent(transform, componentType);
            crctManager.CreateConnection(closestHole1, closestHole2, gameObject);
        }
        else
        {
            Debug.Log("No suitable holes found.");
        }
    }

    private (Transform, Transform) FindClosestPair(Vector3 startPosition, Vector3 endPosition)
    {
        Transform closestHole1 = null;
        Transform closestHole2 = null;
        float minCombinedDistance = Mathf.Infinity;

        // Define a maximum threshold distance to consider a hole as "close enough"
        float maxThresholdDistance = 1f; // Adjust based on your scene scale

        foreach (Transform row in GameObject.Find("holes").transform)
        {
            Transform[] holesInRow = GetHolesFromRow(row);
            for (int i = 0; i < holesInRow.Length; i++)
            {
                for (int j = i + 1; j < holesInRow.Length; j++)
                {
                    float distance1 = Vector3.Distance(startPosition, holesInRow[i].position);
                    float distance2 = Vector3.Distance(endPosition, holesInRow[j].position);
                    float combinedDistance = distance1 + distance2;

                    if (combinedDistance < minCombinedDistance)
                    {
                        minCombinedDistance = combinedDistance;
                        closestHole1 = holesInRow[i];
                        closestHole2 = holesInRow[j];
                    }
                }
            }
        }

        // Validation: Check if the closest pair is within the maximum threshold distance
        if (closestHole1 != null && closestHole2 != null)
        {
            float distanceToClosestHole1 = Vector3.Distance(startPosition, closestHole1.position);
            float distanceToClosestHole2 = Vector3.Distance(endPosition, closestHole2.position);

            if (distanceToClosestHole1 > maxThresholdDistance || distanceToClosestHole2 > maxThresholdDistance)
            {
                Debug.Log("Start or end positions are too far from the nearest holes.");
                return (null, null); // No suitable holes found within threshold
            }
        }
        
        Debug.Log($"Selected Hole 1: {closestHole1.name}, Parent: {closestHole1.parent.name}");
        Debug.Log($"Selected Hole 2: {closestHole2.name}, Parent: {closestHole2.parent.name}");
        
        return (closestHole1, closestHole2);
    }

    private Transform[] GetHolesFromRow(Transform row)
    {
        int childCount = row.childCount;
        Transform[] holes = new Transform[childCount];
        for (int i = 0; i < childCount; i++)
        {
            holes[i] = row.GetChild(i);
        }
        return holes;
    }
}