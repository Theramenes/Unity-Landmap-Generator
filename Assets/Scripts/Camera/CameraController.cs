using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System;
using UnityEngine;

public class CameraController : MonoBehaviour

{
    public FloatVariable movementSpeed;
    public FloatVariable movementLerpSpeed;
    public FloatVariable zoomSpeed;
    public MoveAxis HorizontalKey = new MoveAxis(KeyCode.D, KeyCode.A);
    public MoveAxis VerticalKey = new MoveAxis(KeyCode.W, KeyCode.S);

    public Transform cameraTransform;

    private Vector3 newPosition;
    private Vector3 dragStartPosition;
    private Vector3 dragCurrentPosition;
    private Vector3 newZoomPosition;

    [Serializable]
    public class MoveAxis
    {
        public KeyCode Positive;
        public KeyCode Negative;

        public MoveAxis(KeyCode positive, KeyCode negative)
        {
            Positive = positive;
            Negative = negative;
        }

        public static implicit operator float(MoveAxis axis)
        {
            return (Input.GetKey(axis.Positive) ? 1.0f : 0.0f) - (Input.GetKey(axis.Negative) ? 1.0f : 0.0f);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        newPosition = transform.position;
        newZoomPosition = cameraTransform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        HandleMouseInput();
        HandleKeyMovementInput();
        cameraMove();
    }

    void FixedUpdate()
    {
    }

    void cameraMove()
    {
        transform.position = Vector3.Lerp(transform.position, newPosition, Time.deltaTime * movementLerpSpeed.Value);
        cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition, newZoomPosition, Time.deltaTime * movementLerpSpeed.Value);
    }

    void HandleKeyMovementInput()
    {
        Vector3 moveDirectionNormal = new Vector3(HorizontalKey, 0.0f, VerticalKey).normalized;
        newPosition += moveDirectionNormal * Time.deltaTime * movementSpeed.Value;
    }

    void HandleMouseInput()
    {
        float enter;
        Plane plane = new Plane(Vector3.up, Vector3.zero);
        Ray ray;

        if (Input.mouseScrollDelta.y != 0)
        {
            newZoomPosition += GetCameraZoomNormal() * zoomSpeed.Value * Input.mouseScrollDelta.y;
            Debug.Log("mouseScrollDelta and newZoomPosition:" + Input.mouseScrollDelta.y + newZoomPosition);
        }

        if (Input.GetMouseButtonDown(2))
        {
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (plane.Raycast(ray, out enter))
            {
                dragStartPosition = ray.GetPoint(enter);
            }

            Debug.Log("dragStartPosition:" + dragStartPosition);
        }
        if (Input.GetMouseButton(2))
        {
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (plane.Raycast(ray, out enter))
            {
                dragCurrentPosition = ray.GetPoint(enter);
                newPosition = transform.position - (dragCurrentPosition - dragStartPosition);
            }

            Debug.Log("dragCurrentPosition:" + dragCurrentPosition);
        }
    }

    Vector3 GetCameraZoomNormal()
    {
        float cameraRotationXRad = cameraTransform.eulerAngles.x * Mathf.Deg2Rad;
        float cameraRotationYRad = cameraTransform.eulerAngles.y * Mathf.Deg2Rad;

        Vector3 zoomDirectionNormal = new Vector3(Mathf.Cos(cameraRotationXRad) * Mathf.Sin(cameraRotationYRad),
            -Mathf.Sin(cameraRotationXRad),
            Mathf.Cos(cameraRotationXRad) * Mathf.Cos(cameraRotationYRad));
        Debug.Log("Angles:" + cameraTransform.eulerAngles.x + " " + cameraTransform.eulerAngles.y);
        Debug.Log("cameraTransform:" + Mathf.Sin(cameraRotationXRad) + " " + Mathf.Sin(cameraRotationYRad));
        Debug.Log("cameraZoom:" + zoomDirectionNormal);
        return zoomDirectionNormal;
    }

}
