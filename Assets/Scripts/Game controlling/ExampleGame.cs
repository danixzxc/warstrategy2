using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ExampleGame : GameController
{
    [SerializeField] private Button speed_up_button;

    [SerializeField] private Button[] home_buttons;

    [SerializeField] private Text speed_text;

    [SerializeField] private Animator game_ui_animator;

    [SerializeField] private Animator tower_info_animator;

    [SerializeField] private GameObject range;

    [SerializeField] private Text[] tower_info_name;

    [SerializeField] private Text[] tower_info_level;

    [SerializeField] private Animator message_animator;

    private string[] towers_names = new string[]
    {
        "Machine-gun",
        "Cannon",
        "Sniper Tower",
        "Delay Tower",
        "Radar Tower",
        "Spinner Tower",
        "Grenade Launcher",
        "Radiation Tower",
        "Laser Machine-gun",
        "Laser Gun"
    };

    private void Start()
    {
#if UNITY_EDITOR
#else
        QualitySettings.vSyncCount = 2;
        Application.targetFrameRate = 30;
#endif
        instance = this;
        Time.timeScale = 1;
        buttons_start();
        Invoke("show_message", 3.5f);
        MadPixelAnalytics.AnalyticsManager.CustomEvent("start_example",
    new Dictionary<string, object>() {
        {"param", "value"}
    });
    }

    // Время, которое прошло после нажатия на экран.
    private float hold_time = 0;

    // Место куда было совершено нажатие во время его начала.
    private Vector2 hold_start_pos;

    // Включается если в этом кадре была нажата какая-то из копок.
    private bool button_pressed_in_this_frame = false;


    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            hold_start_pos = Input.mousePosition;
        }
        else if (Input.GetMouseButton(0))
        {
            hold_time += Time.unscaledDeltaTime;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            bool no_one_buttons_active = !tower_info_animator.gameObject.activeSelf;
            if (hold_time <= .3f && Vector2.Distance(hold_start_pos, Input.mousePosition) < 100 && no_one_buttons_active && !button_pressed_in_this_frame && buttons_active && Container.instance.game_loaded)
            {
                active_zone = get_touch_zone(Camera.main.ScreenToWorldPoint(Input.mousePosition));
                int cell = MapInfo.instance.get_cell_object(active_zone);
                if (cell == 40)
                {
                    var tower = MapInfo.instance.get_tower_in_cell(active_zone);
                    range.transform.position = tower.transform.position;
                    float size = tower.range - .15f;
                    if (tower.type != 4) size *= tower.range_multiplier;
                    range.transform.localScale = new Vector3(size, size);
                    range.SetActive(true);

                    tower_info_animator.gameObject.SetActive(true);
                    float buttons_size = 3.6f / Camera.main.orthographicSize;
                    tower_info_animator.transform.position = clamp_position(active_zone);
                    tower_info_animator.transform.localPosition = new Vector3(tower_info_animator.transform.localPosition.x, tower_info_animator.transform.localPosition.y, 0);
                    tower_info_animator.transform.localScale = new Vector3(buttons_size, buttons_size, 1);
                    tower_info_animator.Play("Apear");

                    foreach (var txt in tower_info_name)
                    {
                        txt.text = towers_names[tower.type];
                    }
                    foreach (var txt in tower_info_level)
                    {
                        txt.text = "Lev. " + tower.level;
                    }
                }

            }
            hold_time = 0;
        }

        if (button_pressed_in_this_frame) button_pressed_in_this_frame = false;
    }

    // Загоняет кнопки в рамки экрана, если вышли.
    private Vector2 clamp_position(Vector2 pos)
    {
        float width_halth = Screen.width * Camera.main.orthographicSize / Screen.height;
        float height_halth = Camera.main.orthographicSize;

        var minX = Camera.main.transform.position.x - width_halth + 1.5f;
        var maxX = Camera.main.transform.position.x + width_halth - 1.5f;
        var minY = Camera.main.transform.position.y - height_halth + 1.5f;
        var maxY = Camera.main.transform.position.y + height_halth - 1.5f;

        return new Vector2(Mathf.Clamp(pos.x, minX, maxX), Mathf.Clamp(pos.y, minY, maxY));
    }

    // Все доступные ускорения игры.
    private int[] speed_modes = new int[] { 1, 2 };

    // Индекс текущего ускорения игры.
    private int speed_mode = 0;

    // Логика переключения скорости игры.
    private void speed_logic()
    {
        speed_mode++;
        speed_mode %= speed_modes.Length;
        Time.timeScale = speed_modes[speed_mode];
        speed_text.text = "x" + speed_modes[speed_mode];
        button_pressed_in_this_frame = true;
    }

    // Задает кнопкам нужные функции.
    private void buttons_start()
    {
        if (PlayerPrefs.GetInt("speed_x3_on") == 1) speed_modes = new int[] { 1, 2, 3 };

        speed_up_button.onClick.AddListener(speed_logic);
        foreach (var home_button in home_buttons) home_button.onClick.AddListener(home_logic);
    }

    // Выдает координаты поля по координатам нажатия.
    private Vector2Int get_touch_zone(Vector2 pos)
    {
        int return_x = (int)pos.x;
        int return_y = (int)pos.y;
        if (pos.x >= 0 && pos.x - return_x > .5f) return_x++;
        else if (pos.x < 0 && pos.x - return_x < -.5f) return_x--;
        if (pos.y >= 0 && pos.y - return_y > .5f) return_y++;
        else if (pos.y < 0 && pos.y - return_y < -.5f) return_y--;
        return new Vector2Int(return_x, return_y);
    }

    // Скрывает все кнопки, вызванные игроком.
    public override void hide_buttons()
    {
        tower_info_animator.Play("Disapear");
        range.SetActive(false);
    }

    private void show_message()
    {
        message_animator.gameObject.SetActive(true);
    }

    private void home_logic()
    {
        MadPixelAnalytics.AnalyticsManager.CustomEvent("skip_example",
    new Dictionary<string, object>() {
        {"param", "value"}
    });
        if (Checkpoints.current_save != null) Checkpoints.current_save = null;
        if (WavesCreator.instance) WavesCreator.waves = null;
        LevelLoader.instance.loadScene("episode_1");
    }
}
