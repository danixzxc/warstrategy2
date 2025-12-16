using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SettingsMenuScript : MonoBehaviour
{
    [SerializeField] private Button open_button;

    [SerializeField] private Button close_button;

    [SerializeField] private GameObject menu;

    [SerializeField] private Button[] music_volume_buttons;

    [SerializeField] private Button[] ui_effects_buttons;

    [SerializeField] private Button[] game_effects_buttons;

    [SerializeField] private Image[] music_volume_images;

    [SerializeField] private Image[] ui_effects_images;

    [SerializeField] private Image[] game_effects_images;

    [SerializeField] private Color off_color;


    private void Start()
    {
        open_button.onClick.AddListener(open_menu);
        close_button.onClick.AddListener(close_menu);
        foreach (var button in music_volume_buttons)
        {
            button.onClick.AddListener(music_volume);
        }
        foreach (var button in ui_effects_buttons)
        {
            button.onClick.AddListener(ui_volume);
        }
        foreach (var button in game_effects_buttons)
        {
            button.onClick.AddListener(game_volume);
        }
        set_start_values();
    }

    private void open_menu()
    {
        menu.SetActive(true);
    }

    private void close_menu()
    {
        menu.SetActive(false);
    }

    private void music_volume()
    {
        var music_volume_value = EventSystem.current.currentSelectedGameObject.name[0] - 48;
        PlayerPrefs.SetInt("music_volume", music_volume_value);
        PlayerAudio.instance.update_volume();

        for (int i = 0; i < 8; i++)
        {
            if (i <= music_volume_value) music_volume_images[i].color = Color.yellow;
            else music_volume_images[i].color = off_color;
        }
    }

    private void ui_volume()
    {
        var ui_volume_value = EventSystem.current.currentSelectedGameObject.name[0] - 48;
        PlayerPrefs.SetInt("ui_volume", ui_volume_value);
        PlayerAudio.instance.update_volume();

        for (int i = 0; i < 8; i++)
        {
            if (i <= ui_volume_value) ui_effects_images[i].color = Color.yellow;
            else ui_effects_images[i].color = off_color;
        }
    }

    private void game_volume()
    {
        var game_volume_value = EventSystem.current.currentSelectedGameObject.name[0] - 48;
        PlayerPrefs.SetInt("game_volume", game_volume_value);

        for (int i = 0; i < 8; i++)
        {
            if (i <= game_volume_value) game_effects_images[i].color = Color.yellow;
            else game_effects_images[i].color = off_color;
        }
    }

    private void set_start_values()
    {
        var music_volume_value = PlayerPrefs.GetInt("music_volume");
        for (int i = 0; i < 8; i++)
        {
            if (i <= music_volume_value) music_volume_images[i].color = Color.yellow;
            else music_volume_images[i].color = off_color;
        }

        var ui_volume_value = PlayerPrefs.GetInt("ui_volume");
        for (int i = 0; i < 8; i++)
        {
            if (i <= ui_volume_value) ui_effects_images[i].color = Color.yellow;
            else ui_effects_images[i].color = off_color;
        }

        var game_volume_value = PlayerPrefs.GetInt("game_volume");
        for (int i = 0; i < 8; i++)
        {
            if (i <= game_volume_value) game_effects_images[i].color = Color.yellow;
            else game_effects_images[i].color = off_color;
        }
    }
}
