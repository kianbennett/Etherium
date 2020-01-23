using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class UnitBuilder : Unit {

    [ReadOnly] public float buildAmount; // percentage, 0 - 1
    public float buildDuration;

    private TileData targetTile;
    private bool isBuilding;
    private Structure structureToBuild;
    private float structureRotation;

    private GameObject buildingGhostModel;
    private Healthbar progressBar;

    protected override void Awake() {
        base.Awake();
    }

    protected override void Update() {
        base.Update();

        if(targetTile != null) {
            float distToStructure = Vector2Int.Distance(tile.pos, targetTile.pos);
            if(distToStructure <= 1 && movement.hasReachedDestination) {
                movement.LookAtTile(targetTile);

                buildAmount += Time.deltaTime;
                if(progressBar) updateProgressBar();
                
                if(buildAmount >= buildDuration) {
                    buildAmount = 0;
                    spawnBuilding();
                    cancelBuilding();
                }
                if(!isBuilding) {
                    if(progressBar) Destroy(progressBar.gameObject);
                    progressBar = HUD.instance.CreateProgressBar();
                }
                isBuilding = true;
            } else if(isBuilding) {
                // Cancel building process
               cancelBuilding();
            }
        }
    }

    public void Build(Structure structure, TileData tile, GameObject ghostModel, float rotation) {
        cancelBuilding();
        targetTile = tile;
        structureToBuild = structure;
        structureRotation = rotation;
        buildingGhostModel = ghostModel;
        buildingGhostModel.transform.rotation = Quaternion.Euler(Vector3.up * rotation);

        List<TileData> path = World.instance.pathfinder.Solve(this.tile, tile, false);
        if(path != null) {
            if(path.Count > 1) {
                path.Remove(path.Last());
                movement.SetPath(path);    
            }
        } else {
            Destroy(ghostModel);
        }
    }

    private void spawnBuilding() {
        if(structureToBuild != null && targetTile != null) {
            Structure structure = Instantiate(structureToBuild, targetTile.worldPos, Quaternion.identity);
            structure.transform.rotation = Quaternion.Euler(Vector3.up * structureRotation);
            structure.tile = targetTile;
            targetTile.type = TileType.Structure;
            targetTile.occupiedStructure = structureToBuild;
        }
    }

    private void cancelBuilding() {
        buildAmount = 0;
        targetTile = null;
        structureToBuild = null;
        isBuilding = false;
        Destroy(buildingGhostModel);
        if(progressBar) {
            Destroy(progressBar.gameObject);
        }
    }

    public void MoveAndKeepBuilding(TileData tile) {
        base.MoveToPoint(tile);
    }

    public override void MoveToPoint(TileData tile) {
        base.MoveToPoint(tile);
        cancelBuilding();
    }

    private void updateProgressBar() {
        bool show = progressBar.isOnScreen();
        progressBar.SetWorldPos(targetTile.worldPos + Vector3.up * 1.5f);
        progressBar.gameObject.SetActive(show);
        progressBar.SetPercentage(buildAmount / buildDuration);
    }

    void OnDestroy() {
        if(progressBar) Destroy(progressBar.gameObject);
    }
}
