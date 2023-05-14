using System.Collections.Generic;
using UnityEngine;

public class FogOfWar : MonoBehaviour
{
    [SerializeField] private bool fogOfWarEnabled;
    [SerializeField] private int radius = 5;

    private List<TileData> litTiles;

    public bool FogOfWarEnabled { get { return fogOfWarEnabled; } }
    public int Radius { get { return radius; } }

    public void UpdateFogOfWar()
    {
        // Compound assignment - if litTiles is null then assign it as new()
        litTiles ??= new();

        foreach (TileData tile in litTiles)
        {
            setTileLit(tile, false);
        }
        litTiles.Clear();

        foreach (WorldObject worldObject in PlayerController.instance.OwnedObjects)
        {
            foreach(TileData tile in GetTilesInRadiusOfObject(worldObject))
            {
                setTileLit(tile, true);
                litTiles.Add(tile);
                PlayerController.instance.DiscoverTile(tile);
            }
        }

        foreach (WorldObject worldObject in PlayerController.instance.OwnedObjects)
        {
            foreach(TileData tile in GetTilesInRadiusOfObject(worldObject))
            {
                EnemyController.instance.DiscoverTile(tile);
            }
        }
    }

    private void setTileLit(TileData tile, bool lit)
    {
        tile.lit = lit;
        if (tile.tileObject != null)
        {
            tile.tileObject.SetDark(!lit);
        }
        if (tile.occupiedObject && !tile.occupiedObject.IsPlayerOwned)
        {
            tile.occupiedObject.SetVisible(lit);
        }
        if (tile.occupiedUnit && !tile.occupiedUnit.IsPlayerOwned)
        {
            tile.occupiedUnit.SetVisible(lit);
        }
    }

    private List<TileData> GetTilesInRadiusOfObject(WorldObject worldObject)
    {
        List<TileData> tiles = new();

        for (int i = worldObject.tile.i - radius; i < worldObject.tile.i + radius + 1; i++)
        {
            for (int j = worldObject.tile.j - radius; j < worldObject.tile.j + radius + 1; j++)
            {
                if(World.instance.generator.IsInBounds(i, j))
                {
                    float dist = Vector2.Distance(new Vector2(i, j), worldObject.tile.pos);
                    if (dist <= radius + 0.5f)
                    {
                        TileData tile = World.instance.tileDataMap[i, j];
                        tiles.Add(tile);
                    }
                }
            }
        }

        return tiles;
    }
}
