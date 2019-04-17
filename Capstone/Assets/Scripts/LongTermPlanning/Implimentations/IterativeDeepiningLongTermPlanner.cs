using System.Collections.Generic;
using UnityEngine;
using System;
using PriorityQueueDemo;

// NOTE: Because the LTPEngine does exit out after a certain number of node checks (at the time of writing exit after 500k node checks)
//       then it's possible that this IDLongTermPlanner will loop forever, eachtime increasing the detph but the engine quits before
//       finding a final path
public class IterativeDeepiningLongTermPlanner : ALongTermPlanner
{

    private IDPriorityQueue ID_Queue;

    private BuildingGS initialGS;
    private BuildingGS targetGS;
    private Func<Stack<Work>, bool> callback;


    public override void plan(BuildingGS initialGS, BuildingGS targetGS, Func<Stack<Work>, bool> callback) {
        ID_LTP(initialGS, targetGS, callback);
    }

    public void ID_LTP(BuildingGS initialGS,
                       BuildingGS targetGS,
                       Func<Stack<Work>, bool> callback,
                       int initialDepth = 100) {
        /**
         * Find a path from the initialGS to the targetGS. The resultant path (translted into an ordered list of Work)
         * will be pumped into the provided callback once it's ready. 
         */

        // First, save these incase pathfinding fails and we need to recur with larger depth
        this.initialGS = initialGS;
        this.targetGS = targetGS;
        this.callback = callback;

        this.ID_Queue = new IDPriorityQueue(initialDepth);
        StartCoroutine(LTPEngine.BuildPlan(initialGS, targetGS, ID_Queue, id_processResult));
        StartCoroutine(base.waitForFinish(callback));
    }

    private bool id_processResult(QGameState engineOutput) {
        if (engineOutput == null) {
            // If no path was found, increase the depth and try again
            ID_LTP(initialGS, targetGS, callback, ID_Queue.NextBount);
        } else {
            // Otherwise, we have a valid path so process as normal
            base.processResult(engineOutput);
        }
        return true;
    }
}
