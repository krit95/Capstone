﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PriorityQueueDemo;

public class LongTermPlanning {

    private static readonly int WAIT_TIME = 10;
    private static readonly int MAX_DEPTH = 5000000; // 5M searches


    private int compareGameState(GameState currentGS, GameState targetGS)
    {
        // How many time cycles will it take if we do nothing? 
        int goldDelta = Mathf.Max(0, targetGS.gold - currentGS.gold);
        int goldWaitTime = goldDelta / currentGS.goldPerTick;

        int GPTDelta = Mathf.Max(0, targetGS.goldPerTick - currentGS.goldPerTick);

        return goldWaitTime + GPTDelta;

    }

    private HashSet<GameStateTransition> getNeighbors(GameState gs) {
        // For a given game state return all valid edges out of it

        HashSet<GameStateTransition> result = new HashSet<GameStateTransition>();
        result.Add(waitTransition(gs, WAIT_TIME));

        if (canBuildBank(gs)) {
            // If this game state can build a bank then that's an option
            result.Add(buildBank(gs));
        }
        return result;
    }
    

    public Queue<Work> plan(GameState initialGS, GameState targetGS)
    {
        // TODO: This function is the exact same as the AStart.getPath() function

        QEntry currentQE = BuildPlan(initialGS, targetGS);
        Debug.Log("Final game state gold: " + currentQE.gameState.gold);
        Debug.Log("Final game state gpt: " + currentQE.gameState.goldPerTick);
        Debug.Log("Total Time: " + currentQE.costToGetHere);

        // TODO: using double ended queues may be more efficent
        List<Work> tempList = new List<Work>();

        while (currentQE != null) {
            tempList.Add(currentQE.transitionWork);
            currentQE = currentQE.parent;
        }

        tempList.Reverse();
        Queue<Work> result = new Queue<Work>(tempList);
        return result;

    }


    private QEntry BuildPlan(GameState initialGS, GameState targetGS) {

        PriorityQueue<int, QEntry> priorityQueue = new PriorityQueue<int, QEntry>();

        Dictionary<GameState, int> bestCostToGetHere = new Dictionary<GameState, int>();


        QEntry startQE = new QEntry(initialGS, null, Work.EMPTY, 0);
        int heuristic = compareGameState(initialGS, targetGS);
        priorityQueue.Enqueue(heuristic, startQE);

        int totalChecks = 0;

        while (priorityQueue.Count > 0) {

            totalChecks++;

            QEntry qe = priorityQueue.DequeueValue();

            if (this.compareGameState(qe.gameState, targetGS) <= 0)
            {
                // If we are 0 distance away from the target game state
                // IE: If we have found the target game state
                return qe;
            }

            if (totalChecks > MAX_DEPTH) {
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


            foreach(GameStateTransition gameStateTransition in getNeighbors(qe.gameState)) {
                GameState neighborGameState = gameStateTransition.gameState;
                int edgeCost = gameStateTransition.time;

                int totalCostToExplorNeighbor = qe.costToGetHere + edgeCost;

                if (bestCostToGetHere.ContainsKey(neighborGameState) &&
                    bestCostToGetHere[neighborGameState] <= totalCostToExplorNeighbor) {
                    // If we already have a better way to get to the neighbor
                    continue;
                }

                QEntry neighborQE = new QEntry(neighborGameState, qe, gameStateTransition.job, totalCostToExplorNeighbor);

                heuristic = compareGameState(neighborGameState, targetGS) + totalCostToExplorNeighbor;
                priorityQueue.Enqueue(heuristic, neighborQE);

            }

        } // End while queue is NOT empty
        return null;

    }

    private class QEntry {
        public QEntry parent;
        public Work transitionWork; // transitionWork represents the job done to move from parent to currentGS
                                    // NOTE: This will be EMPTY if parent is null (ie if this QE is the root qe)

        public GameState gameState;
        public int costToGetHere;

        public QEntry(GameState gs, QEntry parent, Work transitionWork, int costToGetHere) {
            this.gameState = gs;
            this.parent = parent;
            this.transitionWork = transitionWork;
            this.costToGetHere = costToGetHere;
        }
    }

    private class GameStateTransition
    {
        /**
         * To represent the transition into this.gamestate
         * The only GameState refrence in this GameStateTransition object represents what is at the end of this "edge"
         * This transition has no idea, nore does it care, about the GameState that it started from
         * 
         * The time field represents the cost to traverse the edge
         */

        public GameState gameState;
        public Work job;
        public int time;

        public GameStateTransition(GameState endGS, Work job, int time) {
            this.gameState = endGS;
            this.job = job;
            this.time = time;
        }

    }


    ////////////////////////////////////////////////

    // This function represents an edge
    private static GameStateTransition waitTransition(GameState gs, int time) {
        GameState endGS = waitGameState(gs, time);
        return new GameStateTransition(endGS, Work.Wait, time);
    }

    private static GameState waitGameState(GameState gs, int time) {
        GameState endGS = new GameState(gs);
        endGS.gold += gs.goldPerTick * time;
        endGS.stone += gs.stonePerTick * time;
        return endGS;
    }

    //////////////////////////////////////////////////

    private static readonly int bankGoldCost = 10;
    private static readonly int bankBuildTime = 1;
    private static readonly int bankGPTChange = 10;
    private static bool canBuildBank(GameState gs) {
        return gs.gold >= bankGoldCost;
    }

    private static GameStateTransition buildBank(GameState gs) {
        GameState endGS = waitGameState(gs, bankBuildTime);
        endGS.gold -= bankGoldCost;
        endGS.goldPerTick += bankGPTChange;

        return new GameStateTransition(endGS, Work.BuildBank, bankBuildTime);
    }

}


public enum Work {
    Wait,
    BuildBank,

    EMPTY // To represent a "null" value of work which is different than Wait
}


public class GameState {
    /**
     * To represent the current resources and expected income of a game world
     */

    public int gold;
    public int goldPerTick;

    public int stone;
    public int stonePerTick;


    public GameState() { }

    public GameState(GameState gs)
    {
        this.gold = gs.gold;
        this.goldPerTick = gs.goldPerTick;

        this.stone = gs.stone;
        this.stonePerTick = gs.stonePerTick;
    }

    public override int GetHashCode() {
        int hash = 17;
        hash = hash * 23 + (this.gold);
        hash = hash * 23 + (this.goldPerTick);
        hash = hash * 23 + (this.stone);
        hash = hash * 23 + (this.stonePerTick);
        return hash;

    }

    public override bool Equals(object obj) {
        // Two Game States are equal if all their fields are the same
        // TODO: To simplify things, perhapse we should round the fields down to the nearest 10s place? Depending on scale? 
        GameState otherGS = obj as GameState;
        if (otherGS == null) { return false; }

        return this.gold == otherGS.gold &&
            this.goldPerTick == otherGS.goldPerTick &&
            this.stone == otherGS.stone &&
            this.stonePerTick == otherGS.stonePerTick;

    }
}


