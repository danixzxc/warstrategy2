using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAudio : MonoBehaviour
{
    [SerializeField] private AudioSource music_source;

    [SerializeField] private AudioSource ui_source;

    [SerializeField] private AudioClip[] ui_souds;

    private float music_volume = 1;

    private bool music_off = false;

    private float music_off_coef = 1;

    public static PlayerAudio instance;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else Destroy(this);
    }

    private void Start()
    {
        update_volume();
    }

    public void music(bool active)
    {
        if (active)
        {
            music_source.Play();
            if (music_off)
            {
                music_off = false;
                music_off_coef = 1;
                music_source.volume = music_volume;
            }
        }
        else
        {
            music_off = true;
        }
    }

    public void play(int index)
    {
        if (index < ui_souds.Length)
        {
            ui_source.clip = ui_souds[index];
            ui_source.Play();
        }
    }

    private void Update()
    {
        if (music_off)
        {
            music_off_coef -= Time.unscaledDeltaTime * .3f;
            if (music_off_coef <= 0)
            {
                music_off = false;
                music_source.Stop();
                music_source.volume = music_volume;
                music_off_coef = 1;
            }
            else
                music_source.volume = music_volume * music_off_coef;
        }
    }

    public void update_volume()
    {
        print("music_volume "+ PlayerPrefs.GetInt("music_volume"));
        music_volume = (float)PlayerPrefs.GetInt("music_volume") / 7;
        music_source.volume = music_volume;
        ui_source.volume = (float)PlayerPrefs.GetInt("ui_volume") / 7;
    }
}
