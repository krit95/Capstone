using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// TODO: Impliment a GameState object pooler
public class BuildingGS {
    // Keeping track of what type of buildings exist is important
    // for when we want to redistribute workers and- or destroy buildings

    protected Dictionary<BuildingType, List<IBuilding>> buildings; // All the building this game state has sorted by their type
                                                                   // NOTE: You should never really add to this directly. 
                                                                   // Please use forceAddBuilding() function instead

    protected Dictionary<BuildingType, Queue<IBuilding>> openSpots; // Any building that is in this dictionary should have at least 1 open slot

    protected Dictionary<ResourceType, int> resourceStockpile;
    protected int resourcesSpent = 0; // Used to track total resources subtracted out of the game state
                                      // Mostly only used when comparing gamestates in A* planner
    public int totalResourcesSpent { get { return this.resourcesSpent; } }

    protected ResourceMap resourceMap; // All of the resources for this gamestate, things like fertil land or coal outcroppings

    private bool validRPT = false;
    private Dictionary<ResourceType, int> _resourceChangePerTick;
    protected Dictionary<ResourceType, int> resourceChangePerTick {
        get {
            if (!validRPT) { buildCPT(); }
            return _resourceChangePerTick;
        }
    }

    private Dictionary<ResourceType, int> _bestPossibleChangePerTick;
    protected Dictionary<ResourceType, int> bestPossibleChangePerTick {
        get {
            if (!validRPT) { buildCPT(); }
            return _bestPossibleChangePerTick;
        }
    }


    // TODO: Remove this constructor
    public BuildingGS() {
        this.buildings = new Dictionary<BuildingType, List<IBuilding>>();
        this.resourceStockpile = new Dictionary<ResourceType, int>();
        this.openSpots = new Dictionary<BuildingType, Queue<IBuilding>>();
        this.resourceMap = new ResourceMap();
        this._resourceChangePerTick = new Dictionary<ResourceType, int>();
    }

    public BuildingGS(IEnumerable<ResourceChange> startingResources) {
        this.buildings = new Dictionary<BuildingType, List<IBuilding>>();
        this.resourceStockpile = new Dictionary<ResourceType, int>();
        this.openSpots = new Dictionary<BuildingType, Queue<IBuilding>>();
        this.resourceMap = new ResourceMap();
        this._resourceChangePerTick = new Dictionary<ResourceType, int>();

        foreach (ResourceChange rc in startingResources) {
            this.resourceStockpile.Add(rc.resourceType, rc.change);
        }
    }

    // Copy constructor
    public BuildingGS(BuildingGS other) {
        // NOTE: This will deepClone all the buildings in the provided other (IE: The populations will be coppied)
        // TODO: Do we really need to deep copy all the buildings? 
        this.buildings = new Dictionary<BuildingType, List<IBuilding>>();
        this.resourceStockpile = new Dictionary<ResourceType, int>();
        this.openSpots = new Dictionary<BuildingType, Queue<IBuilding>>();
        this.resourceMap = new ResourceMap();
        this._resourceChangePerTick = new Dictionary<ResourceType, int>();

        // Buildings
        foreach (KeyValuePair<BuildingType, List<IBuilding>> kvp in other.buildings) {
            foreach (IBuilding otherBuilding in kvp.Value) {
                // Clone and add every building to this game state
                // TODO: Do we have to clone? If we have units assigned then probably yes... 
                IBuilding buildingClone = otherBuilding.deepClone();
                this.forceAddBuilding(buildingClone);
            }
        }

        // Stockpile
        foreach (KeyValuePair<ResourceType, int> kvp in other.resourceStockpile) {
            this.addToStockpile(kvp.Key, kvp.Value);
        }

        // Resources per tick will be handled by adding buildings

        this.resourcesSpent = other.resourcesSpent;
    }


    // Main simulator
    public void timePasses(int numberOfTicks) {
        validRPT = false; // Reset the change per tick values
        // Run every building and if the building can't afford the upkeep cost then it doesn't produce anything this tick
        foreach (KeyValuePair<BuildingType, List<IBuilding>> kvp in this.buildings) {
            BuildingType bt = kvp.Key;
            foreach (IBuilding building in kvp.Value) {
                if (canAffordUpkeep(building)) {
                    // NOTE: building.simulate returns both output(+) and input(-) resources in 
                    //       the provided List<ResourceChange> objects
                    this.addToStockpile(building.simulate(numberOfTicks));
                }
                // Else can't afford upkeep means move to next building
            }
        }
    }


    #region accessors

    public int getStockpile(ResourceType rt) {
        if (this.resourceStockpile.ContainsKey(rt)) { return this.resourceStockpile[rt]; }
        return 0;
    }

