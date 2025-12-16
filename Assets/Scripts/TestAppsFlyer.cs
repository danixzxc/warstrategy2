using AppsFlyerSDK;
using UnityEngine;

public class TestAppsFlyer : MonoBehaviour
{
    void Start()
    {
       // AppsFlyer.initSDK("YOUR_DEV_KEY", "YOUR_APP_ID"); // Replace with your credentials
        Debug.Log("AppsFlyer SDK Version: " + AppsFlyer.getSdkVersion()); // Now it should show
    }
}