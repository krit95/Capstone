using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class MidLevelManager : MonoBehaviour {

    public Text currentStatus;

    public static BuildingGS initialGS;
    public static BuildingGS targetGS;

    public GameController gc = GameController.singleton;
    public ALongTermPlanner ltp;

    private void Awake() {
        GameSetup.TICK_LENGHT_SEC = 0.3f;
        this.currentStatus.text = "Thinking of a plan";
        manage();
    }

    public void manage() {
        GameController.gameState = initialGS;
        ltp.plan(initialGS, targetGS, callback);
    }

    private bool callback(Stack<Work> work) {
        StartCoroutine(executeOrders(work));
        return true;
    }

    private IEnumerator executeOrders(Stack<Work> workOrder) {
        this.currentStatus.text = "Working on the plan";
        foreach (Work w in workOrder) {
            switch (w.workType) {
                case EWork.Wait:
                case EWork.EMPTY:
                    // A wait object is yield returned after the switch
                    break;

                case EWork.BuildBuilding:
                    Vector2Int nextPos = this.nextPos();
                    IBuilding b = BuildingFactory.buildNew(w.buildingType, nextPos.x, nextPos.y);
                    if (!gc.canBuildBuilding(w.buildingType, nextPos.x, nextPos.y))
                        { throw new System.Exception("Can't build the building for some reason : " + w.buildingType); }
                    gc.startBuildBuilding(b);
                    break;

                case EWork.BuyAndAssignWorker:
                    if (!gc.canBuyWorker())
                        { throw new System.Exception("Can't buy worker for some reason"); }
                    gc.forceBuyWorker();
                    IBuilding targetBuilding = gc.getAnyOpenBuilding(w.buildingType);
                    if (targetBuilding == null)
                        { throw new System.Exception("No open building to assign worker to"); }
                    gc.cleanAssignWorker(targetBuilding);
                    break;

                default:
                    throw new System.Exception("Unknown work type: " + Enum.GetName(typeof(EWork), w.workType));

            }
            yield return new WaitForSeconds(w.frameWait * GameSetup.TICK_LENGHT_SEC);
        }
        // Work done
        this.currentStatus.text = "Finished work";
        yield break;
    }


    private int currentX = 5;
    private int currentyY = 5;
    private Vector2Int nextPos() {
        Vector2Int result = new Vector2Int(currentX, currentyY);
        currentX++;
        if (currentX >= GameSetup.BOARD_WIDTH) {
            currentX = 5;
            currentyY++;
        }
        return result;
    }
}