    public int getChangePerTick(ResourceType rt) {
        if (!this.resourceChangePerTick.ContainsKey(rt)) { return 0; }
        return this.resourceChangePerTick[rt];
    }

    public int getBestPossibleChangePerTick(ResourceType rt) {
        if (!this.bestPossibleChangePerTick.ContainsKey(rt)) { return 0; }
        return this.bestPossibleChangePerTick[rt];
    }

    private void buildCPT() {
        // First, calculate the Exact change per tick by simulating the game state
        Dictionary<ResourceType, int> stockpileBackup = new Dictionary<ResourceType, int>(this.resourceStockpile);
        timePasses(1);
        foreach (ResourceType key in this.resourceStockpile.Keys) {
            int currentVal = stockpileBackup.ContainsKey(key)   ? stockpileBackup[key]   : 0;
            int futureVal  = resourceStockpile.ContainsKey(key) ? resourceStockpile[key] : 0;
            int difference = futureVal - currentVal;
            _resourceChangePerTick[key] = difference;
        }
        this.resourceStockpile = stockpileBackup;

        // Determine the best possible output of this game state
        // IE: all buildings maxed out workers
        // Used for A* to determing more tie breaker cases
        this._bestPossibleChangePerTick = new Dictionary<ResourceType, int>();
        foreach(List<IBuilding> buildings in this.buildings.Values) {
            // Go through all the buildings
            if (buildings.Count == 0) { continue; }
            foreach (IBuilding b in buildings) {
                foreach(ResourceChange output in b.bestPossibleOutputResourceProduction()) {
                    // Go through every output quantity (in the best case) for this building
                    if (!_bestPossibleChangePerTick.ContainsKey(output.resourceType))
                        { _bestPossibleChangePerTick[output.resourceType] = 0; }

                    _bestPossibleChangePerTick[output.resourceType] += output.change;
                }
            }
        }

        validRPT = true;
    }

    public HashSet<ResourceType> getCPTResourceTypes() {
        return new HashSet<ResourceType>(this.resourceChangePerTick.Keys);

    }

    public HashSet<ResourceType> getStockpileResourceTypes() {
        return new HashSet<ResourceType>(this.resourceStockpile.Keys);
    }

    public HashSet<ResourceType> getAllResourceTypes() {
        HashSet<ResourceType> result = getStockpileResourceTypes();
        result.UnionWith(getCPTResourceTypes());
        return result;
    }

    public HashSet<ResourceType> slotsForIncome() {
        // Returns every resource we are/ can currently make if we had more workers
        HashSet<ResourceType> result = new HashSet<ResourceType>(this.resourceChangePerTick.Keys);
        foreach (IBuilding openBuildings in this.getOpenSlotBuildings()) {
            foreach (ResourceType rt in openBuildings.outputResources()) {
                result.Add(rt);
            }
        }
        return result;
    }

    private IEnumerable<IBuilding> getOpenSlotBuildings() {
        // Get all the buildings with open slots
        List<IBuilding> result = new List<IBuilding>();
        foreach (KeyValuePair<BuildingType, Queue<IBuilding>> kvp in this.openSpots) {
            if (this.anyOpenSlots(kvp.Key)) {
                result.Add(kvp.Value.Peek());
            }
        }
        return result;
    }

    public List<BuildingType> getOpenSlots() {
        // Get all the types of buildings with open slots
        List<BuildingType> result = new List<BuildingType>(this.openSpots.Count);
        foreach(KeyValuePair<BuildingType, Queue<IBuilding>> kvp in this.openSpots) {
            if (this.anyOpenSlots(kvp.Key)) {
                result.Add(kvp.Key);
            }
        }
        return result;
    }

    public bool anyOpenSlots(BuildingType bt) {
        return (this.getAnyOpenBuilding(bt) != null);
    }

    public int totalOpenSlots() {
        int result = 0;
        foreach(var kvp in openSpots) {
            Queue<IBuilding> buildings = kvp.Value;
            foreach(IBuilding b in buildings) {
                result += b.openWorkerSlots();
            }
        }
        return result;
    }

    public IBuilding getAnyOpenBuilding(BuildingType bt) {
        // Get an instance of a building (of the provided type) that has an open slot, or null
        if (!this.openSpots.ContainsKey(bt)) { return null; } // No key, no slots

        while (this.openSpots[bt].Count > 0) { // Empty queue => no slots
            IBuilding possibleBuild = this.openSpots[bt].Peek();
            if (possibleBuild.openWorkerSlots() <= 0) {
                // If a building is in here with no slots remove it
                this.openSpots[bt].Dequeue();
            } else {
                return possibleBuild;
            }
        }
        return null;
    }
    
