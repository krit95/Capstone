using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstructionResizer : MonoBehaviour {


    // Start is called before the first frame update
    void Start() {
        float newHeight = 0.0f;
        for (int i = 0; i < this.transform.childCount; i++) {
            Transform child = this.transform.GetChild(i);
            newHeight += child.GetComponent<RectTransform>().sizeDelta.y;
        }

        RectTransform myTransform = this.GetComponent<RectTransform>();
        Vector2 newSizeDelta = new Vector2(myTransform.sizeDelta.x, newHeight);
        myTransform.sizeDelta = newSizeDelta;
    }
}
