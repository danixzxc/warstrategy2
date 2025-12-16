using UnityEngine;

/// <summary>
/// Синглтон для скрытия/показа UI Canvas по тройному клику
/// </summary>
public class CanvasToggleManager : MonoBehaviour
{
    public static CanvasToggleManager Instance { get; private set; }

    private Canvas uiCanvas;
    private float lastClickTime = 0f;
    private int clickCount = 0;
    [SerializeField]
    private float TRIPLE_CLICK_DELAY = 0.3f; // Макс время между кликами для тройного нажатия

    private void Awake()
    {
        // Реализация синглтона
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        FindMainCanvas();
    }

    private void OnLevelWasLoaded(int level)
    {
        // При загрузке новой сцены ищем Canvas заново
        FindMainCanvas();
    }

    private void FindMainCanvas()
    {
        uiCanvas = FindObjectOfType<Canvas>();

        if (uiCanvas == null)
        {
            Debug.LogWarning("CanvasToggleManager: Canvas не найден в сцене!");
        }
        else
        {
            Debug.Log($"CanvasToggleManager: Найден Canvas '{uiCanvas.gameObject.name}'");
        }
    }

    private void Update()
    {
        // Проверяем любое нажатие (мышь или тачпад)
        if (Input.GetMouseButtonDown(0) || (Input.touchSupported && Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
        {
            // Проверяем тройное нажатие
            if (Time.time - lastClickTime < TRIPLE_CLICK_DELAY)
            {
                clickCount++;

                // Третье нажатие (0, 1, 2)
                if (clickCount >= 2)
                {
                    ToggleCanvas();
                    clickCount = 0;
                }
            }
            else
            {
                clickCount = 0;
            }

            lastClickTime = Time.time;
        }
    }

    private void ToggleCanvas()
    {
        if (uiCanvas != null)
        {
            uiCanvas.enabled = !uiCanvas.enabled;
            Debug.Log($"CanvasToggleManager: Canvas {(uiCanvas.enabled ? "включен" : "отключен")}");
        }
        else
        {
            Debug.LogWarning("CanvasToggleManager: Canvas не найден, попытка поиска...");
            FindMainCanvas();
        }
    }
}