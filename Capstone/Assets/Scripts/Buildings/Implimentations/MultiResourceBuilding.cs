using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MultiResourceBuilding : IBuilding {
    /**
     * To represent a building that takes in or ouputs multiple types of resources
     */
    
    protected readonly BuildingType bt;
    protected Vector2Int pos;
    protected int currentWorkerCount;

    protected BuildingBlueprint myBlueprint { get { return BuildingFactory.allBluePrints[this.bt]; } }
    protected int maxWorkerCount { get { return myBlueprint.maxPop; } }
    protected List<IResourceProducer> outputProduction { get { return myBlueprint.outputResourceProduction; } }
    protected List<IResourceProducer> inputCosts { get { return myBlueprint.inputResourceCosts; } }

    protected int buildTime { get { return myBlueprint.timeToBuild; } }

    #region Constructors

    public MultiResourceBuilding( BuildingType bt, Vector2Int position ) {
        this.bt = bt;
        this.pos = position;
    }
    
    private MultiResourceBuilding clone() {
        return new MultiResourceBuilding( this.bt, new Vector2Int(pos.x, pos.y) );
    }

    public IBuilding simpleClone() {
        return this.clone();
    }

    public IBuilding deepClone() {
        MultiResourceBuilding result = this.clone();
        result.currentWorkerCount = this.currentWorkerCount;
        return result;
    }

    #endregion

    #region Input/ Output
    public List<ResourceType> outputResources() {
        List<ResourceType> result = new List<ResourceType>();
        foreach (IResourceProducer producer in outputProduction) {
            result.Add(producer.targetResource());
        }
        return result;
    }
    public List<ResourceChange> outputResourceProduction() {
        // What exactly does this specific building produce in a single tick? 
        List<ResourceChange> result = new List<ResourceChange>();
        foreach(IResourceProducer producer in outputProduction) {
            result.Add(producer.simulate(this.currentWorkerCount));
        }
        return result;
    }
    
    public List<ResourceChange> bestPossibleOutputResourceProduction() {
        List<ResourceChange> result = new List<ResourceChange>();
        foreach (IResourceProducer producer in BuildingFactory.allBluePrints[this.bt].outputResourceProduction) {
            result.Add(producer.simulate(this.maxWorkerCount));
        }
        return result;
    }

    public List<ResourceType> inputResources() {
        List<ResourceType> result = new List<ResourceType>();
        foreach (IResourceProducer costs in inputCosts) {
            result.Add(costs.targetResource());
        }
        return result;
    }
    public List<ResourceChange> inputResourceCost() {
        // What exactly does this specific building eat up in a single tick? 
        // NOTE: The result value should alwasy be positive despite intuition saying otherwise
        List<ResourceChange> result = new List<ResourceChange>();
        foreach (IResourceProducer producer in inputCosts) {
            result.Add(producer.simulate(this.currentWorkerCount));
        }
        return result;
    }
    #endregion

    #region Simulations
    public List<ResourceChange> simulate(int time) {
        // NOTE: normally input resources are represented as a positive value, but here they are returned negative

        List<ResourceChange> result = this.changePerTick();
        foreach (ResourceChange rc in result) {
            // NOTE: This implimentation of IBuilding class does not have "building momentum"
            // IE: just because a building has been "alive" for longer does NOT mean it's efficency increases.
            // This allows us to simply multiply the time by the producer.simulate(...) func 
            rc.change *= time;
        }
        return result;
    }


    public List<ResourceChange> changePerTick() {
        // NOTE: normally input resources are represented as a positive value, but here they are returned negative

        // Calculate outputs
        List<ResourceChange> result = this.outputResourceProduction();

        // Calculate inputs
        foreach (ResourceChange cost in inputResourceCost()) {
            // Remember, Blueprint.inputResourceCosts should always be positive despite intuition
            cost.change *= -1;
            result.Add(cost);
        }
        return result;
    }
    #endregion

    #region Accessors
    public List<ResourceChange> costToBuild() {
        return BuildingFactory.allBluePrints[this.bt].buildCost;
    }

    public TileBase buildingIcon() { return BuildingFactory.getIcon(this.bt); }
    public Vector2Int position() { return this.pos; }
    public BuildingType getBuildingType() { return this.bt; }
    public int currentWorkers() { return this.currentWorkerCount; }
    public int openWorkerSlots() { return this.maxWorkerCount - this.currentWorkerCount; }

    public int timeToBuild() { return this.buildTime; }
    #endregion

    #region Workers
    public bool addWorker() {
        if (this.maxWorkerCount <= this.currentWorkerCount) { return false; }
        this.currentWorkerCount++;
        return true;
    }
    
    public bool removeWorker() {
        if (this.currentWorkerCount <= 0) { return false; }
        this.currentWorkerCount--;
        return true;
    }
    #endregion
}
