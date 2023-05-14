using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class BaseController : MonoBehaviour
{
    [SerializeField] private int ownerId;
    [SerializeField] private Material objectMaterial;

    [ReadOnly] public int gems, minerals;
    [ReadOnly] public int warehouses;

    protected StructureBase baseStructure;

    // List of each unit for convenience
    protected List<UnitHarvester> harvesterUnits;
    protected List<UnitBuilder> builderUnits;
    protected List<UnitFighter> fighterUnits;
    protected List<UnitScout> scoutUnits;

    protected List<Structure> ownedStructures;
    protected List<WorldObject> allOwnedObjects;

    protected List<TileData> discoveredTiles;
    protected Dictionary<ResourceObject, float> discoveredResources; // Resource, Time since last visible

    public int MaxGems { get { return 500 + warehouses * 500; } }
    public int MaxMinerals { get { return 2000 + warehouses * 2000; } }

    public BaseController OpponentController { get
    {
        if (ownerId == 0) return EnemyController.instance;
            else return PlayerController.instance;
    } }

    public Material ObjectMaterial { get { return objectMaterial; } }
    public WorldObject[] OwnedObjects { get { return allOwnedObjects.ToArray(); } }
    public UnitFighter[] OwnedFighterUnits { get { return fighterUnits.ToArray(); } }
    public UnitBuilder[] OwnedBuilderUnits { get { return builderUnits.ToArray(); } }
    public Structure[] OwnedStructures { get { return ownedStructures.ToArray(); } }
    public ResourceObject[] DiscoveredResources { get { return discoveredResources.Keys.ToArray(); } }
    public StructureBase BaseStructure { get { return baseStructure; } }
    public float GemsPercentage { get { return (float) gems / MaxGems; } }
    public float MineralsPercentage { get { return (float) minerals / MaxMinerals; } }

    protected virtual void Awake()
    {
        harvesterUnits = new();
        builderUnits = new();
        fighterUnits = new();
        scoutUnits = new();

        ownedStructures = new();
        allOwnedObjects = new();

        discoveredTiles = new();
        discoveredResources = new();
    }

    protected virtual void Update()
    {
        List<ResourceObject> resourcesToReset = new();
        List<ResourceObject> resourcesToIncrease = new();
        foreach(ResourceObject resource in discoveredResources.Keys)
        {
            if(IsWorldObjectVisible(resource))
            {
                resourcesToReset.Add(resource);
            }
            else
            {
                resourcesToIncrease.Add(resource);
            }
        }

        foreach(ResourceObject resourceToReset in resourcesToReset)
        {
            discoveredResources[resourceToReset] = 0.0f;
        }
        foreach(ResourceObject resourceToIncrease in resourcesToIncrease)
        {
            discoveredResources[resourceToIncrease] += Time.deltaTime;
        }
    }

    public void AddGems(int value)
    {
        gems += value;
        gems = Mathf.Clamp(gems, 0, MaxGems);
    }

    public void AddMinerals(int value)
    {
        minerals += value;
        minerals = Mathf.Clamp(minerals, 0, MaxMinerals);
    }

    public bool IsAtMaxResource(ResourceType type)
    {
        if (type == ResourceType.Gem)
        {
            return gems >= MaxGems;
        }
        if (type == ResourceType.Mineral)
        {
            return minerals >= MaxMinerals;
        }
        return false;
    }

    public void SetBaseStructure(StructureBase baseStructure)
    {
        this.baseStructure = baseStructure;
    }

    public void DiscoverTile(TileData tile)
    {
        if(tile == null) return;

        if(!discoveredTiles.Contains(tile))
        {
            discoveredTiles.Add(tile);

            if(tile.occupiedObject != null)
            {
                if(ownerId == 0)
                {
                    tile.occupiedObject.SetDiscovered(true);
                }

                if (tile.occupiedObject is ResourceObject)
                {
                    DiscoverResourceObject(tile.occupiedObject as ResourceObject);
                }
            }
        }
    }

    public virtual void DiscoverResourceObject(ResourceObject resourceObject)
    {
        if(!discoveredResources.ContainsKey(resourceObject))
        {
            discoveredResources.Add(resourceObject, 0);
        }
    }

    public bool HasDiscoveredTile(TileData tile)
    {
        return discoveredTiles.Contains(tile);
    }

    public bool HasAnyHarvestersRemaining()
    {
        return harvesterUnits.Count > 0;
    }

    public float TimeSinceResourceWasLastVisible(ResourceObject resource)
    {
        if(discoveredResources.ContainsKey(resource))
        {
            return discoveredResources[resource];
        }
        return -1;
    }

    public bool IsWorldObjectVisible(WorldObject worldObject)
    {
        if(worldObject.OwnerId == ownerId)
        {
            return true;
        }

        foreach(WorldObject ownedObject in allOwnedObjects)
        {
            if(worldObject.InRangeOfOther(ownedObject))
            {
                return true;
            }
        }

        return false;
    }

    // TODO: Come up with a more interesting looking function for this to show utility theory
    public ResourceType ResourceTypeToPrioritise()
    {
        //float gemDesirability = 1 - (gems / MaxGems);
        //float mineralDesirability = 1 - (minerals / MaxMinerals);

        //return gemDesirability > mineralDesirability ? ResourceType.Gem: ResourceType.Mineral;

        if (gems < 100 || gems < minerals / 2.0f)
        {
            return ResourceType.Gem;
        }

        return ResourceType.Mineral;
    }

    public void RegisterWorldObject(WorldObject worldObject)
    {
        allOwnedObjects.Add(worldObject);

        if(worldObject is UnitHarvester) harvesterUnits.Add(worldObject as UnitHarvester);
        if(worldObject is UnitBuilder) builderUnits.Add(worldObject as UnitBuilder);
        if(worldObject is UnitFighter) fighterUnits.Add(worldObject as UnitFighter);
        if(worldObject is UnitScout) scoutUnits.Add(worldObject as UnitScout);
        if(worldObject is Structure) ownedStructures.Add(worldObject as Structure);
    }

    public void UnregisterWorldObject(WorldObject worldObject)
    {
        allOwnedObjects.Remove(worldObject);

        if(worldObject is UnitHarvester) harvesterUnits.Remove(worldObject as UnitHarvester);
        if(worldObject is UnitBuilder) builderUnits.Remove(worldObject as UnitBuilder);
        if(worldObject is UnitFighter) fighterUnits.Remove(worldObject as UnitFighter);
        if(worldObject is UnitScout) scoutUnits.Remove(worldObject as UnitScout);
        if(worldObject is Structure) ownedStructures.Remove(worldObject as Structure);
    }
}
