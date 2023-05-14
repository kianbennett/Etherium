using System.Collections.Generic;
using UnityEngine;

public class UnitHarvester : Unit
{
    private Dictionary<ResourceType, Healthbar> resourceBars;
    private Dictionary<ResourceType, int> resourceAmounts;
    private Dictionary<ResourceType, int> resourceCapacities;

    // Each frame the unit is harvesting harvestAmount increases
    // When this reaches harvestAmountPerResource resources are gained
    private bool isHarvesting;
    private float harvestDuration;

    private const float harvestDurationForResource = 0.1f;
    private const float harvestSpeed = 0.05f;
    private const int harvestResourceAmount = 5;

    private HarvesterBB bb;
    private HarvesterBT bt;

    public bool IsHarvesting { get { return isHarvesting; } }

    public override void Init(int ownerId)
    {
        base.Init(ownerId);

        resourceBars = new() 
        {
            { ResourceType.Mineral, HUD.instance.CreateResourceBar(ResourceType.Mineral) },
            { ResourceType.Gem, HUD.instance.CreateResourceBar(ResourceType.Gem) }
        };
        resourceAmounts = new()
        {
            { ResourceType.Mineral, 0 },
            { ResourceType.Gem, 0 }
        };
        resourceCapacities = new()
        {
            { ResourceType.Mineral, 100 },
            { ResourceType.Gem, 25 }
        };

        bb = new HarvesterBB(this);
        bt = new HarvesterBT(bb);
        bt.Start(this);
    }

    protected override void Update()
    {
        base.Update();

        if(isHarvesting)
        {
            harvestDuration += Time.deltaTime * harvestSpeed;
            if (harvestDuration >= harvestDurationForResource)
            {
                harvestDuration = 0;
                int resourceAmount = bb.resourceToHarvest.Harvest(harvestResourceAmount);

                resourceAmounts[bb.resourceToHarvest.Type] += resourceAmount;
            }
        }

        // if (HasResourceToHarvest())
        // {
        //     float distToResource = Vector2Int.Distance(tile.pos, bb.resourceToHarvest.tile.pos);

        //     if (distToResource <= 1 && Movement.HasReachedDestination && bb.resourceToHarvest.RemainingResource > 0 && !IsAtMaxResource(bb.resourceToHarvest.Type))
        //     {
        //         isHarvesting = true;

        //         Movement.LookAtTile(bb.resourceToHarvest.tile);

        //         bb.resourceToHarvest.StartUnitHarvest(this);

        //         harvestDuration += Time.deltaTime * harvestSpeed;
        //         if (harvestDuration >= harvestDurationForResource)
        //         {
        //             harvestDuration = 0;
        //             int resourceAmount = bb.resourceToHarvest.Harvest(this, harvestResourceAmount);

        //             resourceAmounts[bb.resourceToHarvest.Type] += resourceAmount;
        //         }   
        //     }
        //     else if (isHarvesting)
        //     {
        //         cancelHarvesting();
        //     }
        // }

        if(modelAnimator)
        {
            modelAnimator.SetBool("IsHarvesting", isHarvesting);
        }

        updateResourceBar(ResourceType.Mineral);
        updateResourceBar(ResourceType.Gem);
    }

    public void HarvestResource(ResourceObject resource)
    {
        if (resource == bb.resourceToHarvest || IsAtMaxResource(resource.Type)) return;

        TileData tile = resource.GetNearestAdjacentTile(transform.position, Movement.IsFlying);

        if(tile != null)
        {
            CancelHarvesting();
            MoveToPoint(tile);
            bb.resourceToHarvest = resource;
        }
    }

    public override void MoveToPoint(TileData tile)
    {
        base.MoveToPoint(tile);
        //CancelHarvesting();
    }

    public void StartHarvesting()
    {
        if(bb.resourceToHarvest == null)
        {
            Debug.Log("Trying to start harvesting without a resource!");
            return;
        }

        bb.resourceToHarvest.StartUnitHarvest(this);
        isHarvesting = true;
    }

    public void CancelHarvesting()
    {
        bb.resourceToHarvest?.CancelUnitHarvest(this);
        bb.resourceToHarvest = null;
        isHarvesting = false;
    }

    private void updateResourceBar(ResourceType type)
    {
        Healthbar resourceBar = resourceBars[type];
        bool show = resourceBar.isOnScreen() && (isHovered || isSelected) && isVisible;
        resourceBar.gameObject.SetActive(show);
        resourceBar.SetWorldPos(transform.position + Vector3.up * (1.15f + 0.15f * (int) type));
        resourceBar.SetPercentage(GetResourcePercentage(type));
    }

    public bool HasResourceToHarvest()
    {
        return bb.resourceToHarvest != null;
    }

    public int GetResource(ResourceType type)
    {
        return resourceAmounts[type];
    }

    public float GetResourcePercentage(ResourceType type)
    {
        return (float) resourceAmounts[type] / resourceCapacities[type];
    }

    public bool IsAtMaxResource(ResourceType type)
    {
        return resourceAmounts[type] >= resourceCapacities[type];
    }

    public bool HasCapacityForAnyResource()
    {
        return !IsAtMaxResource(ResourceType.Gem) || !IsAtMaxResource(ResourceType.Mineral);
    }

    public void EmptyResources()
    {
        resourceAmounts[ResourceType.Gem] = 0;
        resourceAmounts[ResourceType.Mineral] = 0;
    }
}