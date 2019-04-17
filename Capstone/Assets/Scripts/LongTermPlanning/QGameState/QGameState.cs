using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class QGameState {
    // The thing that gets put onto the priority queue           

    public QGameState parent;
    public Work transitionWork; // transitionWork represents the job done to move from parent to currentGS
                                // NOTE: This will be EMPTY if parent is null (ie if this QE is the root qe)

    public BuildingGS gameState;
    public int costToGetHere;

    public QGameState(BuildingGS gs, QGameState parent, Work transitionWork, int costToGetHere) {
        this.gameState = gs;
        this.parent = parent;
        this.transitionWork = transitionWork;
        this.costToGetHere = costToGetHere;
    }
}

// TODO: get this working
public abstract class BiDirectionalQEntry {
    protected BiDirectionalQEntry sourceEdge;
    protected BiDirectionalQEntry goalEdge;
    protected int sourceToNodeCost;
    protected int nodeToGoalCost;


    protected EWork sourceEdgeWork;
    public EWork nodeToDownStreamWork { get { return sourceEdgeWork; } }
    protected BuildingGS gameState;


    public abstract bool touchingSource();

}

public class GoalToSourceQEntry : BiDirectionalQEntry {

    public GoalToSourceQEntry(BuildingGS gs) { this.gameState = gs; }

    public override bool touchingSource() { return false; }
    
    private void updateParent(GoalToSourceQEntry newParent) {
        this.sourceEdge = newParent;
    }

    public void updateChild(GoalToSourceQEntry newChild, int costOfEdge) {
        this.goalEdge = newChild;
        this.nodeToGoalCost = newChild.nodeToGoalCost + costOfEdge;

        newChild.updateParent(this);
    }
}

// This wont work because every gameState has multiple children/ parents
public class SourceToGoalQEntry : BiDirectionalQEntry {

    public SourceToGoalQEntry(BuildingGS gs) { this.gameState = gs; }

    public override bool touchingSource() {
        return true;
    }

    public void updateParent(SourceToGoalQEntry newParent, int costOfEdge) {
        this.sourceEdge = newParent;
        this.sourceToNodeCost = newParent.sourceToNodeCost + costOfEdge;

        newParent.updateChild(this);
    }

    private void updateChild(SourceToGoalQEntry newChild) {
        this.goalEdge = newChild;
    }

}