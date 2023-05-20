using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    private void Update()
    {
        MoveControls();
        RotationControls();
    }
    private void Start()
    {
        distanceFromTarget = defaultDistanceFromTarget;
    }

    [SerializeField]
    [Tooltip("Speed at which the camera moves left, right, upwards, downwards, backwards and forwards")]
    private float speed;
    [SerializeField]
    [Tooltip("Speed at which the camera moves forward and backward when scrolling")]
    private float scrollSpeed;
    private void MoveControls()
    {
        Vector2 scrollDelta = Input.GetMouseButton(1) ? Vector2.zero : Input.mouseScrollDelta;

        Vector3 dir = new Vector3((Input.GetAxis("Horizontal") * speed) + (scrollDelta.x * scrollSpeed), 0f, (Input.GetAxis("Vertical") * speed) + (scrollDelta.y * scrollSpeed));
        if (Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.LeftControl)) dir.y = -speed;
        if (Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.LeftShift)) dir.y = speed;

        if (dir != Vector3.zero)
            transform.Translate(dir * Time.deltaTime);
    }

    [Space]
    [SerializeField]
    [Tooltip("Sensitivity for left-clicked first-person rotation")]
    private float rotationSensitivity;
    [SerializeField]
    [Tooltip("Sensitivity for right-clicked rotation around a point")]
    private float rotateAroundSensitivity;

    private MousePosition.Point? mouseOrigin = null;

    [Space]
    [SerializeField]
    [Tooltip("Target point to show and rotate around while right-clicking")]
    private Transform target;
    [SerializeField]
    [Tooltip("Default distance between the camera and the target point")]
    private float defaultDistanceFromTarget;
    private float distanceFromTarget;
    [SerializeField]
    [Tooltip("Sensitivity for scrolling to change the distance between the camera and the target point")]
    private float distanceFromTargetScrollSensitivity;
    [SerializeField]
    [Tooltip("Minimum and maximum distance between the camera and the target point")]
    private Vector2 distanceFromTargetBounds;

    [Space]
    [SerializeField]
    [Tooltip("Sensitivity for the middle-clicked panning of the camera")]
    private float panSensitivity;
    private void RotationControls()
    {
        bool isRotating = Input.GetMouseButton(0);
        bool isRotatingAround = Input.GetMouseButton(1);
        bool isPanning = Input.GetMouseButton(2);
        int boolCount = Convert.ToByte(isRotating) + Convert.ToByte(isRotatingAround) + Convert.ToByte(isPanning);

        // Only one of the three can be true at a time
        if (boolCount == 1) // lock the cursor
        {
            if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))
            {
                mouseOrigin = MousePosition.GetCursorPosition();
                Cursor.lockState = CursorLockMode.Locked;
            }
        }
        else if (boolCount == 0 && mouseOrigin != null) // unlock the cursor (null check is to make sure it only happens once)
        {
            Cursor.lockState = CursorLockMode.None;
            MousePosition.SetCursorPosition(mouseOrigin ?? throw new System.Exception("Achievement earned: How did we get here?"));

            mouseOrigin = null;
            return;
        }

        Vector2 mouseDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

        target.gameObject.SetActive(isRotatingAround);
        // Rotate the camera around the target point
        if (isRotatingAround)
        {
            SetDistanceFromTarget();

            Vector3 pos = Camera.main.ScreenToViewportPoint(mouseDelta);

            // Apply the rotation
            transform.RotateAround(target.position, Vector3.up, pos.x * rotateAroundSensitivity);
            transform.RotateAround(target.position, transform.right, -pos.y * rotateAroundSensitivity);

            // Move the target so it stays visually at the center of the camera
            target.position = transform.position + transform.forward * distanceFromTarget;
        }

        if (mouseDelta == Vector2.zero) // performance
            return;

        // Rotate camera along Y axis
        else if (isRotating)
        {
            Vector3 pos = Camera.main.ScreenToViewportPoint(mouseDelta);

            transform.RotateAround(transform.position, transform.right, -pos.y * rotationSensitivity);
            transform.RotateAround(transform.position, Vector3.up, pos.x * rotationSensitivity);
        }

        // Move the camera on its XZ plane
        else if (isPanning)
        {
            Vector3 pos = Camera.main.ScreenToViewportPoint(mouseDelta);

            Vector3 move = new Vector3(pos.x, pos.y, 0) * -panSensitivity;
            move = transform.TransformDirection(move);
            transform.Translate(move, Space.World);
        }
    }

    // Changes the distance from the target point if scrolled and reverts it to default if left-clicked
    private void SetDistanceFromTarget()
    {
        // When left-clicked, return to default
        if (Input.GetMouseButtonDown(0))
        {
            distanceFromTarget = defaultDistanceFromTarget;
            return;
        }

        float scrollDelta = -Input.mouseScrollDelta.y * distanceFromTargetScrollSensitivity;

        if (scrollDelta > 0)
            distanceFromTarget *= scrollDelta + 1;
        else
            distanceFromTarget /= -scrollDelta + 1;

        distanceFromTarget = Mathf.Clamp(distanceFromTarget, distanceFromTargetBounds.x, distanceFromTargetBounds.y);
    }
}
