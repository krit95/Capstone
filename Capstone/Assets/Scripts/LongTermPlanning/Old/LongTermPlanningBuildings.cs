using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PriorityQueueDemo;
using System;

// TODO: do we need this? 
/*
public static class LongTermPlanningBuildings {

    public static int planTotalIGTime(BuildingGS initialGS, BuildingGS targetGS) {
        QGameState currentQE = BuildPlan(initialGS, targetGS);
        if (currentQE == null) {
            return -1;
        }

        int totalCost = currentQE.costToGetHere;

        return totalCost;
    }


    public static Queue<EWork> plan(BuildingGS initialGS, BuildingGS targetGS) {

        float startTime = Time.realtimeSinceStartup;
        QGameState currentQE = BuildPlan(initialGS, targetGS);

        float endTime = Time.realtimeSinceStartup;
        Debug.Log(startTime);
        Debug.Log(endTime);
        Debug.Log("delta Time: "); Debug.Log((endTime - startTime));

        if (currentQE == null)
        {
            Debug.Log("No path found");
            return new Queue<EWork>();
        }

        foreach (ResourceType rt in currentQE.gameState.getAllResourceTypes()) {
            Debug.Log("Final game state " + Enum.GetName(rt.GetType(), rt) + ": " + currentQE.gameState.getStockpile(rt));
            Debug.Log("Final game state " + Enum.GetName(rt.GetType(), rt) + " per tick: " + currentQE.gameState.getPerTickChange(rt));
        }

        Debug.Log("Total Time: " + currentQE.costToGetHere);

        // TODO: using double ended queues may be more efficent
        List<EWork> tempList = new List<EWork>();

        while (currentQE != null) {
            tempList.Add(currentQE.transitionWork);
            currentQE = currentQE.parent;
        }

        tempList.Reverse();
        Queue<EWork> result = new Queue<EWork>(tempList);
        return result;

    }

    private static QGameState BuildPlan(BuildingGS initialGS, BuildingGS targetGS) {
        
        // Initialize data structures
        MemoryBoundedPriorityQueue<int, QGameState> priorityQueue = new MemoryBoundedPriorityQueue<int, QGameState>(200);
        Dictionary<BuildingGS, int> bestCostToGetHere = new Dictionary<BuildingGS, int>();

        // Add initial state to queue
        QGameState startQE = new QGameState(initialGS, null, EWork.EMPTY, 0);
        int heuristic = LTPHelper.compareGameState(initialGS, targetGS);
        priorityQueue.Enqueue(heuristic, startQE);
        
        // NOTE: Don't add the initial state to the bestCostToGetHere, that will be done automatically

        int totalChecks = 0;
        while (priorityQueue.Count > 0) {
            totalChecks++;
            QGameState qe = priorityQueue.DequeueValue();

            // Early exit conditions
            if (LTPHelper.compareGameState(qe.gameState, targetGS) <= 0) {
                // If we are 0 distance away from the target game state
                // IE: If we have found the target game state
                return qe;
            } else if (totalChecks > LTPHelper.MAX_DEPTH) {
                return null;
            }
            
            if (bestCostToGetHere.ContainsKey(qe.gameState) &&
                bestCostToGetHere[qe.gameState] <= qe.costToGetHere) {
                // If we've already explored this game state
                // AND if some other path is to this game state is cheeper
                continue;
            } else {
                // Else, this Queue Entry represents a cheeper path to get to this node
                bestCostToGetHere[qe.gameState] = qe.costToGetHere;
            }
            
            foreach(QGameState neighbor in LTPHelper.getNeighbors(qe)) {
                // The neighbor already has an updated gamestate, a job and an updated cost

                if (bestCostToGetHere.ContainsKey(neighbor.gameState) &&
                    bestCostToGetHere[neighbor.gameState] <= neighbor.costToGetHere) {
                    // If we already have a better way to get to the neighbor
                    continue;
                }

                heuristic = LTPHelper.compareGameState(neighbor.gameState, targetGS) + neighbor.costToGetHere;
                priorityQueue.Enqueue(heuristic, neighbor);
            } // End foreach neighbor
        } // End while queue is NOT empty

        // If we get here => no path found
        return null;
    }
    

    
}*/