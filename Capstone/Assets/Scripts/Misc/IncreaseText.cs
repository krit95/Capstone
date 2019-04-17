using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IncreaseText : MonoBehaviour
{
    Text myText;
    float count = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        myText = GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        count += 0.1f;
        myText.text = ((int)count).ToString();
    }
}
