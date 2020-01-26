using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum TileType { None, Ground, Mine, Mineral, Base, Structure, Rock };

public class Island {
    public Vector2Int origin;
    public int size;
    public TileData[] tiles;
    public int index;

    public Vector2Int centre { get { return origin + new Vector2Int((int) ((float) size / 2), (int) ((float) size / 2)); } }

    // public void setTileType(int i, int j, TileType type) {
    //     TileData[] results = tiles.Where(o => o.i == i && o.j == j).ToArray();
    //     if(results.Length > 0) {
    //         results[0].type = type;
    //     }
    // }
}

public class TileData {
    public int i, j;
    public TileType type;
    public int island;
    public Unit occupiedUnit;
    // public Structure occupiedStructure;
    public WorldObject occupiedObject;
    public bool lit; // Is within player view, i.e. not in fog of war

    public Vector2Int pos { get { return new Vector2Int(i, j); } }
    public Vector3 worldPos { get { return World.instance.GetTilePos(this); } }
    public TileObject tileObject { get { return World.instance.GetTileObjectAt(this); } }

    public bool spawnEnemyFighter, spawnEnemyTower;

    // Pathfinding data
    public List<TileData> connections;
    public float global;
    public float local;
    public TileData parent;
    public bool visited;

    public TileData(int i, int j, TileType type) {
        this.i = i;
        this.j = j;
        this.type = type;
    }

    // Ground tiles are always accessible, empty tiles obly accessible if the unit can access them (i.e. flying)
    public bool IsTileAccessible(bool canCrossEmptyTiles) {
        return type == TileType.Ground || (canCrossEmptyTiles && type == TileType.None);
    }
}

public class WorldGenerator : MonoBehaviour {

    public int worldSize;
    public int islandCount;
    public int minIslandSize, maxIslandSize;
    public int seed;
    public bool randomiseSeed;
    public float heightMapCutoff;

    public float scale;
    public int octaves;
    public float persistance;
    public float lacunarity;

    public TileData[,] Generate() {
        if(randomiseSeed) seed = Random.Range(0, int.MaxValue);
        Random.InitState(seed);

        TileData[,] tileMap = new TileData[worldSize, worldSize];

        // Fill map with empty tiles
        for(int i = 0; i < worldSize; i++) {
            for(int j = 0; j < worldSize; j++) {
                tileMap[i, j] = new TileData(i, j, TileType.None);
            }
        }

        // Create islands
        List<Island> islands = new List<Island>();

        Island playerIsland = generateIsland(new Vector2Int(0, 0), maxIslandSize, 0);
        populateIsland(playerIsland);
        islands.Add(playerIsland);

        Island enemyIsland = generateIsland(new Vector2Int(worldSize - maxIslandSize, worldSize - maxIslandSize), maxIslandSize, 1);
        populateIsland(enemyIsland);
        islands.Add(enemyIsland);

        for(int i = 0; i < islandCount; i++) {
            int size = Random.Range(minIslandSize, maxIslandSize);
            Vector2Int pos = getNextPointInArea(islands.Select(o => o.origin).ToArray(), size);
            Island island = generateIsland(pos, size, 2 + i);
            populateIsland(island);
            islands.Add(island);
            // Temporarily add a base at the centre of each island
            // tileMap[island.origin.x + (int) ((float) island.size / 2), island.origin.y + (int) ((float) island.size / 2)].type = TileType.Base;
        }

        for(int i = 0; i < islands.Count; i++) {
            foreach(TileData tile in islands[i].tiles) {
                if(IsInBounds(tile.i, tile.j)) {
                    tileMap[tile.i, tile.j] = tile;
                    // tile.island = i;
                }
            }
        }

        // Clear out area around base to ensure there are no resources blocking the exit
        int clearRadius = 3;
        for(int i = 0; i < clearRadius * 2; i++) {
            for(int j = 0; j < clearRadius * 2; j++) {
                int x = playerIsland.centre.x + i - clearRadius;
                int y = playerIsland.centre.y + j - clearRadius;
                if(IsInBounds(x, y)) {
                    tileMap[x, y].type = TileType.Ground;
                }
            }   
        }
        for(int i = 0; i < clearRadius * 2; i++) {
            for(int j = 0; j < clearRadius * 2; j++) {
                int x = enemyIsland.centre.x + i - clearRadius;
                int y = enemyIsland.centre.y + j - clearRadius;
                if(IsInBounds(x, y)) {
                    tileMap[x, y].type = TileType.Ground;
                    // Stop fighters and towers spawing on/infront of the base
                    tileMap[x, y].spawnEnemyFighter = false;
                    tileMap[x, y].spawnEnemyTower = false;
                }
            }   
        }

        tileMap[enemyIsland.centre.x - 2, enemyIsland.centre.y + 2].spawnEnemyTower = true;
        tileMap[enemyIsland.centre.x + 2, enemyIsland.centre.y - 2].spawnEnemyTower = true;

        tileMap[playerIsland.centre.x, playerIsland.centre.y].type = TileType.Base;
        tileMap[enemyIsland.centre.x, enemyIsland.centre.y].type = TileType.Base;

        

        setUpConnections(tileMap);

        return tileMap;
    }

