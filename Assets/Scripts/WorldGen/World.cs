using System.Collections.Generic;
using UnityEngine;

public class World : Singleton<World>
{
    public static float tileSize = 1.0f;

    public WorldGenerator generator;
    public Pathfinder pathfinder;
    public WorldSurface surface;
    public FogOfWar fogOfWar;
    public Transform unitContainer;

    public TileObject tilePrefab;
    public TileObject tileEdgePrefab;
    public WorldObject basePrefab;
    public WorldObject minePrefab;
    public WorldObject mineralPrefab;
    public WorldObject[] rockPrefabs;
    public Unit unitHarvesterPrefab, unitScoutPrefab, unitFighterPrefab, unitBuilderPrefab;
    public Structure structureDefenceTowerPrefab, structureWallPrefab, structureWallCornerPrefab, structureBridgePrefab, structureWarehousePrefab;

    [HideInInspector] public TileData[,] tileDataMap;
    [HideInInspector] public TileObject[,] tileObjectMap;
    [HideInInspector] public List<TileObject> allTiles = new();
    [HideInInspector] public List<Unit> units = new();

    // Put resource objects in a list to conventiently access from from EnemyController without having to search
    [HideInInspector] public List<ResourceObject> resourceObjects;

    protected override void Awake()
    {
        Build();
    }

    public void Build()
    {
        tileDataMap = generator.Generate();
        // Destroy existing tiles
        clear();

        tileObjectMap = new TileObject[generator.worldSize, generator.worldSize];
        resourceObjects = new List<ResourceObject>();

        // Spawn tile objects
        for (int j = 0; j < generator.worldSize; j++)
        {
            for (int i = 0; i < generator.worldSize; i++)
            {
                if (tileDataMap[i, j].type == TileType.None) continue;
                // The sides of edge tiles are visible so a more complex model is needed, non-edge tiles can just be a plane
                bool tileUp = generator.IsInBounds(i, j + 1) && tileDataMap[i, j + 1].type != 0;
                bool tileDown = generator.IsInBounds(i, j - 1) && tileDataMap[i, j - 1].type != 0;
                bool tileLeft = generator.IsInBounds(i - 1, j) && tileDataMap[i - 1, j].type != 0;
                bool tileRight = generator.IsInBounds(i + 1, j) && tileDataMap[i + 1, j].type != 0;
                bool isEdgeTile = !tileUp || !tileDown || !tileLeft || !tileRight;

                TileObject tile = Instantiate(isEdgeTile ? tileEdgePrefab : tilePrefab, GetTilePos(i, j), Quaternion.identity, transform);
                tile.tileData = tileDataMap[i, j];
                tile.name += " (" + i + ", " + j + ")";
                tileObjectMap[i, j] = tile;
                allTiles.Add(tile);

                WorldObject worldObject = null;

                switch (tileDataMap[i, j].type)
                {
                    case TileType.Base:
                        worldObject = Instantiate(basePrefab);
                        if (tileDataMap[i, j].island == 0)
                        {
                            PlayerController.instance.SetBaseStructure((StructureBase) worldObject);
                            worldObject.SetAsPlayerOwned();
                        }
                        else
                        {
                            EnemyController.instance.SetBaseStructure((StructureBase) worldObject);
                            worldObject.SetAsEnemyOwned();
                        }
                        break;
                    case TileType.Mine:
                        worldObject = Instantiate(minePrefab);
                        resourceObjects.Add((ResourceObject) worldObject);
                        break;
                    case TileType.Mineral:
                        worldObject = Instantiate(mineralPrefab);
                        resourceObjects.Add((ResourceObject) worldObject);
                        break;
                    case TileType.Rock:
                        worldObject = Instantiate(rockPrefabs[Random.Range(0, rockPrefabs.Length)]);
                        worldObject.transform.rotation = Quaternion.Euler(Vector3.up * Random.Range(0.0f, 360.0f));
                        break;
                }

                if (worldObject != null)
                {
                    worldObject.transform.SetParent(tile.transform, false);
                    worldObject.tile = tile.tileData;
                    worldObject.tile.occupiedObject = worldObject;
                    worldObject.Init(worldObject.OwnerId);
                }

                if (tileDataMap[i, j].spawnEnemyFighter)
                {
                    SpawnUnit(unitFighterPrefab, 1, i, j);
                }
                if (tileDataMap[i, j].spawnEnemyTower)
                {
                    SpawnStructure(structureDefenceTowerPrefab, 1, i, j);
                }
            }
        }

        PlayerController.instance.BaseStructure.SetAsPlayerOwned();
        EnemyController.instance.BaseStructure.SetAsEnemyOwned();

        StructureBase playerBase = PlayerController.instance.BaseStructure;
        if(playerBase != null)
        {
            // Start each player with a single harvester unit
            SpawnUnit(unitHarvesterPrefab, 0, playerBase.tile.i, playerBase.tile.j - 1);
            //SpawnUnit(unitScoutPrefab, 0, playerBase.tile.i + 1, playerBase.tile.j - 1);

            // Focus the camera on the player's base
            CameraController.instance.SetAbsolutePosition(playerBase.transform.position);
        }
    }

    public Unit SpawnUnit(Unit prefab, int ownerId, int i, int j)
    {
        Unit unit = Instantiate(prefab.gameObject, GetTilePos(i, j), Quaternion.identity, unitContainer).GetComponent<Unit>();
        unit.tile = tileDataMap[i, j];
        unit.tile.occupiedUnit = unit;
        unit.Init(ownerId);

        units.Add(unit);

        fogOfWar.UpdateFogOfWar();

        return unit;
    }

    public Structure SpawnStructure(Structure prefab, int ownerId, int i, int j)
    {
        Structure structure = Instantiate(prefab, GetTilePos(i, j), Quaternion.identity);
        structure.tile = tileDataMap[i, j];
        structure.tile.type = TileType.Structure;
        structure.Init(ownerId);
        if (structure is StructureBridge)
        {
            structure.tile.type = TileType.Ground;
            World.instance.tileObjectMap[i, j] = structure.GetComponent<TileObjectBridge>();
        }
        else
        {
            structure.tile.occupiedObject = structure;
        }
        fogOfWar.UpdateFogOfWar();
        return structure;
    }

    public Vector3 GetTilePos(int i, int j)
    {
        return new Vector3(-generator.worldSize * tileSize / 2 + tileSize * i, 0, -generator.worldSize * tileSize / 2 + tileSize * j);
    }

    public Vector3 GetTilePos(TileData tile)
    {
        return GetTilePos(tile.i, tile.j);
    }

    public TileData GetTileDatatAt(int i, int j)
    {
        if (generator.IsInBounds(i, j))
        {
            return tileDataMap[i, j];
        }
        return null;
    }

    public TileObject GetTileObjectAt(int i, int j)
    {
        if (generator.IsInBounds(i, j))
        {
            return tileObjectMap[i, j];
        }
        return null;
    }

    public TileObject GetTileObjectAt(TileData tileData)
    {
        return GetTileObjectAt(tileData.i, tileData.j);
    }

    private void clear()
    {
        foreach (TileObject tile in allTiles) Destroy(tile.gameObject);
        foreach (Unit unit in units) Destroy(unit);
        allTiles.Clear();
        units.Clear();
    }
}
