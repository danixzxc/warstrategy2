using MadPixel;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameController : MonoBehaviour
{

    public static GameController instance;

    // Клетка на карте куда нажал игрок.
    [HideInInspector] public Vector2Int active_zone = new Vector2Int();
    public bool buttons_active = true;

    public virtual void hide_buttons()
    {

    }

    public virtual void nextWave()
    {

    }

    public virtual bool asktutorial(string action)
    {
        return true;
    }

    public void restart_logic()
    {

        if (PlayerPrefs.GetString("RateUsShown") == "shown")
        {
            AdsManager.EResultCode code = AdsManager.ShowInter("inter_restart_level");
        }
        PlayerAudio.instance.play(1);
        if (Checkpoints.current_save != null) Checkpoints.current_save = null;
        LevelLoader.instance.loadScene("level_" + GameLogic.instance.current_level);
    }

}