using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using MadPixel;

public class LevelsMenu : MonoBehaviour
{
    [SerializeField] private RateGameLogic rateGameLogic;
    [SerializeField] private GameObject one_star_ref, two_stars_ref, three_stars_ref;

    [SerializeField] private Button[] levels_buttons;
    [SerializeField] private Image[] levels_buttons_images;
    [SerializeField] private Color lock_color;
    [SerializeField] private Button quit_button;

    [SerializeField] private Button show_banner_panel_button;
    [SerializeField] private Button close_banner_panel_button;
    [SerializeField] private Button activate_banner_button;
    [SerializeField] private GameObject banner_panel;
    [SerializeField] private GameObject chests_opened_icon;
    [SerializeField] private Image activate_banner_button_image;
    [SerializeField] private Text activate_banner_button_text;

    [SerializeField] private Text reward_text;
    [SerializeField] private Button reward_button;

    // Самый первый уровень в этрм списке.
    [SerializeField] private int first_buttons_level = 1;

    private int selected_level = 1;
    [SerializeField] private RectTransform buttons_panel;
    [SerializeField] private Transform canvas;
    [SerializeField] private float clamp0 = 1000;
    private float clamp1;

    private float hold_last_pos;
    private float hold_start_pos;


    private Vector2 target_position;

    void Start()
    {
        PlayerAudio.instance.music(true);

        clamp0 *= canvas.localScale.x;
        clamp1 = buttons_panel.transform.position.y;
        target_position = buttons_panel.transform.position;

        int unlocked_levels = 80; //= PlayerPrefs.GetInt("unlocked levels");
        int last_completed_level = PlayerPrefs.GetInt("LastCompletedLevelNum");
        for (int i = 0; i < levels_buttons.Length; i++)
        {
            var button = levels_buttons[i];
            int level = i + 1;
            if (level <= unlocked_levels)
            {
                if (level <= last_completed_level + 1)
                    button.onClick.AddListener(load_level);
                else
                {
                    levels_buttons_images[i].color = lock_color;
                    button.enabled = false;
                }

                int level_stars = PlayerPrefs.GetInt("level_" + level + "_stars");
                if (level_stars == 1)
                {
                    var result = Instantiate(one_star_ref, button.transform);
                }
                else if (level_stars == 2)
                {
                    var result = Instantiate(two_stars_ref, button.transform);
                }
                else if (level_stars == 3)
                {
                    var result = Instantiate(three_stars_ref, button.transform);
                }

                int reward_number = PlayerPrefs.GetInt("reward_count_" + level);
                if (reward_number >= 2)
                {
                    Instantiate(chests_opened_icon, button.transform);
                }
            }
            else
            {

            }
        }
        levels_buttons_images = null;
        quit_button.onClick.AddListener(quit_logic);
        //get_reward_button.onClick.AddListener();
        show_banner_panel_button.onClick.AddListener(show_banner_panel);
        close_banner_panel_button.onClick.AddListener(close_banner_panel);
        activate_banner_button.onClick.AddListener(activate_banner_pressed);
        reward_button.onClick.AddListener(take_reward);


        if (PlayerPrefs.GetInt("banners_activated") == 0)
        {
            activate_banner_button_image.color = Color.green;
         //   if (Player.instance.translations == null) 
            //    activate_banner_button_text.text = "Включить";
           // else
           activate_banner_button_text.text = Player.instance.translations[41];
        }
        else if (Player.instance.translations != null)
        {
           activate_banner_button_text.text = Player.instance.translations[42];

        }

        game_panels_logic(last_completed_level);
        count_reward();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            hold_last_pos = Input.mousePosition.y;
            hold_start_pos = hold_last_pos;
        }
        else if (Input.GetMouseButton(0))
        {
            var delta = Input.mousePosition.y - hold_last_pos;
            var new_position = target_position + new Vector2(0, delta);
            if (new_position.y >= clamp1 && new_position.y <= clamp0)
                target_position = new_position;
            hold_last_pos = Input.mousePosition.y;
        }
        else if (Input.GetMouseButtonUp(0))
        {

        }

        if ((Vector2)buttons_panel.transform.position != target_position)
            buttons_panel.transform.position = Vector2.Lerp(buttons_panel.transform.position, target_position, .2f);
    }

    private void load_level()
    {
        if (Mathf.Abs(hold_start_pos - Input.mousePosition.y) < 10)
        {
            PlayerAudio.instance.play(1);
            LevelLoader.instance.loadScene(EventSystem.current.currentSelectedGameObject.name);
        }
    }

    private void OnAdDismissed(bool success)
    {
        PlayerAudio.instance.play(1);
        LevelLoader.instance.loadScene(EventSystem.current.currentSelectedGameObject.name);
    }

    private void quit_logic()
    {
        Application.Quit();
    }

    private void game_panels_logic(int last_completed_level)
    {
        if (last_completed_level == 12)
        {
            if (PlayerPrefs.GetInt("Expert taken") != 1) rateGameLogic.show_panel(0);
        }
        else if (last_completed_level == 14 || last_completed_level == 15)
        {
            if (PlayerPrefs.GetInt("Rate pressed") != 1) rateGameLogic.show_panel(1);
        }
    }

    private void show_banner_panel()
    {
        print("Show panel");
        banner_panel.SetActive(true);
    }

    private void close_banner_panel()
    {
        print("Close panel");
        banner_panel.SetActive(false);
    }

    private void activate_banner_pressed()
    {
        print("Activate");
        var new_banner_status = (PlayerPrefs.GetInt("banners_activated") + 1) % 2;
        if (new_banner_status == 0)
        {
            activate_banner_button_image.color = Color.green;
            //if (Player.instance.translations == null) 
                //activate_banner_button_text.text = "Включить";
            //else
            activate_banner_button_text.text = Player.instance.translations[41];
            //FirebaseManager.instance.show_banner(false);
            MadPixelAnalytics.AnalyticsManager.CustomEvent("disable_banner",
        new Dictionary<string, object>() {
        {"param", "value"}
        });
        }
        else
        {
            activate_banner_button_image.color = Color.red;
           // if (Player.instance.translations == null) 
               // activate_banner_button_text.text = "Выключить";
           // else
           activate_banner_button_text.text = Player.instance.translations[42];
            //   FirebaseManager.instance.show_banner(true);
            MadPixelAnalytics.AnalyticsManager.CustomEvent("activate_banner",
        new Dictionary<string, object>() {
        {"param", "value"}
        });
        }
        PlayerPrefs.SetInt("banners_activated", new_banner_status);
    }

    private void count_reward()
    {
        string last_reward = PlayerPrefs.GetString("Get reward money time");
        //string last_reward = "06-03-2022   12:35";
        string date = DateTime.UtcNow.ToLocalTime().ToString("MM-dd-yyyy   HH:mm");

        if (last_reward != "")
        {
            DateTime dateTime = DateTime.Parse(last_reward);
            int difference = (int)(DateTime.UtcNow.ToLocalTime() - dateTime).TotalMinutes;
            print("dif - " + difference);
            if (difference > 1430)
            {

            }
        }

        PlayerPrefs.SetString("Get reward money time", date);
    }

    private void take_reward()
    {
        // AppodealManager.instance.show_reward();
    }
}
