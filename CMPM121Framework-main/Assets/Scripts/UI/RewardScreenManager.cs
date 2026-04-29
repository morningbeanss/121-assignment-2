using UnityEngine;
using Unity.VisualScripting;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Rendering;

public class RewardScreenManager : MonoBehaviour
{
    public GameObject rewardUI;
    public Image level_selector;
    public GameObject button;

    //pop up text variables
    public TextMeshProUGUI waveTxt; //tell players how many waves they've completed
    public TextMeshProUGUI healthTxt; //tell players their hp

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameObject waveButt = Instantiate(button, level_selector.transform);
        waveButt.transform.localPosition = new Vector3(0, 0);
        //waveButt.GetComponent<MenuSelectorController>().spawner = this;
        waveButt.GetComponent<MenuSelectorController>().SetLevel("Start Next Wave");

        //these lines below to get the inspector to recognize the object or smth?
        if (waveTxt == null)
        {
            waveTxt = GameObject.Find("WaveText")?.GetComponent<TextMeshProUGUI>();
        }

        if (healthTxt == null)
        {
            healthTxt = GameObject.Find("HealthText")?.GetComponent<TextMeshProUGUI>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.Instance.state == GameManager.GameState.WAVEEND)
        {
            updateRewardUI(); //function to update ui text every time 
            rewardUI.SetActive(true);
        }
        else
        {
            rewardUI.SetActive(false);
        }
    }

    void updateRewardUI()
    {
        //update wave count info
        if (waveTxt != null)
        {
            int wavesCompleted = (Current wave) -1;
            waveTxt.text = $"Waves Completed: {wavesCompleted}";
        }

        if (healthTxt != null)
        {
            int currentHealth = (currentHealth);
            int maxHealth = (maxHealth healt);
            healthTxt.text = $"Health: {currentHealth}/{maxHealth}";
        }
    }
}
