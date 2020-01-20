using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldObject : MonoBehaviour {

    public Outline outline;
    public bool canBeMultiSelected;
    public bool scaleOnHover;
    public Color outlineColourHovered, outlineColourSelected;
    public TileData tile;

    protected bool isHovered, isSelected;
    private float doubleLeftClickTick;
    private const float doubleLeftClickThreshold = 0.5f;
    private const float increasedScale = 1.1f; // Possibly expose this in inspector

    protected virtual void Awake() {
    }

    protected virtual void Update() {
        if(doubleLeftClickTick < doubleLeftClickThreshold * 2) doubleLeftClickTick += Time.deltaTime;

        bool scale = (scaleOnHover && isHovered);
        transform.localScale = Vector3.one * Mathf.Lerp(transform.localScale.x, scale ? increasedScale : 1, Time.deltaTime * 15);

        outline.enabled = isHovered || isSelected;
        if(outline.enabled) {
            outline.OutlineColor = isSelected ? outlineColourSelected : outlineColourHovered;
        }
    }

    void OnMouseEnter() {
        SetHovered(true);
    }

    void OnMouseExit() {
        SetHovered(false);
    }

    public virtual void SetSelected(bool selected) {
        isSelected = selected;
    }

    public virtual void SetHovered(bool hovered) {
        isHovered = hovered;
        PlayerController.instance.objectHovered = hovered ? this : null;
    }

    public bool IsSelected() {
        return isSelected;
    }

    public bool IsHovered() {
        return isHovered;
    }

    public virtual void OnLeftClick() {
        if (doubleLeftClickTick < doubleLeftClickThreshold) OnDoubleLeftClick();
        doubleLeftClickTick = 0;
    }

    public virtual void OnDoubleLeftClick() {
        // TODO: Centre camera on object (and follow)
    }

    public virtual void OnRightClick() {
    }
}
