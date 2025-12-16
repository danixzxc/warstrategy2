using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class PrivacyPolicyButton : MonoBehaviour
{
    public string privacyPolicyUrl = "https://madpixel.dev/privacy.html";

    public void OpenPrivacyPolicy()
    {
        Application.OpenURL(privacyPolicyUrl);
    }
}