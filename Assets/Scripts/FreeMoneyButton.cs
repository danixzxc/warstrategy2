using TMPro;
using UnityEngine;
using static MaxSdkBase;

public class FreeMoneyButton : MonoBehaviour
{
    [SerializeField]
    public TMP_InputField inputField;
    public void GetFreeMoney()
    {

        if (int.TryParse(inputField.text, out int result))
        {
            GameLogic.instance.add_coins(result);
        }
    }
}
