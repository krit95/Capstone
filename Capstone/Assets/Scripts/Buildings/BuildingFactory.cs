using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;

public static class BuildingFactory {
    


    #region bank
    public static TileBase bankTile;  // All sprites are loaded from BuildFactoryMonobehavior.cs monobehavior object
    public static readonly int bankProdPerPop = 5;
    public static readonly int bankMaxPop = 3;
    public static List<ResourceChange> bankBuildCost = new List<ResourceChange>() {
        new ResourceChange(ResourceType.Stone, 10),
        new ResourceChange(ResourceType.Gold, 5),
        new ResourceChange(ResourceType.Wood, 15)
    };
    public static IBuilding buildNewBank(int x, int y) {
        return new SimpleBuilding(
            BuildingType.Bank,
            ResourceType.Gold,
            new Vector2Int(x, y));
    }

    #endregion

    #region stone mason
    public static TileBase stoneMasonTile;
    public static readonly int stoneMasonProdPerPop = 1;
    public static readonly int stoneMasonMaxPop = 6;
    public static readonly List<ResourceChange> stoneMasonBuildCost = new List<ResourceChange>()
    {
        new ResourceChange(ResourceType.Gold, 15),
        new ResourceChange(ResourceType.Wood, 5)
    };
    public static IBuilding buildNewStoneMason(int x, int y) {
        return new SimpleBuilding(
            BuildingType.StoneMason,
            ResourceType.Stone,
            new Vector2Int(x, y));
    }
    #endregion

    #region wood cutter
    public static TileBase woodCutterTile;
    public static readonly int woodCutterProdPerPop = 3;
    public static readonly int woodCutterMaxPop = 2;
    public static readonly List<ResourceChange> woodCutterBuildCost = new List<ResourceChange>() {
        new ResourceChange(ResourceType.Gold, 8)
    };
    public static IBuilding buildNewWoodCutter(int x, int y) {
        return new SimpleBuilding(
            BuildingType.WoodCutter,
            ResourceType.Wood,
            new Vector2Int(x, y));
    }

    #endregion

    #region Silver Mine
    public static TileBase silverMineTile;
    public static readonly int silverMineMaxPop = 8;
    public static readonly List<ResourceChange> silverMineBuildCost = new List<ResourceChange>() {
        new ResourceChange(ResourceType.Stone, 300),
        new ResourceChange(ResourceType.Wood, 45)
    };
    public static readonly List<IResourceProducer> silverMineOutputResources = new List<IResourceProducer>() {
        new SimpleResourceProducer(ResourceType.Silver, (int workers) => { return workers * 3; }),
        new SimpleResourceProducer(ResourceType.Gold, (int workers) => { return workers * 1; }),
    };
    public static IBuilding buildNewSilverMine(int x, int y) {
        return new MultiResourceBuilding( BuildingType.SilverMine, new Vector2Int(x, y));
    }
    #endregion
    
    #region Steel Blacksmith
    public static TileBase steelBlacksmithTile;
    public static readonly int steelBlacksmithMaxPop = 4;
    public static List<ResourceChange> steelBlacksmithBuildCost = new List<ResourceChange>() {
        new ResourceChange(ResourceType.Gold, 100),
        new ResourceChange(ResourceType.Stone, 80),
        new ResourceChange(ResourceType.Wood, 50),
        new ResourceChange(ResourceType.Iron, 20)
    };
    public static List<IResourceProducer> steelBlacksmithOutputResources = new List<IResourceProducer>() {
        new SimpleResourceProducer(ResourceType.Steel, (int workers) => { return workers * 1; })
    };
    public static List<IResourceProducer> steelBlacksmithInputResources = new List<IResourceProducer>() {
        new SimpleResourceProducer(ResourceType.Iron, (int workers) => { return workers * 1; }),
        new SimpleResourceProducer(ResourceType.Coal, (int workers) => { return workers * 2; })
    }; 
    public static IBuilding buildNewSteelSmith(int x, int y) {
        return new MultiResourceBuilding(BuildingType.SteelSmith, new Vector2Int(x, y));
    }
    #endregion

    public static TileBase coalMineTile;
    public static TileBase ironMineTile;

    public static readonly List<BuildingType> allBuildings = new List<BuildingType>() {
        BuildingType.Bank,
        BuildingType.StoneMason,
        BuildingType.WoodCutter,

        BuildingType.SilverMine,
        BuildingType.CoalMine,
        BuildingType.IronMine,
        BuildingType.SteelSmith
    };

    public static TileBase getIcon(BuildingType bt) {
        switch (bt) {
            case BuildingType.Bank: return bankTile;
            case BuildingType.StoneMason: return stoneMasonTile;
            case BuildingType.WoodCutter: return woodCutterTile;

            case BuildingType.SilverMine: return silverMineTile;
            case BuildingType.CoalMine: return coalMineTile;
            case BuildingType.IronMine: return ironMineTile;
            case BuildingType.SteelSmith: return steelBlacksmithTile;
            case BuildingType.NONE:
            default:
                throw new System.Exception("Unknown building type, can't get tile: " + Enum.GetName(typeof(BuildingType), bt));
        }
    }

