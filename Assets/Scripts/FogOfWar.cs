using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class FogOfWar : MonoBehaviour {

    public bool fogOfWarEnabled;

    private List<TileData> litTiles = new List<TileData>();

    public void UpdateFogOfWar() {
        foreach(TileData tile in litTiles) {
            setTileLit(tile, false);
        }
        litTiles.Clear();

        List<WorldObject> ownedObjects = new List<WorldObject>();

        foreach(Structure structure in PlayerController.instance.ownedStructures) {
            if(structure.ownerId == 0) ownedObjects.Add(structure);
        }
        foreach(Unit unit in PlayerController.instance.ownedUnits) {
            if(unit.ownerId == 0) ownedObjects.Add(unit);
        }

        int radius = 5;
        foreach(WorldObject worldObject in ownedObjects) {
            for(int i = worldObject.tile.i - radius; i < worldObject.tile.i + radius + 1; i++) {
                for(int j = worldObject.tile.j - radius; j < worldObject.tile.j + radius + 1; j++) {
                    float dist = Vector2.Distance(new Vector2(i, j), worldObject.tile.pos);
                    if(dist <= radius + 0.5f) {
                        if(World.instance.generator.IsInBounds(i, j)) {
                            TileData tile = World.instance.tileDataMap[i, j];
                            setTileLit(tile, true);
                            litTiles.Add(tile);
                        }
                    }
                }   
            }
        }
    }

    private void setTileLit(TileData tile, bool lit) {
        tile.lit = lit;
        if(tile.tileObject != null) {
            tile.tileObject.SetDark(!lit);
        }
        if(tile.occupiedObject) {
            tile.occupiedObject.SetVisible(lit);
        }
        if(tile.occupiedUnit) {
            tile.occupiedUnit.SetVisible(lit);
        }
    }
}
