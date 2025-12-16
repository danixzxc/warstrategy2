using Google.Play.Review;
using MadPixel;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//Назначить действия кнопкам
public class RateUsPanel : MonoBehaviour
{

    private ReviewManager _reviewManager;
    private PlayReviewInfo _playReviewInfo;

    [SerializeField] private Button rateButton;
    [SerializeField] private Button skipButton;
    [SerializeField] private Button closeButton;

    private const string SkipDateKey = "RateUsSkipDate";

    private void Start()
    {
        PlayerPrefs.SetString("RateUsShown", "shown");
        // Инициализация менеджера отзывов
        _reviewManager = new ReviewManager();

        // Назначение метода обработки нажатия кнопки
        if (rateButton != null)
        {
            rateButton.onClick.AddListener(StartReviewFlow);
        }
        if (skipButton != null)
        {
            skipButton.onClick.AddListener(SkipRateUs);
            skipButton.onClick.AddListener(ShowInterstitial);
        }

    }
    public void StartReviewFlow()
    {
        StartCoroutine(RequestAndLaunchReview());
    }
    private IEnumerator RequestAndLaunchReview()
    {
        Debug.Log("Процесс оценки начат");
        var requestFlowOperation = _reviewManager.RequestReviewFlow();
        yield return requestFlowOperation;

        // Правильная проверка завершения операции
        if (!requestFlowOperation.IsDone)
        {
            Debug.LogError("Операция не завершена");
            yield break;
        }

        if (requestFlowOperation.Error != ReviewErrorCode.NoError)
        {
            Debug.LogError("Ошибка запроса отзыва: " + requestFlowOperation.Error.ToString());
            yield break;
        }

        _playReviewInfo = requestFlowOperation.GetResult();

        var launchFlowOperation = _reviewManager.LaunchReviewFlow(_playReviewInfo);
        yield return launchFlowOperation;

        if (!launchFlowOperation.IsDone)
        {
            Debug.LogError("Запуск процесса оценки не завершен");
            yield break;
        }

        _playReviewInfo = null;

        if (launchFlowOperation.Error != ReviewErrorCode.NoError)
        {
            Debug.LogError("Ошибка запуска отзыва: " + launchFlowOperation.Error.ToString());
            yield break;
        }

        Debug.Log("Процесс оценки завершен");
        PlayerPrefs.SetString("RateUsCompleted", "completed");
    }

    public void ShowInterstitial()
    {
        AdsManager.EResultCode code = AdsManager.ShowInter("inter_after_rateus");
    }

    public void SkipRateUs()
    {
        PlayerPrefs.SetString("RateUsSkipped", "skipped");
        // Сохраняем текущую дату пропуска
        PlayerPrefs.SetString(SkipDateKey, System.DateTime.Now.ToString());
    }

    public void AddSceneSwitchToButtons(string sceneName)
    {
        rateButton.onClick.AddListener(() =>
        {
            LevelLoader.instance.loadScene(sceneName);
        });
        skipButton.onClick.AddListener(() =>
        {
            LevelLoader.instance.loadScene(sceneName);
        });
        closeButton.onClick.AddListener(() =>
        {
            LevelLoader.instance.loadScene(sceneName);
        });
    }
}
