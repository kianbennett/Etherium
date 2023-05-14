using System.Collections.Generic;
using UnityEngine;

public abstract class ControllerAction
{
    protected BaseController controller;

    public ControllerAction(BaseController controller)
    {
        this.controller = controller;
    }

    protected abstract float EvaluateUtility(); // Range of 0-1

    public abstract void Execute();
}

public class ActionBuildWarehouse : ControllerAction
{
    public ActionBuildWarehouse(BaseController controller) : base(controller)
    {
    }

    public override void Execute()
    {
        throw new System.NotImplementedException();
    }

    protected override float EvaluateUtility()
    {
        return controller.OwnedBuilderUnits.Length > 0 ? 1 : 0 *
               controller.minerals >= World.instance.structureWarehousePrefab.BuildCost ? 1 : 0 *
               controller.GemsPercentage *
               controller.MineralsPercentage;
    }
}