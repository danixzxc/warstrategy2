using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RateGameLogic : MonoBehaviour
{
    [SerializeField] private GameObject td_expert_panel, rate_panel;
    [SerializeField] private Button rate_game_button, skip_rate_button, get_expert_reward_button;

    private void Start()
    {
        rate_game_button.onClick.AddListener(rate_game);
        skip_rate_button.onClick.AddListener(skip_rate);
        get_expert_reward_button.onClick.AddListener(get_expert_reward);
    }

    private void rate_game()
    {
        PlayerPrefs.SetInt("Rate pressed", 1);
        PlayerPrefs.SetInt("Skip ad", 2);
        Application.OpenURL("market://details?id=com.Hoody.warstrategytwo");
        rate_panel.SetActive(false); 
        LogRateUs(5);

    }

    private void skip_rate()
    {
        rate_panel.SetActive(false);
        PlayerPrefs.SetInt("Skip ad", 1);
        LogRateUs(1);
    }

    private void get_expert_reward()
    {
        Player.instance.add_money(250000);
        td_expert_panel.SetActive(false);
        PlayerPrefs.SetInt("Skip ad", 7);
        PlayerPrefs.SetInt("Expert taken", 1);
    }

    public void show_panel(int expert_or_rate)
    {
        if (expert_or_rate == 0)
        {
           // FirebaseManager.instance.logEvent("show_expert_panel");
            td_expert_panel.SetActive(true);
        }
        else
        {
           // FirebaseManager.instance.logEvent("show_rate_panel");
            rate_panel.SetActive(true);
        }
    }

    public void LogRateUs(int rateResult)
    {
        int clampedRate = Mathf.Clamp(rateResult, 0, 5);

        MadPixelAnalytics.AnalyticsManager.CustomEvent(
            "rate_us",
            new Dictionary<string, object> {
                {"rate_result", clampedRate}
            }
        );

    }
}
