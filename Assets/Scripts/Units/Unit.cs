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
    public ParticleSystem dustParticles;

    [ReadOnly] public float healthCurrent;
    public float healthMax;
    public int buildTime;
    public int buildCost;

    private Healthbar healthbar;

    protected override void Awake() {
        base.Awake();

        healthCurrent = healthMax;

        healthbar = HUD.instance.CreateHealthbar();
        healthbar.rect.sizeDelta = new Vector2(Mathf.Clamp(healthMax * 8, 0, 80), healthbar.rect.sizeDelta.y);

        HUD.instance.CreateUnitIcon(this);
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

        if(dustParticles != null) {
            bool isMoving = movement.pathNodes != null;
            if(isMoving != dustParticles.isPlaying) {
                if(isMoving) dustParticles.Play();
                    else dustParticles.Stop();
            }
        }

        updateHealthbar();
        
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

    public override void OnRightClick() {
        base.OnLeftClick();

        PlayerController.instance.AttackUnit(this);
    }

    public virtual void MoveToPoint(TileData tile) {
        movement.CalculatePath(tile);
    }

    private bool isInSelectionBox() {
        Vector3 point = CameraController.instance.camera.WorldToScreenPoint(transform.position);
        return HUD.instance.selectionBox.Rect.rect.Contains(point - HUD.instance.selectionBox.transform.position);
    }

    public void Damage(int damage) {
        healthCurrent -= damage;
        if(healthCurrent <= 0) {
            Destroy(gameObject);
        }
    }

    private void updateHealthbar() {
        bool show = healthbar.isOnScreen() && (IsHovered() || IsSelected());
        healthbar.SetWorldPos(transform.position + Vector3.up * 1.0f);
        healthbar.gameObject.SetActive(show);
        healthbar.SetPercentage((float) healthCurrent / healthMax);
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
        if(World.instance) {
            World.instance.units.Remove(this);
        }
        if(healthbar) Destroy(healthbar.gameObject);
    }
}
