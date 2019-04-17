using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuyWorkerButton : MonoBehaviour
{
    Button myButton;

    public void Start()
    {
        myButton = GetComponent<Button>();
        myButton.onClick.AddListener(delegate { myClick(); });
    }

    private void forceTurnOff() { myButton.interactable = false; }
    private void forceTurnOn()  { myButton.interactable = true; }
    public void tryTurnOn() {
        forceTurnOff();
        if (GameController.singleton.canBuyWorker()) {
            forceTurnOn();
        }
    }

    private void myClick() {
        GameController.singleton.forceBuyWorker();
        tryTurnOn();
    }
}
