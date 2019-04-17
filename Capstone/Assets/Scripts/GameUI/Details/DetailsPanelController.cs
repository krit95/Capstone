using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text;
using System;

public class DetailsPanelController : MonoBehaviour {

    public GameController gc;
    public Text upperText;
    public Text lowerText;
    public AssignWorkerButton assignWorkerButton;
    public RemoveWorkerButton removeWorkerButton;

    private IBuilding targetBuilding;

    // Start is called before the first frame update
    void Start() {
        
    }
    

    public void update(Tile t) {
        // Update to show the details of the provided tile
        this.targetBuilding = this.gc.getBuilding(t);
        this.assignWorkerButton.tryTurnOn(targetBuilding);
        this.removeWorkerButton.tryTurnOn(targetBuilding);
        this.redraw();
    }

    public void redraw() {
        if (this.targetBuilding == null) {
            this.clear();
            return;
        }

        this.upperText.text = targetBuilding.position().ToString();
        StringBuilder sb = new StringBuilder();

        sb.Append("Building type: ");
        sb.Append(Enum.GetName(typeof(BuildingType), this.targetBuilding.getBuildingType()));  // TODO: Pretyify this string
        sb.Append("\n\nWorkers: ");
        sb.Append(targetBuilding.currentWorkers());
        sb.Append("\\");
        sb.Append(targetBuilding.currentWorkers() + targetBuilding.openWorkerSlots());
        sb.Append("\n-Resource Change-\n");
        foreach(ResourceChange rc in targetBuilding.changePerTick()) {
            sb.Append(Enum.GetName(typeof(ResourceType), rc.resourceType));
            sb.Append(": ");
            sb.Append(rc.change);
        }

        this.lowerText.text = sb.ToString();
        float preferedHeight = this.lowerText.preferredHeight;
        RectTransform rectTrans = this.lowerText.GetComponent<RectTransform>();
        rectTrans.sizeDelta = new Vector2(rectTrans.sizeDelta.x, preferedHeight);

    }


    public void clear() {
        this.upperText.text = "";
        this.lowerText.text = "";
    }
}
