using System.Linq;
using UnityEngine;

public class ScoutBT : BehaviourTree
{
    private ScoutBB bb;

    public ScoutBT(ScoutBB bb)
    {
        this.bb = bb;
    }

    protected override BTNode Build()
    {
        Selector rootSelector = new(bb);

        Flee flee = new(bb);
        FleeDecorator fleeDecorator = new(flee, bb);

        Sequence patrolSequence = new(bb);
        patrolSequence.AddChild(new MoveToClosestUndiscoveredTile(bb));
        patrolSequence.AddChild(new WaitUntilFinishedMoving(bb));

        rootSelector.AddChild(fleeDecorator);
        rootSelector.AddChild(patrolSequence);

        return rootSelector;
    }
}