using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PriorityQueueDemo;

public class UnmodifiedLongTermPlanner : ALongTermPlanner {


    public override void plan(BuildingGS initialGS, BuildingGS targetGS, Func<Stack<Work>, bool> callback) {

        PriorityQueue<QPriority, QGameState> queue = new PriorityQueue<QPriority, QGameState>();
        StartCoroutine(LTPEngine.BuildPlan(initialGS, targetGS, queue, base.processResult));
        StartCoroutine(base.waitForFinish(callback));

    }
}