    public int totalBuildingCount() {
        int result = 0;
        foreach(var kvp in this.buildings) {
            result += kvp.Value.Count;
        }
        return result;
    }

    public int totalWorkerCount() {
        int result = 0;
        foreach (var kvp in this.buildings) {
            foreach(IBuilding b in kvp.Value) {
                result += b.currentWorkers();
            }
        }
        return result;
    }
    #endregion

    #region Modifying Buildings

    public bool canAffordUpkeep(IBuilding building) {
        // Returns true if the all resources stay positive after paying the upkeep costs of 
        // the provided building
        foreach(ResourceChange rc in building.inputResourceCost()) {
            if (!this.resourceStockpile.ContainsKey(rc.resourceType)) {
                // No stockpile key => no resources
                return false;
            }

            if (this.resourceStockpile[rc.resourceType] < rc.change) {
                // If we have less than the required amout to pay upkeep
                return false;
            }
        }
        return true;
    }

    public bool canAffordBuilding(BuildingType bt) {
        // Check initial build costs
        foreach (ResourceChange rc in BuildingFactory.allBluePrints[bt].buildCost) {
            if (!this.resourceStockpile.ContainsKey(rc.resourceType) ||
                this.resourceStockpile[rc.resourceType] < rc.change) {
                // If  we don't have the right key or 
                // our current stockpile is less than the price
                return false;
            }
        }
        return true;
    }

    public bool canBuyBuilding(IBuilding possibleBuilding) {
        // TODO: in the future, buildings may cost differently depending on where they're built etc.
        bool result =  canAffordBuilding(possibleBuilding.getBuildingType());  // Stockpile considerations
        result      &= canAffordUpkeep(possibleBuilding);  // per tick upkeep considerations

        PositionDependentBuilding posBuilding = possibleBuilding as PositionDependentBuilding;
        if (posBuilding != null) {
            // Can the provided building be built in this game state? 
            result &= posBuilding.canBuild(this);
        }
        return result;
    }
    
    public void buyBuilding(IBuilding newBuilding) {
        // Note this function does no validation checking.
        // IE: going negative is ok, so a check must be made before calling this function

        // First, change the stockpiles accordingly by subtracting the cost of the building
        this.spendForBuilding(newBuilding);

        // Then add the building and it's resource per tick count to this game state
        this.forceAddBuilding(newBuilding);
    }

    public void spendForBuilding(IBuilding newBuilding) {
        // Spend the money on the new building
        // No validation checking is done
        this.subtractFromStockpile(newBuilding.costToBuild());
    }

    public void forceAddBuilding(IBuilding newBuilding) {
        // Add the provided building to the collection
        // AND updates this game state's resourceChangePerTick accordingly

        // This is probably the only place that we add a building directly
        BuildingType newBT = newBuilding.getBuildingType();
        if ( ! this.buildings.ContainsKey(newBT)) { this.buildings[newBT] = new List<IBuilding>(); }
        this.buildings[newBT].Add(newBuilding);
        
        // Update the income per tick of this game state
        foreach (ResourceChange rc in newBuilding.changePerTick()) {
            this.addResourcePerTick(rc.resourceType, rc.change);
        }
        
        if (newBuilding.openWorkerSlots() > 0) {
            // If there's open slots
            
            // Add the open slots to the queue
            if (!this.openSpots.ContainsKey(newBT)) { this.openSpots[newBT] = new Queue<IBuilding>(); }
            this.openSpots[newBT].Enqueue(newBuilding);
        }
    }

    #endregion

    #region Modifying Workers
    
    public bool canBuyWorker() {
        foreach(ResourceChange rc in WorkerFactory.WORKER_COST) {
            if (this.resourceStockpile[rc.resourceType] < rc.change)  {
                return false;
            }
        }
        return true;
    }
    
    public void forceBuyWorker() {
        this.subtractFromStockpile(WorkerFactory.WORKER_COST);
    }

    public void assignWorker(IBuilding targetBuilding) {
        // Assing a worker to the provided building
        // NOTE: The game state doesn't really care about where the worker came from, 
        //       IE it will assume the cost for a worker has already been paid


        // subtract out what this building provides to the income
        foreach (ResourceChange rc in targetBuilding.changePerTick())
            { this.resourceChangePerTick[rc.resourceType] -= rc.change; }

        // add a worker to the building
        targetBuilding.addWorker();

        // Add back all the resources this building now provides
        foreach (ResourceChange rc in targetBuilding.changePerTick())
            { this.resourceChangePerTick[rc.resourceType] += rc.change; }
    }
    
