﻿using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering.PostProcessing;

/*
*    Contains camera movement functionality
*/

public class CameraController : Singleton<CameraController>
{
    public new Camera camera;
    public Transform target;
    public PostProcessVolume postProcessVolume;

    public float panSpeed, rotationSpeed, zoomSpeed;
    public float minCameraSize, maxCameraSize;
    public bool panSmoothing;
    public float panSmoothingValue;

    // TargetPosition is separate from Target.position to allow for interpolated camera movement
    [ReadOnly] public Vector3 targetPosition;

    private bool isRotating, isGrabbing;
    private Vector3 localCameraOffset;
    private Vector3 worldSpaceGrab, worldSpaceGrabLast;
    private float orthographicSize;

    protected override void Awake()
    {
        base.Awake();

        localCameraOffset = camera.transform.localPosition;
        targetPosition = target.position;
        orthographicSize = camera.orthographicSize;
    }

    void Update()
    {
        // TODO: move this to InputHandler
        isRotating = Input.GetMouseButton(2);

        if (isRotating)
        {
            float rot = Input.GetAxis("Mouse X") * rotationSpeed;
            target.Rotate(Vector3.up, rot);
        }

        // Only zoom if the mouse isn't over a UI element to avoid zooming when scrolling
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            if (Input.mouseScrollDelta.y > 0) Zoom(-zoomSpeed);
            if (Input.mouseScrollDelta.y < 0) Zoom(zoomSpeed);
        }

        if (Input.GetKey(KeyCode.Space) && PlayerController.instance.selectedObjects.Count > 0)
        {
            Vector3 averagePosition = Vector3.zero;
            foreach(WorldObject selectedObject in PlayerController.instance.selectedObjects)
            {
                averagePosition += selectedObject.transform.position;
            }
            averagePosition /= PlayerController.instance.selectedObjects.Count;
            targetPosition = averagePosition;
        }

        target.position = panSmoothing ? Vector3.Lerp(target.position, targetPosition, Time.deltaTime * panSmoothingValue) : targetPosition;
        camera.transform.localPosition = Vector3.Lerp(camera.transform.localPosition, localCameraOffset, Time.deltaTime * 10);
        camera.orthographicSize = Mathf.Lerp(camera.orthographicSize, orthographicSize, Time.deltaTime * 20);
    }

    public void Move(Vector3 delta)
    {
        targetPosition += delta;
        targetPosition.x = Mathf.Clamp(targetPosition.x, -World.instance.generator.worldSize / 2.0f, World.instance.generator.worldSize / 2.0f);
        targetPosition.z = Mathf.Clamp(targetPosition.z, -World.instance.generator.worldSize / 2.0f, World.instance.generator.worldSize / 2.0f);
    }

    public void PanLeft()
    {
        if (isGrabbing) return;
        float cameraSizeScalar = (1 + orthographicSize / minCameraSize * 0.25f);
        Move(-target.right * panSpeed * cameraSizeScalar * Time.deltaTime);
    }

    public void PanRight()
    {
        if (isGrabbing) return;
        float cameraSizeScalar = (1 + orthographicSize / minCameraSize * 0.25f);
        Move(target.right * panSpeed * cameraSizeScalar * Time.deltaTime);
    }

    public void PanForward()
    {
        if (isGrabbing) return;
        float cameraSizeScalar = (1 + orthographicSize / minCameraSize * 0.25f);
        Move(target.forward * panSpeed * cameraSizeScalar * Time.deltaTime);
    }

    public void PanBackward()
    {
        if (isGrabbing) return;
        float cameraSizeScalar = (1 + orthographicSize / minCameraSize * 0.25f);
        Move(-target.forward * panSpeed * cameraSizeScalar * Time.deltaTime);
    }

    public void SetAbsolutePosition(Vector3 pos)
    {
        targetPosition = pos;
        target.position = pos;
        worldSpaceGrab = Vector3.zero;
        worldSpaceGrabLast = Vector3.zero;
    }

    // Gets the ground position where the mouse right clicks
    public void Grab()
    {
        Ray ray = camera.ScreenPointToRay(Input.mousePosition);
        bool hit = Physics.Raycast(ray, out RaycastHit hitInfo, float.MaxValue, 1 << LayerMask.NameToLayer("Ground"));
        if (hit)
        {
            worldSpaceGrab = hitInfo.point;
            worldSpaceGrabLast = worldSpaceGrab;
            isGrabbing = true;
        }
    }

    // Called when the mouse moves while grabbing
    public void Pan()
    {
        Ray ray = camera.ScreenPointToRay(Input.mousePosition);
        bool hit = Physics.Raycast(ray, out RaycastHit hitInfo, float.MaxValue, 1 << LayerMask.NameToLayer("Ground"));
        if (hit)
        {
            worldSpaceGrab = hitInfo.point;
            Vector3 delta = worldSpaceGrab - worldSpaceGrabLast;
            Move(-delta);
        }
    }

    public void Release()
    {
        isGrabbing = false;
    }

    public void Zoom(float delta)
    {
        orthographicSize += delta * orthographicSize / 5;
        if (orthographicSize < minCameraSize || orthographicSize > maxCameraSize)
        {
            orthographicSize = Mathf.Clamp(orthographicSize, minCameraSize, maxCameraSize);
            return;
        }
        Vector3 pos = localCameraOffset - camera.transform.localRotation * Vector3.forward * delta * 4;
        localCameraOffset = pos;
        return;
    }

    public void SetVignetteEnabled(bool enabled)
    {
        PostProcessProfile profile = postProcessVolume.profile;
        profile.TryGetSettings(out Vignette vignette);
        vignette.enabled.value = enabled;
    }

    public void SetBlurEnabled(bool enabled)
    {
        PostProcessProfile profile = postProcessVolume.profile;
        profile.TryGetSettings(out DepthOfField dof);
        dof.enabled.value = enabled;
    }
}
