using UnityEngine;
using UnityEngine.UI;

public class setleveltext : MonoBehaviour
{
    [SerializeField] private Text text;
    void Start()
    {
        if (Player.instance.translations != null)
        {
            text.text = Player.instance.translations[44];
        }
        text.text += " " + GameLogic.instance.current_level;
        Destroy(this);
    } 
}
