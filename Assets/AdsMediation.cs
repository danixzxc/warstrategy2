using Firebase.Analytics;
using UnityEngine;

public class AdsMediation : MonoBehaviour
{
    public static AdsMediation Instance;
    /*
public void Start()
{

#if UNITY_ANDROID
string appKey = "1f255bfc5"; // Android
#elif UNITY_IPHONE
string appKey = "1f25718f5"; // IOS
#else
string appKey = "1f255bfc5"; // Другие платформы
#endif

Debug.Log("unity-script: IronSource.Agent.validateIntegration");
IronSource.Agent.validateIntegration();

Debug.Log("unity-script: unity version" + IronSource.unityVersion());

// SDK init
Debug.Log("unity-script: IronSource.Agent.init");

IronSource.Agent.init(appKey);
//IronSource.Agent.init(IronSourceAdUnits.BANNER);
//    IronSource.Agent.init(IronSourceAdUnits.REWARDED_VIDEO);
IronSource.Agent.init(IronSourceAdUnits.INTERSTITIAL);
IronSourceEvents.onImpressionDataReadyEvent += ImpressionSuccessEvent;

//    InitReward();
//   InitBanner();
InitInterstitial();
}

public void ShowRewardVideo()
{
Debug.Log("SHOW REWARD");

if (IronSource.Agent.isRewardedVideoAvailable())
{
    IronSource.Agent.showRewardedVideo();
}
else
{
    IronSource.Agent.loadRewardedVideo();
}
}
*/
    public void ShowInterstitialAd()
    {
        Debug.Log("SHOW INTERSTITIAL");
        /*
        if (IronSource.Agent.isInterstitialReady())
        {
            IronSource.Agent.showInterstitial();
        }
        else
        {
            IronSource.Agent.loadInterstitial();
        }*/

    }
        /*
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            Instance = this;
        }

        Debug.Log("ADS initialized");

        DontDestroyOnLoad(gameObject);
    }

    private void InitBanner()
    {
        IronSourceBannerEvents.onAdLoadFailedEvent += BannerAdLoadFailedEvent;
        IronSourceBannerEvents.onAdScreenDismissedEvent += BannerAdScreenDismissedEvent;
    }

    private void InitInterstitial()
    {
        
        IronSourceInterstitialEvents.onAdLoadFailedEvent += InterstitialAdLoadFailedEvent;
        IronSourceInterstitialEvents.onAdShowFailedEvent += InterstitialAdShowFailedEvent;
        IronSourceInterstitialEvents.onAdClosedEvent += InterstitialAdClosedEvent;
        IronSourceInterstitialEvents.onAdOpenedEvent += InterstitialAdOpenedEvent;
        
    }

    private void InitReward()
    {   
        IronSourceRewardedVideoEvents.onAdShowFailedEvent += RewardedVideoAdShowFailedEEvent;
        IronSourceRewardedVideoEvents.onAdOpenedEvent += RewardedVideoAdOpenedEvent;
        IronSourceRewardedVideoEvents.onAdClosedEvent += RewardedVideoAdClosedEvent;
        IronSourceRewardedVideoEvents.onAdLoadFailedEvent += RewardedVideoAdLoadFailedEvent;
    }

    private void ImpressionSuccessEvent(IronSourceImpressionData impressionData)
    {
        if (impressionData != null)
        {
            Parameter[] AdParameters ={
                    new Parameter("ad_platform","ironSource"),
                    new Parameter("ad_source",impressionData.adNetwork),
                    new Parameter("ad_unit_name",impressionData.adUnit),
                    new Parameter("ad_format",impressionData.instanceName),
                    new Parameter("currency","USD"),
                    new Parameter("value",(double)impressionData.revenue)
            };
            FirebaseAnalytics.LogEvent("ad_impression", AdParameters);
        }
        if (impressionData != null)
        {
            Parameter[] AdParameters ={
                    new Parameter("ad_platform","ironSource"),
                    new Parameter("ad_source",impressionData.adNetwork),
                    new Parameter("ad_unit_name",impressionData.adUnit),
                    new Parameter("ad_format",impressionData.instanceName),
                    new Parameter("currency","USD"),
                    new Parameter("value",(double)impressionData.revenue)
            };
            FirebaseAnalytics.LogEvent("ad_iron_source", AdParameters);
        }
    }

    private void OnApplicationPause(bool isPaused)
    {
        IronSource.Agent.onApplicationPause(isPaused);
    }

    private void BannerAdScreenDismissedEvent(IronSourceAdInfo adInfo)
    {
        Debug.Log("failed to load banner");
        Invoke("CreateAndLoad_Banner", 2);
    }

    void BannerAdLoadFailedEvent(IronSourceError error)
    {
        Invoke("CreateAndLoad_Banner", 2.5f);
    }


    private void InterstitialAdOpenedEvent(IronSourceAdInfo adInfo)
    {
        Time.timeScale = 0;
        AudioListener.volume = 0;
    }

    private void InterstitialAdShowFailedEvent(IronSourceError ironSourceError, IronSourceAdInfo adInfo)
    {
        Time.timeScale = 1;
        AudioListener.volume = 1;

        Debug.Log("failed to show interstition");
        LoadInterstitial();
    }

    private void InterstitialAdLoadFailedEvent(IronSourceError obj)
    {
        Debug.Log("failed to load interstitial");
    }

    private void InterstitialAdClosedEvent(IronSourceAdInfo adInfo)
    {
        Time.timeScale = 1;
        AudioListener.volume = 1;

        Debug.Log("interstitial closed");
        LoadInterstitial();
    }

    private void RewardedVideoAdOpenedEvent(IronSourceAdInfo adInfo)
    {
        Time.timeScale = 0;
        AudioListener.volume = 0;

        Debug.Log("Rewarded ad opened");
    }

    private void RewardedVideoAdEndedEvent()
    {
        Debug.Log("reward video ended");
        LoadRewardVideo();
    }

    private void RewardedVideoAdShowFailedEEvent(IronSourceError ironSourceError, IronSourceAdInfo adInfo)
    {
        Time.timeScale = 1;
        AudioListener.volume = 1;

        Debug.Log("Failed to show reward video");
        LoadRewardVideo();
    }

    private void RewardedVideoAdClosedEvent(IronSourceAdInfo adInfo)
    {
        Time.timeScale = 1;
        AudioListener.volume = 1;

        Debug.Log("reward video was closed");
        LoadRewardVideo();
    }

    private void RewardedVideoAdLoadFailedEvent(IronSourceError obj)
    {
        Debug.Log("Failed to load reward video");
    }

    private void LoadRewardVideo()
    {
        if (!IronSource.Agent.isRewardedVideoAvailable())
            IronSource.Agent.loadRewardedVideo();
    }

    private void LoadInterstitial()
    {
        if (!IronSource.Agent.isInterstitialReady())
            IronSource.Agent.loadInterstitial();
    }*/
}

