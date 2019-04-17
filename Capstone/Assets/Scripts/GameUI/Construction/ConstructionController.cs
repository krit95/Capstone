using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConstructionController : MonoBehaviour {

    public ConstructionButton buttonPrefab;
    public GameObject buttonHolder;

    public BuyWorkerButton buyWorkerButton;

    private GameController gc;
    private static Tile tile;  // The tile that this Construction Controller is planning to build on

    // Start is called before the first frame update
    void Start() {
        this.gc = GameController.singleton;

        foreach (BuildingType bt in BuildingFactory.allBuildings) {
            // Make a button for each building type
            ConstructionButton newButton = Instantiate(buttonPrefab);
            newButton.Start();
            newButton.setBuildingType(bt);

            newButton.transform.SetParent(this.buttonHolder.transform);
        }



        // Constructions starts turned off
        this.turnOff();
    }
    

    private IEnumerable<ConstructionButton> allConstructionButtons() {
        int totalNumOfButtons = this.buttonHolder.transform.childCount;
        List<ConstructionButton> result = new List<ConstructionButton>(totalNumOfButtons);
        for (int i = 0; i < totalNumOfButtons; i++) {
            ConstructionButton button = this.buttonHolder.transform.GetChild(i).GetComponent<ConstructionButton>();
            result.Add(button);
        }
        return result;
    }

    public void turnOn(Tile t)  {
        // NOTE: Each button is responsible for checking if they can build themselves
        this.gameObject.SetActive(true);
        tile = t;

        foreach (ConstructionButton cb in allConstructionButtons()) {
            cb.tryTurnOn(t.x, t.y);
        }
        buyWorkerButton.tryTurnOn();
    }


    public void turnOff() {
        this.gameObject.SetActive(false);
        tile = null;
    }



    public void buildBuilding(BuildingType bt) {
        if (ConstructionController.tile == null) {
            throw new System.Exception("Trying to build without a tile. Please call turnOn(Tile t) before trying to build something");
        }
        IBuilding newBuilding = BuildingFactory.buildNew(bt, tile.x, tile.y);
        this.gc.startBuildBuilding(newBuilding);
    }


}
