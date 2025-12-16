using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class translate : MonoBehaviour
{
    [SerializeField] private Text[] texts;
    [SerializeField] private int[] translation_index;
    [SerializeField] private bool awake = true;

    private void Awake()
    {
        if (awake)
        {
            load();
        }
    }
    private void Start()
    {
        if (!awake)
        {
            load();
        }
    }
    private void load()
    {
        if (Player.instance.translations != null)
        {
            for (int i = 0; i < texts.Length; i++)
            {
                texts[i].text = Player.instance.translations[translation_index[i]];
            }
        }
        Destroy(this);
    }
}
