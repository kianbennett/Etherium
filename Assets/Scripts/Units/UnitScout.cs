public class UnitScout : Unit
{
    private ScoutBB bb;
    private ScoutBT bt;

    public override void Init(int ownerId)
    {
        base.Init(ownerId);

        bb = new ScoutBB(this);
        bt = new ScoutBT(bb);
        bt.Start(this);
    }
}
