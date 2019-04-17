using System.Collections;
using System.Collections.Generic;
using System;
using PriorityQueueDemo;

public class MemoryBoundedLongTermPlanner : ALongTermPlanner {
    

    public override void plan(BuildingGS initialGS, BuildingGS targetGS, Func<Stack<Work>, bool> callback) {
        memoryBoundedLTP(initialGS, targetGS, callback);
    }

    public void memoryBoundedLTP(BuildingGS initialGS,
                                 BuildingGS targetGS,
                                 Func<Stack<Work>, bool> callback,
                                 int memoryBound = 100000) {
        /**
         * Find a path from the initialGS to the targetGS. The resultant path (translted into an ordered list of Work)
         * will be pumped into the provided callback once it's ready. 
         */
        PriorityQueue<QPriority, QGameState> memoryBoundedQ = new MemoryBoundedPriorityQueue(memoryBound);
        StartCoroutine(LTPEngine.BuildPlan(initialGS, targetGS, memoryBoundedQ, base.processResult));
        StartCoroutine(base.waitForFinish(callback));
    }
}
