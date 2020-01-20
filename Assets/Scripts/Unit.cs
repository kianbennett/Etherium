using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Unit : WorldObject {

    public LineRenderer pathLineRenderer;
    public Transform pathLineTriangle;
    public UnitActionHandler actionHandler;
    public UnitMovement movement;
    public Transform model;

    public float healthMax;
    [ReadOnly] public float healthCurrent;

    protected override void Awake() {
        base.Awake();

        healthCurrent = healthMax;

        HUD.instance.CreateHealthbar(this);
    }

    protected override void Update () {
        base.Update();
        // Set selected based on whether the unit is inside the HUD selection box
        if(HUD.hasSelectionBox) {
            if (!isSelected && isInSelectionBox()) {
                PlayerController.instance.SelectObject(this);
            } else if(isSelected && !isInSelectionBox()) {
                PlayerController.instance.DeselectObject(this);
            }
        }
        
        // TODO: Look at this
        // bool hasPath = movement.pathNodes != null;
        // pathLineRenderer.gameObject.SetActive(hasPath);
        // pathLineTriangle.gameObject.SetActive(hasPath);
        // if(hasPath) {
        //     pathLineTriangle.position = movement.pathNodes.Last().worldPos;
        //     float angle = Vector3.Angle(movement.pathNodes.Last().worldPos - movement.pathNodes[movement.pathNodes.Count - 2].worldPos, Vector3.right) - 90;
        //     pathLineTriangle.rotation = Quaternion.Euler(Vector3.up * angle);
        // }
	}

    public override void SetHovered(bool hovered) {
        base.SetHovered(hovered);
    }

    public override void SetSelected(bool selected) {
        base.SetSelected(selected);
    }

    public override void OnLeftClick() {
        base.OnLeftClick();

        PlayerController.instance.SelectObject(this);
    }

    public void MoveToPoint(TileData tile) {
        movement.CalculatePath(tile);
    }

    private bool isInSelectionBox() {
        Vector3 point = CameraController.instance.camera.WorldToScreenPoint(transform.position);
        return HUD.instance.selectionBox.Rect.rect.Contains(point - HUD.instance.selectionBox.transform.position);
    }

    void OnDestroy() {
        if(PlayerController.instance) {
            if (PlayerController.instance.selectedObjects.Contains(this)) {
                PlayerController.instance.selectedObjects.Remove(this);
            }
            if (PlayerController.instance.objectHovered == this) {
                PlayerController.instance.objectHovered = null;
            }
        }
    }
}
