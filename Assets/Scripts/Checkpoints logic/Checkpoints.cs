using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Checkpoints : MonoBehaviour
{
    [SerializeField] private Button save_checkpoint_button;
    [SerializeField] private Button load_checkpoint_button;

    public static Checkpoint_save current_save;

    private Checkpoint_save current_wave_save;

    public static Checkpoints instance;

    private void Awake()
    {
        instance = this;
        if (current_save != null)
        {
            GameLogic.instance.base_health = current_save.health;
            GameLogic.instance.coins_count = current_save.coins;
        }
    }
    private void Start()
    {
        if (save_checkpoint_button && load_checkpoint_button)
        {
            save_checkpoint_button.onClick.AddListener(save_checkpoint);
            load_checkpoint_button.onClick.AddListener(load_checkpoint);
        }
    }

    private void save_checkpoint()
    {
        if (Container.instance.game_loaded && current_wave_save != null)
        {
            current_save = new Checkpoint_save();

            current_save.map_info = (int[])current_wave_save.map_info.Clone();
            current_save.wave = current_wave_save.wave;
            current_save.health = current_wave_save.health;
            current_save.coins = current_wave_save.coins;
            current_save.specials_timers = (int[])current_wave_save.specials_timers.Clone();
        }
    }

    private void load_checkpoint()
    {
        if (current_save != null)
        {
            MadPixelAnalytics.AnalyticsManager.CustomEvent(
    "load_checkpoint_used",
    new Dictionary<string, object>() {
        {"level", GameLogic.instance.current_level }
    }
);
            LevelLoader.instance.loadScene("level_" + GameLogic.instance.current_level);
        }
    }

    public void save_wave_check_point()
    {
        current_wave_save = new Checkpoint_save();
        current_wave_save.map_info = MapInfo.instance.get_map_info();
        current_wave_save.wave = WavesLogic.instance.current_wave;
        current_wave_save.health = GameLogic.instance.base_health;
        current_wave_save.coins = GameLogic.instance.coins_count;
        current_wave_save.specials_timers = (int[])SpecialsLogic.instance.current_load_timers.Clone();
    }

    // Вызавается сразу после загрузки уровня и выполняется если это был переход к чекпоинту.
    public void load_checkpoint_to_game()
    {
        if (current_save != null)
        {
            MapInfo.instance.set_map_info(current_save.map_info);
        }
    }
}
