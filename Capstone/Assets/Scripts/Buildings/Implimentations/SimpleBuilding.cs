using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


public interface IBuilding  {
    /**
     * A building is an object that always produces some resource per tick
     *      (IE: The output resource will always be positive and greater than 0)
     *      
     * A building may have the folowing qualities
     *  - incoming resources to start production 
     *      (IE output resource = 1 steel, will require incoming resource = (1 iron + 2 coal) )
     *  - a worker cap
     *      IE, how many workers can work in this building
     *      No worker cap, or 0 >= cap implies infinite workers can work in this building
     */


    // The type and amount of resources it takes to build this building
    // NOTE: This should always be a positive value
    // IE ResourceChange.value > 0
    // if ResourceChange.value < 0 => you get the resource when the building is initially built
    List<ResourceChange> costToBuild();

    List<ResourceType> outputResources();
    List<ResourceChange> outputResourceProduction();
    List<ResourceChange> bestPossibleOutputResourceProduction();

    List<ResourceType> inputResources();
    List<ResourceChange> inputResourceCost();

    // This will return ALL the resources that will be changed by this building
    // over the given time perioud
    // The outputResources and input resources will be accounted for in the result
    // NOTE: normally input resources are represented as a positive value, but here they are represented negative
    List<ResourceChange> simulate(int time);

    // How much does this building change the resources per tick?
    // Think of the results of this function like asking for the buildings ResourcePerTick values for 
    //   all the related resources to this building.
    // NOTE: normally input resources are represented as a positive value, but here they are returned negative
    List<ResourceChange> changePerTick();

    // Clones this building, but the new building has no population
    IBuilding simpleClone();

    // Clones this building, but the new building has the same population as this building
    IBuilding deepClone();



    // Accessors
    TileBase buildingIcon();
    Vector2Int position();
    BuildingType getBuildingType();
    
    int currentWorkers();
    int openWorkerSlots();

    int timeToBuild();

    // Modifiers
    bool addWorker();
    bool removeWorker();
    
    // TODO: Claimed spots vs actually filled spots
}


public class SimpleBuilding : IBuilding {
    /**
     * A simple building that has 
     *  - only one output resource
     *  - no input resources
     *  - no population cap
     * 
     */
    private readonly BuildingType bt;

    private readonly ResourceType outputResource;
    private int currentPop;
    private int maxPop { get { return BuildingFactory.allBluePrints[this.bt].maxPop; } }
    
    private Vector2Int pos;

    #region Constructors

    // TODO: Clean this up. Maybe have a building factory class? 
    public SimpleBuilding(
        BuildingType bt,
        ResourceType outputResource,
        Vector2Int position)
    {
        this.bt = bt;
        this.outputResource = outputResource;
        this.pos = position;
    }
    
    private SimpleBuilding clone() {        
        // TODO: Consider, is having every clone point to the same buildCost object ok? 
        return new SimpleBuilding(
            this.bt,
            this.outputResource,
            new Vector2Int(pos.x, pos.y)
            );
    }
    
    public IBuilding simpleClone() {
        return this.clone();
    }
    
    public IBuilding deepClone() {
        SimpleBuilding result = this.clone();
        result.currentPop = this.currentPop;
        return result;
    }

    #endregion

    public List<ResourceChange> costToBuild() {
        return BuildingFactory.allBluePrints[this.bt].buildCost;
    }

    public List<ResourceType> inputResources() {
        // SimpleBuildings have no input resources
        return new List<ResourceType>();
    }
    public List<ResourceChange> inputResourceCost() {
        return new List<ResourceChange>();
    }

    public List<ResourceType> outputResources() {
        return new List<ResourceType>() { outputResource };
    }
    public List<ResourceChange> outputResourceProduction()
    {
        List<ResourceChange> result = new List<ResourceChange>();
        foreach (IResourceProducer producer in BuildingFactory.allBluePrints[this.bt].outputResourceProduction) {
            result.Add(producer.simulate(this.currentPop));
        }
        return result;
    }

    public List<ResourceChange> bestPossibleOutputResourceProduction() {
        List<ResourceChange> result = new List<ResourceChange>();
        foreach(IResourceProducer producer in BuildingFactory.allBluePrints[this.bt].outputResourceProduction) {
            result.Add(producer.simulate(this.maxPop));
        }
        return result;
    }



    public List<ResourceChange> simulate(int time) {
        // NOTE: SimpleBuildings only have an outputResource and no input resources
        List<ResourceChange> result = changePerTick();
        foreach (ResourceChange rc in result) {
            rc.change *= time;
        }
        return result;
    }

    public List<ResourceChange> changePerTick() {
        List<ResourceChange> result = new List<ResourceChange>();
        // Calculate outputs
        // For a simple builing there should only ever be 1 output
        foreach (IResourceProducer producer in BuildingFactory.allBluePrints[this.bt].outputResourceProduction) {
            ResourceChange prodSimulate = producer.simulate(this.currentPop);
            result.Add(prodSimulate);
        }

        // Calculate inputs
        // For a simple building there should never be inputs
        foreach (IResourceProducer costs in BuildingFactory.allBluePrints[this.bt].inputResourceCosts) {
            // Remember, Blueprint.inputResourceCosts should always be positive despite intuition
            ResourceChange costSimulate = costs.simulate(this.currentPop);
            costSimulate.change = -1 * costSimulate.change;
            result.Add(costSimulate);
        }
        return result;
    }

    #region Accessors
    public TileBase buildingIcon() { return BuildingFactory.getIcon(this.bt); }
    public Vector2Int position() { return this.pos; }
    public BuildingType getBuildingType() { return this.bt; }
    public int currentWorkers() { return this.currentPop; }
    public int openWorkerSlots() { return this.maxPop - this.currentPop; }
    public int timeToBuild() { return BuildingFactory.allBluePrints[this.bt].timeToBuild; }

    #endregion

    public bool addWorker() {
        if (this.maxPop <= this.currentPop) { return false; }
        this.currentPop++;
        return true;
    }

    public bool removeWorker()
    {
        if (this.currentPop <= 0) { return false; }
        this.currentPop--;
        return true;
    }
}

// TODO: Put this somewhere else
public class ResourceChange {
    /**
     * A simple container that describes a target resource and how it should change (up or down)
     */

    public ResourceType resourceType;
    public int change;

    public ResourceChange(ResourceType rt, int change) {
        this.resourceType = rt;
        this.change = change;
    }
}

