using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class UnitBuilder : Unit {

    [ReadOnly] public float buildAmount; // percentage, 0 - 1
    private const float buildSpeed = 0.5f;

    private Structure structureToBuild;
    private TileData targetTile;
    private bool isBuilding;

    private GameObject buildingGhostModel;

    protected override void Update() {
        base.Update();

        if(targetTile != null) {
            float distToStructure = Vector2Int.Distance(tile.pos, targetTile.pos);
            if(distToStructure <= 1 && movement.hasReachedDestination) {
                movement.LookAtTile(targetTile);

                buildAmount += Time.deltaTime * buildSpeed;
                
                if(buildAmount >= 1) {
                    buildAmount = 0;
                    spawnBuilding();
                    Destroy(buildingGhostModel);
                }
                isBuilding = true;
            } else if(isBuilding) {
                // Cancel building process
                buildAmount = 0;
                targetTile = null;
                structureToBuild = null;
                isBuilding = false;
                Destroy(buildingGhostModel);
            }
        }
    }

    public void Build(Structure structure, TileData tile, GameObject ghostModel) {
        buildingGhostModel = Instantiate(ghostModel, tile.worldPos, Quaternion.identity);
        targetTile = tile;
        structureToBuild = structure;

        List<TileData> path = World.instance.pathfinder.Solve(this.tile, tile, false);
        if(path != null && path.Count > 2) {
            path.Remove(path.Last());
            movement.SetPath(path);
        }
    }

    private void spawnBuilding() {
        if(structureToBuild != null && targetTile != null) {
            Structure structure = Instantiate(structureToBuild, targetTile.worldPos, Quaternion.identity);
        }
    }
}
