using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class AiPersonalityButton : MonoBehaviour {
    public AIPersonalityType value;

    private Button myButton;
    private HomeScreenController homeScreenController;

    public void Start() {
        this.myButton = this.GetComponent<Button>();
        myButton.interactable = true;

        myButton.onClick.AddListener(delegate { myClick(); });
    }
    
    public void setup(AIPersonalityType value, HomeScreenController controller) {
        this.homeScreenController = controller;
        this.value = value;
        Text myText = this.transform.GetChild(0).GetComponent<Text>();
        myText.text = this.value.ToString();
    }

    public void forceTurnOn()  { this.myButton.interactable = true; }
    public void forceTurnOff() { this.myButton.interactable = false; ; }

    private void myClick() {
        homeScreenController.updatePersonality(this);
    }
}
