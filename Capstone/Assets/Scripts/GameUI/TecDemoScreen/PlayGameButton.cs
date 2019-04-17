using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayGameButton : MonoBehaviour
{
    // Start is called before the first frame update
    void Start() {
        this.GetComponent<Button>().onClick.AddListener(delegate { onClick(); });
    }

    public void onClick() {
        SceneManager.LoadScene("SampleScene");
    }
}
