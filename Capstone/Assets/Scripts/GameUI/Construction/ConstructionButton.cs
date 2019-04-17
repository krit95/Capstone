using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class ConstructionButton : MonoBehaviour {
    private BuildingType bt = BuildingType.NONE;

    private Button myButton;
    private int x;
    private int y;
    private ConstructionController constructionController;

    public void Start() {
        this.myButton = this.GetComponent<Button>();
        myButton.interactable = false;

        this.constructionController = GameObject.FindWithTag(GameSetup.CONSTRUCTION_CONTROLLER_TAG).GetComponent<ConstructionController>();

        myButton.onClick.AddListener(delegate { myClick(); });
    }

    // What type of building does this button try to build
    public void setBuildingType(BuildingType bt) {
        this.bt = bt;

        Text myText = this.transform.GetChild(0).GetComponent<Text>();
        myText.text = BuildingFactory.allBluePrints[bt].buildingName;
    }


    public bool canCurrentlyBuild(int x, int y) {
        // Can we currently build the building this button is trying to build? 
        return GameController.singleton.canBuildBuilding(bt, x, y);
    }


    private void forceTurnOn()  { this.myButton.interactable = true; }
    private void forceTurnOff() { this.myButton.interactable = false; ; }
    
    public void tryTurnOn(int x, int y) {
        // Only turn this button on if we can currently build
        if (canCurrentlyBuild(x, y)) {
            this.x = x;
            this.y = y;
            forceTurnOn();
        }
        else { forceTurnOff(); }
    }


    private void myClick() {
        constructionController.buildBuilding(bt);
        tryTurnOn(x, y);
    }
}
