using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PlayerController : BaseController
{
    [SerializeField] private GameObject moveMarkerPrefab;
    [SerializeField] private Material buildingGhostMaterial, buildingGhostMaterialSet;
    [SerializeField] private Material buildingGhostMaterialBad;
    [SerializeField] private Material playerColourMaterial, enemyColourMaterial;

    [ReadOnly] public List<WorldObject> selectedObjects;
    [ReadOnly] public WorldObject objectHovered;
    [ReadOnly] public bool isPlacingStructure;

    // Perhaps move this to UnitBuilder
    private GameObject structureGhostModel;
    private Renderer[] structureGhostRenderers;
    private UnitBuilder unitToBuild;
    private Structure structureToBuild;
    private float structureRotation;

    public Material EnemyMaterial { get { return enemyColourMaterial; } }

    public static PlayerController instance;

    protected override void Awake()
    {
        base.Awake();
        selectedObjects = new();

        if(instance == null)
        {
            instance = this;
        }
    }

    protected override void Update()
    {
        base.Update();

        // If selecting any units
        if ((selectedObjects.Where(o => o is Unit).Count() > 0 || isPlacingStructure) && !HUD.IsMouseOverHUD())
        {
            World.instance.surface.isChoosingTile = true;
        }
        else
        {
            World.instance.surface.isChoosingTile = false;
        }

        if (isPlacingStructure && structureGhostModel != null && !HUD.IsMouseOverHUD())
        {
            if (HUD.IsMouseOverHUD())
            {
                structureGhostModel.SetActive(false);
            }
            else
            {
                TileData tileHovered = World.instance.tileDataMap[World.instance.surface.tileHitCoords.x, World.instance.surface.tileHitCoords.y];
                structureGhostModel.transform.position = World.instance.GetTilePos(tileHovered);
                structureGhostModel.transform.rotation = Quaternion.Lerp(structureGhostModel.transform.rotation, Quaternion.Euler(Vector3.up * structureRotation), Time.deltaTime * 20);
                bool canBuild = true;
                if (structureToBuild is StructureBridge)
                {
                    if (tileHovered.type != TileType.None || tileHovered.connections.Where(o => o.type == TileType.Ground).Count() == 0)
                    {
                        canBuild = false;
                    }
                }
                else if (tileHovered.type != TileType.Ground || tileHovered.occupiedUnit != null)
                {
                    canBuild = false;
                }
                foreach (Renderer renderer in structureGhostRenderers)
                {
                    renderer.material = canBuild ? buildingGhostMaterial : buildingGhostMaterialBad;
                }
            }
        }

        if (isPlacingStructure)
        {
            if (Input.GetKeyDown(KeyCode.Q)) structureRotation -= 90;
            if (Input.GetKeyDown(KeyCode.E)) structureRotation += 90;
        }
    }

    public void SelectObject(WorldObject worldObject)
    {
        if (!selectedObjects.Contains(worldObject)) selectedObjects.Add(worldObject);
        worldObject.SetSelected(true);
    }

    public void DeselectObject(WorldObject worldObject)
    {
        if (selectedObjects.Contains(worldObject)) selectedObjects.Remove(worldObject);
        worldObject.SetSelected(false);
    }

    public void DeselectAll()
    {
        foreach (WorldObject worldObject in selectedObjects) worldObject.SetSelected(false);
        selectedObjects.Clear();
    }

    // Move a group of characters around a target position so they don't all end up at the same point
    public void MoveUnits(TileData tile, bool showMarker)
    {
        Unit[] selectedUnits = selectedObjects.Where(o => o is Unit).Select(o => (Unit)o).ToArray();
        if (selectedUnits.Length == 0) return;

        // Clear actions
        foreach (Unit unit in selectedUnits) unit.ActionHandler.ClearActions();

        // Don't show the marker if no path was generated successfully
        bool moveSuccessful = false;

        // Move each unit
        for (int i = 0; i < selectedUnits.Length; i++)
        {
            selectedUnits[i].MoveToPoint(tile);
            if (selectedUnits[i].Movement.HasPath)
            {
                moveSuccessful = true;
            }
        }

        // Spawn marker
        if (showMarker && moveSuccessful) Instantiate(moveMarkerPrefab, World.instance.GetTilePos(tile.i, tile.j), Quaternion.identity);
    }

    public void HarvestResource(ResourceObject resource)
    {
        UnitHarvester[] selectedUnits = selectedObjects.Where(o => o is UnitHarvester).Select(o => (UnitHarvester)o).ToArray();
        if (selectedUnits.Length == 0) return;

        for (int i = 0; i < selectedUnits.Length; i++)
        {
            selectedUnits[i].HarvestResource(resource);
        }
    }

    public void DepositResources(StructureBase baseStructure)
    {
        
    }

    public void AttackObject(WorldObject worldObject)
    {
        if (worldObject.OwnerId != 1) return;
        UnitFighter[] selectedUnits = selectedObjects.Where(o => o is UnitFighter).Select(o => (UnitFighter)o).ToArray();
        if (selectedUnits.Length == 0) return;

        for (int i = 0; i < selectedUnits.Length; i++)
        {
            if (selectedUnits[i] != worldObject) selectedUnits[i].Attack(worldObject);
        }
    }

    public void Build(Structure structure)
    {
        UnitBuilder[] selectedUnits = selectedObjects.Where(o => o is UnitBuilder).Select(o => (UnitBuilder)o).ToArray();
        if (selectedUnits.Length == 0) return;

        StartBuildingPlacement(selectedUnits[0], structure);
    }

    public void StartBuildingPlacement(UnitBuilder unit, Structure structure)
    {
        if (structureGhostModel) Destroy(structureGhostModel);
        structureGhostModel = Instantiate(structure.Model);
        Vector2Int tileHovered = World.instance.surface.tileHitCoords;
        structureGhostModel.transform.position = World.instance.GetTilePos(tileHovered.x, tileHovered.y);
        structureGhostRenderers = structureGhostModel.GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer renderer in structureGhostRenderers)
        {
            renderer.material = buildingGhostMaterial;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
        }

        unitToBuild = unit;
        structureToBuild = structure;
        isPlacingStructure = true;
        structureRotation = 0;
    }

    public void CancelBuildingPlacement()
    {
        if (structureGhostModel) Destroy(structureGhostModel);
        isPlacingStructure = false;
    }

    public void RepairUnit()
    {
        Unit[] selectedUnits = selectedObjects.Where(o => o is Unit).Select(o => (Unit)o).ToArray();
        int cost = 0;
        foreach (Unit unit in selectedUnits)
        {
            cost += unit.GetRepairCost();
        }
        if (gems >= cost)
        {
            AddGems(-cost);
            foreach (Unit unit in selectedUnits)
            {
                unit.Repair();
            }
        }
    }

    public void RepairStructure()
    {
        Structure[] selectedStructures = selectedObjects.Where(o => o is Structure).Select(o => (Structure)o).ToArray();
        int cost = 0;
        foreach (Structure structure in selectedStructures)
        {
            cost += structure.GetRepairCost();
        }
        if (minerals >= cost)
        {
            AddMinerals(-cost);
            foreach (Structure structure in selectedStructures)
            {
                structure.Repair();
            }
        }
    }

    public void StopMoving()
    {
        Unit[] selectedUnits = selectedObjects.Where(o => o is Unit).Select(o => (Unit)o).ToArray();
        foreach (Unit unit in selectedUnits)
        {
            unit.Movement.StopMoving();
        }
    }

    public void LeftClickTile(TileData tile)
    {
        if (isPlacingStructure)
        {
            if (structureToBuild is StructureBridge && tile.type != TileType.None)
            {
                CancelBuildingPlacement();
                return;
            }
            isPlacingStructure = false;
            foreach (MeshRenderer renderer in structureGhostModel.GetComponentsInChildren<MeshRenderer>())
            {
                renderer.material = buildingGhostMaterialSet;
            }
            unitToBuild.Build(structureToBuild, tile, structureGhostModel, structureRotation);
            structureGhostModel = null;
        }
    }

    public void RightClickTile(TileData tile)
    {
        if (isPlacingStructure)
        {
            CancelBuildingPlacement();
        }
        else if(tile.occupiedObject is ResourceObject)
        {
            HarvestResource(tile.occupiedObject as ResourceObject);
        }
        else
        {
            MoveUnits(tile, true);
        }
    }
}
