using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldObject : MonoBehaviour {

    public Outline outline;
    public bool canBeMultiSelected;
    public bool scaleOnHover;
    public Color outlineColourHovered, outlineColourSelected;
    public TileData tile;
    public GameObject model;
    public new Collider collider;
    public Renderer[] playerColourRenderers;
    [ReadOnly] public bool isVisible;
    [ReadOnly] public int ownerId = -1; // -1 no owner; 0 player; 1 enemy

    protected bool isHovered, isSelected;
    private float doubleLeftClickTick;
    private const float doubleLeftClickThreshold = 0.5f;
    private const float increasedScale = 1.1f; // Possibly expose this in inspector

    protected virtual void Awake() {
        SetVisible(!World.instance.fogOfWar.fogOfWarEnabled);
    }

    // This needs to go in Start since Awake is called before ownerId is set
    protected virtual void Start() {
        if(ownerId == 1) {
            foreach(Renderer renderer in playerColourRenderers) {
                renderer.material = PlayerController.instance.enemyColourMaterial;
            }
            EnemyController.instance.ownedObjects.Add(this);
        }
    }

    protected virtual void Update() {
        if(doubleLeftClickTick < doubleLeftClickThreshold * 2) doubleLeftClickTick += Time.deltaTime;

        bool scale = (scaleOnHover && isHovered);
        transform.localScale = Vector3.one * Mathf.Lerp(transform.localScale.x, scale ? increasedScale : 1, Time.deltaTime * 15);

        if(outline) {
            outline.enabled = isHovered || isSelected;
            if(outline.enabled) {
                outline.OutlineColor = isSelected ? outlineColourSelected : outlineColourHovered;
            }
        }
    }

    protected virtual void OnMouseEnter() {
        if(!HUD.instance.IsMouseOverHUD()) SetHovered(true);
    }

    protected virtual void OnMouseExit() {
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

    public virtual void SetVisible(bool visible) {
        isVisible = visible;
        if(model) model.SetActive(visible);
        if(collider) collider.enabled = visible;
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

    protected virtual void OnDestroy() {
        if(ownerId == 1 && !GameManager.quitting) {
            EnemyController.instance.ownedObjects.Remove(this);
        }
    }
}
