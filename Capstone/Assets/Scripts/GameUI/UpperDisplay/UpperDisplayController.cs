using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpperDisplayController : MonoBehaviour {

    public Text indicator;

    public Button resourceDisplayButton;
    public Button targetDisplayButton;
    public Button AIDIsplayButton;
    public Button AITargetDisplayButton;

    public GameObject resourceDisplay;
    public GameObject targetDisplay;
    public GameObject aiDisplay;
    public GameObject aiTargetDisplay;

    public void Start() {
        this.resourceDisplayButton.onClick.AddListener(delegate { showDisplay(resourceDisplay, "My Resources"); });
        this.targetDisplayButton.onClick.AddListener(delegate { showDisplay(targetDisplay, "Target State"); });
        this.AIDIsplayButton.onClick.AddListener(delegate { showDisplay(aiDisplay, "AI Resources"); });
        this.AITargetDisplayButton.onClick.AddListener(delegate { showDisplay(aiTargetDisplay, "AI Target State"); });

        showDisplay(resourceDisplay, "My Resources");

    }

    private void hideAll() {
        resourceDisplay.gameObject.SetActive(false);
        targetDisplay.gameObject.SetActive(false);
        aiDisplay.gameObject.SetActive(false);
        aiTargetDisplay.gameObject.SetActive(false);
    }

    private void showDisplay(GameObject activeDisplay, string name) {
        hideAll();
        activeDisplay.SetActive(true);
        indicator.text = name;
    }

}
