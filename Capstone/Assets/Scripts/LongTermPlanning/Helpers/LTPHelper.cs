using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class LTPHelper {
    
    public static readonly int MAX_DEPTH = 500000; // 500K searches


    public static int unaquirableResources(BuildingGS currentGS, BuildingGS targetGS) {
        // Are there any resources that are impossible to get at this moment (ignoring workers)
        HashSet<ResourceType> unaquirableResources = targetGS.getCPTResourceTypes();
        unaquirableResources.ExceptWith(currentGS.slotsForIncome());
        return unaquirableResources.Count;
    }

    public static RemainingDistance estematedRemainingDistance(BuildingGS currentGS, BuildingGS targetGS) {
        // How far away are we from the target? 
        RemainingDistance result = new RemainingDistance();

        // Compare numerical distance
        foreach (ResourceType rt in targetGS.getAllResourceTypes()) {
            int currentStockpile = currentGS.getStockpile(rt);
            int targetStockpile = targetGS.getStockpile(rt);
            int stockpileDelta = Mathf.Max(0, targetStockpile - currentStockpile);
            result.updateStockpileDelta(stockpileDelta);


            int currentResourcePerTick = currentGS.getChangePerTick(rt);
            int targetResourcePerTick = targetGS.getChangePerTick(rt);
            int rptDelta = Mathf.Max(0, targetResourcePerTick - currentResourcePerTick);
            result.updateCPTDelta(rt, rptDelta);

            int currentBestResourcePerTick = currentGS.getBestPossibleChangePerTick(rt);
            int bestPossibleRPTDelta = Mathf.Max(0, targetResourcePerTick - currentBestResourcePerTick);
            result.updateBestPossibleCPTDelta(bestPossibleRPTDelta);

            if (currentResourcePerTick <= 0) { 
                result.addInfinity();
            } else {
                float exactWaitTime = stockpileDelta / (float)currentResourcePerTick;
                int estWaitTime = (int)(exactWaitTime + 0.5f);
                result.updateWaitTime(estWaitTime);
                
                float bestPossibleWaitTime = stockpileDelta / (float)currentBestResourcePerTick;
                int estBestWaitTime = (int)(bestPossibleWaitTime + 0.5f);
                result.updateBestPossibleWaitTime(estBestWaitTime);
            }
        }
        return result;
    }

    public static HashSet<QGameState> getNeighbors(QGameState qEntry) {
        // For a given game state return all valid edges out of it

        HashSet<QGameState> result = new HashSet<QGameState>();
        BuildingGS gs = qEntry.gameState;
        
        // Branches related to workers
        if (gs.canBuyWorker()) {
            // If we have the resources to build a new worker
            foreach (BuildingType bt in gs.getOpenSlots()) {
                // One branch for every possible type of worker slot we can fill
                QGameState neighbor = QGameStateFactory.buyWorker(qEntry, bt);
                result.Add(neighbor);
            }
        }

        // The length of all the no-op edges we want to consider
        HashSet<int> waitTimes = new HashSet<int>() { 10 };

        // Branches related to Buildings
        // TODO: Why build a building if we can't populate it with a worker? 
        foreach (BuildingType bt in BuildingFactory.allBuildings) {
            // One branch for every new possible building
            IBuilding possibleBuilding = BuildingFactory.buildNew(bt, -1, -1); // TODO: do we care about pos when doing A*? 
            if (gs.canBuyBuilding(possibleBuilding)) {
                // If we can build this building, then add a branch
                QGameState neighbor = QGameStateFactory.buyBuilding(qEntry, possibleBuilding);
                result.Add(neighbor);
            }
        }

        // Add in some no-op edges
        foreach (int waitTime in waitTimes) {
            result.Add(QGameStateFactory.waitTransition(qEntry, waitTime));
        }

        return result;
    }
}

public class HiddenRequirement {
    // A mapping of Resource type to it's potential hidden cost per turn pre-reqs

    private Dictionary<ResourceType, BuildingRequirements> allRequirements;
    private Dictionary<ResourceType, Dictionary<ResourceType, int>> allHiddenRequirementsCache; // To cache old work

    public HiddenRequirement() {

        this.allRequirements = new Dictionary<ResourceType, BuildingRequirements>();
        this.allHiddenRequirementsCache = new Dictionary<ResourceType, Dictionary<ResourceType, int>>();

        foreach (KeyValuePair<BuildingType, BuildingBlueprint> kvp in BuildingFactory.allBluePrints) {
            // Go through all the buildings
            BuildingType bt = kvp.Key;
            BuildingBlueprint bluePrint = kvp.Value;

            foreach (IResourceProducer buildingOutput in bluePrint.outputResourceProduction) {
                // Consider every output to this building
                ResourceType outputResourceType = buildingOutput.targetResource();

                if (!allRequirements.ContainsKey(outputResourceType))
                    { allRequirements[outputResourceType] = new BuildingRequirements(); }

                // the inputs to this building are a "hidden" extra requirement to the outputs so count them 
                allRequirements[outputResourceType].merge(bluePrint.inputResourceCosts);

            }
        }
    }

