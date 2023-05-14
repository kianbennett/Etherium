﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UnitMovement : MonoBehaviour
{
    [SerializeField] private Unit unit;

    [Header("Parameters")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private AnimationCurve accCurve, decCurve;
    [SerializeField] private bool isFlying;

    private Vector3 moveDir, lookDir;
    private bool hasReachedDestination;

    // The distance over which to decelerate to zero
    private float decDist;
    private float totalPathLength;
    private float distTravelled;
    private int targetNode;
    private float currentSpeed;

    private TileData tileDestination;
    private List<TileData> pathNodes;
    private List<TileData> remainingPathNodes;

    public TileData TileDestination { get { return tileDestination; } }
    public bool HasReachedDestination { get { return hasReachedDestination; } }

    public bool IsFlying { get { return isFlying; } }
    public bool HasPath { get { return pathNodes != null; } }
    public TileData[] RemainingPathNodes { get { return remainingPathNodes.ToArray(); } }

    void Awake()
    {
        decDist = decCurve.keys.Last().time;
        lookDir = Vector3.back;
        unit.ModelRotation = Quaternion.LookRotation(lookDir);
        remainingPathNodes = new();
    }

    void Update()
    {
        if (pathNodes != null && !hasReachedDestination)
        {
            currentSpeed = calculateCurrentSpeed(moveSpeed);

            distTravelled += currentSpeed * Time.deltaTime;
            transform.position += moveDir * currentSpeed * Time.deltaTime;

            if (DistFromLastWaypoint(transform.position) > DistToNextWaypoint())
            {
                NextTargetNode();
            }
        }
        // Set transform rotation from LookDir
        if (lookDir != Vector3.zero)
        {
            lookDir.y = 0; // Lock to xz axis
            unit.ModelRotation = Quaternion.Lerp(unit.ModelRotation, Quaternion.LookRotation(lookDir), Time.deltaTime * 10);
        }
    }

    public void CalculatePath(TileData target)
    {
        StartCoroutine(World.instance.pathfinder.Solve(unit.tile, target, isFlying, delegate(List<TileData> path)
        {
            if (path == null)
            {
                return;
            }

            // If there is already a unit moving to the destination then remove the last tile and move to the next free tile
            do
            {
                if (target.occupiedUnit != null && target.occupiedUnit != unit)
                {
                    path.Remove(path.Last());
                    target = path.Last();
                }
            }
            while (path.Count >= 2 && path.Last().occupiedUnit != null && path.Last().occupiedUnit != unit);

            target.occupiedUnit = unit;
            if (tileDestination != null) tileDestination.occupiedUnit = null;
            tileDestination = target;

            SetPath(path);
        }));
    }

    public void SetPath(List<TileData> path)
    {
        // Put previous tile at the beginning of the path to avoid awkward movement when moving between tiles
        if (pathNodes != null && pathNodes.Count > 0 && targetNode > 0)
        {
            path.Insert(0, pathNodes[targetNode - 1]);
        }

        pathNodes = path;
        remainingPathNodes = new(path);
        hasReachedDestination = false;
        totalPathLength = (pathNodes.Count - 1) * World.tileSize;
        distTravelled = 0;
        targetNode = 0;

        SetTargetNode(1);

        distTravelled = DistFromLastWaypoint(transform.position);
    }

    private float calculateCurrentSpeed(float maxSpeed)
    {
        if (distTravelled < totalPathLength - decDist)
        {
            return accCurve.Evaluate(distTravelled) * maxSpeed;
        }
        else
        {
            // return decCurve.Evaluate(distTravelled - (totalPathLength - decDist)) * maxSpeed;
            return maxSpeed;
        }
    }

    public void SetTargetNode(int node)
    {
        // Debug.Log("Set target node: " + node);

        if (node >= pathNodes.Count)
        {
            // Debug.Log("Path completed");
            pathNodes = null;
            hasReachedDestination = true;
            remainingPathNodes.Clear();
            return;
        }

        moveDir = pathNodes[node].worldPos - pathNodes[node - 1].worldPos;
        lookDir = moveDir;
        targetNode = node;
        unit.tile = pathNodes[node];

        remainingPathNodes.Remove(pathNodes[node - 1]);

        if (unit.IsPlayerOwned)
        {
            World.instance.fogOfWar.UpdateFogOfWar();
        }
    }

    public void EndPath()
    {
        pathNodes = null;
        hasReachedDestination = true;
    }

    public void NextTargetNode()
    {
        SetTargetNode(targetNode + 1);
    }

    public void LookAtTile(TileData tile)
    {
        lookDir = tile.worldPos - unit.tile.worldPos;
    }

    // Distance between unit and the most recent node passed 
    public float DistFromLastWaypoint(Vector3 position)
    {
        // Debug.Log(pathNodes.Count + ", " + targetNode);
        if (targetNode > 0)
        {
            return Vector3.Distance(position, pathNodes[targetNode - 1].worldPos);
        }
        else
        {
            return 0;
        }
    }

    // Distance between next node and previous one, should always be equal to tileSize
    public float DistToNextWaypoint()
    {
        if (targetNode > 0)
        {
            return Vector3.Distance(pathNodes[targetNode].worldPos, pathNodes[targetNode - 1].worldPos);
        }
        else
        {
            return 0;
        }
    }

    public float DistToPathEnd()
    {
        return totalPathLength - distTravelled;
    }

    // Stops on the next tile they would have entered
    public void StopMoving()
    {
        if (pathNodes != null)
        {
            // pathNodes.RemoveRange(targetNode, pathNodes.Count() - targetNode - 1);
            pathNodes = new List<TileData>() { pathNodes[targetNode - 1], pathNodes[targetNode] };
            targetNode = 1;
            SetPath(pathNodes);
        }
    }
}
