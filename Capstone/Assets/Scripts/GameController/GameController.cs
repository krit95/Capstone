using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GameController : MonoBehaviour
{
    /**
     * A thing to keep track of all the stuff happening in the game
     */

    public static GameController singleton;

    // Controllers
    public static BuildingGS gameState;
    private MapController mapController;

    // Resources
    public ResourceDisplayController resourceDisplay;

    // Buildings
    private Dictionary<Tile, IBuilding> buildingPositions;

    // Workers
    public Tilemap characterLayer;
    public TileBase dwarfBase;
    public Dwarf dwarfPrefab;

    private Queue<Dwarf> freeWorkers = new Queue<Dwarf>();

    private void Awake() {
        buildSingleton();
    }

    private void buildSingleton() { 
        // This method will ensure that MapController.singleton is populated
        if (singleton == null) { singleton = this; }
        else if (singleton.Equals(this)) { return; }
        else {
            // Else the singleton already exist AND this object isn't it
            // There can only be one...
            GameObject.Destroy(this);
        }
    }

    private void Start() {
        this.mapController = MapController.singleton;

        this.buildingPositions = new Dictionary<Tile, IBuilding>();

        //ResourceChange gold = new ResourceChange(ResourceType.Gold, 1000);
        //ResourceChange stone = new ResourceChange(ResourceType.Stone, 1000);
        //ResourceChange wood = new ResourceChange(ResourceType.Wood, 1000);
        //this.gameState = new BuildingGS(new List<ResourceChange>(3) { gold, stone, wood});
        this.displayAllResourceCount();

        // Set up worker units to be drawn
        Dwarf.staticInitalize(characterLayer, dwarfBase);
        
    }


    private float lastGameTick = 0;
    private void Update() {
        if ((Time.time - lastGameTick) > GameSetup.TICK_LENGHT_SEC) {
            // If it's been a while since our last game tick
            // Propogate an "in game tick"

            tick();
            lastGameTick = Time.time;
        }
    }

    private void tick() {
        // Simulate one tick of this game
        gameState.timePasses(1);
        displayAllResourceCount();

        resourceDisplay.updateTick();
    }

    private void displayAllResourceCount() {
        if (gameState == null) { Debug.Log("Null game state"); }
        foreach (ResourceType rt in gameState.getAllResourceTypes()) {
            this.resourceDisplay.updateResourceCount(rt, gameState.getStockpile(rt));
        }
    }
    
    #region Buildings

    public IBuilding getBuilding(Tile pos) {
        if (this.buildingPositions.ContainsKey(pos)) { return this.buildingPositions[pos]; }
        return null;
    }
    
    public IBuilding getAnyOpenBuilding(BuildingType bt) {
        // Returns an instance of the provided BT with a free worker slot
        // If no such building exists, null is returned
        return gameState.getAnyOpenBuilding(bt);
    }

    public bool canBuildBuilding(BuildingType bt, int x, int y) {
        return gameState.canAffordBuilding(bt) &&
               this.mapController.isWalkable(x, y);
    }

    public void startBuildBuilding(IBuilding newBuilding) {
        // Spend the cash money 
        // NOTE: validation is done at button click time
        gameState.spendForBuilding(newBuilding);

        // Start the construction process of this new building
        StartCoroutine(waitForConstruction(newBuilding));
    }

    private IEnumerator waitForConstruction(IBuilding newBuilding) {
        // First, mark the target tile as occupied
        mapController.construction(newBuilding.position());
        // Wait for the designated time
        yield return new WaitForSeconds(newBuilding.timeToBuild() * GameSetup.TICK_LENGHT_SEC);
        forceBuildBuilding(newBuilding);
    }
    
    public void forceBuildBuilding(IBuilding newBuilding) {
        // Reccord the position of the building
        occupyBuildingSpace(newBuilding);

        // Give the building to the resource manager logic
        // We've already spent for the building in the "startBuildBuilding" func
        gameState.forceAddBuilding(newBuilding);

        // Display the new resource stockpiles/ income per turn
        displayBuildingResouceDelta(newBuilding);

        // Draw the building
        this.mapController.addBuilding(newBuilding);
    }

    private void displayBuildingResouceDelta(IBuilding newBuiding) {
        // Updates the resource display for only the resources this building cares about
        // IE: input, output and cost to build resources

        // Update the income per turn for the building
        List<ResourceType> allResources = newBuiding.outputResources();
        allResources.AddRange(newBuiding.inputResources());
        foreach(ResourceType rt in allResources) {
            int newStockpile = gameState.getStockpile(rt);
            int newRPT = gameState.getChangePerTick(rt);
            resourceDisplay.updateCountAndRPT(rt, newStockpile, newRPT);
        }

        // Update the cost to build the building
        foreach(ResourceChange rc in newBuiding.costToBuild()) {
            if (allResources.Contains(rc.resourceType)) { continue; } // If we've already updated this resource type above ignore it
            int newStockpile = gameState.getStockpile(rc.resourceType);
            resourceDisplay.updateResourceCount(rc.resourceType, newStockpile);
         }
    }


    private void occupyBuildingSpace(IBuilding b) {
        // Store the provided building in this internal tile -> building map
        Tile buildingPos = new Tile(b.position());
        this.buildingPositions[buildingPos] = b;
    }

    #endregion

    #region Workers

    public bool anyFreeWorkers() {
        return this.freeWorkers.Count > 0;
    }

    public void cleanAssignWorker(IBuilding building) {
        // Grabs a random Dwarf and tells them to walk to a building
        Dwarf d = this.freeWorkers.Dequeue();

        // This coroutine will come calling back to us once the dwarf is in position
        StartCoroutine(d.assignWork(building, forceAssignWorker));
        resourceDisplay.workerAssigned();
    }

    private bool forceAssignWorker(Dwarf d, IBuilding building) {
        // NOTE: You should probably be using cleanAssignWorker
        // This function is most only for the dwarfs to call when they're for assignment
        // Forces the provided worker to be assigned into the building
        // This will dissapear the worker wherever it is on the map
        // It is assumed the worker has already been removed from the free worker queue
        
        // The gameState will update the worker counts for us
        gameState.assignWorker(building);

        // Update the display
        // TODO: Break this function up into dispalyInputs, displayOutputs and displayCosts
        displayBuildingResouceDelta(building);
        d.eraseSelf();

        // TODO: Worker pooling
        GameObject.Destroy(d.gameObject);

        return true;
    }

    public void unassignWorker(IBuilding building) {
        // the GameState will update the worker counts for us
        gameState.unassignWorker(building);
        
        displayBuildingResouceDelta(building);

        spawnWorker(building.position());
        resourceDisplay.workerUnAssigned();
    }

    public bool canBuyWorker() {
        return gameState.canBuyWorker();
    }

    public void forceBuyWorker() {
        // Spend the cash money 
        gameState.forceBuyWorker();
        // Place the worker at 0,0
        this.spawnWorker();
        // Update the UI
        resourceDisplay.addTotalWorker();

        // TODO: put this somewhere else
        foreach(ResourceChange rc in WorkerFactory.WORKER_COST) {
            resourceDisplay.updateResourceCount(rc.resourceType, gameState.getStockpile(rc.resourceType));
        }
    }

    // TODO: Put this in a worker factory
    private void spawnWorker()               { spawnWorker(findOpenTile()); }
    private void spawnWorker(Vector2Int pos) { spawnWorker(new Tile(pos.x, pos.y)); }
    private void spawnWorker(Tile startTile) {
        Dwarf newWorker = Instantiate(this.dwarfPrefab);
        freeWorkers.Enqueue(newWorker); // Add the worker to the pool of unassigned workers
        newWorker.initalize(startTile);
    }

    private Tile findOpenTile() {
        int x = 0;
        int y = 0;
        while ( ! mapController.isWalkable(x, y) ) {
            if (x < GameSetup.BOARD_WIDTH) { x++; }
            else {
                x = 0;
                y++;
            }
        }
        return new Tile(x, y);
    }
    
    #endregion

}
