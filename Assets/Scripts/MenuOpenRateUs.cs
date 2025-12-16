using Google.Play.Review;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuOpenRateUs : MonoBehaviour
{

    [SerializeField] private Button openRateUsPanelButton;
    [SerializeField] private GameObject rateUsParent;
    [SerializeField] private GameObject _rateUsPrefab;

    private void Start()
    {
        // Скрываем кнопку, если оценка уже была выполнена
        if (PlayerPrefs.GetString("RateUsCompleted") == "completed")
        {
            openRateUsPanelButton.gameObject.SetActive(false);
            return;
        }
        else
        {
            openRateUsPanelButton.onClick.AddListener(() => { Instantiate(_rateUsPrefab, rateUsParent.transform); });
        }
    }
}
