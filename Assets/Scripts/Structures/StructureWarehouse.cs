public class StructureWarehouse : Structure
{
    public override void Init(int ownerId)
    {
        base.Init(ownerId);
        if (IsPlayerOwned)
        {
            PlayerController.instance.warehouses++;
        }
        else
        {
            EnemyController.instance.warehouses++;
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (!GameManager.IsQuitting)
        {
            if (IsPlayerOwned)
            {
                PlayerController.instance.warehouses--;
                // Use these to clamp the values to the new max
                PlayerController.instance.AddGems(0);
                PlayerController.instance.AddMinerals(0);
            }
            else
            {
                EnemyController.instance.AddGems(0);
                EnemyController.instance.AddMinerals(0);
            }
        }
    }
}
