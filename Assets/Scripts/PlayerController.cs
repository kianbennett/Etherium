using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PlayerController : Singleton<PlayerController> {

    public GameObject moveMarkerPrefab;
    public Material buildingGhostMaterial, buildingGhostMaterialSet;

    [HideInInspector] public List<WorldObject> selectedObjects;
    [ReadOnly] public StructureBase playerBase;
    [ReadOnly] public WorldObject objectHovered;
    [ReadOnly] public bool isPlacingStructure;
    [ReadOnly] public int minerals;
    [ReadOnly] public int gems;

    // Perhaps move this to UnitBuilder
    private GameObject structureGhostModel;
    private UnitBuilder unitToBuild;
    private Structure structureToBuild;
    private float structureRotation;

    protected override void Awake() {
        base.Awake();
    }

    void Update() {
        // If selecting any units
        if((selectedObjects.Where(o => o is Unit).Count() > 0 || isPlacingStructure) && !HUD.instance.IsMouseOverHUD()) {
            World.instance.surface.isChoosingTile = true;
        } else {
            World.instance.surface.isChoosingTile = false;
        }

        if(isPlacingStructure && structureGhostModel != null && !HUD.instance.IsMouseOverHUD()) {
            TileData tileHovered = World.instance.tileDataMap[World.instance.surface.tileHitCoords.x, World.instance.surface.tileHitCoords.y];
            structureGhostModel.transform.position = World.instance.GetTilePos(tileHovered);
            structureGhostModel.transform.rotation = Quaternion.Lerp(structureGhostModel.transform.rotation, Quaternion.Euler(Vector3.up * structureRotation), Time.deltaTime * 20);
            bool canBuild = tileHovered.type == TileType.None || tileHovered.type == TileType.Ground;
            // if(structureToBuild is StructureBridge && tileHovered.type != TileType.None) canBuild = false;
            structureGhostModel.SetActive(canBuild);
        }

        // if(Input.GetKeyDown(KeyCode.A)) Build(World.instance.structureDefenceTowerPrefab);
        // if(Input.GetKeyDown(KeyCode.S)) Build(World.instance.structureWallPrefab);
        // if(Input.GetKeyDown(KeyCode.D)) Build(World.instance.structureWallCornerPrefab);
        // if(Input.GetKeyDown(KeyCode.F)) Build(World.instance.structureBridgePrefab);

        if(isPlacingStructure) {
            if(Input.GetKeyDown(KeyCode.Q)) structureRotation -= 90;
            if(Input.GetKeyDown(KeyCode.E)) structureRotation += 90;
        }
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
        if(structureGhostModel) Destroy(structureGhostModel);
        structureGhostModel = Instantiate(structure.model);
        Vector2Int tileHovered = World.instance.surface.tileHitCoords;
        structureGhostModel.transform.position = World.instance.GetTilePos(tileHovered.x, tileHovered.y);
        foreach(MeshRenderer renderer in structureGhostModel.GetComponentsInChildren<MeshRenderer>()) {
            renderer.material = buildingGhostMaterial;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
        }

        unitToBuild = unit;
        structureToBuild = structure;
        isPlacingStructure = true;
        structureRotation = 0;
    }

    public void CancelBuildingPlacement() {
        if(structureGhostModel) Destroy(structureGhostModel);
        isPlacingStructure = false;
    }

    public void LeftClickTile(TileData tile) {
        if(isPlacingStructure) {
            if(structureToBuild is StructureBridge && tile.type != TileType.None) {
                CancelBuildingPlacement();
                return;
            }
            isPlacingStructure = false;
            foreach(MeshRenderer renderer in structureGhostModel.GetComponentsInChildren<MeshRenderer>()) {
                renderer.material = buildingGhostMaterialSet;
            }
            unitToBuild.Build(structureToBuild, tile, structureGhostModel, structureRotation);
            structureGhostModel = null;
        }
    }

    public void RightClickTile(TileData tile) {
        if(isPlacingStructure) {
            CancelBuildingPlacement();
        } else {
            MoveUnits(tile, true);
        }
    }
}
