using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class EnemyController : BaseController
{
    public static EnemyController instance;

    protected override void Awake()
    {
        base.Awake();

        if(instance == null)
        {
            instance = this;
        }
    }

    private IEnumerator harvestCoroutine()
    {
        bool harvest = true;

        while (harvest)
        {
            // Short delay before each loop to simulate thinking time
            yield return new WaitForSeconds(1);

            for (int i = 0; i < harvesterUnits.Count; i++)
            {
                if (!harvesterUnits[i].HasResourceToHarvest())
                {
                    // Units alternate between looking for gems and minerals
                    ResourceObject[] nearestResources = FindNearestFullResource(harvesterUnits[i].tile, (ResourceType)(i % 2));
                    if (nearestResources.Length > 0)
                    {
                        harvesterUnits[i].HarvestResource(nearestResources[0]);
                    }
                }
            }
        }
    }

    private IEnumerator buildUnitCoroutine()
    {
        bool buildUnits = true;

        // Build one of each unit to start with
        while (builderUnits.Count == 0)
        {
            yield return new WaitForSeconds(1);
            baseStructure.AddUnitToQueue(World.instance.unitBuilderPrefab);
        }
        while (fighterUnits.Count == 0)
        {
            yield return new WaitForSeconds(1);
            baseStructure.AddUnitToQueue(World.instance.unitFighterPrefab);
        }
        while (scoutUnits.Count == 0)
        {
            yield return new WaitForSeconds(1);
            baseStructure.AddUnitToQueue(World.instance.unitScoutPrefab);
        }

        // Top off units if the count falls too low
        // Don't have infinite units to conserve gems for repairs
        while (buildUnits)
        {
            yield return new WaitForSeconds(1);

            if (harvesterUnits.Count < 2)
            {
                baseStructure.AddUnitToQueue(World.instance.unitHarvesterPrefab);
            }

            if (builderUnits.Count < 2)
            {
                baseStructure.AddUnitToQueue(World.instance.unitBuilderPrefab);
            }

            if (fighterUnits.Count < 6)
            {
                baseStructure.AddUnitToQueue(World.instance.unitFighterPrefab);
            }

            if (scoutUnits.Count < 2)
            {
                baseStructure.AddUnitToQueue(World.instance.unitScoutPrefab);
            }
        }
    }

    private IEnumerator buildStructureCoroutine()
    {
        bool buildStructures = true;

        while (buildStructures)
        {
            yield return new WaitForSeconds(1);

            if (gems < World.instance.structureDefenceTowerPrefab.BuildCost) continue;

            StructureDefenceTower[] towersOwned = allOwnedObjects.Where(o => o is StructureDefenceTower).Select(o => (StructureDefenceTower)o).ToArray();

            foreach (UnitBuilder builder in builderUnits)
            {
                if (!builder.HasStructureToBuild() && towersOwned.Length < 1)
                {
                    builder.Build(World.instance.structureDefenceTowerPrefab, GetFreeTileInArea(baseStructure.tile.pos, 4), null, 0);
                }
            }
        }
    }

    private IEnumerator fightCoroutine()
    {
        bool fight = true;

        while (fight)
        {
            yield return new WaitForSeconds(1);
        }
    }

    // Every 5 seconds cycle through all owned units/structures and repair them if possible, with a 1 second delay between them
    private IEnumerator repairCoroutine()
    {
        bool repair = true;

        while (repair)
        {
            yield return new WaitForSeconds(5);

            foreach (WorldObject worldObject in allOwnedObjects)
            {
                if (worldObject is Unit)
                {
                    int repairCost = ((Unit)worldObject).GetRepairCost();
                    if (repairCost > 0 && gems >= repairCost)
                    {
                        AddGems(-repairCost);
                        ((Unit)worldObject).Repair();
                    }
                }
                if (worldObject is Structure)
                {
                    int repairCost = ((Structure)worldObject).GetRepairCost();
                    if (repairCost > 0 && gems >= repairCost)
                    {
                        AddGems(-repairCost);
                        ((Structure)worldObject).Repair();
                    }
                }
                yield return new WaitForSeconds(1);
            };
        }
    }

    private IEnumerator exploreCoroutine()
    {
        bool explore = true;

        while (explore)
        {
            yield return new WaitForSeconds(1);
        }
    }

    public ResourceObject[] FindNearestFullResource(TileData origin, ResourceType type)
    {
        return World.instance.resourceObjects.Where(o => o.Type == type && o.RemainingResource > 0.5f).OrderBy(o => Vector2Int.Distance(origin.pos, o.tile.pos)).ToArray();
    }

    public TileData GetFreeTileInArea(Vector2Int origin, int radius)
    {
        List<TileData> candidates = new List<TileData>();
        for (int i = 0; i < radius * 2; i++)
        {
            for (int j = 0; j < radius * 2; j++)
            {
                int x = origin.x + i - radius;
                int y = origin.y + j - radius;
                if (World.instance.generator.IsInBounds(x, y))
                {
                    TileData tile = World.instance.tileDataMap[x, y];
                    if (tile.occupiedObject == null && tile.occupiedUnit == null)
                    {
                        candidates.Add(tile);
                    }
                }
            }
        }
        return candidates[Random.Range(0, candidates.Count)];
    }
}
