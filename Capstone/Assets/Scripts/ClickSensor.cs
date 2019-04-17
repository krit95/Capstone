using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ClickSensor : MonoBehaviour
{
    public EventSystem eventSystem;
    public GraphicRaycaster graphicRaycaster;
    public GameController gc;
    public DetailsPanelController detailsPanel;
    public ConstructionController constructionPannel;
    public Tilemap map;

    private void Update() {
        if (Input.GetMouseButtonUp(0)) {
            // If we presseed the mouse button

            PointerEventData pointerEvent = new PointerEventData(eventSystem);
            pointerEvent.position = Input.mousePosition;

            List<RaycastResult> rayHits = new List<RaycastResult>();
            graphicRaycaster.Raycast(pointerEvent, rayHits);

            if (rayHits.Count > 0) {
                // If we hit a UI element
                return;
            }
            

            this.constructionPannel.turnOff();
            Tile selectedTile = this.tileClicked();

            if (this.gc.getBuilding(selectedTile) != null) {
                // If there is a building on the tile
                // Show the details
                this.detailsPanel.update(selectedTile);
            } else {
                // Else, show the construction pannel
                this.constructionPannel.turnOn(selectedTile);

            }
        }
    }

    public void addBuildingsTest() {
        IBuilding bank1 = BuildingFactory.buildNew(BuildingType.Bank, 0, 0);
        IBuilding bank2 = BuildingFactory.buildNew(BuildingType.Bank, 1, 1);
        IBuilding bank3 = BuildingFactory.buildNew(BuildingType.Bank, 2, 2);
        this.gc.forceBuildBuilding(bank1);
        this.gc.forceBuildBuilding(bank2);
        this.gc.forceBuildBuilding(bank3);

        IBuilding sm1 = BuildingFactory.buildNew(BuildingType.StoneMason, 5, 5);
        IBuilding sm2 = BuildingFactory.buildNew(BuildingType.StoneMason, 5, 6);
        IBuilding sm3 = BuildingFactory.buildNew(BuildingType.StoneMason, 5, 7);
        this.gc.forceBuildBuilding(sm1);
        this.gc.forceBuildBuilding(sm2);
        this.gc.forceBuildBuilding(sm3);

        IBuilding wc1 = BuildingFactory.buildNew(BuildingType.WoodCutter, 10, 5);
        IBuilding wc2 = BuildingFactory.buildNew(BuildingType.WoodCutter, 10, 6);
        IBuilding wc3 = BuildingFactory.buildNew(BuildingType.WoodCutter, 10, 7);
        this.gc.forceBuildBuilding(wc1);
        this.gc.forceBuildBuilding(wc2);
        this.gc.forceBuildBuilding(wc3);
    }

    public void updateSidePanel() {
        // Updates the display panel on the side
    }

    public Tile tileClicked() {
        Vector3 clicked = map.layoutGrid.LocalToCell(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        return new Tile((int)clicked.x, (int)clicked.y);
    }
}
