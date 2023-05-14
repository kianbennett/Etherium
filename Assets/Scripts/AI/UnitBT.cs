using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Pick a random undisovered tile and move to it
public class MoveToClosestUndiscoveredTile : BTNode
{
    private new readonly UnitBB bb;

    public MoveToClosestUndiscoveredTile(UnitBB bb) : base(bb)
    {
        this.bb = bb;
    }

    public override BTStatus Execute()
    {
        if (bb.tileToMoveTo != null)
        {
            if (!bb.unit.Movement.HasReachedDestination)
            {
                return BTStatus.RUNNING;
            }
            else
            {
                bb.tileToMoveTo = null;
            }
        }

        BaseController controller = bb.unit.GetController();

        TileObject[] undiscoveredTiles = World.instance.allTiles
            .Where(o => !controller.HasDiscoveredTile(o.tileData))
            .OrderBy(o => Vector3.Distance(o.transform.position, bb.unit.transform.position))
            .ToArray();

        if (undiscoveredTiles.Length > 0)
        {
            bb.tileToMoveTo = undiscoveredTiles.First().tileData;
            bb.unit.MoveToPoint(bb.tileToMoveTo);
            return BTStatus.RUNNING;
        }

        return BTStatus.FAILURE;
    }
}

// Returns true when the unit has reached it's destination and clear tileToMoveTo
public class WaitUntilFinishedMoving : BTNode
{
    private new readonly UnitBB bb;

    public WaitUntilFinishedMoving(UnitBB bb) : base(bb)
    {
        this.bb = bb;
    }

    public override BTStatus Execute()
    {
        if (bb.tileToMoveTo != null && bb.unit.Movement.HasReachedDestination)
        {
            bb.tileToMoveTo = null;
            return BTStatus.SUCCESS;
        }

        return BTStatus.FAILURE;
    }
}

public class Flee : BTNode
{
    private new readonly UnitBB bb;

    public Flee(UnitBB bb) : base(bb)
    {
        this.bb = bb;
    }

    public override BTStatus Execute()
    {
        BaseController controller = bb.unit.GetController();
        BaseController opponent = controller.OpponentController;

        Unit[] opponentUnitsInRange = opponent.OwnedFighterUnits
            .Where(o => o.InRangeOfOther(bb.unit))
            .ToArray();

        StructureDefenceTower[] opponentTowersInRange = opponent.OwnedStructures
            .Where(o => o is StructureDefenceTower && o.InRangeOfOther(bb.unit))
            .Select(o => o as StructureDefenceTower)
            .ToArray();

        List<WorldObject> allOpponentObjects = new();
        allOpponentObjects.AddRange(opponentUnitsInRange);
        allOpponentObjects.AddRange(opponentUnitsInRange);

        WorldObject closestObject = allOpponentObjects.OrderBy(o => Vector3.Distance(bb.unit.transform.position, o.transform.position)).FirstOrDefault();

        if(closestObject)
        {
            Vector2Int direction = closestObject.tile.pos - bb.unit.tile.pos;
            Vector2Int tileCoords = bb.unit.tile.pos - direction;
            TileData tileData = World.instance.GetTileDatatAt(tileCoords.x, tileCoords.y);

            if (tileData != null)
            {
                bb.tileToMoveTo = tileData;
                bb.unit.MoveToPoint(tileData);
                return BTStatus.SUCCESS;
            }
        }

        return BTStatus.FAILURE;
    }
}

public class FleeDecorator : DecoratorNode
{
    private new readonly UnitBB bb;

    public FleeDecorator(BTNode wrappedNode, UnitBB bb) : base(wrappedNode, bb)
    {
        this.bb = bb;
    }

    public override BTStatus Execute()
    {
        BaseController controller = bb.unit.GetController();
        BaseController opponent = controller.OpponentController;

        Unit[] opponentUnitsInRange = opponent.OwnedFighterUnits
            .Where(o => o.InRangeOfOther(bb.unit))
            .ToArray();

        StructureDefenceTower[] opponentTowersInRange = opponent.OwnedStructures
            .Where(o => o is StructureDefenceTower && o.InRangeOfOther(bb.unit))
            .Select(o => o as StructureDefenceTower)
            .ToArray();

        if(opponentUnitsInRange.Length > 0 || opponentTowersInRange.Length > 0)
        {
            return wrappedNode.Execute();
        }

        return BTStatus.FAILURE;
    }
}