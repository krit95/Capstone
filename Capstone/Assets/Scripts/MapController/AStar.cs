using System.Collections.Generic;
using UnityEngine;
using PriorityQueueDemo;
using System;

// TODO: You can probably make this static
class AStar {

    public readonly Tile startTile;
    public readonly Tile endTile;

    // A lookup table for currently best found distances to a given tile
    private Dictionary<Tile, float> bestCostToTile;

    private readonly int MAX_DEPTH = 50000;
    private int totalChecks = 0; // How many nodes have we explored? Used for early termination checks

    public AStar(Tile start, Tile end) {
        // NOTE: This function goes to the map controller and pulls the actual tiles/ edges
        MapController mc = MapController.singleton;
        this.startTile = mc.getTile(start);
        this.endTile = mc.getTile(end);

        this.bestCostToTile = new Dictionary<Tile, float>();
    }

    public Queue<Tile> getPath() {
        PathfindingQueueEntry currentQE = path();

        // TODO: There's ways to optimize this function using double ended queues but I'm a little lazy rn
        List<Tile> tempList = new List<Tile>();

        while (currentQE != null) {
            tempList.Add(currentQE.tile);
            currentQE = currentQE.parentTile;
        }
        tempList.Reverse();
        Queue<Tile> result = new Queue<Tile>(tempList);
        return result;
    }

    private float heuristic(Tile t) {
        return Vector2.Distance(t.position, endTile.position);
    }

    private PathfindingQueueEntry path() {
        // If a path exists between the start and end tile this function will find it
        // Two possible return values:
        //  1) null result => No possible path
        //  2) A PathfindingQueueEntry representation of the endTIle
        //      This object will contain a parent queue entry, and recursivly point back towards the startTile entry


        PriorityQueue<float, PathfindingQueueEntry> priorityQueue = new PriorityQueue<float, PathfindingQueueEntry>();
        
        // Start Queue Entry has no parent and a cost of 0 to access
        PathfindingQueueEntry startQE = new PathfindingQueueEntry(startTile, null, 0);
        priorityQueue.Enqueue(heuristic(startTile), startQE);

        while (priorityQueue.Count > 0) {
            totalChecks++;
            PathfindingQueueEntry qe = priorityQueue.DequeueValue();

            if (endTile.Equals(qe.tile)) {
                // If we're at the endTile
                return qe;
            }
            if (totalChecks > MAX_DEPTH) {
                return null;
            }

            if (bestCostToTile.ContainsKey(qe.tile) &&
                bestCostToTile[qe.tile] <= qe.costToGetHere) {
                // If we've already explored this tile
                // AND if some other path to this tile is cheeper
                continue;
            } else {
                // Else, this Queue Entry represents a cheeper path to get to this node
                bestCostToTile[qe.tile] = qe.costToGetHere;
            }

            foreach (TileEdge neighborEdge in qe.neighbors()) {
                // A tile edge points from the current queue entry tile to the neighbor
                // It's wrapped with the weight/ cost of the edge

                Tile neighborTile = neighborEdge.tile;
                if ( ! neighborTile.isWalkable) {
                    // If the neighbor is unwalkable, ignore it right away
                    continue;
                }

                float edgeCost = neighborEdge.weight;
                float totalCostToExploreNeighbor = qe.costToGetHere + edgeCost;

                if (bestCostToTile.ContainsKey(neighborTile) &&
                    bestCostToTile[neighborTile] <= totalCostToExploreNeighbor)
                {
                    // If we already have a cheeper way to get to the neighbor then dont add it to the queue
                    continue;
                }

                PathfindingQueueEntry neighborQE = new PathfindingQueueEntry(neighborTile, qe, totalCostToExploreNeighbor);

                priorityQueue.Enqueue(heuristic(neighborTile) + totalCostToExploreNeighbor, neighborQE);

            } // end foreach neighbor

        } // end While queue is not empty

        // If the endTile never enterd the priority queue
        // IE: If we can't find a path to the end
        return null;
    }



    ///////////////////////////////////////////////////////////////


    private class PathfindingQueueEntry
    {
        // A container and utility to help us sort, modify and reason about pathfinding on tiles

        public PathfindingQueueEntry parentTile;
        public Tile tile;

        public float costToGetHere;

        public PathfindingQueueEntry(Tile tile, PathfindingQueueEntry parent, float costToGetHere) {
            this.tile = tile;
            this.parentTile = parent;
            this.costToGetHere = costToGetHere;
        }

        public HashSet<TileEdge> neighbors()
            { return MapController.singleton.getNeighborEdges(this.tile); }
        
    } // END PathfindingQueueEntry class

} // END AStart class

