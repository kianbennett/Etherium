using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputHandler : Singleton<InputHandler>
{
    [ReadOnly] public Vector2 MouseDelta;

    private Vector2 leftClickInit, rightClickInit;
    private Vector2 leftClickDelta, rightClickDelta;
    private Vector2 lastMousePos;
    private bool leftClickHeld, rightClickHeld;
    private const int minDistForDrag = 5;

    void Update()
    {
        // TODO: disable input during scene transition
        handleKeys();

        MouseDelta = (Vector2)Input.mousePosition - lastMousePos;
        lastMousePos = Input.mousePosition;

        if (!EventSystem.current.IsPointerOverGameObject())
        {
            if (Input.GetMouseButtonDown(0))
            {
                leftClickHeld = true;
                leftClickInit = Input.mousePosition;
                onLeftClickDown();
            }
            if (Input.GetMouseButtonDown(1))
            {
                rightClickHeld = true;
                rightClickInit = Input.mousePosition;
                onRightClickDown();
            }
        }

        if (leftClickHeld)
        {
            float d = leftClickDelta.magnitude;
            leftClickDelta = (Vector2)Input.mousePosition - leftClickInit;

            // The first frame the mouse is dragged
            if (d == 0 && leftClickDelta.magnitude != 0) onLeftClickDragStart();
            onLeftClickHold();
        }

        if (rightClickHeld)
        {
            rightClickDelta = (Vector2)Input.mousePosition - rightClickInit;
            onRightClickHold();
        }

        if (leftClickHeld && Input.GetMouseButtonUp(0))
        {
            leftClickHeld = false;
            onLeftClickRelease();
        }

        if (rightClickHeld && Input.GetMouseButtonUp(1))
        {
            rightClickHeld = false;
            onRightClickRelease();
        }

        if (Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.Escape)) GameManager.instance.TogglePaused();
        if (Input.GetKeyDown(KeyCode.F4)) GameManager.instance.ToggleSpedUp();
    }

    private void handleKeys()
    {
        if (Input.GetKey(KeyCode.LeftArrow)) CameraController.instance.PanLeft();
        if (Input.GetKey(KeyCode.RightArrow)) CameraController.instance.PanRight();
        if (Input.GetKey(KeyCode.UpArrow)) CameraController.instance.PanForward();
        if (Input.GetKey(KeyCode.DownArrow)) CameraController.instance.PanBackward();
    }

    private void onLeftClickDown()
    {
    }

    private void onLeftClickDragStart()
    {
        HUD.instance.selectionBox.Show();
        PlayerController.instance.DeselectAll();
    }

    private void onLeftClickHold()
    {
        if (leftClickDelta.magnitude != 0)
        {
            HUD.instance.selectionBox.SetPos(leftClickInit + leftClickDelta / 2);
            HUD.instance.selectionBox.SetSize(leftClickDelta);
        }
        else
        {
            HUD.instance.selectionBox.SetSize(Vector2.zero);
        }
    }

    private void onLeftClickRelease()
    {
        PlayerController playerController = PlayerController.instance;
        if (leftClickDelta.magnitude == 0)
        {
            if (!playerController.isPlacingStructure)
            {
                playerController.DeselectAll();
            }
            if (playerController.objectHovered && playerController.objectHovered.OwnerId != 1)
            {
                playerController.objectHovered.OnLeftClick();
            }
            else
            {
                // bool hit = World.instance.surface.GetMousedOverCoordinates(out Vector3 hitPoint, out Vector2Int hitCoords);
                Vector2Int hitCoords = World.instance.surface.tileHitCoords;
                if (World.instance.generator.IsInBounds(hitCoords.x, hitCoords.y))
                {
                    TileData tile = World.instance.tileDataMap[hitCoords.x, hitCoords.y];
                    playerController.LeftClickTile(tile);
                }
                else
                {
                    Debug.Log("Point outside world :(");
                }
            }
        }
        HUD.instance.selectionBox.Hide();
    }

    private void onRightClickDown()
    {
        CameraController.instance.Grab();
    }

    private void onRightClickHold()
    {
        if (rightClickDelta.magnitude >= minDistForDrag && MouseDelta.magnitude > 0)
        {
            CameraController.instance.Pan();
        }
    }

    private void onRightClickRelease()
    {
        PlayerController playerController = PlayerController.instance;
        CameraController.instance.Release();

        if (rightClickDelta.magnitude < minDistForDrag)
        {
            if (playerController.objectHovered && playerController.objectHovered.OwnerId != 1)
            {
                playerController.objectHovered.OnRightClick();
            }
            else
            {
                // bool hit = World.instance.surface.GetMousedOverCoordinates(out Vector3 hitPoint, out Vector2Int hitCoords);
                Vector2Int hitCoords = World.instance.surface.tileHitCoords;
                if (World.instance.generator.IsInBounds(hitCoords.x, hitCoords.y))
                {
                    TileData tile = World.instance.tileDataMap[hitCoords.x, hitCoords.y];
                    playerController.RightClickTile(tile);
                }
                else
                {
                    Debug.Log("Point outside world :(");
                }
            }
        }
    }
}
