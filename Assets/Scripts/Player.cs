using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class Player : MonoBehaviour
{
    public static Player instance;

    public int money;

    public string[] translations;

    [SerializeField] private TextAsset[] translations_assets;

    [SerializeField] private TextAsset test;

    private void Awake()
    {
        if (instance != null) Destroy(gameObject);
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

#if UNITY_EDITOR

#else
            QualitySettings.vSyncCount = 2;  
            Application.targetFrameRate = 33;
#endif

            int session_number = PlayerPrefs.GetInt("Session num");
            print(session_number);
           // if (session_number == 0)
            if(true) //cheat build
            {
                PlayerPrefs.SetInt("tower_0_level", 2);
                PlayerPrefs.SetInt("tower_1_level", 2);

                PlayerPrefs.SetInt("tower_0_level", 3);
                PlayerPrefs.SetInt("tower_1_level", 3);
                PlayerPrefs.SetInt("tower_2_level", 3);
                PlayerPrefs.SetInt("tower_3_level", 3);
                PlayerPrefs.SetInt("tower_4_level", 3);
                PlayerPrefs.SetInt("tower_5_level", 3);
                PlayerPrefs.SetInt("tower_6_level", 3);
                PlayerPrefs.SetInt("tower_7_level", 3);
                PlayerPrefs.SetInt("tower_8_level", 3);
                PlayerPrefs.SetInt("tower_9_level", 3);
                PlayerPrefs.SetInt("LastCompletedLevelNum", 55);
                for (int i = 1; i <= 55; i++)
                {
                    PlayerPrefs.SetInt("level_" + i + "_stars", 3);
                    PlayerPrefs.SetInt("reward_count_" + i, 2);
                }
               

                PlayerPrefs.SetInt("music_volume", 5);
                PlayerPrefs.SetInt("ui_volume", 5);
                PlayerPrefs.SetInt("game_volume", 5);
            }
            PlayerPrefs.SetInt("Session num", session_number + 1);

            money = PlayerPrefs.GetInt("money");

           /* if (Application.systemLanguage == SystemLanguage.Russian || Application.systemLanguage == SystemLanguage.Ukrainian || Application.systemLanguage == SystemLanguage.Belarusian || Application.systemLanguage == SystemLanguage.Estonian)
            {
                translations = null;
            }
            else if (Application.systemLanguage == SystemLanguage.Portuguese)
            {
                translations = JsonUtility.FromJson<dictionary>(translations_assets[1].text).words;
            }
            else
            {
           */
                translations = JsonUtility.FromJson<dictionary>(translations_assets[0].text).words;
           // }

            if (test != null) translations = JsonUtility.FromJson<dictionary>(test.text).words;

            translations_assets = null;
        }
    }

    private class dictionary
    {
        public string[] words;
    }

    public bool add_money(int value)
    {
        if (money + value >= 0)
        {
            money += value;
            PlayerPrefs.SetInt("money", money);
            return true;
        }
        return false;
    }

    public string get_money()
    {
        if (money < 1000) return "" + money;
        else if (money < 1000000) return "" + (money / 1000) + "." + (money / 100 % 10) + "K";
        else if (money < 1000000000) return "" + (money / 1000000) + "." + (money / 100000 % 10) + "M";
        else return "" + (money / 1000000000) + "." + (money / 100000000 % 10) + "B";
    }
}
