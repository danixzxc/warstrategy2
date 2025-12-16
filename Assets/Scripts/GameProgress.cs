using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameProgress : MonoBehaviour
{
    [SerializeField] private bool clean_progress = false;
    [SerializeField] private bool unlock_all_towers = false;
    [SerializeField] private bool three_stars = false;
    [SerializeField] private bool unlock_to_rate_panels = false;
    [SerializeField] private bool give_x2 = false;
    [SerializeField] private bool noad = false;
    [SerializeField] private bool _return_progress = false;

    void Update()
    {
        if (_return_progress) return_progress();

        if (clean_progress)
        {
            PlayerPrefs.DeleteAll();
            clean_progress = false;
        }
        if (unlock_all_towers)
        {
            PlayerPrefs.SetInt("tower_0_level", 3);
            PlayerPrefs.SetInt("tower_1_level", 3);
            PlayerPrefs.SetInt("tower_2_level", 3);
            PlayerPrefs.SetInt("tower_3_level", 3);
            PlayerPrefs.SetInt("tower_4_level", 3);
            PlayerPrefs.SetInt("LastCompletedLevelNum", 55);
            unlock_all_towers = false;

            if (three_stars)
            for (int i = 1; i <= 55; i++)
            {
                PlayerPrefs.SetInt("level_" + i + "_stars", 3);
                PlayerPrefs.SetInt("reward_count_" + i, 2);
            }
        }
        if (unlock_to_rate_panels)
        {
            PlayerPrefs.SetInt("tower_0_level", 3);
            PlayerPrefs.SetInt("tower_1_level", 3);
            PlayerPrefs.SetInt("tower_2_level", 3);
            PlayerPrefs.SetInt("tower_3_level", 3);
            PlayerPrefs.SetInt("tower_4_level", 3);
            PlayerPrefs.SetInt("LastCompletedLevelNum", 12);
            unlock_to_rate_panels = false;
        }
        if (give_x2)
        {
            PlayerPrefs.SetInt("money_x2_on", 1);
            PlayerPrefs.SetInt("bought_5", 1);
            give_x2 = false;
        }
        if (noad)
        {
            PlayerPrefs.SetInt("no_ads_on", 1);
            PlayerPrefs.SetInt("bought_8", 1);
            noad = false;
        }
    }

    private void return_progress()
    {
        if (PlayerPrefs.GetInt("repro") == 0)
        {
            for (int i = 1; i <= 38; i++)
            {
                PlayerPrefs.SetInt("level_" + i + "_stars", 3);
                PlayerPrefs.SetInt("reward_count_" + i, 1);
            }
            PlayerPrefs.SetInt("LastCompletedLevelNum", 38);
            PlayerPrefs.SetInt("no_ads_on", 1);
            PlayerPrefs.SetInt("bought_8", 1);
            Player.instance.add_money(50000000);
            PlayerPrefs.SetInt("tower_0_level", 3);
            PlayerPrefs.SetInt("tower_1_level", 3);
            PlayerPrefs.SetInt("tower_2_level", 3);
            PlayerPrefs.SetInt("tower_3_level", 3);
            PlayerPrefs.SetInt("tower_4_level", 3);
            PlayerPrefs.SetInt("repro", 10);
            LevelLoader.instance.loadScene("episode_1");
        }
    }
}
