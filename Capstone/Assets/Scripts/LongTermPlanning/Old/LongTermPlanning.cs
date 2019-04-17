using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PriorityQueueDemo;
using System;


/*
public static class LongTermPlanning {
    
    private static int WAIT_TIME = 20;
    public static int public_wait_time { get { return WAIT_TIME; } set { WAIT_TIME = Mathf.Max(0, value); } }
    public static readonly int MAX_DEPTH = 500000; // 500K searches


    public static int compareGameState(GameState currentGS, GameState targetGS) {
        // How many time cycles will it take if we do nothing? 
        // NOTE: It's expected that currentGS and targetGS have the types of resources
        //       (Or at the least, that targetGS is a subset of currentGS)

        int maxWaitTime = 0; 
        int totalRPTDelta = 0;

        foreach (var kvp in targetGS.resources) {
            Resource currentResource = currentGS.resources[kvp.Key];
            Resource targetResource = kvp.Value;

            // How long will it take to get to the target if we just wait? 
            int resourceDelta = Mathf.Max(0, targetResource.resourceCount - currentResource.resourceCount);

            int resourceWaitTime = (currentResource.resourcePerTick == 0) ?
                                       WAIT_TIME * 1000 : // If we make this value int.maxValue then we have overflow problems
                                       resourceDelta / currentResource.resourcePerTick;

            // How far are we from the target resource income rate? 
            int RPTDelta = Mathf.Max(0, targetResource.resourcePerTick - currentResource.resourcePerTick);

            maxWaitTime = Mathf.Max(maxWaitTime, resourceWaitTime);
            totalRPTDelta += RPTDelta;
        }


        return maxWaitTime + totalRPTDelta;
    }

    // TODO: put this somewhere else
    public static uint sigfigify(int n, int sigFigs) { return sigfigify((uint)n, sigFigs); }
    public static uint sigfigify(uint n, int sigFigs) {
        // NOTE: When casting a negative int into a uint the far left "negative marker" will cout as a sig fig

        int sizeOfN = 0;
        uint nPrime = n;
        while (nPrime > 0) {
            nPrime /= 10;
            sizeOfN++;
        }

        if (sigFigs >= sizeOfN) { return n; }

        int digitsCut = sizeOfN - sigFigs;
        uint trimmedN = n / (uint)Mathf.Pow(10, digitsCut); // Cut off the right most values
        return trimmedN * (uint)Mathf.Pow(10, digitsCut); // Replace them all with 0s 
    }

    private static HashSet<QGameState> getNeighbors(QGameState qEntry) {
        // For a given game state return all valid edges out of it

        HashSet<QGameState> result = new HashSet<QGameState>();
        result.Add(waitTransition(qEntry, WAIT_TIME));

        if (canBuildWorker(qEntry)) {
            // If we have the resources to build a new worker
            result.Add(buildGoldMiner(qEntry));
            result.Add(buildWoodUnit(qEntry));
            result.Add(buildStoneMiner(qEntry));
        }


        return result;
    }

    public static int planTotalIGTime(GameState initialGS, GameState targetGS) {
        QGameState currentQE = BuildPlan(initialGS, targetGS);
        if (currentQE == null) {
            return -1;
        }

        return currentQE.costToGetHere;
    }

    public static Queue<EWork> plan(GameState initialGS, GameState targetGS) {

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

        foreach (var kvp in currentQE.gameState.resources) {
            Debug.Log("Final game state " + kvp.Value.name + ": " + kvp.Value.resourceCount);
            Debug.Log("Final game state " + kvp.Value.name + " per tick: " + kvp.Value.resourcePerTick);
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


    private static QGameState BuildPlan(GameState initialGS, GameState targetGS) {

        //AccordionPriorityQueue<int, QGameState> priorityQueue = new AccordionPriorityQueue<int, QGameState>(400);
        MemoryBoundedPriorityQueue<int, QGameState> priorityQueue = new MemoryBoundedPriorityQueue<int, QGameState>(200);
        //PriorityQueue<int, QGameState> priorityQueue = new PriorityQueue<int, QGameState>(200);

        Dictionary<GameState, int> bestCostToGetHere = new Dictionary<GameState, int>();


        QGameState startQE = new QGameState(initialGS, null, EWork.EMPTY, 0);
        int heuristic = compareGameState(initialGS, targetGS);
        priorityQueue.Enqueue(heuristic, startQE);

        int totalChecks = 0;

        while (priorityQueue.Count > 0) {

            totalChecks++;

            QGameState qe = priorityQueue.DequeueValue();

            if (compareGameState(qe.gameState, targetGS) <= 0)
            {
                // If we are 0 distance away from the target game state
                // IE: If we have found the target game state
                return qe;
            }

            if (totalChecks > MAX_DEPTH)
            {
                Debug.Log("exit case max depth");
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


            foreach(QGameState neighbor in getNeighbors(qe)) {
                // The neighbor already has an updated gamestate, a job and an updated cost



                if (bestCostToGetHere.ContainsKey(neighbor.gameState) &&
                    bestCostToGetHere[neighbor.gameState] <= neighbor.costToGetHere) {
                    // If we already have a better way to get to the neighbor
                    continue;
                }

                heuristic = compareGameState(neighbor.gameState, targetGS) + neighbor.costToGetHere;
                priorityQueue.Enqueue(heuristic, neighbor);

            }

        } // End while queue is NOT empty
        Debug.Log("exit case no path");
        return null;

    }

    private class QGameState {
        // The thing that gets put onto the priority queue           

        public QGameState parent;
        public EWork transitionWork; // transitionWork represents the job done to move from parent to currentGS
                                    // NOTE: This will be EMPTY if parent is null (ie if this QE is the root qe)

        public GameState gameState;
        public int costToGetHere;

        public QGameState(GameState gs, QGameState parent, EWork transitionWork, int costToGetHere) {
            this.gameState = gs;
            this.parent = parent;
            this.transitionWork = transitionWork;
            this.costToGetHere = costToGetHere;
        }
    }


    ////////////////////////////////////////////////

    // This function represents an edge
    private static QGameState waitTransition(QGameState qe, int time) {
        GameState endGS = waitGameState(qe.gameState, time);
        int timeCost = qe.costToGetHere + time;
        return new QGameState(endGS, qe, EWork.Wait, timeCost);
    }

    private static GameState waitGameState(GameState gs, int time) {
        GameState endGS = new GameState(gs);
        endGS.timePasses(time);
        return endGS;
    }

    //////////////////////////////////////////////////

    public static readonly int workerBuildTime = 20;
    public static readonly int workerCostGold = 10;
    public static readonly int workerCostStone = 10;
    public static readonly int workerCostWood = 10;

    private static bool canBuildWorker(QGameState qe) {
        return qe.gameState.resources["gold"].resourceCount >= workerCostGold &&
            qe.gameState.resources["stone"].resourceCount >= workerCostStone &&
            qe.gameState.resources["wood"].resourceCount >= workerCostWood;
    }

    private static QGameState buildGoldMiner(QGameState qe) {
        GameState oldGameState = qe.gameState;
        GameState newGameState = waitGameState(oldGameState, workerBuildTime);

        // Subtract the cost of building a worker
        newGameState.resources["gold"].resourceCount -= workerCostGold;
        newGameState.resources["stone"].resourceCount -= workerCostStone;
        newGameState.resources["wood"].resourceCount -= workerCostWood;


        newGameState.resources["gold"].addWorkers(1);

        int timeCost = qe.costToGetHere + workerBuildTime;
        return new QGameState(newGameState, qe, EWork.BuyWorkerBank, timeCost);

    }

    private static QGameState buildStoneMiner(QGameState qe)
    {
        GameState oldGameState = qe.gameState;
        GameState newGameState = waitGameState(oldGameState, workerBuildTime);

        // Subtract the cost of building a worker
        newGameState.resources["gold"].resourceCount -= workerCostGold;
        newGameState.resources["stone"].resourceCount -= workerCostStone;
        newGameState.resources["wood"].resourceCount -= workerCostWood;

        newGameState.resources["stone"].addWorkers(1);

        int timeCost = qe.costToGetHere + workerBuildTime;
        return new QGameState(newGameState, qe, EWork.BuyWorkerStoneMason, timeCost);

    }
    
    private static QGameState buildWoodUnit(QGameState qe)
    {
        GameState oldGameState = qe.gameState;
        GameState newGameState = waitGameState(oldGameState, workerBuildTime);

        // Subtract the cost of building a worker
        newGameState.resources["gold"].resourceCount -= workerCostGold;
        newGameState.resources["stone"].resourceCount -= workerCostStone;
        newGameState.resources["wood"].resourceCount -= workerCostWood;

        newGameState.resources["wood"].addWorkers(1);

        int timeCost = qe.costToGetHere + workerBuildTime;
        return new QGameState(newGameState, qe, EWork.BuyWorkerWoodCutter, timeCost);

    }
}
*/