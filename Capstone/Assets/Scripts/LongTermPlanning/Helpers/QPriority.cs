using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class QPriority : IComparable {

    private HiddenRequirement hiddenReq;
    bool preCalculatedPreRecDelta = false;
    private int _reccomendedPreRecDelta;
    public int reccomendedPreRecDelta { get {
            if (!preCalculatedPreRecDelta) { calculatePreRecDelta(); }
            return _reccomendedPreRecDelta;
        } }

    private QGameState currentNode;
    public int costToGetHere { get { return this.currentNode.costToGetHere; } }

    private BuildingGS targetGS;
    public int unaquirableResourceCount { get { return LTPHelper.unaquirableResources(currentNode.gameState, targetGS); } }
    public int totalResourcesSpent { get { return currentNode.gameState.totalResourcesSpent; } }

    private RemainingDistance distanceRuler;
    public int numberOfInfinities { get { return distanceRuler.NumberOfInfinities; } }
    public int totalStockpileDelta{ get { return distanceRuler.TotalStockpileDelta; } }
    public int maxWaitTime        { get { return distanceRuler.MaxWaitTime; } }
    public int totalWaitTime      { get { return distanceRuler.TotalWaitTime; } }
    public int bestMaxWaitTime    { get { return distanceRuler.BestMaxWaitTime; } }
    public int totalCPTDelta      { get { return distanceRuler.TotalChangePerTickDelta; } }
    public int bestCPTDelta       { get { return distanceRuler.BestChangePerTickDelta; } }

    public int estTotalDist       { get { return this.costToGetHere + this.maxWaitTime; } }
    public bool wait              { get { return currentNode.transitionWork.workType == EWork.Wait; } }
    public bool assignWorker      { get { return currentNode.transitionWork.workType == EWork.BuyAndAssignWorker; } }


    public QPriority(QGameState node, BuildingGS targetGS, HiddenRequirement hiddenReq) {
        this.hiddenReq = hiddenReq;
        this.currentNode = node;
        this.targetGS = targetGS;
        this.distanceRuler = LTPHelper.estematedRemainingDistance(node.gameState, targetGS);
    }

    private void calculatePreRecDelta() {
        // Look through the remaining rpt delta
        // For each type of resource, how many pre recs do we have
        // Over the required amount 

        _reccomendedPreRecDelta = 0;

        // Figure out what rpt types are not up to snuff
        foreach(ResourceType actualTargetResource in distanceRuler.getRemainingResourceTypes()) {
             //Debug.Log("Considering " + actualTargetResource);

            int contribution = 0;
            // Find all the reccomended pre-recs for that resource
            foreach (var kvp in hiddenReq.getAllHiddenCosts(actualTargetResource)) {
                ResourceType reccomendedExtraResource = kvp.Key;
                int reccomendedExtraIncome = kvp.Value;
                //Debug.Log("           has hiddenRed: " + reccomendedExtraResource);

                // Calculate our actual extra income for the recomended resource
                int surplussIncome = Math.Max(0, currentNode.gameState.getChangePerTick(reccomendedExtraResource) - targetGS.getChangePerTick(reccomendedExtraResource));
                //Debug.Log("          surpluss = " + surplussIncome);
                //Debug.Log("          reccomended surplus: " + reccomendedExtraIncome);
                contribution += Math.Max(0, reccomendedExtraIncome - surplussIncome);
                //Debug.Log("          new individual contra: " + contribution);
            }
            //Debug.Log("Contribution : " + contribution);
            //Debug.Log("num of resouce: " + distanceRuler.specificCPTDistance(actualTargetResource));
            _reccomendedPreRecDelta += contribution * distanceRuler.specificCPTDistance(actualTargetResource);
            //Debug.Log("new preRecDelta: " + _reccomendedPreRecDelta);
        }
        //Debug.Log("End");
        preCalculatedPreRecDelta = true;
    }

    public int CompareTo(object obj) {
        QPriority other = obj as QPriority;
        if (other == null) { return -1; } // this thing is smaller than a non QPriority obj
        /**
         * -- Most important reasons --
         * fewer infinities should win
         * access to more resources should win
         * shorter remaining distance should win
         * faster velocity should win
         * less resources spent should win
         * -- least important reasons --
         * 
         */
         
        if (this.numberOfInfinities != other.numberOfInfinities)
            { return this.numberOfInfinities - other.numberOfInfinities; }
        if (this.unaquirableResourceCount != other.unaquirableResourceCount)
            { return this.unaquirableResourceCount - other.unaquirableResourceCount; }
        
        if (this.totalCPTDelta != other.totalCPTDelta)
            { return this.totalCPTDelta - other.totalCPTDelta; }
        // Figure out which one has less "hidden" pre-recs that are unsatisfied
        
        if (this.estTotalDist != other.estTotalDist)
            { return this.estTotalDist - other.estTotalDist; }

        // When nothing seems to be different, a assigning a worker operation always wins
        if (this.assignWorker != other.assignWorker) {
            if (this.assignWorker) { return -1; }
            else { return 1; }
        }

        if (this.bestMaxWaitTime != other.bestMaxWaitTime)
            { return this.bestMaxWaitTime - other.bestMaxWaitTime; }

        if (reccomendedPreRecDelta != other.reccomendedPreRecDelta)
            { return reccomendedPreRecDelta - other.reccomendedPreRecDelta; }

        if (this.bestCPTDelta != other.bestCPTDelta)
            { return this.bestCPTDelta - other.bestCPTDelta; }

        int myTotalBuildingCount = this.currentNode.gameState.totalBuildingCount();
        int otherTotalBuildingCount = other.currentNode.gameState.totalBuildingCount();
        if (myTotalBuildingCount != otherTotalBuildingCount)
            { return myTotalBuildingCount - otherTotalBuildingCount; }

        // When nothing seems to be different, a wait operation always wins
        if (this.wait != other.wait) {
            if (this.wait) { return -1; }
            else { return 1; }
        }

        // All things equal go for the cheaper option
        return this.totalResourcesSpent - other.totalResourcesSpent;

        /*
         * Examples:
         * 10 vs 7
         * 10 - 7  =  3  => 2nd entry goes first
         * 
         * 7  vs 10
         * 7  - 10 = -3  => 1st entry goes first
         */
    }
}
