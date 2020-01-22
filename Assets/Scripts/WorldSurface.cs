using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldSurface : MonoBehaviour {

    public GameObject tileSelect;
    public Transform gridLines;
    public LineRenderer[] gridLinesX, gridLinesZ;

    [HideInInspector] public bool isChoosingTile;
    [HideInInspector] public Vector3 mouseHitPoint;
    [HideInInspector] public Vector2Int tileHitCoords;

    void Update() {
        bool hit = getMousedOverCoordinates(out Vector3 hitPoint, out Vector2Int hitCoords);
        mouseHitPoint = hitPoint;
        tileHitCoords = hitCoords;
        
        tileSelect.SetActive(hit && isChoosingTile);
        gridLines.gameObject.SetActive(hit && isChoosingTile);
        if (hit) {
            float x = hitCoords.x * World.tileSize - World.tileSize * World.instance.generator.worldSize / 2;
            float z = hitCoords.y * World.tileSize - World.tileSize * World.instance.generator.worldSize / 2;
            tileSelect.transform.position = new Vector3(x, 0.01f, z);
            gridLines.position = new Vector3(x, 0.01f, z);

            for(int i = 0; i < gridLinesX.Length; i++) {
                gridLinesX[i].transform.position = new Vector3(hitPoint.x, 0.01f, gridLinesX[i].transform.position.z);
                float alpha = Mathf.Clamp(2.5f - Mathf.Abs(gridLinesX[i].transform.position.z - hitPoint.z), 0, float.MaxValue) * 0.05f;
                GameUtil.SetMaterialAlpha(gridLinesX[i].material, alpha);
            }
            for(int i = 0; i < gridLinesZ.Length; i++) {
                gridLinesZ[i].transform.position = new Vector3(gridLinesZ[i].transform.position.x, 0.01f, hitPoint.z);
                float alpha = Mathf.Clamp(2.5f - Mathf.Abs(gridLinesZ[i].transform.position.x - hitPoint.x), 0, float.MaxValue) * 0.05f;
                GameUtil.SetMaterialAlpha(gridLinesZ[i].material, alpha);
            }
        }
    }

    // Calculates the point on the ground hit by the mouse, and the coordinates of that tile
    private bool getMousedOverCoordinates(out Vector3 hitPoint, out Vector2Int hitCoords) {
        Ray ray = CameraController.instance.camera.ScreenPointToRay(Input.mousePosition);
        bool hit = Physics.Raycast(ray, out RaycastHit hitInfo, float.MaxValue, 1 << LayerMask.NameToLayer("Ground"));
        if(hit) {
            Vector3 point = hitInfo.point;
            int i = (int) (Math.Round(point.x / World.tileSize) + World.tileSize * World.instance.generator.worldSize / 2);
            int j = (int) (Math.Round(point.z / World.tileSize) + World.tileSize * World.instance.generator.worldSize / 2);
            hitPoint = point;
            hitCoords = new Vector2Int(i, j);
        } else {
            hitPoint = Vector3.zero;
            hitCoords = Vector2Int.zero;
        }
        return hit;
    }
}
