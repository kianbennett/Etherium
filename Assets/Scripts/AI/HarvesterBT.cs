using System.Linq;
using UnityEngine;

public class HarvesterBT : BehaviourTree
{
    private HarvesterBB bb;

    public HarvesterBT(HarvesterBB bb)
    {
        this.bb = bb;
    }

    protected override BTNode Build()
    {
        Selector rootSelector = new(bb);

        // Find a resource to harvest
        Selector findResourceSelector = new(bb);
        findResourceSelector.AddChild(new CheckVisibleResources(bb));
        findResourceSelector.AddChild(new CheckDiscoveredResources(bb));
        findResourceSelector.AddChild(new MoveToClosestUndiscoveredTile(bb));

        // Harvest that resource
        Sequence harvestResourceSequence = new(bb);
        harvestResourceSequence.AddChild(new MoveToResource(bb));
        harvestResourceSequence.AddChild(new DelayNode(0.25f, bb));
        harvestResourceSequence.AddChild(new HarvestResource(bb));
        
        // Deposit resource
        Sequence depositResourceSequence = new(bb);
        depositResourceSequence.AddChild(new MoveToBase(bb));
        depositResourceSequence.AddChild(new DelayNode(0.25f, bb));
        depositResourceSequence.AddChild(new DepositResource(bb));

        FindResourceDecorator findResourceRoot = new(findResourceSelector, bb);
        HarvestResourceDecorator harvestResourceRoot = new(harvestResourceSequence, bb);
        DepositResourceDecorator depositResourceRoot = new(depositResourceSequence, bb);

        rootSelector.AddChild(findResourceRoot);
        rootSelector.AddChild(harvestResourceRoot);
        rootSelector.AddChild(depositResourceRoot);

        return rootSelector;
    }
}

// Get the nearest resource from the resources that are visible
public class CheckVisibleResources : BTNode
{
    private HarvesterBB hBB;
    
    public CheckVisibleResources(HarvesterBB bb) : base(bb)
    {
        hBB = bb;
    }

    public override BTStatus Execute()
    {
        BaseController controller = hBB.unit.GetController();

        ResourceType resourceType = controller.ResourceTypeToPrioritise();
        if(hBB.unit.IsAtMaxResource(resourceType))
        {
            resourceType = (ResourceType) (((int) resourceType + 1) % 2);
        }

        ResourceObject nearestResource = controller.DiscoveredResources
            .Where(o => controller.IsWorldObjectVisible(o) && o.HasSpaceToHarvest() && o.Type == resourceType && o.RemainingResourcePercentage > 0.4)
            .OrderBy(o => Vector3.Distance(hBB.unit.transform.position, o.transform.position))
            .FirstOrDefault();

        if(nearestResource)
        {
            hBB.resourceToHarvest = nearestResource;
            hBB.resourceToCheck = null;
            hBB.tileToMoveTo = null;
            return BTStatus.SUCCESS;
        }

        return BTStatus.FAILURE;
    }
}

// If there are discovered resources that haven't been visible for a certain period of time, move to the nearest one
public class CheckDiscoveredResources : BTNode
{
    private HarvesterBB hBB;
    
    public CheckDiscoveredResources(HarvesterBB bb) : base(bb)
    {
        hBB = bb;
    }

    public override BTStatus Execute()
    {
        BaseController controller = hBB.unit.GetController();

        // If the unit has a resource to check and hasn't reached it yet, then the node has succeeded so we 
        // don't continue to the next node in the selector
        if(hBB.resourceToCheck && hBB.unit.Movement.HasPath)
        {
            //if(!controller.IsWorldObjectVisible(hBB.resourceToCheck))
            //{
                return BTStatus.RUNNING;
            //}
        }

        // If we come in range of the resource we've checked it
        if(hBB.resourceToCheck && controller.IsWorldObjectVisible(hBB.resourceToCheck))
        {
            hBB.resourceToCheck = null;
        }

        ResourceType resourceType = controller.ResourceTypeToPrioritise();
        if (hBB.unit.IsAtMaxResource(resourceType))
        {
            resourceType = (ResourceType) (((int) resourceType + 1) % 2);
        }

        ResourceObject nearestResource = controller.DiscoveredResources
            .Where(o => !controller.IsWorldObjectVisible(o) && o.Type == resourceType && controller.TimeSinceResourceWasLastVisible(o) > 20)
            .OrderBy(o => Vector3.Distance(hBB.unit.transform.position, o.transform.position))
            .FirstOrDefault();

        if(nearestResource)
        {
            hBB.resourceToCheck = nearestResource;
            hBB.unit.MoveToPoint(nearestResource.tile);
            return BTStatus.RUNNING;
        }

        return BTStatus.FAILURE;
    }
}

// Moves to a tile next to the resource and look at it - return success when the unit has reached the tile
public class MoveToResource : BTNode
{
    private HarvesterBB hBB;
    
