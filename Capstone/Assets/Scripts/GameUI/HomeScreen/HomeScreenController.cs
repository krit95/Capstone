using System;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HomeScreenController : MonoBehaviour
{

    public CampaignTypeButton campaignButtonPrefab;
    public AiPersonalityButton personalityButtonPrefab;

    public GameObject campaignButtonHolder;
    public GameObject personalityButtonHolder;

    public AIPersonalityType aIPersonality;
    public CampaignType campaignType;

    public List<CampaignTypeButton> campaignTypeButtons;
    public List<AiPersonalityButton> aiPersonalityTypeButtons;
    public Button startGameButton;

    // Start is called before the first frame update
    void Start() {
        foreach (CampaignType campaign in Enum.GetValues(typeof(CampaignType))) {
            CampaignTypeButton button = Instantiate<CampaignTypeButton>(campaignButtonPrefab);
            button.setup(campaign, this);
            campaignTypeButtons.Add(button);
            button.transform.SetParent(campaignButtonHolder.transform);
        }

        foreach (AIPersonalityType personality in Enum.GetValues(typeof(AIPersonalityType))) {
            AiPersonalityButton button = Instantiate<AiPersonalityButton>(personalityButtonPrefab);
            button.setup(personality, this);
            aiPersonalityTypeButtons.Add(button);
            button.transform.SetParent(personalityButtonHolder.transform);
        }

        startGameButton.onClick.AddListener(delegate { startGame(); });
    }

    private void startGame() {
        startGameButton.interactable = false;
        God.campaignType = campaignType;
        God.aiPersonality = aIPersonality;

        // Move to next scene
        SceneManager.LoadScene("SampleScene");
    }
    

    public void updatePersonality(AiPersonalityButton personalityButton) {
        this.aIPersonality = personalityButton.value;
        foreach(AiPersonalityButton b in this.aiPersonalityTypeButtons) {
            b.forceTurnOn();
        }
        personalityButton.forceTurnOff();
    }

    public void updateCampaignType(CampaignTypeButton campaignButton) {
        this.campaignType = campaignButton.value;
        foreach (CampaignTypeButton b in this.campaignTypeButtons) {
            b.forceTurnOn();
        }
        campaignButton.forceTurnOff();
    }



}
