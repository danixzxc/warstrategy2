using MadPixel;
using System.Collections;
using UnityEngine;

public class InterstitialTimer : MonoBehaviour
{
    [SerializeField] private float checkInterval = 10f;

    private void Start()
    {
        StartCoroutine(CheckForInterstitialRoutine());
    }

    private IEnumerator CheckForInterstitialRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(checkInterval);

            // ѕровер€ем условие и показываем рекламу
            if (PlayerPrefs.GetString("RateUsShown") == "shown")
            {
                AdsManager.EResultCode code = AdsManager.ShowInter("inter_menu");
            }
        }
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }
}