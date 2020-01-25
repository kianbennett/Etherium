using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class UnitBuilder : Unit {

    [ReadOnly] public float buildAmount; // percentage, 0 - 1

    private TileData targetTile;
    private bool isBuilding;
    private Structure structureToBuild;
    private float structureRotation;

    private GameObject buildingGhostModel;
    private Healthbar progressBar;

    protected override void Awake() {
        base.Awake();

        progressBar = HUD.instance.CreateProgressBar();
    }

    protected override void Update() {
        base.Update();

        if(targetTile != null) {
            float distToStructure = Vector2Int.Distance(tile.pos, targetTile.pos);
            if(distToStructure <= 1 && movement.hasReachedDestination) {
                movement.LookAtTile(targetTile);

                buildAmount += Time.deltaTime;
                isBuilding = true;

                // If the player has lost the required resources on the journey then cancel the build
                if(GameManager.instance.minerals < structureToBuild.buildCost) {
                    cancelBuilding();
                }
                
                if(buildAmount >= structureToBuild.buildTime) {
                    buildAmount = 0;
                    spawnStructure();
                    cancelBuilding();
                }
            } else if(isBuilding) {
                // Cancel building process
               cancelBuilding();
            }
        }
        updateProgressBar();
    }

    public void Build(Structure structure, TileData tile, GameObject ghostModel, float rotation) {
        cancelBuilding();
        targetTile = tile;
        structureToBuild = structure;
        structureRotation = rotation;
        buildingGhostModel = ghostModel;
        buildingGhostModel.transform.rotation = Quaternion.Euler(Vector3.up * rotation);

        List<TileData> path = World.instance.pathfinder.Solve(this.tile, tile, structure is StructureBridge);

        // If the structure is a bridge then allow empty tiles in the path, but only one (to avoid the unit crossing empty tiles to build the structure)
        if(structure is StructureBridge) {
            if(path.Where(o => o.type == TileType.None).Count() != 1 || path.Last().type != TileType.None) {
                path = null;
            }
        }
        
        if(path != null) {
            if(path.Count > 1) {
                path.Remove(path.Last());
                movement.SetPath(path);    
            }
        } else {
            cancelBuilding();
        }
    }

    private void spawnStructure() {
        if(structureToBuild != null && targetTile != null && GameManager.instance.minerals >= structureToBuild.buildCost) {
            Structure structure = Instantiate(structureToBuild, targetTile.worldPos, Quaternion.identity);
            structure.transform.rotation = Quaternion.Euler(Vector3.up * structureRotation);
            structure.tile = targetTile;
            targetTile.type = TileType.Structure;
            if(structure is StructureBridge) {
                targetTile.type = TileType.Ground;
            } else {
                targetTile.occupiedStructure = structureToBuild;
            }
            GameManager.instance.AddMinerals(-structureToBuild.buildCost);
        }
    }

    private void cancelBuilding() {
        buildAmount = 0;
        targetTile = null;
        structureToBuild = null;
        isBuilding = false;
        Destroy(buildingGhostModel);
    }

    public void MoveAndKeepBuilding(TileData tile) {
        base.MoveToPoint(tile);
    }

    public override void MoveToPoint(TileData tile) {
        base.MoveToPoint(tile);
        cancelBuilding();
    }

    private void updateProgressBar() {
        bool show = progressBar.isOnScreen() && isBuilding;
        progressBar.gameObject.SetActive(show);
        if(show) {
            progressBar.SetWorldPos(targetTile.worldPos + Vector3.up * 1.5f);    
            progressBar.SetPercentage(buildAmount / structureToBuild.buildTime);
        }
    }

    protected override void OnDestroy() {
        base.OnDestroy();
        if(progressBar) {
            cancelBuilding();
            Destroy(progressBar.gameObject);
        }
    }
}