    public void unassignWorker(IBuilding targetBuilding) {
        foreach (ResourceChange rc in targetBuilding.changePerTick())
        { this.resourceChangePerTick[rc.resourceType] -= rc.change; }

        // add a worker to the building
        targetBuilding.removeWorker();

        // Add back all the resources this building now provides
        foreach (ResourceChange rc in targetBuilding.changePerTick())
        { this.resourceChangePerTick[rc.resourceType] += rc.change; }
    }

    // TODO: This seems like it's only really used by the LongTermPlanner
    public void buyAndAssignWorker(BuildingType bt) {
        // purchace and assign a worker to the provided building type
        if ( ! this.canBuyWorker() ||
             ! this.openSpots.ContainsKey(bt) ||
            this.openSpots[bt].Count <= 0) {
            // If we have no open spots for the specified building
            throw new System.Exception();
            return;
        }
        // Else we have the gold and we have the spot 

        this.forceBuyWorker();
        forceAddWorker(bt);
    }

    // Todo this seems like it is only used in the LongTermPlanner? 
    public void forceAddWorker(BuildingType bt) {
        if (this.openSpots.ContainsKey(bt) &&
            this.openSpots[bt].Count > 0) {
            // Look at the head of the queue
            IBuilding b = this.openSpots[bt].Peek();

            if (b.openWorkerSlots() <= 0) {
                throw new System.Exception("Some building in openSpots was full!!");
            }

            // Update the rpt
            assignWorker(b);

            if (b.openWorkerSlots() <= 0) {
                // If we've filled the building then remove it
                this.openSpots[bt].Dequeue();
            }

            return;
        }
        else {
            // Else we have no open slots for the person
            throw new System.Exception("Tried to add a worker to a building type that has no empty slots. BuildingType:" + Enum.GetName(typeof(BuildingType), bt));
        }
    } // End forceAddWorker();


    #endregion

    #region Modifying Resources
    
    private void addToStockpile(IEnumerable<ResourceChange> resources) { foreach (ResourceChange rc in resources) { addToStockpile(rc); } }
    private void addToStockpile(ResourceChange rc) { changeResources(rc, true); }
    private void addToStockpile(ResourceType rt, int change) { changeResources(rt, change, true); }

    private void subtractFromStockpile(IEnumerable<ResourceChange> resources) { foreach (ResourceChange rc in resources) { subtractFromStockpile(rc); } }
    private void subtractFromStockpile(ResourceChange rc) { changeResources(rc, false); }
    private void subtractFromStockpile(ResourceType rt, int change) { changeResources(rt, change, false); }

    private void changeResources(ResourceChange rc, bool addTo) {
        changeResources(rc.resourceType, rc.change, addTo);
    }
    private void changeResources(ResourceType rt, int change, bool addTo) {
        if (!this.resourceStockpile.ContainsKey(rt))
            { this.resourceStockpile[rt] = 0; }

        if (addTo) { this.resourceStockpile[rt] += change; }
        else       {
            this.resourceStockpile[rt] = Mathf.Max(0, this.resourceStockpile[rt] - change);
            this.resourcesSpent += change;
        }
    }

    // Resource Per Tick
    public void addResourcePerTick(ResourceType rt, int addition) {
        if (!this.resourceChangePerTick.ContainsKey(rt))
        { this.resourceChangePerTick[rt] = 0; }

        this.resourceChangePerTick[rt] = Mathf.Max(0, this.resourceChangePerTick[rt] + addition);
    }

    public void setStockpile(ResourceType rt, int newCount) {
        // NOTE: This should only ever be used for testing reasons
        this.addToStockpile(new ResourceChange(rt, newCount)); 
    }


    #endregion

    #region Modifying Resource Nodes

    public List<IResourceNode> getResourceNodes(Vector2Int pos) {
        // All IResourceNode objects at the provieded tile
        return this.resourceMap.getNodes(pos);
    }
    public List<IResourceNode> getResourceNodes(Vector2Int pos, ResourceType rt) {
        // All IResourceNode objects related to the provided type at the provided tile
        return this.resourceMap.getNodes(pos, rt);
    }

    public void addResourceNode(IResourceNode newNode, Vector2Int pos) {
        this.resourceMap.addNode(newNode, pos);
    }

    #endregion

