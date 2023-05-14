public class HarvesterBB : UnitBB
{
    public new UnitHarvester unit;

    public ResourceObject resourceToHarvest;
    public ResourceObject resourceToCheck;

    public HarvesterBB(UnitHarvester unit) : base(unit)
    {
        this.unit = unit;
    }
}
