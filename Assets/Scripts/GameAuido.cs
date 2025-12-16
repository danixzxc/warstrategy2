using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class GameAuido : MonoBehaviour
{
    public static GameAuido instance;
    private AudioSource[][] layers;
    private float[] layers_relative_volumes = new float[] { 0.65f, 0.6f,0.7f,0.7f };

    private void Start()
    {
        if (instance == null) instance = this;
        layers = new AudioSource[4][];

        var volume = (float)PlayerPrefs.GetInt("game_volume") / 7;
        for (int i = 0; i < layers_relative_volumes.Length; i++)
        {
            layers_relative_volumes[i] *= volume;
        }

        Addressables.LoadAssetAsync<AudioClip>("shoot_0").Completed += handle =>
        {
            // Звуки стрельбы пуллеметов.
            layers[0] = new AudioSource[3];
            for (int i = 0; i < 3; i++)
            {
                layers[0][i] = gameObject.AddComponent<AudioSource>();
                layers[0][i].volume = layers_relative_volumes[0];
                layers[0][i].clip = handle.Result;
            }
        };
            Addressables.LoadAssetAsync<AudioClip>("shoot_1").Completed += handle =>
        {
            // Звуки стрельбы снайперов.
            layers[2] = new AudioSource[3];
            for (int i = 0; i < 3; i++)
            {
                layers[2][i] = gameObject.AddComponent<AudioSource>();
                layers[2][i].volume = layers_relative_volumes[2];
                layers[2][i].clip = handle.Result;
            }
        };
        Addressables.LoadAssetAsync<AudioClip>("shoot_2").Completed += handle =>
        {
            // Звуки стрельбы пушек.
            layers[1] = new AudioSource[3];
            for (int i = 0; i < 3; i++)
            {
                layers[1][i] = gameObject.AddComponent<AudioSource>();
                layers[1][i].volume = layers_relative_volumes[1];
                layers[1][i].clip = handle.Result;
            }



            // Звуки взрывов.
            layers[3] = new AudioSource[1];
            layers[3][0] = gameObject.AddComponent<AudioSource>();
            layers[3][0].volume = layers_relative_volumes[3];
            layers[3][0].clip = handle.Result;
        };
    }

    public void Play(int layer)
    {
        bool done = false;
        foreach (var source in layers[layer])
        {
            if (!source.isPlaying)
            {
                source.Play();
                done = true;
                break;
            }
        }
        if (!done) layers[layer][0].Play();
    }

    public void set_speed(float speed)
    {
        if (speed > 3) speed = 3;
        for (int layer = 0; layer < layers.Length; layer++)
        {
            for (int source = 0; source < layers[layer].Length; source++)
            {
                layers[layer][source].pitch = speed;
            }
        }
    }
}
