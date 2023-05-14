using System.Linq;
using UnityEngine;

public class Unit : WorldObject 
{
    [SerializeField] protected Animator modelAnimator;
    [SerializeField] private LineRenderer pathLineRenderer;
    [SerializeField] private Transform pathLineTriangle;
    [SerializeField] private UnitActionHandler actionHandler;
    [SerializeField] private UnitMovement movement;
    [SerializeField] private Transform[] wheels;
    [SerializeField] private ParticleSystem dustParticles;

    [SerializeField] private float healthMax;
    [SerializeField] private int buildTime;
    [SerializeField] private int buildCost;

    public int BuildTime { get { return buildTime; } }
    public int BuildCost { get { return buildCost; } }
    public UnitMovement Movement { get { return movement; } }
    public float HealthPercentage { get { return healthCurrent / healthMax; } }
    public UnitActionHandler ActionHandler { get { return actionHandler; } }

    public Quaternion ModelRotation 
    { 
        get { return model.transform.rotation; } 
        set { model.transform.rotation = value; } 
    }

    private Healthbar healthbar;
    private float healthCurrent;

    private float animMoveSpeed;

    protected override void Awake() 
    {
        base.Awake();

        healthbar = HUD.instance.CreateHealthbar();
        healthCurrent = healthMax;

        // healthbar.rect.sizeDelta = new Vector2(Mathf.Clamp(healthMax * 8, 0, 80), healthbar.rect.sizeDelta.y);
    }

    public override void Init(int ownerId)
    {
        base.Init(ownerId);

        if (IsPlayerOwned)
        {
            HUD.instance.CreateUnitIcon(this);
        }
    }

    protected override void Update () 
    {
        base.Update();

        // Set selected based on whether the unit is inside the HUD selection box
        if(HUD.hasSelectionBox && IsPlayerOwned) 
        {
            if (!isSelected && isInSelectionBox()) 
            {
                PlayerController.instance.SelectObject(this);
            } 
            else if(isSelected && !isInSelectionBox()) 
            {
                PlayerController.instance.DeselectObject(this);
            }
        }

        if(dustParticles != null) 
        {
            bool isMoving = Movement.HasPath;
            if(isMoving != dustParticles.isPlaying) 
            {
                if(isMoving) 
                {
                    dustParticles.Play();
                }
                else
                {
                    dustParticles.Stop();
                }
            }
        }

        foreach (Transform wheel in wheels)
        {
            if (Movement.HasPath)
            {
                wheel.Rotate(250 * Time.deltaTime * Vector3.forward);
            }
            //else
            //{
            //    wheel.localRotation = Quaternion.Euler(Vector3.forward * Mathf.MoveTowardsAngle(wheel.localRotation.eulerAngles.z, 0, Time.deltaTime * 250));
            //}
        }

        updateHealthbar();

        if (modelAnimator)
        {
            animMoveSpeed = Mathf.Lerp(animMoveSpeed, Movement.HasPath ? 1 : 0, Time.deltaTime * 10);
            modelAnimator.SetFloat("MoveSpeed", animMoveSpeed);
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

        pathLineRenderer.gameObject.SetActive(movement.HasPath);
        // pathLineTriangle.gameObject.SetActive(movement.HasPath);
        if (movement.HasPath)
        {
            Vector3[] lineRendererPositions = movement.RemainingPathNodes.Select(o => o.worldPos + Vector3.up * 0.01f).ToArray();

            // pathLineTriangle.position = lineRendererPositions.Last();
            // if(lineRendererPositions.Length > 1)
            // {
            //     float angle = Vector3.Angle(lineRendererPositions.Last() - lineRendererPositions[^2], Vector3.right) - 90;
            //     pathLineTriangle.rotation = Quaternion.Euler(Vector3.up * angle);
            // }

            lineRendererPositions = lineRendererPositions.Prepend(transform.position + Vector3.up * 0.01f).ToArray();
            pathLineRenderer.positionCount = lineRendererPositions.Length;
            pathLineRenderer.SetPositions(lineRendererPositions);
        }
    }

    public override void OnLeftClick() 
    {
        base.OnLeftClick();

        if(IsPlayerOwned) 
        {
            PlayerController.instance.SelectObject(this);
        }
    }

    public override void OnRightClick() 
    {
        base.OnRightClick();

        if(IsPlayerOwned) 
        {
            PlayerController.instance.AttackObject(this);
        }
    }

    public virtual void MoveToPoint(TileData tile) 
    {
        movement.CalculatePath(tile);
    }

    private bool isInSelectionBox() 
    {
        UISelectionBox selectionBox = HUD.instance.selectionBox;
        Vector3 point = CameraController.instance.camera.WorldToScreenPoint(transform.position);
        return selectionBox.Rect.rect.Contains(point - selectionBox.transform.position);
    }

    public void Damage(int damage) 
    {
        healthCurrent -= damage;
        if(healthCurrent <= 0) 
        {
            Destroy(gameObject);
        }
    }

    private void updateHealthbar() 
    {
        bool show = healthbar.isOnScreen() && (isHovered || isSelected) && isVisible;
        healthbar.gameObject.SetActive(show);
        healthbar.SetWorldPos(transform.position + Vector3.up * 1.0f);
        healthbar.SetPercentage((float) healthCurrent / healthMax);
    }

    public int GetRepairCost() 
    {
        return (int) ((healthMax - healthCurrent) * 2.0f);
    }

    public void Repair() 
    {
        healthCurrent = healthMax;
    }

    protected override void OnDestroy() 
    {
        base.OnDestroy();

        if(!GameManager.IsQuitting) 
        {
            if (PlayerController.instance.selectedObjects.Contains(this)) 
            {
                PlayerController.instance.selectedObjects.Remove(this);
            }
            if (PlayerController.instance.objectHovered == this) 
            {
                PlayerController.instance.objectHovered = null;
            }
            World.instance.units.Remove(this);
            World.instance.fogOfWar.UpdateFogOfWar();
        }

        if(healthbar) Destroy(healthbar.gameObject);
    }
}
