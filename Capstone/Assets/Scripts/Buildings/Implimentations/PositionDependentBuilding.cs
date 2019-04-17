using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PositionDependentBuilding : MultiResourceBuilding {

    private BuildingGS gameState;
    private int extractionRatePerWorker;
    
    #region Constructors
    PositionDependentBuilding(BuildingType bt,
                              Vector2Int position,
                              BuildingGS gs,
                              int extractionRatePerWorker
                              ) : base(bt, position) {
        this.gameState = gs;
        this.extractionRatePerWorker = extractionRatePerWorker;
    }

    private PositionDependentBuilding clone(BuildingGS newGS) {
        return new PositionDependentBuilding(this.bt, new Vector2Int(pos.x, pos.y), newGS, this.extractionRatePerWorker);
    }

    public new IBuilding simpleClone(BuildingGS newGS) {
        return this.clone(newGS);
    }

    public new IBuilding deepClone(BuildingGS newGS) {
        PositionDependentBuilding result = this.clone(newGS);
        result.currentWorkerCount = this.currentWorkerCount;
        return result;
    }
    #endregion

    public bool canBuild(BuildingGS gameState) {
        // Can this building be built in the provided game state? 
        return getRelevantResourceNodes(gameState).Count > 0;
    }

    #region Overrides
    new public List<ResourceChange> outputResourceProduction() {
        List<ResourceChange> result = base.outputResourceProduction();

        // Only different than base is adding in the production from resource nodes
        // TODO: Consider making a version of this Building that doesn't even use the base class related Producer objects
        foreach(IResourceNode node in this.getRelevantResourceNodes()) {
            result.Add(this.extraction(node));
        }

        return result;
    }
    #endregion

    #region Utility
    private List<IResourceNode> getRelevantResourceNodes(BuildingGS gameState) {
        // How many un-claimed nodes that produce this.outputResources() exist in the map? 
        List<IResourceNode> result = new List<IResourceNode>();
        foreach (ResourceType rt in base.outputResources()) {
            result.AddRange(gameState.getResourceNodes(this.pos, rt));
        }
        return result;
    }

    private List<IResourceNode> getRelevantResourceNodes() { return getRelevantResourceNodes(this.gameState); }

    private ResourceChange extraction(IResourceNode resourceNode) {
        return new ResourceChange(
            resourceNode.resourceType(), 
            resourceNode.extract(extractionRatePerWorker * currentWorkerCount)
        );
    }
    #endregion

}
