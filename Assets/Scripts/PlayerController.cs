using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PlayerController : Singleton<PlayerController> {

    public GameObject moveMarkerPrefab;
    public Material buildingGhostMaterialGood, buildingGhostMaterialBad;

    [HideInInspector] public List<WorldObject> selectedObjects;
    [ReadOnly] public WorldObject objectHovered;
    [ReadOnly] public bool isPlacingBuilding;
    [ReadOnly] public int minerals;
    [ReadOnly] public int gems;

    // Perhaps move this to UnitBuilder
    private GameObject buildingGhostModel;
    private UnitBuilder unitToBuild;
    private Structure structureToBuild;

    protected override void Awake() {
        base.Awake();
    }

    void Update() {
        // If selecting any units
        if(selectedObjects.Where(o => o is Unit).Count() > 0 || isPlacingBuilding) {
            World.instance.surface.isChoosingTile = true;
        } else {
            World.instance.surface.isChoosingTile = false;
        }

        if(isPlacingBuilding && buildingGhostModel != null) {
            TileData tileHovered = World.instance.tileDataMap[World.instance.surface.tileHitCoords.x, World.instance.surface.tileHitCoords.y];
            buildingGhostModel.transform.position = World.instance.GetTilePos(tileHovered);
            // buildingGhostModel.SetActive(tileHovered.type == TileType.None || tileHovered.type == TileType.Ground);
            // if(buildingGhostModel.activeSelf) {
            //     buildingGhostModel.material = tileHovered.type == TileType.Ground ? buildingGhostMaterialGood : buildingGhostMaterialBad;
            // }
        }

        if(Input.GetKeyDown(KeyCode.A)) Build(World.instance.structureDefenceTowerPrefab);
    }

    public void SelectObject(WorldObject worldObject) {
        if(!selectedObjects.Contains(worldObject)) selectedObjects.Add(worldObject);
        worldObject.SetSelected(true);
    }

    public void DeselectObject(WorldObject worldObject) {
        if (selectedObjects.Contains(worldObject)) selectedObjects.Remove(worldObject);
        worldObject.SetSelected(false);
    }

    public void DeselectAll() {
        foreach(WorldObject worldObject in selectedObjects) worldObject.SetSelected(false);
        selectedObjects.Clear();
    }

    // Move a group of characters around a target position so they don't all end up at the same point
    public void MoveUnits(TileData tile, bool showMarker) {
        Unit[] selectedUnits = selectedObjects.Where(o => o is Unit).Select(o => (Unit) o).ToArray();
        if (selectedUnits.Length == 0) return;

        // Clear actions
        foreach (Unit unit in selectedUnits) unit.actionHandler.ClearActions();

        // Spawn marker
        if (showMarker) Instantiate(moveMarkerPrefab, World.instance.GetTilePos(tile.i, tile.j), Quaternion.identity);

        // Move each unit
        for (int i = 0; i < selectedUnits.Length; i++) {
            selectedUnits[i].MoveToPoint(tile);
        }
    }

    public void HarvestResource(ResourceObject resource) {
        UnitHarvester[] selectedUnits = selectedObjects.Where(o => o is UnitHarvester).Select(o => (UnitHarvester) o).ToArray();
        if (selectedUnits.Length == 0) return;

        for (int i = 0; i < selectedUnits.Length; i++) {
            selectedUnits[i].HarvestResource(resource);
        }
    }

    public void AttackUnit(Unit target) {
        UnitFighter[] selectedUnits = selectedObjects.Where(o => o is UnitFighter).Select(o => (UnitFighter) o).ToArray();
        if(selectedUnits.Length == 0) return;

        for(int i = 0; i < selectedUnits.Length; i++) {
            if(selectedUnits[i] != target) selectedUnits[i].Attack(target);
        }
    }

    public void Build(Structure structure) {
        UnitBuilder[] selectedUnits = selectedObjects.Where(o => o is UnitBuilder).Select(o => (UnitBuilder) o).ToArray();
        if (selectedUnits.Length == 0) return;

        StartBuildingPlacement(selectedUnits[0], structure);
    }

    public void StartBuildingPlacement(UnitBuilder unit, Structure structure) {
        if(buildingGhostModel) Destroy(buildingGhostModel);
        buildingGhostModel = Instantiate(structure.Model);
        Vector2Int tileHovered = World.instance.surface.tileHitCoords;
        buildingGhostModel.transform.position = World.instance.GetTilePos(tileHovered.x, tileHovered.y);
        foreach(MeshRenderer renderer in buildingGhostModel.GetComponentsInChildren<MeshRenderer>()) {
            renderer.material = buildingGhostMaterialGood;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
        }

        unitToBuild = unit;
        structureToBuild = structure;
        isPlacingBuilding = true;
    }

    public void CancelBuildingPlacement() {
        if(buildingGhostModel) Destroy(buildingGhostModel);
        isPlacingBuilding = false;
    }

    public void RightClickTile(TileData tile) {
        if(isPlacingBuilding) {
            CancelBuildingPlacement();
            unitToBuild.Build(structureToBuild, tile, buildingGhostModel);
        } else {
            MoveUnits(tile, true);
        }
    }
}
