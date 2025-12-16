using Firebase;
using Firebase.Analytics;
using Firebase.Extensions;
using MadPixel;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirebaseManager : MonoBehaviour
{

    public static FirebaseManager instance;

    // Пауза между показами рекламы.
    [SerializeField] private float ad_wait_time = 280;

    // Работает ли полноэкранная реклама
    public bool on_ad = true;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(this);
    }
    
    void Start()
    {

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            var dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                InitializeFirebase();
            }
            else
            {
                Debug.LogError(
                  "Could not resolve all Firebase dependencies: " + dependencyStatus);
            }
        });

        // Проеверка наличия отключения реламы.
        if (PlayerPrefs.GetInt("no_ads_on") == 1) on_ad = false;
        else
        {
            // Запуск полноэкранной рекламы.
         //   Appodeal.SetAutoCache(AppodealAdType.Interstitial, false);
         //   Appodeal.SetInterstitialCallbacks(this);
         //   Appodeal.Initialize("7b24e7f087c797f7bdea1e0fb4dc1ae6e71efacfd24b27a6", AppodealAdType.Interstitial, this);
            StartCoroutine(ad_logic());
        }
    }

    void InitializeFirebase()
    {
        FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
    }

    // Управление показами полноэкранной рекламы.
    private IEnumerator ad_logic()
    {
        while (on_ad)
        {
            yield return new WaitForSecondsRealtime(ad_wait_time);
            int skip_ad_count = PlayerPrefs.GetInt("Skip ad");
            //  if (skip_ad_count <= 0) Appodeal.Cache(AppodealAdType.Interstitial);
            if (skip_ad_count <= 0) //AdsMediation.Instance.ShowInterstitialAd();
            {

                if (PlayerPrefs.GetString("RateUsShown") == "shown")
                    AdsManager.ShowInter("inter_restart_level"); 
            }
            else PlayerPrefs.SetInt("Skip ad", skip_ad_count - 1);
        }
    }


    // Отправка событий.
    public void logEvent(string name)
    {
        FirebaseAnalytics.LogEvent(name);
        print(name);
    }

    // Выдает цифру в формате 000.
    public string get_string_num(int num)
    {
        string _return = "" + num;
        string with_0 = "";
        for (int i = 0; i < 3 -_return.Length; i++)
        {
            with_0 += '0';
        }
        with_0 += _return;
        return with_0;
    }
}

 