    public override int GetHashCode() {
        // Two game States are equal if they have:
        // - the same stockpiles
        // - the same income per tick
        
        // TODO: Consider number of buildings an important field
        // TODO: Consider adding "free-land remaining" to this hash
        int hash = 17;

        // Set up the resource change per tick values. 
        // This ensures no "resourceStockpile was modified during loop" error
        if (!validRPT) { buildCPT(); }

        foreach (KeyValuePair<ResourceType, int> kvp in this.resourceStockpile) {
            int resourceChangePerTick = this.getChangePerTick(kvp.Key);
            hash = (hash * 23) + Convert.ToInt32(kvp.Key);
            hash = (hash * 23) + kvp.Value;
            hash = (hash * 23) + resourceChangePerTick;
        }
        return hash;
    }

    public override bool Equals(object obj) {
        /**
         * Two Game States are equal if they have 
         * - same resource stockpile
         * - same resource income per tick
         */

        // TODO: Consider number of buildings an important field? Maybe how much free land is remaining? 

        BuildingGS otherGS = obj as BuildingGS;
        if (otherGS == null) { return false; }

        // Different number of fields => fail
        if (this.resourceStockpile.Count != otherGS.resourceChangePerTick.Count ||
            this.resourceChangePerTick.Count != otherGS.resourceChangePerTick.Count)
        {
            return false;
        }

        // Check type of resources
        int otherResourceCount;
        foreach (KeyValuePair<ResourceType, int> kvp in this.resourceStockpile) {
            if ( ! otherGS.resourceStockpile.TryGetValue(kvp.Key, out otherResourceCount)) {
                // Try to access a given key in the other dictionary
                // If it fails, return false
                return false;
            }
            if (kvp.Value != otherResourceCount) {
                // If the key is contained, but the stockpile count is different
                return false;
            }
            
            if (this.resourceChangePerTick[kvp.Key] != otherGS.resourceChangePerTick[kvp.Key]) {
                // If the change per tick is different
                return false;
            }

        }
        return true;
    }
}


// TODO: This might be broken in the long term planner 
//       later notes: Not sure what might be broken except possibly the clone functions returning the non-roundable stuff
public class RoundableBuildingGameState : BuildingGS {
    protected int percision = 4;

    public RoundableBuildingGameState() : base() { }
    public RoundableBuildingGameState(int percision) : base()  {
        this.percision = percision;
    }
    public RoundableBuildingGameState(IEnumerable<ResourceChange> startingResources, int percision) : base(startingResources) {
        this.percision = percision;
    }
    
    public override int GetHashCode() {
        // Two game States are equal if they have:
        // - the same stockpiles
        // - the same income per tick

        // TODO: Consider number of buildings an important field
        // TODO: Consider adding "free-land remaining" to this hash
        int hash = 17;
        foreach (KeyValuePair<ResourceType, int> kvp in this.resourceStockpile) {
            int resourceChangePerTick = this.resourceChangePerTick[kvp.Key];
            hash = (hash * 23) + Convert.ToInt32(kvp.Key);
            hash = (hash * 23) + MathHelp.sigfigify(kvp.Value, this.percision);
            hash = (hash * 23) + MathHelp.sigfigify(resourceChangePerTick, this.percision);
        }
        return hash;
    }

    public override bool Equals(object obj) {
        /**
         * Two Game States are equal if they have 
         * - same resource stockpile
         * - same resource income per tick
         */

        // TODO: Consider number of buildings an important field? Maybe how much free land is remaining? 

        RoundableBuildingGameState otherGS = obj as RoundableBuildingGameState;
        if (otherGS == null) { return false; }

        // Different number of fields => fail
        if (this.resourceStockpile.Count != otherGS.resourceChangePerTick.Count ||
            this.resourceChangePerTick.Count != otherGS.resourceChangePerTick.Count) {
            return false;
        }

        // Check type of resources
        int otherResourceCount;
        foreach (KeyValuePair<ResourceType, int> kvp in this.resourceStockpile) {
            if (!otherGS.resourceStockpile.TryGetValue(kvp.Key, out otherResourceCount)) {
                // Try to access a given key in the other dictionary
                // If it fails, return false
                return false;
            }

            int thisStockpileSigFig = MathHelp.sigfigify(kvp.Value, this.percision);
            int otherStockpileSigFig = MathHelp.sigfigify(otherResourceCount, this.percision);
            if (thisStockpileSigFig != otherStockpileSigFig) {
                // If the key is contained, but the stockpile count is different
                return false;
            }

            int this_RPT_SigFig = MathHelp.sigfigify(this.resourceChangePerTick[kvp.Key], this.percision);
            int other_RPT_SigFig = MathHelp.sigfigify(otherGS.resourceChangePerTick[kvp.Key], this.percision);
            if (this_RPT_SigFig != other_RPT_SigFig) {
                // If the change per tick is different
                return false;
            }

        }
        return true;
    }

}