    // Each island has it's own perlin noise map
    // TODO: Try islands with varying width and heigth
    public Island generateIsland(Vector2Int origin, int size, int islandIndex) {
        float[,] noiseMap = NoiseGenerator.GenerateNoiseMap(size, size, seed, scale, octaves, persistance, lacunarity, Vector2.zero, NoiseGenerator.NormalizeMode.Global);
        float[,] falloffMap = FalloffGenerator.GenerateFalloffMap(size);

        List<TileData> tiles = new List<TileData>();

        for(int j = 0; j < size; j++) {
            for(int i = 0; i < size; i++) {
                float value = Mathf.Clamp01(noiseMap[i, j] - falloffMap[i, j]); // Get value by subtracting falloff from noise
                // If this is above a certain value then add a tile
                if(value > heightMapCutoff) {
                    TileData tile = new TileData(origin.x + i, origin.y + j, TileType.Ground);
                    tile.island = islandIndex;
                    tiles.Add(tile);
                }
            }
        }

        Island island = new Island() {
            origin = origin,
            size = size,
            tiles = tiles.ToArray(),
            index = islandIndex
        };
        return island;
    }

    // Set up connections between adjacent nodes
    private void setUpConnections(TileData[,] tileMap) {
        for(int i = 0; i < worldSize; i++) {
            for(int j = 0; j < worldSize; j++) {
                TileData tile = tileMap[i, j];
                tile.connections = new List<TileData>();

                // Check tile above
                if(IsInBounds(i, j + 1) && (tileMap[i, j + 1].type == TileType.None || tileMap[i, j + 1].type == TileType.Ground)) {
                    tile.connections.Add(tileMap[i, j + 1]);
                }
                // Check tile below
                if(IsInBounds(i, j - 1) && (tileMap[i, j - 1].type == TileType.None || tileMap[i, j - 1].type == TileType.Ground)) {
                    tile.connections.Add(tileMap[i, j - 1]);
                }
                // Check tile left
                if(IsInBounds(i - 1, j) && (tileMap[i - 1, j].type == TileType.None || tileMap[i - 1, j].type == TileType.Ground)) {
                    tile.connections.Add(tileMap[i - 1, j]);
                }
                // Check tile right
                if(IsInBounds(i + 1, j) && (tileMap[i + 1, j].type == TileType.None || tileMap[i + 1, j].type == TileType.Ground)) {
                    tile.connections.Add(tileMap[i + 1, j]);
                }
            }
        }
    }

