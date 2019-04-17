using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class God : MonoBehaviour
{
    private float lastGameTick = 0;

    public ResourceDisplayController meTargetDisplay, aiResourceDisplay, aiTargetDisplay;
    public Text targetReachedText;
    public static CampaignType campaignType = CampaignType.Normal;
    public static AIPersonalityType aiPersonality = AIPersonalityType.Conservative;
    public ALongTermPlanner planner;

    private static readonly int noOfActors = 2;

    private GameState[] gameStates;
    private static int indexHuman = 0;
    private static int indexAi = 1;

    private BuildingGS aiCurrentGameState;
    private List<BuildingGS> targetGameStates = new List<BuildingGS>();

    private int[] currentTargetGsIndex = new int[noOfActors];

    private Stack<Work> aiCurrentPlan;

    private int bank, stone, wood, silver;
    private Boolean targetReachedBoolAi = false;

    public void Awake()
    {
        Debug.Log("God awoken!");
        Debug.Log(campaignType + ":" + aiPersonality);
        setStartGameState();
    }

    public void Start()
    {
        setTargetGameStates();
        planNextTargetGsAi();
    }

    private void doNextWorkAi()
    {
        if (aiCurrentPlan.Count > 0)
        {
            Work nextAiWork = aiCurrentPlan.Pop();
            Debug.Log("Next work: " + nextAiWork.workType);
            doWork(nextAiWork);
        }
        else
        {
            Debug.Log("No next work.");
            if (isTargetReached(indexAi))
            {
                targetReached(indexAi);
            }
            else { Debug.Log("Done all work, yet target not reached"); }
        }
    }

    private void planNextTargetGsAi()
    { 
        BuildingGS targetGs = modifyGameStateForPersonality(this.targetGameStates[currentTargetGsIndex[indexAi]], aiPersonality);
        Debug.Log(aiCurrentGameState);
        ltpPlan(aiCurrentGameState, targetGs);
    }

    private void ltpPlan(BuildingGS initialGs, BuildingGS targetGsAi)
    {
        planner.plan(initialGs, targetGsAi, planningCompleteCallBack);
    }

    public bool planningCompleteCallBack(Stack<Work> result)
    {
        Debug.Log("Path found");
        this.aiCurrentPlan = result;
        doNextWorkAi();
        return true;
    }

    private BuildingGS modifyGameStateForPersonality(BuildingGS targetGsHuman, AIPersonalityType aiPersonality)
    {
        switch (aiPersonality)
        {
            case AIPersonalityType.GoldDigger:
                targetGsHuman.setStockpile(ResourceType.Gold, targetGsHuman.getStockpile(ResourceType.Gold) * 2);
                targetGsHuman.addResourcePerTick(ResourceType.Gold, targetGsHuman.getChangePerTick(ResourceType.Gold) * 2);
                break;
            case AIPersonalityType.Pragmatist:
                foreach (ResourceType rt in targetGsHuman.getStockpileResourceTypes())
                {
                    targetGsHuman.setStockpile(rt, (int) (targetGsHuman.getStockpile(rt) * 1.3));
                }
                break;
            case AIPersonalityType.Warrior:
            case AIPersonalityType.ScienceGeek:
            case AIPersonalityType.Conservative:
            default:
                break;
        }
        return targetGsHuman;
    }

    private void setStartGameState()
    {
        BuildingGS initialGs = new BuildingGS();
        initialGs.setStockpile(ResourceType.Gold, 1000);
        initialGs.addResourcePerTick(ResourceType.Gold, 0);
        initialGs.setStockpile(ResourceType.Stone, 1000);
        initialGs.addResourcePerTick(ResourceType.Stone, 0);
        initialGs.setStockpile(ResourceType.Wood, 1000);
        initialGs.addResourcePerTick(ResourceType.Wood, 0);

        GameController.gameState = initialGs;
        aiCurrentGameState = new BuildingGS(initialGs);
    }


    private void setTargetGameStates()
    {
        BuildingGS targetGs = new BuildingGS();
        targetGs.setStockpile(ResourceType.Gold, 2000);
        targetGs.addResourcePerTick(ResourceType.Gold, 10);
        targetGs.setStockpile(ResourceType.Stone, 2000);
        targetGs.addResourcePerTick(ResourceType.Stone, 5);
        targetGs.setStockpile(ResourceType.Wood, 2000);
        targetGs.addResourcePerTick(ResourceType.Wood, 6);
        targetGs.setStockpile(ResourceType.Silver, 200);
        targetGs.addResourcePerTick(ResourceType.Silver, 9);
        targetGameStates.Add(targetGs);

        BuildingGS targetGs2 = new BuildingGS();
        targetGs2.setStockpile(ResourceType.Gold, 4000);
        targetGs2.addResourcePerTick(ResourceType.Gold, 10);
        targetGs2.setStockpile(ResourceType.Stone, 4000);
        targetGs2.addResourcePerTick(ResourceType.Stone, 5);
        targetGs2.setStockpile(ResourceType.Wood, 4000);
        targetGs2.addResourcePerTick(ResourceType.Wood, 6);
        targetGs2.setStockpile(ResourceType.Silver, 400);
        targetGs2.addResourcePerTick(ResourceType.Silver, 9);
        targetGameStates.Add(targetGs2);

        BuildingGS targetGs3 = new BuildingGS();
        targetGs3.setStockpile(ResourceType.Gold, 8000);
        targetGs3.addResourcePerTick(ResourceType.Gold, 20);
        targetGs3.setStockpile(ResourceType.Stone, 8000);
        targetGs3.addResourcePerTick(ResourceType.Stone, 20);
        targetGs3.setStockpile(ResourceType.Wood, 8000);
        targetGs3.addResourcePerTick(ResourceType.Wood, 20);
        targetGs3.setStockpile(ResourceType.Silver, 800);
        targetGs3.addResourcePerTick(ResourceType.Silver, 20);
        targetGameStates.Add(targetGs3);

        Debug.Log("target game states size: " + targetGameStates.Count);
        for (int indexActor = 0; indexActor < noOfActors; indexActor++)
        {
            updateTargetDisplay(targetGameStates[currentTargetGsIndex[indexActor]], indexActor);
        }
    }

    private void Update()
    {
        if (Time.time - lastGameTick > GameSetup.TICK_LENGHT_SEC)
        {
            // If it's been a while since our last game tick
            // Propogate an "in game tick"
            tick();
            lastGameTick = Time.time;
        }

    }

    private void tick()
    {
        aiCurrentGameState.timePasses(1);
        for (int indexActor = 0; indexActor < noOfActors; indexActor++)
        {
            if (isTargetReached(indexActor))
            {
                targetReached(indexActor);
            }
        }
        Debug.Log("Target reached bool: " + targetReachedBoolAi);
        updateAiResourceDisplay();
    }

    private void targetReached(int indexActor)
    {
        celebrateTargetReached(indexActor);
        planForNextTarget(indexActor);
    }

    private void planForNextTarget(int indexActor)
    {
        if (this.currentTargetGsIndex[indexActor] < targetGameStates.Count - 1)
        {
            currentTargetGsIndex[indexActor]++;
            if (indexActor == indexAi) {
                planNextTargetGsAi();
            }
            updateTargetDisplay(targetGameStates[currentTargetGsIndex[indexActor]], indexActor);
        }
        else
        {
            declareGameWon(indexActor);
        }
    }

    private void declareGameWon(int indexActor)
    {
        Debug.Log("Game won by player" + (indexActor + 1));
        targetReachedText.text = "Game won by: " + (indexActor + 1);
    }

    private void celebrateTargetReached(int indexActor)
    {
        Debug.Log("Taget reached for player " + (indexActor + 1));
        Debug.Log("Taget reached for player " + (indexActor + 1));
        Debug.Log("Taget reached for player " + (indexActor + 1));
        Debug.Log("Taget reached for player " + (indexActor + 1));
        targetReachedText.text = "Target reached for: " + (indexActor + 1);
        if (indexActor == indexAi) targetReachedBoolAi = true;
    }

    private bool isTargetReached(int indexActor)
    {
        BuildingGS gsToCheck = GameController.gameState;
        if (indexActor == indexAi) {
            gsToCheck = aiCurrentGameState;
        }
        BuildingGS targetGs = targetGameStates[currentTargetGsIndex[indexActor]];
        foreach (ResourceType rt in gsToCheck.getAllResourceTypes())
        {
            if ((gsToCheck.getStockpile(rt) < targetGs.getStockpile(rt))
                || (gsToCheck.getChangePerTick(rt) < targetGs.getChangePerTick(rt))) {
                return false;
            }
        }
        Debug.Log("Target reached for : " + indexActor);
        return true;
    }

    private void updateAiResourceDisplay()
    {
        Debug.Log("Updating ai resource display");
        Debug.Log(bank + ", " + stone + ", " + wood + ", " + silver);
        foreach (ResourceType rt in aiCurrentGameState.getAllResourceTypes())
        {
            //Debug.Log("update: " + rt + ", " + aiCurrentGameState.getStockpile(rt));
            aiResourceDisplay.updateCountAndRPT(rt, aiCurrentGameState.getStockpile(rt), aiCurrentGameState.getChangePerTick(rt));
        }
        aiResourceDisplay.updateWorkers();
    }

    private void updateTargetDisplay(BuildingGS gameState, int indexActor)
    {
        Debug.Log("actor to update target display: " + indexActor);
        ResourceDisplayController rdc = meTargetDisplay;
        if (indexActor == indexAi)
        {
            rdc = aiTargetDisplay;
        }
        foreach (ResourceType rt in gameState.getAllResourceTypes())
        {
            rdc.updateCountAndRPT(rt, gameState.getStockpile(rt), gameState.getChangePerTick(rt));
        }
    }


    private void doWork(Work nextAiWork)
    {
        //Debug.Log("Updating ai resource display");
        //foreach (ResourceType rt in aiCurrentGameState.getAllResourceTypes())
        //{
        //    Debug.Log("RT: " + rt + ", " + aiCurrentGameState.getStockpile(rt));
        //    aiResourceDisplay.updateCountAndRPT(rt, aiCurrentGameState.getStockpile(rt), aiCurrentGameState.getChangePerTick(rt));
        //}
        Debug.Log("Doing work: " + nextAiWork.workType);
        switch (nextAiWork.workType)
        {
            case EWork.BuildBuilding:
                startBuildBuilding(BuildingFactory.buildNew(nextAiWork.buildingType, -1, -1), nextAiWork.frameWait);
                break;
            case EWork.BuyAndAssignWorker:
                Debug.Log("Worker for: " + nextAiWork.buildingType);
                if (aiCurrentGameState.canBuyWorker())
                {
                    Debug.Log("can buy worker");
                    aiCurrentGameState.buyAndAssignWorker(nextAiWork.buildingType);
                    aiResourceDisplay.workerAssigned();
                    aiResourceDisplay.addTotalWorker();

                    switch (nextAiWork.buildingType)
                    {
                        case BuildingType.Bank:
                            bank++;
                            break;
                        case BuildingType.SilverMine:
                            silver++;
                            break;
                        case BuildingType.StoneMason:
                            stone++;
                            break;
                        case BuildingType.WoodCutter:
                            wood++;
                            break;
                    }
                }
                else
                {
                    Debug.Log("cannot buy worker");
                }
                doNextWorkAi();
                break;
            case EWork.Wait:
                StartCoroutine(aiWait(nextAiWork.frameWait));
                break;
            default:
                doNextWorkAi();
                break;
        }
    }

    private IEnumerator aiWait(int frameWait)
    {
        yield return new WaitForSeconds(frameWait * GameSetup.TICK_LENGHT_SEC);
        doNextWorkAi();
    }

    public void startBuildBuilding(IBuilding newBuilding, int frameWait)
    {
        // Spend the cash money 
        // NOTE: validation is done at button click time
        aiCurrentGameState.spendForBuilding(newBuilding);

        // Start the construction process of this new building
        StartCoroutine(waitForConstruction(newBuilding, frameWait));
    }

    private IEnumerator waitForConstruction(IBuilding newBuilding, int frameWait)
    {
        // Wait for the designated time
        yield return new WaitForSeconds(frameWait * GameSetup.TICK_LENGHT_SEC);
        forceBuildBuilding(newBuilding);
    }

    public void forceBuildBuilding(IBuilding newBuilding)
    {
        // Give the building to the resource manager logic
        // We've already spent for the building in the "startBuildBuilding" func
        aiCurrentGameState.forceAddBuilding(newBuilding);

        // Display the new resource stockpiles/ income per turn
        displayBuildingResouceDelta(newBuilding);
        doNextWorkAi();
    }

    private void displayBuildingResouceDelta(IBuilding newBuiding)
    {
        // Updates the resource display for only the resources this building cares about
        // IE: input, output and cost to build resources

        // Update the income per turn for the building
        List<ResourceType> allResources = newBuiding.outputResources();
        allResources.AddRange(newBuiding.inputResources());
        foreach (ResourceType rt in allResources)
        {
            int newStockpile = aiCurrentGameState.getStockpile(rt);
            int newRPT = aiCurrentGameState.getChangePerTick(rt);
            aiResourceDisplay.updateCountAndRPT(rt, newStockpile, newRPT);
        }

        // Update the cost to build the building
        foreach (ResourceChange rc in newBuiding.costToBuild())
        {
            if (allResources.Contains(rc.resourceType)) { continue; } // If we've already updated this resource type above ignore it
            int newStockpile = aiCurrentGameState.getStockpile(rc.resourceType);
            aiResourceDisplay.updateResourceCount(rc.resourceType, newStockpile);
        }
    }

}