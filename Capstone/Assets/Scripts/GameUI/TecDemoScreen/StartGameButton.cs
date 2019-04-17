using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StartGameButton : MonoBehaviour {

    public InputField initialGoldStock;
    public InputField initialStoneStock;
    public InputField initialWoodStock;
    public InputField initialSilverStock;
    public InputField initialIronStock;
    public InputField initialCoalStock;
    public InputField initialSteelStock;

    public InputField targetGoldStock;
    public InputField targetStoneStock;
    public InputField targetWoodStock;
    public InputField targetSilverStock;
    public InputField targetIronStock;
    public InputField targetCoalStock;
    public InputField targetSteelStock;

    public InputField targetGoldIncome;
    public InputField targetStoneIncome;
    public InputField targetWoodIncome;
    public InputField targetSilverIncome;
    public InputField targetIronIncome;
    public InputField targetCoalIncome;
    public InputField targetSteelIncome;


    // Start is called before the first frame update
    void Start() {
        this.GetComponent<Button>().onClick.AddListener(delegate { onClick(); });
    }


    public void onClick() {
        int igs;
        int istos;
        int iws;
        int isils;
        int iis;
        int ics;
        int istes;

        int tgs;
        int tstos;
        int tws;
        int tsils;
        int tis;
        int tcs;
        int tstes;

        int tgcpt;
        int tstocpt;
        int twcpt;
        int tsilcpt;
        int ticpt;
        int tccpt;
        int tstecpt;

        try  {
            igs =   int.Parse(initialGoldStock  .text);
            istos = int.Parse(initialStoneStock .text);
            iws =   int.Parse(initialWoodStock  .text);
            isils = int.Parse(initialSilverStock.text);
            iis =   int.Parse(initialIronStock  .text);
            ics =   int.Parse(initialCoalStock  .text);
            istes = int.Parse(initialSteelStock .text);

            tgs =   int.Parse(targetGoldStock  .text);
            tstos = int.Parse(targetStoneStock .text);
            tws =   int.Parse(targetWoodStock  .text);
            tsils = int.Parse(targetSilverStock.text);
            tis =   int.Parse(targetIronStock  .text);
            tcs =   int.Parse(targetCoalStock  .text);
            tstes = int.Parse(targetSteelStock .text);

            tgcpt =   int.Parse(targetGoldIncome  .text);
            tstocpt = int.Parse(targetStoneIncome .text);
            twcpt =   int.Parse(targetWoodIncome  .text);
            tsilcpt = int.Parse(targetSilverIncome.text);
            ticpt =   int.Parse(targetIronIncome  .text);
            tccpt =   int.Parse(targetCoalIncome  .text);
            tstecpt = int.Parse(targetSteelIncome .text);

            BuildingGS initialGS = new BuildingGS();
            if (igs != 0) { initialGS.setStockpile(ResourceType.Gold, igs); }
            if (istos != 0) { initialGS.setStockpile(ResourceType.Stone, istos); }
            if (iws != 0) { initialGS.setStockpile(ResourceType.Wood, iws); }
            if (isils != 0) { initialGS.setStockpile(ResourceType.Silver, isils); }
            if (iis != 0) { initialGS.setStockpile(ResourceType.Iron, iis); }
            if (ics != 0) { initialGS.setStockpile(ResourceType.Coal, ics); }
            if (istes != 0) { initialGS.setStockpile(ResourceType.Steel, istes); }

            BuildingGS targetGS = new BuildingGS();
            if (tgs != 0) { targetGS.setStockpile(ResourceType.Gold, tgs); }
            if (tstos != 0) { targetGS.setStockpile(ResourceType.Stone, tstos); }
            if (tws != 0) { targetGS.setStockpile(ResourceType.Wood, tws); }
            if (tsils != 0) { targetGS.setStockpile(ResourceType.Silver, tsils); }
            if (tis != 0) { targetGS.setStockpile(ResourceType.Iron, tis); }
            if (tcs != 0) { targetGS.setStockpile(ResourceType.Coal, tcs); }
            if (tstes != 0) { targetGS.setStockpile(ResourceType.Steel, tstes); }

            if (tgcpt != 0) { targetGS.addResourcePerTick(ResourceType.Gold, tgcpt); }
            if (tstocpt != 0) { targetGS.addResourcePerTick(ResourceType.Stone, tstocpt); }
            if (twcpt != 0) { targetGS.addResourcePerTick(ResourceType.Wood, twcpt); }
            if (tsilcpt != 0) { targetGS.addResourcePerTick(ResourceType.Silver, tsilcpt); }
            if (ticpt != 0) { targetGS.addResourcePerTick(ResourceType.Iron, ticpt); }
            if (tccpt != 0) { targetGS.addResourcePerTick(ResourceType.Coal, tccpt); }
            if (tstecpt != 0) { targetGS.addResourcePerTick(ResourceType.Steel, tstecpt); }

            MidLevelManager.targetGS = targetGS;
            MidLevelManager.initialGS = initialGS;
            SceneManager.LoadScene("AIControlled");

        } catch (System.FormatException e) {

        }
    }

}
