using System.Collections;
using System.Collections.Generic;
using System;
using PriorityQueueDemo;

public class AccordionLongTermPlanner : ALongTermPlanner {
    public override void plan(BuildingGS initialGS, BuildingGS targetGS, Func<Stack<Work>, bool> callback) {
        accordionLTP(initialGS, targetGS, callback);
    }

    public void accordionLTP(BuildingGS initialGS,
                             BuildingGS targetGS,
                             Func<Stack<Work>, bool> callback,
                             int memoryBound = 20000) {
        PriorityQueue<QPriority, QGameState> accordionQ = new AccordionPriorityQueue(memoryBound);
        StartCoroutine(LTPEngine.BuildPlan(initialGS, targetGS, accordionQ, base.processResult));
        StartCoroutine(base.waitForFinish(callback));
    }
}