    public static IBuilding buildNew(BuildingType bt, int x, int y) {
        switch (bt) {
            case BuildingType.Bank: return buildNewBank(x, y);
            case BuildingType.StoneMason: return buildNewStoneMason(x, y);
            case BuildingType.WoodCutter: return buildNewWoodCutter(x, y);

            case BuildingType.SilverMine: return buildNewSilverMine(x, y);
            case BuildingType.CoalMine: return new MultiResourceBuilding(BuildingType.CoalMine, new Vector2Int(x, y));
            case BuildingType.IronMine: return new MultiResourceBuilding(BuildingType.IronMine, new Vector2Int(x, y));
            case BuildingType.SteelSmith: return buildNewSteelSmith(x, y);
            case BuildingType.NONE:
            default: throw new System.Exception("Unknown building type, can't build. Building Type: " + Enum.GetName(typeof(BuildingType), bt));
        }
    }

    public static Dictionary<BuildingType, BuildingBlueprint> allBluePrints = new Dictionary<BuildingType, BuildingBlueprint>() {
        // Bank
        { BuildingType.Bank, new BuildingBlueprint(
            buildingname: "Bank",
            maxPop: bankMaxPop,
            buildCost: bankBuildCost,
            timeToBuild: 3,
            outputResourceProduction: new List<IResourceProducer>() {
                new SimpleResourceProducer(ResourceType.Gold, (int numOfWorkers) => { return numOfWorkers * bankProdPerPop; })
                }
            )
        },

        // Stone Mason
        { BuildingType.StoneMason, new BuildingBlueprint(
            buildingname: "Stone Mason",
            maxPop: stoneMasonMaxPop,
            buildCost: stoneMasonBuildCost,
            timeToBuild: 10,
            outputResourceProduction: new List<IResourceProducer>() {
                new SimpleResourceProducer(ResourceType.Stone, (int numOfWorkers) => { return numOfWorkers * stoneMasonProdPerPop; })
                }
            )
        },

        // Wood Cutter
        { BuildingType.WoodCutter, new BuildingBlueprint(
            buildingname: "Wood Cutter",
            maxPop: woodCutterMaxPop,
            buildCost: woodCutterBuildCost,
            timeToBuild: 15,
            outputResourceProduction: new List<IResourceProducer>() {
                new SimpleResourceProducer(ResourceType.Wood, (int numOfWorkers) => { return numOfWorkers * woodCutterProdPerPop; })
                }
            )
        },

        // Silver Mine
        { BuildingType.SilverMine, new BuildingBlueprint(
            buildingname: "Silver Mine",
            maxPop: silverMineMaxPop,
            buildCost: silverMineBuildCost,
            timeToBuild: 30,
            outputResourceProduction: silverMineOutputResources
            )
        },

        // Coal Maker
        { BuildingType.CoalMine, new BuildingBlueprint(
            buildingname: "Coal Maker",
            maxPop: 3,
            buildCost: new List<ResourceChange>() {
                new ResourceChange(ResourceType.Stone, 100),
                new ResourceChange(ResourceType.Wood, 50),
                new ResourceChange(ResourceType.Gold, 90)
            },
            timeToBuild: 5,
            outputResourceProduction: new List<IResourceProducer>() {
                new SimpleResourceProducer(ResourceType.Coal, (int workers) => { return workers * 3; })
            },
            inputResourceProduction: new List<IResourceProducer>() {
                new SimpleResourceProducer(ResourceType.Wood, (int workers) => { return workers * 1; })
            }
            )
        },

        // Iron Mine
        { BuildingType.IronMine, new BuildingBlueprint(
           buildingname: "Iron Mine",
           maxPop: 8,
           buildCost: new List<ResourceChange>() {
               new ResourceChange(ResourceType.Gold, 80),
               new ResourceChange(ResourceType.Stone, 50),
               new ResourceChange(ResourceType.Wood, 60)
           }, 
           timeToBuild: 6,
           outputResourceProduction: new List<IResourceProducer>() {
               new SimpleResourceProducer(ResourceType.Iron, (int workers) => {return workers * 1; })
           }
           )
        },

        // Steel Smith
        { BuildingType.SteelSmith, new BuildingBlueprint(
            buildingname: "Steel Smith",
            maxPop: steelBlacksmithMaxPop,
            buildCost: steelBlacksmithBuildCost,
            timeToBuild: 5,
            outputResourceProduction: steelBlacksmithOutputResources,
            inputResourceProduction: steelBlacksmithInputResources
            )
        },

    };
}


public class BuildingBlueprint {
    public static bool inputResources = true;

    public readonly string buildingName;
    
    public readonly int maxPop;
    public readonly List<ResourceChange> buildCost;
    public readonly int timeToBuild;

    public readonly List<IResourceProducer> outputResourceProduction;
    public readonly List<IResourceProducer> _inputResourceCosts;
    public List<IResourceProducer> inputResourceCosts { // Note: despite intuition, this should always be a positive vale
        get {
            if (inputResources) { return _inputResourceCosts; }
            else                { return new List<IResourceProducer>(0); }
        }
    } 

    public BuildingBlueprint(
        string buildingname = "Un-named building",
        int maxPop = 0,
        List<ResourceChange> buildCost = null,
        int timeToBuild = 0,
        List<IResourceProducer> outputResourceProduction = null,
        List<IResourceProducer> inputResourceProduction = null  // Note: despite intuition, this should always be a positive vale
        )
    {
        this.buildingName = buildingname;
        this.maxPop = maxPop;
        this.timeToBuild = timeToBuild;

        if (buildCost == null) {
            this.buildCost = new List<ResourceChange>();
            Debug.Log(buildingName);
            throw new System.Exception();
        } else {
            this.buildCost = buildCost;
        }
        
        this.outputResourceProduction = (outputResourceProduction == null) ? new List<IResourceProducer>() : outputResourceProduction;
        this._inputResourceCosts = (inputResourceProduction == null)   ? new List<IResourceProducer>() : inputResourceProduction;
    }

}