     // Place resources randomly across each island - each island must have a minimum number of each resource
    private void populateIsland(Island island) {
        // island.setTileType(island.origin.x + (int) ((float) island.size / 2), island.origin.y + (int) ((float) island.size / 2), TileType.Base);

        int mines = Random.Range(3, 6);
        int minerals = Random.Range(island.size - 10, island.size);
        int rocks = Random.Range(island.size / 4, island.size / 2);

        if(minerals < 0) minerals = 0;
        if(rocks < 0) rocks = 0;

        List<TileData> mineTiles = new List<TileData>();
        List<TileData> mineralTiles = new List<TileData>();
        List<TileData> rockTiles = new List<TileData>();
        for(int i = 0; i < mines; i++) {
            TileData tile = null;
            // Make sure new tile hasn't already got a mine on it
            do {
                tile = island.tiles[Random.Range(0, island.tiles.Length)];
            } while(mineTiles.Contains(tile));

            mineTiles.Add(tile);
            tile.type = TileType.Mine;
        }
        for(int i = 0; i < minerals; i++) {
            TileData tile = null;
            do {
                tile = island.tiles[Random.Range(0, island.tiles.Length)];
            } while(mineralTiles.Contains(tile) || mineTiles.Contains(tile));

            mineralTiles.Add(tile);
            tile.type = TileType.Mineral;
        }
        for(int i = 0; i < rocks; i++) {
            TileData tile = null;
            do {
                tile = island.tiles[Random.Range(0, island.tiles.Length)];
            } while(mineTiles.Contains(tile));

            rockTiles.Add(tile);
            tile.type = TileType.Rock;
        }

        // Don't spawn enemies on the player's island
        if(island.index != 0) {
            int enemies = Random.Range(1, 4);
            int towers = Random.Range(0, 2);

            // Put more fighters on the enemy's base island
            if(island.index == 1) {
                enemies = Random.Range(3, 6);
            }

            List<TileData> enemyTiles = new List<TileData>();
            List<TileData> towerTiles = new List<TileData>();

            for(int i = 0; i < enemies; i++) {
                TileData tile = null;
                do {
                    tile = island.tiles[Random.Range(0, island.tiles.Length)];
                } while(enemyTiles.Contains(tile) || mineTiles.Contains(tile));

                enemyTiles.Add(tile);
                tile.type = TileType.Ground;
                tile.spawnEnemyFighter = true;
            }
            for(int i = 0; i < towers; i++) {
                TileData tile = null;
                do {
                    tile = island.tiles[Random.Range(0, island.tiles.Length)];
                } while(towerTiles.Contains(tile) || mineTiles.Contains(tile));

                towerTiles.Add(tile);
                tile.type = TileType.Ground;
                tile.spawnEnemyTower = true;
            }
        }
    }

    // Use Mitchell's best candidate algorithm to distribute points evenly across a surface
    // For each point generate a set of samples and use the furthest away from any existing point
    private Vector2Int getNextPointInArea(Vector2Int[] existing, int size) {
        Vector2Int bestCandidate = default;
        float bestDistance = 0;
        int sampleCount = 10; // High count yields better distrubition but lower performance
        for(int i = 0; i < sampleCount; i++) {
            Vector2Int candidate = new Vector2Int(Random.Range(0, worldSize - size), Random.Range(0, worldSize - size));
            float distance = distToClosest(candidate, existing);
            if(distance > bestDistance) {
                bestCandidate = candidate;
                bestDistance = distance;
            }
        }
        return bestCandidate;
    }

    // Use Mitchell's best candidate algorithm but for tiles in a list
    private TileData getNextTileInArea(TileData[] existing, TileData[] tiles, TileType[] exclusions) {
        TileData bestCandidate = null;
        float bestDistance = 0;
        int sampleCount = 10;
        for(int i = 0; i < sampleCount; i++) {
            TileData candidate = tiles[Random.Range(0, tiles.Length)];
            if(exclusions != null && exclusions.Contains(candidate.type)) continue;

            float distance = distToClosest(candidate.pos, existing.Select(o => o.pos).ToArray());
            if(distance > bestDistance) {
                bestCandidate = candidate;
                bestDistance = distance;
            }
        }
        return bestCandidate;
    }

    // Brute force should be fine as number of samples is usually low
    private float distToClosest(Vector2Int point, Vector2Int[] existing) {
        float dist = 0;
        foreach(Vector2Int p in existing) {
            float d = Vector2Int.Distance(point, p);
            if(dist == 0 || d < dist) dist = d;
        }
        return dist;
    }

    // Is within the world tile array bounds
    public bool IsInBounds(int i, int j) {
        return (i >= 0 && i < worldSize && j >= 0 && j < worldSize);
    }
}