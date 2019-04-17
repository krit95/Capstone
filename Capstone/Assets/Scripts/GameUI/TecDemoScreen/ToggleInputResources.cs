using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleInputResources : MonoBehaviour
{
    private Text buttonText;

    // Start is called before the first frame update
    void Start() {
        this.buttonText = this.transform.GetChild(0).GetComponent<Text>();
        this.GetComponent<Button>().onClick.AddListener(delegate { myClick(); });
    }


    public void myClick() {
        BuildingBlueprint.inputResources = !BuildingBlueprint.inputResources;
        if (BuildingBlueprint.inputResources)
            { this.buttonText.text = "Input Resources ON"; }
        else
            { this.buttonText.text = "Input Resources OFF"; }
    }
}
