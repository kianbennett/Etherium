using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PlayerController : Singleton<PlayerController> {

    public GameObject moveMarkerPrefab;

    [HideInInspector] public List<WorldObject> selectedObjects;
    [ReadOnly] public WorldObject objectHovered;

    public int minerals;
    public int gems;

    protected override void Awake() {
        base.Awake();
    }

    void Update() {
        // If selecting any units
        if(selectedObjects.Where(o => o is Unit).Count() > 0) {
            World.instance.surface.isChoosingTile = true;
        } else {
            World.instance.surface.isChoosingTile = false;
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

    public void DeselectAllCharacters() {
        foreach(WorldObject character in selectedObjects) character.SetSelected(false);
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
}
