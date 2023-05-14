using System.Collections;
using System.Linq;
using UnityEngine;

public class UnitStateHarvest : UnitState
{
    public UnitStateHarvest(UnitHarvester unitHarvester) : base(unitHarvester)
    {
    }

    protected override IEnumerator stateIEnum()
    {
        UnitHarvester unitHarvester = unit as UnitHarvester;
        ResourceObject closestFreeResourceObject = null;

        while(closestFreeResourceObject == null)
        {
            closestFreeResourceObject = World.instance.resourceObjects
                .Where(o => o.IsVisible && o.HasSpaceToHarvest() && o.RemainingResourcePercentage > 0.5f)
                .OrderBy(o => Vector3.Distance(unit.transform.position, o.transform.position))
                .FirstOrDefault();

            yield return new WaitForSeconds(0.5f);
        }

        unitHarvester.HarvestResource(closestFreeResourceObject);

        yield return null;
    }
}