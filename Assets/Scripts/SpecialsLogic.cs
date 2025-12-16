using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;

public class SpecialsLogic : MonoBehaviour
{
    [SerializeField] private Button portal_button;

    [SerializeField] private Button bomb_button;

    [SerializeField] private Button missile_button;

    [SerializeField] private GameObject portal;

    private GameObject bomb;

    private GameObject missile;

    [SerializeField] private Image portal_load_image;

    [SerializeField] private Image bomb_load_image;

    [SerializeField] private Image missile_load_image;

    [SerializeField] private Image portal_button_image;

    [SerializeField] private Image bomb_button_image;

    [SerializeField] private Image missile_button_image;

    [SerializeField] private int portal_reload_time = 3600;

    [SerializeField] private int bomb_reload_time = 4545;

    private int missile_reload_time = 2600;

    [SerializeField] private int portal_working_time = 0;

    private bool portal_working = false;

    private int portal_work_current_time = 0;

    [HideInInspector] public int[] current_load_timers = new int[3];

    public static SpecialsLogic instance;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        print("strike: " + PlayerPrefs.GetInt("missile_strike_on"));
        if (PlayerPrefs.GetInt("missile_strike_on") == 1)
        {
            print("missile");
            missile_button.gameObject.SetActive(true);
        }

        portal_button.onClick.AddListener(portal_pressed);
        bomb_button.onClick.AddListener(bomb_pressed);
        missile_button.onClick.AddListener(missile_pressed);

        portal_button_image.color = Color.gray;
        bomb_button_image.color = Color.gray;
        missile_button_image.color = Color.gray;

        if (Checkpoints.current_save != null)
        {
            current_load_timers = (int[])Checkpoints.current_save.specials_timers.Clone();

            if (current_load_timers[0] >= portal_reload_time)
            {
                portal_button_image.color = Color.white;
                portal_load_image.fillAmount = 1;
            }
            if (current_load_timers[1] >= bomb_reload_time)
            {
                bomb_button_image.color = Color.white;
                bomb_load_image.fillAmount = 1;
            }
            if (current_load_timers[2] >= missile_reload_time)
            {
                missile_button_image.color = Color.white;
                missile_load_image.fillAmount = 1;
            }
        }
        string name = (GameLogic.instance.game_pack == 1) ? "" : $"pack_{GameLogic.instance.game_pack}_";
        Addressables.LoadAssetAsync<GameObject>(name + "bomb_special").Completed += handle =>
        {
            bomb = handle.Result;
        };

        Addressables.LoadAssetAsync<GameObject>(name + "missile_special").Completed += handle =>
        {
            missile = handle.Result;
        };

    }


    private void FixedUpdate()
    {
        if (current_load_timers[0] < portal_reload_time)
        {
            current_load_timers[0]++;
            portal_load_image.fillAmount = (float)current_load_timers[0] / portal_reload_time;
            if (current_load_timers[0] >= portal_reload_time)
            {
                portal_button_image.color = Color.white;
            }
        }

        if (current_load_timers[1] < bomb_reload_time)
        {
            current_load_timers[1]++;
            bomb_load_image.fillAmount = (float)current_load_timers[1] / bomb_reload_time;
            if (current_load_timers[1] >= bomb_reload_time)
            {
                bomb_button_image.color = Color.white;
            }
        }

        if (current_load_timers[2] < missile_reload_time)
        {
            current_load_timers[2]++;
            missile_load_image.fillAmount = (float)current_load_timers[2] / missile_reload_time;
            if (current_load_timers[2] >= missile_reload_time)
            {
                missile_button_image.color = Color.white;
            }
        }

        if (portal_working)
        {
            portal_work_current_time++;
            if (portal_work_current_time >= portal_working_time)
            {
                EnemiesLogic.instance.SetPortal(false, GameController.instance.active_zone);
                portal_working = false;
                portal_work_current_time = 0;
                portal.SetActive(false);
            }
        }
    }

    private void portal_pressed()
    {
        if (GameController.instance.asktutorial($"portal"))
        {
            if (current_load_timers[0] >= portal_reload_time)
            {
                portal.transform.position = (Vector2)GameController.instance.active_zone;
                portal.SetActive(true);
                EnemiesLogic.instance.SetPortal(true, GameController.instance.active_zone);
                portal_button_image.color = Color.gray;
                current_load_timers[0] = 0;

                portal_working = true;
            }
            GameController.instance.hide_buttons();
        }
    }

    private void bomb_pressed()
    {
        if (bomb)
            if (GameController.instance.asktutorial($"bomb"))
            {
                if (current_load_timers[1] >= bomb_reload_time)
                {
                    var new_bomb = Instantiate(bomb);
                    new_bomb.transform.position = (Vector2)GameController.instance.active_zone;
                    bomb_button_image.color = Color.gray;
                    current_load_timers[1] = 0;
                }
                GameController.instance.hide_buttons();
            }
    }

    private void missile_pressed()
    {
        if (missile)
            if (GameController.instance.asktutorial($"missile"))
            {
                if (current_load_timers[2] >= missile_reload_time)
                {
                    var new_missile = Instantiate(missile);
                    new_missile.transform.position = (Vector2)GameController.instance.active_zone;
                    missile_button_image.color = Color.gray;
                    current_load_timers[2] = 0;
                }
                GameController.instance.hide_buttons();
            }
    }
}
