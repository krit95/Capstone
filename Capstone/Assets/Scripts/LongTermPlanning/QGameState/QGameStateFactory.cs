using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class QGameStateFactory {

    // This function represents an edge
    public static QGameState waitTransition(QGameState qe, int time) {
        BuildingGS endGS = waitGameState(qe.gameState, time);

        int timeCost = qe.costToGetHere + time;
        Work newWork = new Work(EWork.Wait, BuildingType.NONE, time);

        return new QGameState(endGS, qe, newWork, timeCost);
    }

    public static BuildingGS waitGameState(BuildingGS gs, int time) {
        BuildingGS endGS = new BuildingGS(gs);
        endGS.timePasses(time);
        return endGS;
    }

    //////////////////////////////////////////////////


    public static QGameState buyWorker(QGameState qGS, BuildingType bt) {
        BuildingGS newGameState = waitGameState(qGS.gameState, 1);
        newGameState.buyAndAssignWorker(bt);

        int costToGetHere = qGS.costToGetHere + 3;
        Work newWork = new Work(EWork.BuyAndAssignWorker, bt, 3);

        return new QGameState(newGameState, qGS, newWork, costToGetHere);
    }

    public static QGameState buyBuilding(QGameState qGS, IBuilding newBuilding) {
        BuildingGS newGameState = waitGameState(qGS.gameState, newBuilding.timeToBuild()); 
        newGameState.buyBuilding(newBuilding);

        int costToGetHere = qGS.costToGetHere + newBuilding.timeToBuild();   
        Work newWork = new Work(EWork.BuildBuilding, newBuilding.getBuildingType(), newBuilding.timeToBuild());

        return new QGameState(newGameState, qGS, newWork, costToGetHere);
    }
}
