using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResourceDisplayController : MonoBehaviour {
    

    public ResourceText textPrefab;
    public GameObject textContainer;
    private  Dictionary<ResourceType, ResourceText> textBoxes;

    public Text tickText;
    private int tickCount = 0;

    public Text workerText;
    private int totalWorkers = 0;
    private int busyWorkers = 0;
    
    // Start is called before the first frame update
    void Awake() {
        textBoxes = new Dictionary<ResourceType, ResourceText>();
    }
    
    public void updateTick() {
        tickCount++;
        tickText.text = "Tick: " + tickCount;
    }

    public void updateResourceCount(ResourceType rt, int newVal) {
        validateResourceType(rt);
        textBoxes[rt].changeValue(newVal).updateText(); 
    }

    public void updateCountAndRPT(ResourceType rt, int newCount, int newRPT) {
        validateResourceType(rt);
        textBoxes[rt].changeValueAndRPT(newCount, newRPT).updateText();
    }

    public void addTotalWorker() {
        this.totalWorkers++;
        updateWorkers();
    }
    public void workerAssigned() {
        this.busyWorkers++;
        updateWorkers();
    }
    public void workerUnAssigned() { 
        this.busyWorkers--;
        updateWorkers();
    }

    public void updateWorkers() {  workerText.text = "Workers: " + busyWorkers + " / " + totalWorkers; }


    private void validateResourceType(ResourceType rt) {
        if ( ! textBoxes.ContainsKey(rt)) {
            ResourceText newResourceType = Instantiate(textPrefab);
            newResourceType.setResourceType(rt);
            newResourceType.transform.parent = textContainer.transform;

            textBoxes.Add(rt, newResourceType);
        }
    }


    public void testDisplay() {
        this.updateCountAndRPT(ResourceType.Gold, 100, 100);
        this.updateCountAndRPT(ResourceType.Wood, 77, -1);
        this.updateCountAndRPT(ResourceType.Stone, 1000000, 500);
    }

}
