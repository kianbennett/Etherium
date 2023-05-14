using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum ResourceType { Gem, Mineral }

public class ResourceObject : WorldObject
{
    public ResourceType Type { get { return type; } }
    public float RemainingResource { get { return currentResourceAmount; } }
    public float RemainingResourcePercentage { get { return currentResourceAmount / resourceCapacity; } }

    [SerializeField] private GameObject[] models; // First model is empty, last is full
    [SerializeField] private ResourceType type;
    [SerializeField] private int resourceCapacity;
    [SerializeField] private float resourceBarOffset;
    [SerializeField] private float resourceRegenDelay;
    [SerializeField] private float resourceRegenRate;

    [SerializeField, ReadOnly] private float lastHarvestTime;

    private float currentResourceAmount;
    private float shakeTick;
    private const float shakeInterval = 0.05f, shakeDist = 0.05f;
    private Vector3 shakeOffset;
    private Healthbar resourceBar;

    private List<UnitHarvester> unitsHarvesting;

    protected override void Awake()
    {
        base.Awake();

        currentResourceAmount = resourceCapacity;
        lastHarvestTime = float.MaxValue;
        resourceBar = HUD.instance.CreateResourceBar(type);
        unitsHarvesting = new();
    }

    protected override void Update()
    {
        base.Update();

        // Regen resource when not being harvested
        if (currentResourceAmount < resourceCapacity && lastHarvestTime > resourceRegenDelay)
        {
            setCurrentResourceAmount(currentResourceAmount + Time.deltaTime * resourceRegenRate);
        }
        lastHarvestTime += Time.deltaTime;

        updateShake();
        updateResourceBar();
    }

    public override void OnRightClick()
    {
        base.OnRightClick();

        PlayerController.instance.HarvestResource(this);
    }

    public int Harvest(int amount)
    {   
        float resourceAmountBeforeHarvest = currentResourceAmount;
        setCurrentResourceAmount(currentResourceAmount - amount);
        lastHarvestTime = 0;

        return (int) (resourceAmountBeforeHarvest - currentResourceAmount);
    }

    public void StartUnitHarvest(UnitHarvester unit)
    {
        if(!unitsHarvesting.Contains(unit))
        {
            unitsHarvesting.Add(unit);    
        }
    }

    public void CancelUnitHarvest(UnitHarvester unit)
    {
        unitsHarvesting.Remove(unit);
    }

    private void setCurrentResourceAmount(float amount)
    {
        // Stop the resource amount from going below zero or beyond the capacity
        amount = Mathf.Clamp(amount, 0.0f, resourceCapacity);

        currentResourceAmount = amount;
        if (!isVisible) return;

        updateModels(RemainingResourcePercentage);
    }

    private void updateShake()
    {
        if (unitsHarvesting.Count > 0)
        {
            shakeTick += Time.deltaTime;
            if (shakeTick > shakeInterval)
            {
                shakeTick = 0;
                shakeOffset.x = -shakeDist / 2 + Random.Range(0, shakeDist) * unitsHarvesting.Count;
                shakeOffset.y = -shakeDist / 2 + Random.Range(0, shakeDist) * unitsHarvesting.Count;
                shakeOffset.z = -shakeDist / 2 + Random.Range(0, shakeDist) * unitsHarvesting.Count;
            }
        }
        else
        {
            shakeOffset = Vector3.zero;
        }
        foreach (GameObject model in models)
        {
            model.transform.localPosition = shakeOffset;
        }
    }

    private void updateResourceBar()
    {
        bool show = resourceBar.isOnScreen() && (isHovered || unitsHarvesting.Count > 0) && isVisible;
        resourceBar.gameObject.SetActive(show);
        resourceBar.SetWorldPos(transform.position + Vector3.up * resourceBarOffset);
        resourceBar.SetPercentage(currentResourceAmount / resourceCapacity);
        resourceBar.SetOffset(shakeOffset);
    }

    protected override void OnMouseEnter()
    {
        base.OnMouseEnter();
        // If the player has a harvester unit selected and it at max resource, show a tooltip

        // PROBABLY DONT NEED THIS ANYMORE
        
        UnitHarvester[] selectedUnits = PlayerController.instance.selectedObjects.Where(o => o is UnitHarvester).Select(o => (UnitHarvester)o).ToArray();
        if (PlayerController.instance.IsAtMaxResource(type) && selectedUnits.Length > 0)
        {
            HUD.instance.ShowTooltip("At max capacity");
        }
    }

    protected override void OnMouseExit()
    {
        base.OnMouseExit();
        HUD.instance.HideTooltip();
    }

    public override void SetVisible(bool visible)
    {
        base.SetVisible(visible);

        foreach (GameObject model in models)
        {
            model.SetActive(isDiscovered);
        }

        if (visible)
        {
            updateModels(RemainingResourcePercentage);
        }
        else if(isDiscovered)
        {
            updateModels(0);
        }
    }

    public bool HasSpaceToHarvest()
    {
        return tile.connections.Where(o => o.occupiedUnit == null && o.occupiedObject == null && o.IsTileAccessible(false)).Count() > 0;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (resourceBar) Destroy(resourceBar.gameObject);
    }

    // Set model in array to active depending on the amount of resources it has left
    private void updateModels(float resourcePercentage)
    {
        float a = resourcePercentage * (models.Length - 1);
        for (int i = 0; i < models.Length; i++)
        {
            models[i].SetActive(a <= i && a > i - 1);
        }
    }
}