    public Dictionary<ResourceType, int> getAllHiddenCosts(ResourceType rt) {
        // Hidden cost of a resource is the hidden cost of the resource
        // PLUSS all the hidden costs of all those associated resources per turn
        // EX: Steel = (1 Iron, 1 coal) = (1 Iron, 1 Coal, 1 Wood)
        if (!this.allRequirements.ContainsKey(rt)) { return new Dictionary<ResourceType, int>(); }

        if (this.allHiddenRequirementsCache.ContainsKey(rt)) { return allHiddenRequirementsCache[rt]; }
        Dictionary<ResourceType, int> result = new Dictionary<ResourceType, int>();
        merge(result, allRequirements[rt].getHiddenRequirements(), 1);
        foreach(KeyValuePair<ResourceType, int> kvp in allRequirements[rt].getHiddenRequirements()) {
            Dictionary<ResourceType, int> nextLevelHiddenCost = getAllHiddenCosts(kvp.Key);
            merge(result, nextLevelHiddenCost, kvp.Value); // Merge into result
        }

        this.allHiddenRequirementsCache[rt] = result;
        return result;
    }
    
    private void merge(Dictionary<ResourceType, int> a, Dictionary<ResourceType, int> b, int multiplier) {
        // Take all the elements in b and add them to a
        foreach(KeyValuePair<ResourceType, int> kvp in b) {
            if (!a.ContainsKey(kvp.Key)) { a[kvp.Key] = (kvp.Value * multiplier); }
            else                         { a[kvp.Key] += (kvp.Value * multiplier); }
        }
    }


    private class BuildingRequirements {
        // To represent the hidden requirements for a target output resource
        // Given a few buildings via the merge function, this dictionary will
        // tell a caller what reccomended mix of extra resources per turn 
        // are needed as a pre-requisit before earning 1 rpt of the origional resources

        private Dictionary<ResourceType, int> req;

        public BuildingRequirements() {
            this.req = new Dictionary<ResourceType, int>();
        }

        public void merge(List<IResourceProducer> otherBuildingInput) {
            foreach (IResourceProducer prod in otherBuildingInput) {
                // Look through all the producers and measure exactly what 
                // the input cost is
                ResourceChange requiredInput = prod.simulate(1);
                if (!this.req.ContainsKey(requiredInput.resourceType)) {
                    // New resource type as a pre-rec
                    req[requiredInput.resourceType] = requiredInput.change;
                } else {
                    // Else, the least efficent input wins
                    // Note, this is comparing between efficency of buildings
                    req[requiredInput.resourceType] =
                        Mathf.Max(req[requiredInput.resourceType],
                                   requiredInput.change);
                }
            }
        }

        public Dictionary<ResourceType, int> getHiddenRequirements() {
            return req;
        }
        
    }
}

public class RemainingDistance {

    private int _numberOfInfinities = 0;
    public int NumberOfInfinities { get { return _numberOfInfinities; } }

    private int _totalStockpileDelta = 0;
    public int TotalStockpileDelta { get { return _totalStockpileDelta; } }

    private int _maxWaitTime = 0;
    public int MaxWaitTime { get { return _maxWaitTime; } }

    private int _totalWaitTime = 0;
    public int TotalWaitTime { get { return _totalWaitTime; } }

    private int _bestMaxlWaitTime = 0;
    public int BestMaxWaitTime { get { return _bestMaxlWaitTime; } }

    private int _totalChangePerTickDelta = 0;
    public int TotalChangePerTickDelta { get { return _totalChangePerTickDelta; } }

    private int _bestChangePerTickDelta = 0;
    public int BestChangePerTickDelta { get { return _bestChangePerTickDelta; } }

    private Dictionary<ResourceType, int> _exactChangePerTickDeltaDictionary;

    public RemainingDistance() {
        _exactChangePerTickDeltaDictionary = new Dictionary<ResourceType, int>();
    }

    public void addInfinity() { _numberOfInfinities++; }

    public void updateStockpileDelta(int stockpileDelta) { this._totalStockpileDelta += stockpileDelta; }

    public void updateWaitTime(int estTime) {
        _totalWaitTime += estTime;
        _maxWaitTime = Mathf.Max(_maxWaitTime, estTime);
    }

    public void updateBestPossibleWaitTime(int estTime) {
        _bestMaxlWaitTime = Math.Max(_bestMaxlWaitTime, estTime);
    }

    public void updateCPTDelta(ResourceType rt, int cptDelta) {
        _exactChangePerTickDeltaDictionary[rt] = cptDelta;
        this._totalChangePerTickDelta += cptDelta;
    }

    public void updateBestPossibleCPTDelta(int cptDelta) {
        this._bestChangePerTickDelta += cptDelta;
    }

    public int specificCPTDistance(ResourceType rt) {
        if (!_exactChangePerTickDeltaDictionary.ContainsKey(rt)) { return 0; }
        return _exactChangePerTickDeltaDictionary[rt];
    }

    public IEnumerable<ResourceType> getRemainingResourceTypes() {
        return this._exactChangePerTickDeltaDictionary.Keys;
    }

    public bool atTarget() {
        return _numberOfInfinities <= 0 && 
               _totalWaitTime <= 0 &&
               _totalChangePerTickDelta <= 0;
    }
}
