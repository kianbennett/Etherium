using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class UnitBuilder : Unit 
{
    private Structure structureToBuild;
    private TileData targetTile;
    private float structureRotation;

    private GameObject buildingGhostModel;
    private Healthbar progressBar;

    private bool isBuilding;
    private float buildAmount;

    protected override void Awake() 
    {
        base.Awake();

        progressBar = HUD.instance.CreateProgressBar();
    }

    protected override void Update() 
    {
        base.Update();

        if(targetTile != null) 
        {
            float distToStructure = Vector2Int.Distance(tile.pos, targetTile.pos);

            if(distToStructure <= 1 && Movement.HasReachedDestination) 
            {
                Movement.LookAtTile(targetTile);

                buildAmount += Time.deltaTime;
                isBuilding = true;

                // If the player has lost the required resources on the journey then cancel the build
                if(GameManager.instance.GetResourceAmount(OwnerId, ResourceType.Mineral) < structureToBuild.BuildCost) 
                {
                    cancelBuilding();
                }
                
                if(structureToBuild && buildAmount >= structureToBuild.BuildTime) 
                {
                    buildAmount = 0;
                    spawnStructure();
                    cancelBuilding();
                }
            } 
            else if(isBuilding) 
            {
               cancelBuilding();
            }
        }
        updateProgressBar();
    }

    public void Build(Structure structure, TileData tile, GameObject ghostModel, float rotation) 
    {
        cancelBuilding();
        targetTile = tile;
        structureToBuild = structure;
        structureRotation = rotation;
        buildingGhostModel = ghostModel;

        if(buildingGhostModel != null) 
        {
            buildingGhostModel.transform.rotation = Quaternion.Euler(Vector3.up * rotation);
        }

        StartCoroutine(World.instance.pathfinder.Solve(this.tile, tile, structure is StructureBridge, delegate(List<TileData> path)
        {
            // If the structure is a bridge then allow empty tiles in the path, but only one (to avoid the unit crossing empty tiles to build the structure)
            if (structure is StructureBridge)
            {
                if (path.Where(o => o.type == TileType.None).Count() != 1 || path.Last().type != TileType.None)
                {
                    path = null;
                }
            }

            if (path != null)
            {
                if (path.Count > 1)
                {
                    path.Remove(path.Last());
                    Movement.SetPath(path);
                }
            }
            else
            {
                cancelBuilding();
            }
        }));
    }

    private void spawnStructure() 
    {
        if(structureToBuild != null && targetTile != null && PlayerController.instance.minerals >= structureToBuild.BuildCost) 
        {
            Structure structure = World.instance.SpawnStructure(structureToBuild, OwnerId, targetTile.i, targetTile.j);
            structure.transform.rotation = Quaternion.Euler(Vector3.up * structureRotation);
            PlayerController.instance.AddMinerals(-structureToBuild.BuildCost);
        }
    }

    private void cancelBuilding() 
    {
        buildAmount = 0;
        targetTile = null;
        structureToBuild = null;
        isBuilding = false;
        if(buildingGhostModel) Destroy(buildingGhostModel);
    }

    public void MoveAndKeepBuilding(TileData tile) 
    {
        base.MoveToPoint(tile);
    }

    public override void MoveToPoint(TileData tile) 
    {
        base.MoveToPoint(tile);
        cancelBuilding();
    }

    private void updateProgressBar() 
    {
        bool show = progressBar.isOnScreen() && isBuilding;
        progressBar.gameObject.SetActive(show);
        
        if(targetTile != null) 
        {
            progressBar.SetWorldPos(targetTile.worldPos + Vector3.up * 1.5f);    
        } 
        if(show) 
        {
            progressBar.SetPercentage(buildAmount / structureToBuild.BuildTime);
        }
    }

    protected override void OnDestroy() 
    {
        base.OnDestroy();

        if(progressBar) 
        {
            cancelBuilding();
            Destroy(progressBar.gameObject);
        }
    }

    public bool HasStructureToBuild()
    {
        return structureToBuild != null;
    }
}
