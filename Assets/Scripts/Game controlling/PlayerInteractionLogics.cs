using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayerInteractionLogics : GameController
{
    [SerializeField] private Button speed_up_button;

    [SerializeField] private Button[] pause_buttons;

    [SerializeField] private Button[] restart_buttons;

    [SerializeField] private Button[] home_buttons;

    [SerializeField] private Button[] tower_buttons;

    [SerializeField] private Text speed_text;

    [SerializeField] private Text[] upgrade_text;

    [SerializeField] private Text[] cell_text;

    [SerializeField] private Animator game_ui_animator;

    [SerializeField] private Animator tower_buttons_animator;

    [SerializeField] private Animator tower_options_animator;

    [SerializeField] private Animator specials_animator;

    [SerializeField] private Button upgrade_tower_button;

    [SerializeField] private Button sell_tower_button;

    [SerializeField] private GameObject range;

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
        load_towers_buttons();
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
            bool no_one_buttons_active = !tower_buttons_animator.gameObject.activeSelf && !tower_options_animator.gameObject.activeSelf && !specials_animator.gameObject.activeSelf;
            if (hold_time <= .3f && Vector2.Distance(hold_start_pos, Input.mousePosition) < 100 && no_one_buttons_active && !button_pressed_in_this_frame && buttons_active && Container.instance.game_loaded)
            {
                active_zone = get_touch_zone(Camera.main.ScreenToWorldPoint(Input.mousePosition));
                int cell = MapInfo.instance.get_cell_object(active_zone);
                if (cell == 0)
                {
                    place_buttons(tower_buttons_animator, active_zone);
                    var tower = MapInfo.instance.get_tower_in_cell(active_zone);
                }
                else if (cell == 10)
                {
                    place_buttons(specials_animator, active_zone);
                }
                else if (cell == 20)
                {
                    MapInfo.instance.set_environment_target(active_zone);
                }
                else if (cell == 40)
                {
                    var tower = MapInfo.instance.get_tower_in_cell(active_zone);
                    int tower_max_level = PlayerPrefs.GetInt($"tower_{tower.type}_level");
                    if (tower.level >= tower_max_level) upgrade_tower_button.gameObject.SetActive(false);
                    else upgrade_tower_button.gameObject.SetActive(true);
                    range.transform.position = tower.transform.position;
                    float size = tower.range - .15f;
                    if (tower.type != 4) size *= tower.range_multiplier;
                    range.transform.localScale = new Vector3(size, size);
                    range.SetActive(true);
                    for (int i = 0; i < upgrade_text.Length; i++)
                    {
                        upgrade_text[i].text = "" + towers_costs[tower.type];
                        cell_text[i].text = "" + (tower.level * towers_cell_values[tower.type]);
                    }

                    place_buttons(tower_options_animator, active_zone);
                }

            }
            hold_time = 0;
        }

        if (button_pressed_in_this_frame) button_pressed_in_this_frame = false;

        //#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Space)) pause_logic();
        else if (Input.GetKeyDown(KeyCode.S) && !paused) speed_logic();
        else if (Input.GetKeyDown(KeyCode.H) && !paused)
        {
            if (game_ui_animator.gameObject.activeSelf) game_ui_animator.gameObject.SetActive(false);
            else game_ui_animator.gameObject.SetActive(true);
        }
        //#endif
    }

    [SerializeField] private Sprite locked_tower_sprite;

    private void load_towers_buttons()
    {
        for (int button_index = 0; button_index < tower_buttons.Length; button_index++)
        {
            if (PlayerPrefs.GetInt($"tower_{button_index}_level") == 0)
            {
                tower_buttons[button_index].GetComponent<Image>().sprite = locked_tower_sprite;
                tower_buttons[button_index].transform.localScale = new Vector3(1, 1);
            }
        }

    }

    // Нужна для того, чтобы кнопка нажималаси только один раз. (например появление двух разных башень на одном месте).
    private bool pressed = false;

    // Выполняет логику высвечивания одного из набора кнопок.
    private void place_buttons(Animator animator, Vector2 pos)
    {
        pressed = false;
        animator.gameObject.SetActive(true);
        float size = 3.6f / Camera.main.orthographicSize;
        animator.transform.position = clamp_position(pos);
        animator.transform.localPosition = new Vector3(animator.transform.localPosition.x, animator.transform.localPosition.y, 0);
        animator.transform.localScale = new Vector3(size, size, 1);
        animator.Play("Apear");
        PlayerAudio.instance.play(0);
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

        float x = Mathf.Clamp(pos.x, minX, maxX);
        float y = Mathf.Clamp(pos.y, minY, maxY);
        var button = tower_buttons[9].transform;
        if (x > 0) button.localPosition = new Vector3(-400, button.localPosition.y);
        else button.localPosition = new Vector3(400, button.localPosition.y);

        return new Vector2(x, y);
    }

    // Все доступные ускорения игры.
    private int[] speed_modes = new int[] { 1, 2 };

    // Индекс текущего ускорения игры.
    private int speed_mode = 0;

    // Логика переключения скорости игры.
    private void speed_logic()
    {
        PlayerAudio.instance.play(1);
        speed_mode++;
        speed_mode %= speed_modes.Length;
        Time.timeScale = speed_modes[speed_mode];
        speed_text.text = "x" + speed_modes[speed_mode];
        button_pressed_in_this_frame = true;
        //   gameAuido.set_speed(speed_modes[speed_mode]);
    }

    // Задает кнопкам нужные функции.
    private void buttons_start()
    {
        if (PlayerPrefs.GetInt("speed_x3_on") == 1) speed_modes = new int[] { 1, 2, 3 };

#if UNITY_EDITOR
        speed_modes = new int[] { 1, 2, 8 };
#endif
#if PLATFORM_STANDALONE_OSX
        speed_modes = new int[] { 1, 2, 8 };
#endif

        speed_up_button.onClick.AddListener(speed_logic);
        foreach (var pause_button in pause_buttons) pause_button.onClick.AddListener(pause_logic);
        foreach (var restart_button in restart_buttons) restart_button.onClick.AddListener(restart_logic);
        foreach (var home_button in home_buttons) home_button.onClick.AddListener(home_logic);
        foreach (var tower_button in tower_buttons)
        {
            tower_button.onClick.AddListener(tower_button_pressed);
        }
        upgrade_tower_button.onClick.AddListener(upgrade_tower_logic);
        sell_tower_button.onClick.AddListener(sell_tower_logic);
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

    // Когда игрок пытается заспавнить башню.
    private void tower_button_pressed()
    {
        if (!pressed)
        {
            int tower_index = EventSystem.current.currentSelectedGameObject.name[0] - 48;
            if (PlayerPrefs.GetInt($"tower_{tower_index}_level") != 0)
            {
                if (GameLogic.instance.add_coins(-towers_costs[tower_index]))
                {
                    MapInfo.instance.spawn_tower(tower_index, active_zone);
                }
            }
        }
        pressed = true;
    }

    private bool paused = false;

    // Пауза
    private void pause_logic()
    {
        if (game_ui_animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1)
        {
            PlayerAudio.instance.play(1);
            if (!paused)
            {
                MadPixelAnalytics.AnalyticsManager.CustomEvent("pause_pressed",
            new Dictionary<string, object>() {
        {"param", "value"}
            });
                Time.timeScale = 0;
                game_ui_animator.Play("pause");
                paused = true;
                CameraMovement.instance.moving = false;
                buttons_active = false;
            }
            else
            {
                Time.timeScale = speed_modes[speed_mode];
                game_ui_animator.Play("play");
                paused = false;
                CameraMovement.instance.moving = true;
                buttons_active = true;
            }
        }
        button_pressed_in_this_frame = true;
    }



    private int[] towers_costs = new int[]
    {
        100,
        200,
        250,
        200,
        200,
        100,
        200,
        100,
        100,
        400
    };

    private int[] towers_cell_values = new int[]
    {
        75,
        150,
        185,
        150,
        150,
        75,
        150,
        75,
        75,
        300
    };

    private void upgrade_tower_logic()
    {
        if (!pressed)
        {
            var tower = MapInfo.instance.get_tower_in_cell(active_zone);
            if (tower.level < 3)
                if (GameLogic.instance.add_coins(-towers_costs[tower.type]))
                {
                    float rotation = (tower.type != 7) ? tower.gun.eulerAngles.z: 0;
                    MapInfo.instance.upgrade_tower(active_zone, rotation);
                }
        }
        pressed = true;
    }

    private void sell_tower_logic()
    {
        if (!pressed)
        {
            var tower = MapInfo.instance.get_tower_in_cell(active_zone);
            GameLogic.instance.add_coins(tower.level * towers_cell_values[tower.type]);
            MapInfo.instance.remove_tower(active_zone);
        }
        pressed = true;
    }

    // Скрывает все кнопки, вызванные игроком.
    public override void hide_buttons()
    {
        if (tower_buttons_animator.gameObject.activeSelf)
        {
            tower_buttons_animator.Play("Disapear");
            PlayerAudio.instance.play(2);
        }
        if (tower_options_animator.gameObject.activeSelf)
        {
            tower_options_animator.Play("Disapear");
            range.SetActive(false);
            PlayerAudio.instance.play(2);
        }
        if (specials_animator.gameObject.activeSelf)
        {
            specials_animator.Play("Disapear");
            PlayerAudio.instance.play(2);
        }
    }

    private void home_logic()
    {
        PlayerAudio.instance.play(1);
        if (Checkpoints.current_save != null) Checkpoints.current_save = null;
        if (WavesCreator.instance) WavesCreator.waves = null;
        LevelLoader.instance.loadScene("episode_1");
    }
}
