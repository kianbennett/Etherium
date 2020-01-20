using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputHandler : Singleton<InputHandler> {

    [ReadOnly] public Vector2 MouseDelta;

    private Vector2 leftClickInit, rightClickInit;
    private Vector2 leftClickDelta, rightClickDelta;
    private Vector2 lastMousePos;
    private bool leftClickHeld, rightClickHeld;
    private const int minDistForDrag = 5;

    void Update() {
        // TODO: disable input during scene transition
        handleKeys();

        MouseDelta = (Vector2) Input.mousePosition - lastMousePos;
        lastMousePos = Input.mousePosition;

        if (!EventSystem.current.IsPointerOverGameObject()) {
            if (Input.GetMouseButtonDown(0)) {
                leftClickHeld = true;
                leftClickInit = Input.mousePosition;
                onLeftClickDown();
            }
            if (Input.GetMouseButtonDown(1)) {
                rightClickHeld = true;
                rightClickInit = Input.mousePosition;
                onRightClickDown();
            }
        }

        if (leftClickHeld) {
            float d = leftClickDelta.magnitude;
            leftClickDelta = (Vector2) Input.mousePosition - leftClickInit;

            // The first frame the mouse is dragged
            if (d == 0 && leftClickDelta.magnitude != 0) onLeftClickDragStart();
            onLeftClickHold();
        }

        if (rightClickHeld) {
            rightClickDelta = (Vector2) Input.mousePosition - rightClickInit;
            onRightClickHold();
        }

        if (Input.GetMouseButtonUp(0)) {
            leftClickHeld = false;
            onLeftClickRelease();
        }

        if (Input.GetMouseButtonUp(1)) {
            rightClickHeld = false;
            onRightClickRelease();
        }
    }

    private void handleKeys() {
        // TODO: put these character actions into some central character manager
        // if (Input.GetKeyDown(KeyCode.S)) {
        //     foreach (Character character in PlayerController.instance.SelectedCharacters) {
        //         if (!character.movement.isSitting && character.actionHandler.ClearActions()) {
        //             character.movement.StopMoving();
        //         }
        //     }
        // }

        // if (Input.GetKeyDown(KeyCode.LeftShift)) {
        //     foreach (Character character in PlayerController.instance.SelectedCharacters) {
        //         character.movement.isRunning = !character.movement.isRunning;
        //     }
        // }

        if (Input.GetKey(KeyCode.LeftArrow)) CameraController.instance.PanLeft();
        if (Input.GetKey(KeyCode.RightArrow)) CameraController.instance.PanRight();
        if (Input.GetKey(KeyCode.UpArrow)) CameraController.instance.PanForward();
        if (Input.GetKey(KeyCode.DownArrow)) CameraController.instance.PanBackward();
    }

    private void onLeftClickDown() {
    }

    private void onLeftClickDragStart() {
        HUD.instance.selectionBox.Show();
        PlayerController.instance.DeselectAllCharacters();
    }

    private void onLeftClickHold() {
        if (leftClickDelta.magnitude != 0) {
            HUD.instance.selectionBox.SetPos(leftClickInit + leftClickDelta / 2);
            HUD.instance.selectionBox.SetSize(leftClickDelta);
        } else {
            HUD.instance.selectionBox.SetSize(Vector2.zero);
        }
    }

    private void onLeftClickRelease() {
        if (leftClickDelta.magnitude == 0) {
            PlayerController.instance.DeselectAllCharacters();
            if (PlayerController.instance.objectHovered) {
                PlayerController.instance.objectHovered.OnLeftClick();
            }
        }
        HUD.instance.selectionBox.Hide();
    }

    private void onRightClickDown() {
        CameraController.instance.Grab();
    }

    private void onRightClickHold() {
        if (rightClickDelta.magnitude >= minDistForDrag && MouseDelta.magnitude > 0) {
            CameraController.instance.Pan();
        }
    }

    private void onRightClickRelease() {
        CameraController.instance.Release();

        if (rightClickDelta.magnitude < minDistForDrag) {
            if (PlayerController.instance.objectHovered) {
                PlayerController.instance.objectHovered.OnRightClick();
            } else {
                bool hit = World.instance.surface.GetMousedOverCoordinates(out Vector3 hitPoint, out Vector2Int hitCoords);
                if(World.instance.generator.IsInBounds(hitCoords.x, hitCoords.y)) {
                    TileData tile = World.instance.tileDataMap[hitCoords.x, hitCoords.y];
                    // Debug.Log(hitPoint + ", " + hitCoords + ", " + tile.type);
                    if (hit) PlayerController.instance.MoveUnits(tile, true);
                } else {
                    Debug.Log("Point outside world :(");
                }
            }
        }
    }
}
