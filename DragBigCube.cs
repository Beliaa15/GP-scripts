using Unity.VisualScripting;
using UnityEngine;

public class DragBigCube : MonoBehaviour
{
    private Camera mainCamera;
    private bool isDragging = false;
    private Vector3 offset;
    private float zCoord;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void OnMouseDown()
    {
        isDragging = true;
        zCoord = mainCamera.WorldToScreenPoint(gameObject.transform.position).z;
        offset = gameObject.transform.position - GetMousePosition();
    }

    private void OnMouseUp()
    {
        isDragging = false;
    }

    private void OnMouseDrag()
    {
        if (isDragging)
        {
            transform.position = GetMousePosition() + offset;
            transform.position = new Vector3(transform.position.x, 0.3f, transform.position.z);
        }
    }

    private Vector3 GetMousePosition()
    {
        Vector3 mouseScreenPosition = Input.mousePosition;
        mouseScreenPosition.z = zCoord;
        return mainCamera.ScreenToWorldPoint(mouseScreenPosition);
    }
}
