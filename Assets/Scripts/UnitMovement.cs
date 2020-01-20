using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class UnitMovement : MonoBehaviour {

    public Unit unit;

    public float moveSpeed;
    public AnimationCurve accCurve, decCurve;
    public bool isFlying;

    [ReadOnly] public Vector3 lookDir;
    [ReadOnly] public bool canMove = true;
    [ReadOnly] public bool hasReachedDestination;
    
     // The distance over which to decelerate to zero
    private float decDist;
    private float totalPathLength;
    private float distTravelled;
    private int targetNode;
    private float currentSpeed;

    [HideInInspector] public TileData tileDestination;

    [HideInInspector] public List<TileData> pathNodes;

    void Awake() {
        decDist = decCurve.keys.Last().time;
    }

    void Update() {
        if (pathNodes != null && !hasReachedDestination) {
            currentSpeed = getCurrentSpeed(moveSpeed);

            distTravelled += currentSpeed * Time.deltaTime;
            transform.position += lookDir * currentSpeed * Time.deltaTime;

            if(DistFromLastWaypoint(transform.position) > DistToNextWaypoint()) {
                NextTargetNode();
            }
        }
        // Set transform rotation from LookDir
        if (lookDir != Vector3.zero) {
            lookDir.y = 0; // Lock to xz axis
            unit.model.rotation = Quaternion.Lerp(unit.model.rotation, Quaternion.LookRotation(lookDir), Time.deltaTime * 10);
        }
    }

    public void CalculatePath(TileData target) {
        List<TileData> path = World.instance.pathfinder.Solve(unit.tile, target, isFlying);
        if(path == null) {
            return;
        }

        // Put previous tile at the beginning of the path to avoid awkward movement when moving between tiles
        if(pathNodes != null) {
            path.Insert(0, pathNodes[targetNode - 1]);
        }

        // If there is already a unit moving to the destination then remove the last tile and move to the next free tile
        do {
            if(target.occupiedUnit != null && target.occupiedUnit != unit) {
                path.Remove(path.Last());
                target = path.Last();
            }
        } while(path.Count > 0 && path.Last().occupiedUnit != null && path.Last().occupiedUnit != unit);
        
        pathNodes = path;
        hasReachedDestination = false;
        totalPathLength = (pathNodes.Count - 1) * World.tileSize;
        distTravelled = 0;
        targetNode = 0;

        target.occupiedUnit = unit;
        if(tileDestination != null) tileDestination.occupiedUnit = null;
        tileDestination = target;

        SetTargetNode(1);

        distTravelled = DistFromLastWaypoint(transform.position);
    }

    private float getCurrentSpeed(float maxSpeed) {
        if (distTravelled < totalPathLength - decDist) {
            return accCurve.Evaluate(distTravelled) * maxSpeed;
        } else {
            // return decCurve.Evaluate(distTravelled - (totalPathLength - decDist)) * maxSpeed;
            return maxSpeed;
        }
    }

    public void SetTargetNode(int node) {
        // Debug.Log("Set target node: " + node);

        if (node >= pathNodes.Count) {
            // Debug.Log("Path completed");
            pathNodes = null;
            hasReachedDestination = true;
            return;
        }

        lookDir = pathNodes[node].worldPos - pathNodes[node - 1].worldPos;
        targetNode = node;
        unit.tile = pathNodes[node];
    }

    public void NextTargetNode() {
        SetTargetNode(targetNode + 1);
    }

    public void LookAtTile(TileData tile) {
        lookDir = tile.worldPos - unit.tile.worldPos;
    }

    // Distance between unit and the most recent node passed 
    public float DistFromLastWaypoint(Vector3 position) {
        if (targetNode > 0) {
            return Vector3.Distance(position, pathNodes[targetNode - 1].worldPos);
        } else {
            return 0;
        }
    }

    // Distance between next node and previous one, should always be equal to tileSize
    public float DistToNextWaypoint() {
        if (targetNode > 0) {
            return Vector3.Distance(pathNodes[targetNode].worldPos, pathNodes[targetNode - 1].worldPos);
        } else {
            return 0;
        }
    }

    public float DistToPathEnd() {
        return totalPathLength - distTravelled;
    }

    // Stops on the next tile they would have entered
    public void StopMoving() {
    }
}
