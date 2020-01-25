using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructureBase : Structure {

    private Healthbar progressBar;

    private List<Unit> unitBuildQueue = new List<Unit>();
    private float buildTick;

    protected override void Awake() {
        base.Awake();
        
        progressBar = HUD.instance.CreateProgressBar();
    }

    protected override void Update() {
        base.Update();

        if(unitBuildQueue.Count > 0) {
            buildTick += Time.deltaTime;
            if(buildTick > unitBuildQueue[0].buildTime) {
                buildTick = 0;
                spawnUnit(unitBuildQueue[0]);
                unitBuildQueue.RemoveAt(0);
            }
        } else {
            buildTick = 0;
        }
        updateProgressBar();

        // if(Input.GetKeyDown(KeyCode.Alpha1)) AddUnitToQueue(World.instance.unitarvesterPrefab);
        // if(Input.GetKeyDown(KeyCode.Alpha2)) AddUnitToQueue(World.instance.unitFighterPrefab);
        // if(Input.GetKeyDown(KeyCode.Alpha3)) AddUnitToQueue(World.instance.unitScoutPrefab);
        // if(Input.GetKeyDown(KeyCode.Alpha4)) AddUnitToQueue(World.instance.unitBuilderPrefab);
    }

    public void AddUnitToQueue(Unit unit) {
        unitBuildQueue.Add(unit);
    }

    private void spawnUnit(Unit unit) {
        Unit spawnedUnit = Instantiate(unit, transform.position, Quaternion.identity);
        spawnedUnit.tile = tile;
        spawnedUnit.tile.occupiedUnit = spawnedUnit;
        World.instance.units.Add(spawnedUnit);

        // Start with tile directly outside the entrance
        TileData startingTile = World.instance.tileDataMap[tile.i, tile.j - 1];
        TileData freeTile = null;

        // Create empty list
        // add starting tile to list
        // if not free, add each connection to list
        // go through the list and check if the tile is free - if it is but has a unit on it then add it's connections to the list

        List<TileData> tilesToCheck = new List<TileData>();
        List<TileData> checkedTiles = new List<TileData>();
        tilesToCheck.Add(startingTile);
        bool checkForTile = true;

        while(checkForTile) {
            for(int i = tilesToCheck.Count - 1; i >= 0; i--) {
                if (tilesToCheck[i].IsTileAccessible(unit.movement.isFlying)) {
                    if(tilesToCheck[i].occupiedUnit) {
                        bool hasConnection = false;
                        foreach (TileData connectedTile in tilesToCheck[i].connections) {
                            if (connectedTile.IsTileAccessible(unit.movement.isFlying) && !checkedTiles.Contains(connectedTile)) {
                                tilesToCheck.Add(connectedTile);
                                hasConnection = true;
                            }
                        }
                        if(!hasConnection) checkForTile = false;
                    } else {
                        freeTile = tilesToCheck[i];
                        checkForTile = false;
                        break;
                    }
                }
                checkedTiles.Add(tilesToCheck[i]);
                tilesToCheck.RemoveAt(i);
            }
        }

        // If there is no free tile available, move onto the starting tile (may double up units)
        if(freeTile != null) {
            spawnedUnit.MoveToPoint(freeTile);
        } else {
            // Use movement.SetPath so it doesn't check if the tile has a unit on it
            spawnedUnit.movement.SetPath(new List<TileData>() { tile, startingTile });
        }
    }

    private void updateProgressBar() {
        bool show = progressBar.isOnScreen() && unitBuildQueue.Count > 0;
        if(show) {
            progressBar.SetWorldPos(transform.position + Vector3.up * 1.5f);
            progressBar.gameObject.SetActive(show);
            progressBar.SetPercentage(buildTick / unitBuildQueue[0].buildTime);
        }
    }

    protected override void OnDestroy() {
        base.OnDestroy();
        if(progressBar) Destroy(progressBar.gameObject);
    }
}
