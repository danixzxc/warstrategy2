using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using System.IO;
using UnityEngine.Networking;

public class GameLogic : MonoBehaviour
{
    [SerializeField] private Transform end_game_place;

    [SerializeField] private Text health_text;

    [SerializeField] private Text wave_text;

    [SerializeField] private Text coins_text;

    [SerializeField] private int gameType = 0;

    [SerializeField] private string new_tower;

    [SerializeField] private int new_tower_level;

    [SerializeField] private string load_help = "";

    public int current_level = 1;

    public int coins_count = 100;

    public int base_health = 10;

    public int game_pack = 1;

    private bool endless_coins = false;

    public static GameLogic instance;

    private void Awake()
    {
        instance = this;
    }

    public void LogLevelFinish(
    int levelNumber,
    string result)
    {
        MadPixelAnalytics.AnalyticsManager.CustomEvent(
            "level_finish",
            new Dictionary<string, object> {
            {"level_number", levelNumber},
            {"result", result},
                {"bSendEventsBuffer", true }
            }
        );

    }

    public void LogLevelStart(
    int levelNumber)
    {
        MadPixelAnalytics.AnalyticsManager.CustomEvent(
            "level_start",
            new Dictionary<string, object> {
            {"level_number", levelNumber},
                {"bSendEventsBuffer", true }
            }
        );
    }
    private void Start()
    {
        if (current_level != 0)
        {
            LogLevelStart(current_level);
        }

        PlayerAudio.instance.music(false);
        end_game_place = GameObject.Find("end_game_place").transform;
        coins_text.text = "" + coins_count;
        health_text.text = "" + base_health;
    }

    public bool add_coins(int value)
    {
        int new_coins_value = coins_count + value;

        if (new_coins_value >= 0 || endless_coins)
        {
            coins_count = new_coins_value;
            coins_text.text = "" + new_coins_value;
            return true;
        }
        return false;
    }

    public void damage_base()
    {
        if (gameType == 0)
        {
            base_health--;
            if (base_health <= 0)
            {
                health_text.text = "0";
                GameController.instance.buttons_active = false;
                CameraMovement.instance.moving = false;
                Time.timeScale = 0;
                Addressables.InstantiateAsync("loose_panel", end_game_place);
                LogLevelFinish(current_level, "lose");
            }
            else
            {
                health_text.text = "" + base_health;
            }

            if (base_health == 9)
            {
#if PLATFORM_ANDROID
                Handheld.Vibrate();
#endif
            }
        }
    }

    public void next_wave(int current_wave, int last_wave)
    {
        if (current_wave >= last_wave)
        {
            if (base_health > 0)
            {
                if (gameType == 0) Invoke("win_logic", 3f);
                else Invoke("go_home", 3f);
            }
        }
        else
        {
            wave_text.text = (current_wave + 1) + "/" + last_wave;
        }
    }

    private void win_logic()
    {
        GameController.instance.buttons_active = false;
        CameraMovement.instance.moving = false;
        Time.timeScale = 0;

        Addressables.InstantiateAsync("win_panel", end_game_place).Completed += handle =>
        {
            int stars_count = 3;
            if (base_health < 10) stars_count = 2;
            if (base_health < 5) stars_count = 1;

            var lastLevel = PlayerPrefs.GetInt("LastCompletedLevelNum");
            if (current_level > lastLevel) PlayerPrefs.SetInt("LastCompletedLevelNum", current_level);


            LogLevelFinish(current_level, "win");

            if (new_tower != "")
            {
                int tower_level = PlayerPrefs.GetInt(new_tower);
                if (tower_level < new_tower_level)
                {
                    PlayerPrefs.SetInt(new_tower, new_tower_level);
                }
            }


            var win_logic = handle.Result.GetComponent<EndGameLogic>();
            if (load_help != "")
            {
                if (PlayerPrefs.GetInt(load_help + "_done") == 0) win_logic.help_name = load_help;
            }
            win_logic.set_stars(stars_count);


        };
    }

    private void go_home()
    {

            if (Checkpoints.current_save != null) Checkpoints.current_save = null;
            if (WavesCreator.instance) WavesCreator.waves = null;
            LevelLoader.instance.loadScene("episode_1");
        
    }
}
