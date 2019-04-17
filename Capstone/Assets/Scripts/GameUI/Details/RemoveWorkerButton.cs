using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RemoveWorkerButton : MonoBehaviour {

    private Button myButton;
    private IBuilding focusedBuilding;
    public DetailsPanelController detailsPanel;

    private void Start() {
        myButton = GetComponent<Button>();
        myButton.onClick.AddListener(delegate { this.myClick(); });
    }

    private void turnOn() { this.myButton.interactable = true; }
    public void tryTurnOn(IBuilding newBuilding) {
        if (newBuilding.currentWorkers() > 0)
        {
            // If the building has workers in it
            focusedBuilding = newBuilding;
            turnOn();
        }
        else
        {
            turnOff();
        }
    }

    private void turnOff()  {
        focusedBuilding = null;
        this.myButton.interactable = false;
    }

    private void myClick() {
        GameController.singleton.unassignWorker(focusedBuilding);
        tryTurnOn(focusedBuilding);
        detailsPanel.redraw();
    }

}
