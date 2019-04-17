using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text;
using System;

public class ResourceText : MonoBehaviour {
    private StringBuilder sb = new StringBuilder();

    private Text myText;
    private string opening;
    private string value;
    bool positiveCPT;
    private string changePerTurn;

    private void Awake()
    {
        this.myText = this.GetComponent<Text>();
    }

    public void updateText() {
        sb.Clear();
        sb.Append(opening);
        sb.Append(value);
        sb.Append(" ");
        if (positiveCPT) { sb.Append("+"); } // NOTE: negative integers already have a '-' char baked in
        sb.Append(changePerTurn);

        myText.text = sb.ToString();
    }

    public void setResourceType(ResourceType rt) {
        sb.Clear();
        sb.Append(Enum.GetName(typeof(ResourceType), rt));
        sb.Append(": ");
        this.opening = sb.ToString();
    }

    public ResourceText changeValue(int newValue) {
        this.value = Stringify.shortString(newValue);
        return this;
    }

    public ResourceText changeResourcePerTurn(int newRPT) {
        this.changePerTurn = Stringify.shortString(newRPT);
        this.positiveCPT = newRPT > 0;
        return this;
    }

    public ResourceText changeValueAndRPT(int newValue, int newRPT) {
        return changeValue(newValue).changeResourcePerTurn(newRPT);
    }
}

public static class Stringify {
    public static StringBuilder sb = new StringBuilder();

    public static string shortString(int x) {
        int absX = Mathf.Abs(x);
        if(absX < 10000) {
            // [0, 999]
            return x.ToString();
        } else if (absX < 1000000) {
            // [1k, 999k]
            return (x / 1000).ToString() + "." + ((x % 1000) / 100) + "K";
        } else {
            // [1M, infinity] 1000 000 000
            return (x / 1000000).ToString() + "M";
        }
    }


}
