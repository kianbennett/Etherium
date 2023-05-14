using System.Linq;
using UnityEngine;

public class WorldObject : MonoBehaviour 
{
    [SerializeField] private Outline outline;
    [SerializeField] private bool canBeMultiSelected;
    [SerializeField] private bool scaleOnHover;
    [SerializeField] private Color outlineColourHovered, outlineColourSelected;
    [SerializeField] protected GameObject model;
    [SerializeField] private new Collider collider;
    [SerializeField] private Renderer[] playerColourRenderers;

    [Header("Object Darken")]
    [SerializeField] private Renderer[] renderersToDarken;
    [SerializeField] private Material materialDefault, materialDark;
    [SerializeField] private bool discoveredObjectAlwaysVisible;
    
    [HideInInspector] public TileData tile;

    protected bool isHovered, isSelected;
    protected bool isVisible, isDiscovered;
    private int ownerId = -1; // -1 no owner; 0 player; 1 enemy

    private float doubleLeftClickTick;
    private const float doubleLeftClickThreshold = 0.5f;
    private const float increasedScale = 1.1f; // Possibly expose this in inspector

    private Renderer[] allRenderers;

    public int OwnerId { get { return ownerId; } }
    public bool IsOwned { get { return IsPlayerOwned || IsEnemyOwned; } }
    public bool IsPlayerOwned { get { return ownerId == 0; } }
    public bool IsEnemyOwned { get { return ownerId == 1; } }
    public bool IsSelected { get { return isSelected; } }
    public bool IsVisible { get { return isVisible; } }
    public GameObject Model { get { return model; } }

    protected virtual void Awake() 
    {
        allRenderers = GetComponentsInChildren<Renderer>(true);
        //SetVisible(!World.instance.fogOfWar.FogOfWarEnabled);
    }

    protected virtual void Update() 
    {
        if(doubleLeftClickTick < doubleLeftClickThreshold * 2) doubleLeftClickTick += Time.deltaTime;

        bool scale = (scaleOnHover && isHovered);
        transform.localScale = Vector3.one * Mathf.Lerp(transform.localScale.x, scale ? increasedScale : 1, Time.deltaTime * 15);

        if(outline) 
        {
            outline.enabled = isHovered || isSelected;
            if(outline.enabled) 
            {
                outline.OutlineColor = isSelected ? outlineColourSelected : outlineColourHovered;
            }
        }
    }

    public virtual void Init(int ownerId)
    {
        this.ownerId = ownerId;

        SetVisible(IsPlayerOwned || !World.instance.fogOfWar.FogOfWarEnabled);

        BaseController owner = GetController();
        if(owner)
        {
            owner.RegisterWorldObject(this);

            foreach (Renderer renderer in playerColourRenderers)
            {
                renderer.material = owner.ObjectMaterial;
            }
        }
    }

    protected virtual void OnMouseEnter() 
    {
        if(!HUD.IsMouseOverHUD()) SetHovered(true);
    }

    protected virtual void OnMouseExit() 
    {
        SetHovered(false);
    }

    public virtual void SetSelected(bool selected) 
    {
        isSelected = selected;
    }

    public virtual void SetHovered(bool hovered) 
    {
        isHovered = hovered;
        PlayerController.instance.objectHovered = hovered ? this : null;
    }

    public virtual void SetVisible(bool visible) 
    {
        isVisible = visible;
        if (model) model.SetActive(visible || (isDiscovered && discoveredObjectAlwaysVisible));
        if (collider) collider.enabled = visible;

        if(isDiscovered)
        {
            foreach (Renderer renderer in allRenderers)
            {
                renderer.shadowCastingMode = visible ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.Off;
            }

            foreach (Renderer renderer in renderersToDarken)
            {
                renderer.material = visible ? materialDefault : materialDark;
            }
        }
    }
    
    public virtual void SetDiscovered(bool discovered)
    {
        isDiscovered = discovered;
    }

    public void SetAsPlayerOwned()
    {
        ownerId = 0;
    }

    public void SetAsEnemyOwned()
    {
        ownerId = 1;
    }

    public virtual void OnLeftClick() 
    {
        if (doubleLeftClickTick < doubleLeftClickThreshold) 
        {
            OnDoubleLeftClick();
        }
        doubleLeftClickTick = 0;
    }

    public virtual void OnDoubleLeftClick() 
    {
        // TODO: Centre camera on object (and follow)
    }

    public virtual void OnRightClick() 
    {
    }

    protected virtual void OnDestroy() 
    {
        if(!GameManager.IsQuitting && IsOwned) 
        {
            GetController().UnregisterWorldObject(this);
        }
    }

    public BaseController GetController()
    {
        switch(ownerId)
        {
            case 0:
                return PlayerController.instance;
            case 1:
                return EnemyController.instance;
        }
        return null;
    }

    public Color GetOutlineColor()
    {
        return isSelected ? outlineColourSelected : outlineColourHovered;
    }

    public TileData GetNearestAdjacentTile(Vector3 origin, bool includeEmptyTiles)
    {
        return tile.connections
            .Where(o => (o.occupiedUnit == null || o.occupiedUnit == this) && o.IsTileAccessible(includeEmptyTiles))
            .OrderBy(o => Vector3.Distance(o.worldPos, origin))
            .FirstOrDefault();
    }

    public bool InRangeOfOther(WorldObject other)
    {
        float distToOwned = Vector3.Distance(transform.position, other.transform.position);
        return distToOwned <= World.instance.fogOfWar.Radius + 0.5f;
    }
}
