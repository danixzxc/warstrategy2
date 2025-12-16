using Google.Play.Review;
using MadPixel;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AppReviewManager : MonoBehaviour
{
    [SerializeField] private GameObject _rateUsPrefab;
    public GameObject spawnedRateUsPrefab;

    private const int DaysToWait = 3;
    private const string SkipDateKey = "RateUsSkipDate";

    // Singleton instance
    private static AppReviewManager _instance;

    public static AppReviewManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<AppReviewManager>();
                if (_instance == null)
                {
                    GameObject obj = new GameObject("AppReviewManager");
                    _instance = obj.AddComponent<AppReviewManager>();
                }
            }
            return _instance;
        }
    }

    private void Awake()
    {
        // Ensure only one instance exists
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        // Optional: Initialize your prefab if needed
        if (_rateUsPrefab == null)
        {
            Debug.LogWarning("RateUsPrefab is not assigned in AppReviewManager!");
        }
    }

    public bool CheckAfterLevelCompletion()
    {
        if (SceneManager.GetActiveScene().name == "001")
            return false;

        if (PlayerPrefs.GetString("RateUsCompleted") == "completed")
        {
            ShowInterstitial();
            return false;
        }
        if (PlayerPrefs.GetInt("LastCompletedLevelNum") >= 2 &&
            PlayerPrefs.GetString("RateUsSkipped") != "skipped")
        {
            InstantiateRateUs();
            return true;
        }
        else if (PlayerPrefs.GetInt("LastCompletedLevelNum") >= 2 &&
            PlayerPrefs.GetString("RateUsSkipped") == "skipped" &&
            HasEnoughTimePassedSinceSkip())
        {
            InstantiateRateUs();
            return true;
        }
        else if (PlayerPrefs.GetString("RateUsShown") == "shown")
        {
            ShowInterstitial();
            return false;
        }
        else
        {
            return false;
        }
    }

    private void InstantiateRateUs()
    {
        spawnedRateUsPrefab = Instantiate(_rateUsPrefab, GameObject.Find("rate_us_place").transform);

        RectTransform rt = spawnedRateUsPrefab.GetComponent<RectTransform>();

        // 1. Устанавливаем анкоры (0,0) и (1,1) для растягивания на весь родительский контейнер
        rt.anchorMin = Vector2.zero;    // Левый нижний угол (0, 0)
        rt.anchorMax = Vector2.one;    // Правый верхний угол (1, 1)

        // 2. Обнуляем смещение (чтобы не было отступов)
        rt.offsetMin = Vector2.zero;   // Нижний-левый оффсет (left, bottom)
        rt.offsetMax = Vector2.zero;   // Верхний-правый оффсет (right, top)

        // 3. Сбрасываем позицию и поворот (опционально)
        rt.anchoredPosition = Vector2.zero;
        rt.localRotation = Quaternion.identity;

        // 4. Сбрасываем масштаб (если нужно)
        rt.localScale = Vector3.one;
    }

    // Проверяет, прошло ли достаточно времени с момента пропуска оценки
    private bool HasEnoughTimePassedSinceSkip()
    {
        if (!PlayerPrefs.HasKey(SkipDateKey))
            return false;

        string skipDateString = PlayerPrefs.GetString(SkipDateKey);
        if (string.IsNullOrEmpty(skipDateString))
            return false;

        System.DateTime skipDate;
        if (!System.DateTime.TryParse(skipDateString, out skipDate))
            return false;

        System.TimeSpan timePassed = System.DateTime.Now - skipDate;
        return timePassed.TotalDays >= DaysToWait; // Changed from TotalMinutes to TotalDays
    }

    public void ShowInterstitial()
    {
        AdsManager.EResultCode code = AdsManager.ShowInter("inter_after_level");
    }

    public void TestSpawnRateUs()
    {
        Instantiate(_rateUsPrefab, GameObject.Find("rate_us_place").transform);
    }
}