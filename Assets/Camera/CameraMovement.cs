using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    void Update()
    {
        MoveControls();
        RotationControls();
    }

    [SerializeField]
    private float speed;
    [SerializeField]
    private float scrollSpeed;
    private void MoveControls()
    {
        Vector3 dir = new Vector3((Input.GetAxis("Horizontal") * speed) + (Input.mouseScrollDelta.x * scrollSpeed), 0f, (Input.GetAxis("Vertical") * speed) + (Input.mouseScrollDelta.y * scrollSpeed));
        if (Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.LeftControl)) dir.y = -speed;
        if (Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.LeftShift)) dir.y = speed;

        if (dir != Vector3.zero)
            transform.Translate(dir * Time.deltaTime);
    }

    [Space]
    [SerializeField]
    private float rotationSensitivity;
    [SerializeField]
    private float rotateAroundSensitivity;
    [SerializeField]
    private float panSensitivity;

    private MousePosition.Point mouseOrigin;

    [SerializeField]
    private Transform target;
    [SerializeField]
    private float distanceFromTarget;
    private void RotationControls()
    {
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))
        {
            mouseOrigin = MousePosition.GetCursorPosition();
            Cursor.lockState = CursorLockMode.Locked;
        }
        else if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1) || Input.GetMouseButtonUp(2))
        {
            Cursor.lockState = CursorLockMode.None;
            MousePosition.SetCursorPosition(mouseOrigin);
        }

        Vector2 mouseDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

        bool isRotating = Input.GetMouseButton(0);
        bool isRotatingAround = Input.GetMouseButton(1);
        bool isPanning = Input.GetMouseButton(2);

        // Rotate camera along Y axis
        if (isRotating)
        {
            Vector3 pos = Camera.main.ScreenToViewportPoint(mouseDelta);

            transform.RotateAround(transform.position, transform.right, -pos.y * rotationSensitivity);
            transform.RotateAround(transform.position, Vector3.up, pos.x * rotationSensitivity);
        }

        // Rotate the camera around the target point
        target.gameObject.SetActive(isRotatingAround);
        if (isRotatingAround)
        {
            Vector3 pos = Camera.main.ScreenToViewportPoint(mouseDelta);

            // Apply the rotation
            transform.RotateAround(target.position, Vector3.up, pos.x * rotateAroundSensitivity);
            transform.RotateAround(target.position, transform.right, -pos.y * rotateAroundSensitivity);

            target.position = transform.position + transform.forward * distanceFromTarget;
        }

        // Move the camera on its XZ plane
        if (isPanning)
        {
            Vector3 pos = Camera.main.ScreenToViewportPoint(mouseDelta);

            Vector3 move = new Vector3(pos.x, pos.y, 0) * -panSensitivity;
            move = transform.TransformDirection(move);
            transform.Translate(move, Space.World);
        }
    }
}