    public MoveToResource(HarvesterBB bb) : base(bb)
    {
        hBB = bb;
    }

    public override BTStatus Execute()
    {
        if(hBB.resourceToHarvest)
        {
            if(hBB.tileToMoveTo != null)
            {
                if(hBB.unit.Movement.HasReachedDestination)
                {
                    hBB.unit.Movement.LookAtTile(hBB.resourceToHarvest.tile);
                    hBB.tileToMoveTo = null;
                    // Once we've reached the destination then we can continue to the next node in the sequence
                    return BTStatus.SUCCESS;
                }
            }
            else
            {
                hBB.tileToMoveTo = hBB.resourceToHarvest.GetNearestAdjacentTile(hBB.unit.transform.position, hBB.unit.Movement.IsFlying);
                hBB.unit.MoveToPoint(hBB.tileToMoveTo);
            }
        }

        return BTStatus.FAILURE;
    }
}

// Deplete the resource over time and add it to the unit's stores
public class HarvestResource : BTNode
{
    private HarvesterBB hBB;
    
    public HarvestResource(HarvesterBB bb) : base(bb)
    {
        hBB = bb;
    }

    public override BTStatus Execute()
    {
        bool hasCompletedHarvesting = hBB.resourceToHarvest.RemainingResource <= 0 || hBB.unit.IsAtMaxResource(hBB.resourceToHarvest.Type);

        if(!hasCompletedHarvesting)
        {
            if(!hBB.unit.IsHarvesting) hBB.unit.StartHarvesting();

            return BTStatus.FAILURE;
        }

        hBB.unit.CancelHarvesting();
        return BTStatus.SUCCESS;
    }
}

// Moves to a tile next to the resource and look at it - return success when the unit has reached the tile
public class MoveToBase : BTNode
{
    private HarvesterBB hBB;
    
    public MoveToBase(HarvesterBB bb) : base(bb)
    {
        hBB = bb;
    }

    public override BTStatus Execute()
    {
        BaseController controller = hBB.unit.GetController();

        if(hBB.tileToMoveTo != null)
        {
            if(hBB.unit.Movement.HasReachedDestination)
            {
                hBB.unit.Movement.LookAtTile(controller.BaseStructure.tile);
                hBB.tileToMoveTo = null;
                // Once we've reached the destination then we can continue to the next node in the sequence
                return BTStatus.SUCCESS;
            }
        }
        else
        {
            hBB.tileToMoveTo = controller.BaseStructure.GetNearestAdjacentTile(hBB.unit.transform.position, hBB.unit.Movement.IsFlying);
            hBB.unit.MoveToPoint(hBB.tileToMoveTo);
        }

        return BTStatus.FAILURE;
    }
}

// Empty the unit's resources and add them to the player's store
public class DepositResource : BTNode
{
    private HarvesterBB hBB;
    
    public DepositResource(HarvesterBB bb) : base(bb)
    {
        hBB = bb;
    }

    public override BTStatus Execute()
    {
        hBB.unit.GetController().AddGems(hBB.unit.GetResource(ResourceType.Gem));
        hBB.unit.GetController().AddMinerals(hBB.unit.GetResource(ResourceType.Mineral));
        hBB.unit.EmptyResources();

        return BTStatus.SUCCESS;
    }
}

// If the unit has capacity and DOESN'T have a resource to harvest
public class FindResourceDecorator : DecoratorNode
{
    private HarvesterBB hBB;
    
    public FindResourceDecorator(BTNode wrappedNode, HarvesterBB bb) : base(wrappedNode, bb)
    {
        hBB = bb;
    }

    public override BTStatus Execute()
    {
        if(!hBB.resourceToHarvest && hBB.unit.HasCapacityForAnyResource())
        {
            return wrappedNode.Execute();
        }

        return BTStatus.FAILURE;
    }
}

// If the unit has capacity and DOES have a resource to harvest
public class HarvestResourceDecorator : DecoratorNode
{
    private HarvesterBB hBB;

    public HarvestResourceDecorator(BTNode wrappedNode, HarvesterBB bb) : base(wrappedNode, bb)
    {
        hBB = bb;
    }

    public override BTStatus Execute()
    {
        if(hBB.resourceToHarvest != null && hBB.unit.HasCapacityForAnyResource())
        {
            return wrappedNode.Execute();
        }

        return BTStatus.FAILURE;
    }
}

// If the unit has no more capacity
public class DepositResourceDecorator : DecoratorNode
{
    private HarvesterBB hBB;
    
    public DepositResourceDecorator(BTNode wrappedNode, HarvesterBB bb) : base(wrappedNode, bb)
    {
        hBB = bb;
    }

    public override BTStatus Execute()
    {
        if(!hBB.unit.HasCapacityForAnyResource())
        {
            return wrappedNode.Execute();
        }

        return BTStatus.FAILURE;
    }
}