using GogoGaga.OptimizedRopesAndCables;
using UnityEngine;
using System.Collections.Generic;

public class WireManager : MonoBehaviour
{
    public KeyCode placementKey = KeyCode.W;
    public GameObject wirePrefab;
    public LayerMask holeLayer;
    public float wireYOffset = 0.1f;

    private bool isPlacingWire = false;
    private Transform firstHole;
    private GameObject currentWire;

    // Static list to keep track of all wire instances
    public static List<Wire> allWires = new List<Wire>();

    void Update()
    {
        HandleInput();
        UpdatePlacement();
    }

    void HandleInput()
    {
        if (Input.GetKeyDown(placementKey))
        {
            ToggleWirePlacement();
        }
    }

    void ToggleWirePlacement()
    {
        isPlacingWire = !isPlacingWire;
        if (!isPlacingWire) CancelPlacement();
    }

    void UpdatePlacement()
    {
        if (!isPlacingWire) return;

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, holeLayer))
            {
                if (firstHole == null)
                {
                    StartNewWire(hit.transform);
                }
                else
                {
                    CompleteWire(hit.transform);
                }
            }
        }
    }

    void StartNewWire(Transform hole)
    {
        firstHole = hole;
        currentWire = Instantiate(wirePrefab);

        Vector3 wirePosition = firstHole.position + Vector3.up * wireYOffset;
        currentWire.transform.position = wirePosition;

        Rope rope = currentWire.GetComponent<Rope>();
        if (rope != null)
        {
            rope.SetStartPoint(firstHole, true);
        }

        // Add the new wire to the list
        Wire wireComponent = currentWire.GetComponent<Wire>();
        if (wireComponent != null)
        {
            allWires.Add(wireComponent);
        }
    }

    void CompleteWire(Transform endHole)
    {
        Rope rope = currentWire.GetComponent<Rope>();
        if (rope != null)
        {
            rope.SetEndPoint(endHole, true);

            // Connect the circuit nodes
            if (CircuitManager.Instance != null)
            {
                CircuitManager.Instance.Connect(firstHole, endHole);
            }
            else
            {
                Debug.LogWarning("CircuitManager instance not found!");
            }
        }

        currentWire = null;
        firstHole = null;
    }

    void CancelPlacement()
    {
        if (currentWire != null)
        {
            // Remove connections if wire was partially placed
            if (firstHole != null && CircuitManager.Instance != null)
            {
                CircuitManager.Instance.RemoveConnection(firstHole, currentWire.transform);
            }

            // Remove the wire from the list
            Wire wireComponent = currentWire.GetComponent<Wire>();
            if (wireComponent != null)
            {
                allWires.Remove(wireComponent);
            }

            Destroy(currentWire);
        }
        firstHole = null;
    }
}