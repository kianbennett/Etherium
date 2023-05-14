using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathfinder : MonoBehaviour
{
    public IEnumerator Solve(TileData begin, TileData destination, bool canCrossEmptyTiles, System.Action<List<TileData>> onComplete)
    {
        System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();

        List<TileData> result = null;

        if (begin != null && destination != null /*&& destination.IsTileAccessible(canCrossEmptyTiles)*/)
        {
            TileData directConnection = begin.connections.Find(c => c == destination);

            // Nothing to solve if there is a direct connection between these two locations
            if (directConnection != null)
            {
                result = new List<TileData>
                {
                    begin,
                    destination
                };
            }
            else
            {
                // Set all the state to its starting values
                List<TileData> tilesToBeTested = new();

                for (int i = 0; i < World.instance.generator.worldSize; i++)
                {
                    for (int j = 0; j < World.instance.generator.worldSize; j++)
                    {
                        World.instance.tileDataMap[i, j].parent = null;
                        World.instance.tileDataMap[i, j].global = float.MaxValue;
                        World.instance.tileDataMap[i, j].local = float.MaxValue;
                        World.instance.tileDataMap[i, j].visited = false;
                    }
                }

                // Setup the start node to be zero away from start and estimate distance to target
                TileData currentNode = begin;
                currentNode.local = 0.0f;
                currentNode.global = heuristic(begin, destination);

                // Maintain a list of nodes to be tested and begin with the start node, keep going
                // as long as we still have nodes to test and we haven't reached the destination
                tilesToBeTested.Add(currentNode);

                // Keep track of the closest node to the destination - in case we can't reach the destination we still want
                // to move to the nearest tile
                TileData bestNode = currentNode;

                float timer = 0.0f;

                while (tilesToBeTested.Count > 0 && currentNode != destination)
                {
                    float time = stopwatch.ElapsedTicks;

                    // Begin by sorting the list each time by the heuristic
                    tilesToBeTested.Sort((a, b) => (int)(a.global - b.global));

                    // Remove any tiles that have already been visited
                    tilesToBeTested.RemoveAll(n => n.visited);

                    // Check that we still have locations to visit
                    if (tilesToBeTested.Count > 0)
                    {
                        // Mark this note visited and then process it
                        currentNode = tilesToBeTested[0];
                        currentNode.visited = true;

                        // If we aren't going to be able to reach the destination tile then get the next closest tile
                        if(currentNode.IsTileAccessible(canCrossEmptyTiles))
                        {
                            if(heuristic(currentNode, destination) < heuristic(bestNode, destination))
                            {
                                bestNode = currentNode;
                            }
                        }

                        // Check each neighbour, if it is accessible and hasn't already been 
                        // processed then add it to the list to be tested 
                        for (int count = 0; count < currentNode.connections.Count; ++count)
                        {
                            TileData neighbour = currentNode.connections[count];

                            // Only move through tiles that don't have anything built on them
                            // TODO: Move tile type checking to be per-unit
                            if (!neighbour.visited && neighbour.IsTileAccessible(canCrossEmptyTiles))
                            {
                                tilesToBeTested.Add(neighbour);
                            }

                            // Calculate the local goal of this location from our current location and 
                            // test if it is lower than the local goal it currently holds, if so then
                            // we can update it to be owned by the current node instead 
                            float possibleLocalGoal = currentNode.local + distance(currentNode, neighbour);
                            if (possibleLocalGoal < neighbour.local)
                            {
                                neighbour.parent = currentNode;
                                neighbour.local = possibleLocalGoal;
                                neighbour.global = neighbour.local + heuristic(neighbour, destination);
                            }
                        }
                    }

                    // If taking too long skip to the next frame
                    timer += stopwatch.ElapsedTicks - time;
                    if (timer > 50000)
                    {
                        timer = 0;
                        yield return null;
                    }
                }

                TileData endNode = destination.visited ? destination : bestNode;

                // Build path if we found one, by checking if the destination was visited, if so then 
                // we have a solution, trace it back through the parents and return the reverse route
                result = new List<TileData>();
                TileData routeNode = endNode;

                while (routeNode.parent != null)
                {
                    result.Add(routeNode);
                    routeNode = routeNode.parent;
                }
                result.Add(routeNode);
                result.Reverse();

                //Debug.LogFormat("Path Found: {0} steps {1} long", result.Count, endNode.local);
            }
        }
        else
        {
            Debug.LogWarning("Cannot find path for invalid nodes");
        }

        onComplete(result);
    }

    private float distance(TileData a, TileData b)
    {
        // Use the length of the connection between these two nodes to find the distance, this 
        // is used to calculate the local goal during the search for a path to a location
        float result = float.MaxValue;
        bool directConnection = a.connections.Find(c => c == b) != null;
        result = World.tileSize;
        return result;
    }

    private float heuristic(TileData a, TileData b)
    {
        // Use the locations of the node to estimate how close they are by line of sight
        // experiment here with better ways of estimating the distance. This is used  to
        // calculate the global goal and work out the best order to prossess nodes in
        return Vector3.Distance(World.instance.GetTilePos(a), World.instance.GetTilePos(b));
    }
}
