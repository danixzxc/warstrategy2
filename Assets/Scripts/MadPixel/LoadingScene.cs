using MadPixel.InApps;
using MadPixelAnalytics;
using MadPixel;
using System.Collections;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.SceneManagement;

public class LoadingScene : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(LoadingCoroutine());
    }
    private IEnumerator LoadingCoroutine()
    {
        yield return new WaitUntil(() => AdsManager.Ready());

        MobileInAppPurchaser.Instance.OnPurchaseResult += OnIAPPurchaseResult;
        MobileInAppPurchaser.Instance.Init(
            MobileInAppPurchaser.AdsFreeSKU,
            MobileInAppPurchaser.ConsumablesList,
            MobileInAppPurchaser.NonConsumablesList,
            MobileInAppPurchaser.SubscriptionsList);
       
        // yield return new WaitUntil(() => MobileInAppPurchaser.Instance.IsInitialized());

        AnalyticsManager.Instance.Init();
        yield return new WaitForSeconds(3f);
        LevelLoader.instance.loadScene("episode_1");
    }
    /*private IEnumerator LoadingCoroutine()
    {
        // Инициализируем рекламу
         yield return new WaitUntil(() => AdsManager.Ready());
        AnalyticsManager.Instance.Init();
        yield return new WaitForSeconds(4f);
        // Инициализируем IAP
          MobileInAppPurchaser.Instance.OnPurchaseResult += OnIAPPurchaseResult;
          MobileInAppPurchaser.Instance.Init(
              MobileInAppPurchaser.AdsFreeSKU,
              MobileInAppPurchaser.ConsumablesList,
              MobileInAppPurchaser.NonConsumablesList,
              MobileInAppPurchaser.SubscriptionsList);

          // Ждем инициализации IAP
          yield return new WaitUntil(() => MobileInAppPurchaser.Instance.IsInitialized());

        yield return new WaitForSeconds(1f);
       
        LevelLoader.instance.loadScene("episode_1");
    }
*/
    private void OnIAPPurchaseResult(Product product)
    {
        // Перенаправляем событие в Shop через статический метод
        Shop.HandleIAPPurchaseResult(product);
    }
}