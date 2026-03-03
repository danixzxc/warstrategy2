using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class Tutorial : GameController
{
    [SerializeField] private Button speed_up_button;

    [SerializeField] private Button[] pause_buttons;

    [SerializeField] private Button[] restart_buttons;

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

    [SerializeField] private int[] actions_waves;
    [SerializeField] private string[] tutorial_actions;
    [SerializeField] private string[] messages;
    [SerializeField] private int[] intervals;
    [SerializeField] private Vector2Int[] focus_places;
    [SerializeField] private Transform tutorial_hand;
    [SerializeField] private GameObject message_panel;
    [SerializeField] private Text message_text;
    [SerializeField] private GameController playerInteractions;
    [SerializeField] private GameObject finalPanel;
    [SerializeField] private Button final_button;

    [SerializeField]private int tutorial_timer = 0;
    [SerializeField] private string tutorial_name = "tutorial_1";
    private int current_tutorial_step = 0;
    private bool waiting = false;

    private void finalLogic()
    {
        if (PlayerPrefs.GetInt(tutorial_name) == 0)
            LogTutorialStep("02_finish");
        PlayerPrefs.SetInt(tutorial_name, 1);
        Time.timeScale = 1;
        speed_mode = 2;
        speed_text.text = "x2";
        playerInteractions.enabled = true;
        Destroy(finalPanel);
        Destroy(this);
    }

    private bool check_action(string action)
    {
        if (waiting && !CameraMovement.instance.moveToTarget)
        {
            bool on_place = true;
            if (focus_places[current_tutorial_step] != active_zone && action != "speed") on_place = false;
            if (action == tutorial_actions[current_tutorial_step] && on_place)
            {
                current_tutorial_step++;
                tutorial_timer = 0;

                waiting = false;
                Time.timeScale = speed_modes[speed_mode];
                tutorial_hand.gameObject.SetActive(false);
                message_panel.SetActive(false);
                CameraMovement.instance.moving = true;

                if (current_tutorial_step < tutorial_actions.Length)
                {
                    if (tutorial_timer >= intervals[current_tutorial_step] && WavesLogic.instance.current_wave == actions_waves[current_tutorial_step])
                    {
                        start_waiting_player();
                    }
                }
                else
                {

                }

                return true;
            }
        }
        return false;
    }

    private void placeHand()
    {
        tutorial_hand.gameObject.SetActive(true);
        if (messages[current_tutorial_step] != "")
        {
            message_panel.SetActive(true);
            message_text.text = messages[current_tutorial_step];
        }
        string frag = tutorial_actions[current_tutorial_step].Substring(0, 4);

        if (frag == "spaw")
        {
            int button_index = int.Parse(tutorial_actions[current_tutorial_step].Substring(6, 1));

            tutorial_hand.transform.position = tower_buttons[button_index].transform.position;

            // Делаем кнопку активной
            tower_buttons[button_index].interactable = true;
        }
        else if (frag == "spee") tutorial_hand.transform.position = speed_up_button.transform.position;
        else if (frag == "bomb" || frag == "sell") tutorial_hand.transform.position = (Vector2)(focus_places[current_tutorial_step] + Vector2.down);
        else if (frag == "port" || frag == "upgr") tutorial_hand.transform.position = (Vector2)(focus_places[current_tutorial_step] + Vector2.up);
        else tutorial_hand.transform.position = (Vector2)focus_places[current_tutorial_step];
    }
    private void Start()
    {
#if UNITY_EDITOR
#else
        QualitySettings.vSyncCount = 2;
        Application.targetFrameRate = 30;
#endif
        if (true) //testing tutorial
          //  if (PlayerPrefs.GetInt(tutorial_name) == 0)
        {
            instance = this;
            Time.timeScale = 1;
            buttons_start();
            load_towers_buttons();
            LogTutorialStep("01_start");

            if (Player.instance.translations != null)
            {
                messages = new string[13];
                messages[0] = Player.instance.translations[12];
                messages[1] = Player.instance.translations[13];
                messages[2] = Player.instance.translations[14];
                messages[3] = Player.instance.translations[15];
                messages[4] = Player.instance.translations[16];
                messages[5] = Player.instance.translations[17];
                messages[6] = "";
                messages[7] = Player.instance.translations[18];
                messages[8] = Player.instance.translations[19];
                messages[9] = Player.instance.translations[20];
                messages[10] = Player.instance.translations[47];
                messages[11] = Player.instance.translations[48];
                messages[12] = "";
            }
          
        }
        else
        {
            finalLogic();
        }
    }
    public void LogTutorialStep(string stepName)
    {
        string[] stepParts = stepName.Split('_');
        string formattedStepName = stepParts.Length > 1 ? $"{stepParts[0]}_{stepParts[1]}" : stepName;

        MadPixelAnalytics.AnalyticsManager.CustomEvent(
            "tutorial",
            new Dictionary<string, object> {
            {"step_name", formattedStepName},
                {"bSendEventsBuffer", true }
            }
        );
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
                    if (check_action($"buttons"))
                        place_buttons(tower_buttons_animator, active_zone);
                }
                else if (cell == 10)
                {
                    if (check_action($"specials"))
                        place_buttons(specials_animator, active_zone);
                }
                else if (cell == 20)
                {
                    if (check_action($"target"))
                        MapInfo.instance.set_environment_target(active_zone);
                }
                else if (cell == 40)
                {
                    if (check_action($"options"))
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

            }
            hold_time = 0;
        }

        if (button_pressed_in_this_frame) button_pressed_in_this_frame = false;

        if (waiting && CameraMovement.instance.moveToTarget == false && !tutorial_hand.gameObject.activeSelf)
        {
            placeHand();
        }
    }

    private void FixedUpdate()
    {
        if (Container.instance.game_loaded && Container.instance.enemies_loaded) tutorial_timer++;
        if (tutorial_timer >= intervals[current_tutorial_step] && WavesLogic.instance.current_wave == actions_waves[current_tutorial_step])
        {
            start_waiting_player();
        }
    }

    private void start_waiting_player()
    {
        if (tutorial_actions[current_tutorial_step] == "finish")
        {
            Time.timeScale = 0;
            finalPanel.SetActive(true);
        }
        else
        {
            waiting = true;
            Time.timeScale = 0;
            CameraMovement.instance.setTarget((Vector2)focus_places[current_tutorial_step]);
            CameraMovement.instance.moveToTarget = true;
            CameraMovement.instance.moving = false;
        }
    }


    [SerializeField] private Sprite locked_tower_sprite;

    private void load_towers_buttons()
    {
        for (int button_index = 0; button_index < 10; button_index++)
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
    private int[] speed_modes = new int[] { 1, 2, 3, 4 };

    // Индекс текущего ускорения игры.
    private int speed_mode = 2;

    // Логика переключения скорости игры.
    private void speed_logic()
    {
        if (check_action("speed"))
        {
            speed_mode++;
            speed_mode %= speed_modes.Length;
            Time.timeScale = speed_modes[speed_mode];
            speed_text.text = "x" + speed_modes[speed_mode];
            button_pressed_in_this_frame = true;
            //   gameAuido.set_speed(speed_modes[speed_mode]);
        }
    }

    // Задает кнопкам нужные функции.
    private void buttons_start()
    {
        speed_up_button.onClick.AddListener(speed_logic);
        foreach (var restart_button in restart_buttons) restart_button.onClick.AddListener(restart_logic);
        foreach (var tower_button in tower_buttons)
        {
            tower_button.onClick.AddListener(tower_button_pressed);
        }
        upgrade_tower_button.onClick.AddListener(upgrade_tower_logic);
        sell_tower_button.onClick.AddListener(sell_tower_logic);
        final_button.onClick.AddListener(finalLogic);
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
        int tower_index = EventSystem.current.currentSelectedGameObject.name[0] - 48;
        if (check_action($"spawn({tower_index})"))
        {
            if (!pressed)
            {
                if (PlayerPrefs.GetInt($"tower_{tower_index}_level") != 0)
                {
                    if (GameLogic.instance.add_coins(-towers_costs[tower_index]))
                    {
                        MapInfo.instance.spawn_tower(tower_index, active_zone);
                    }
                }
            }
            pressed = true;
            hide_buttons();
        }
    }

    private bool paused = false;

    private int[] towers_costs = new int[]
    {
        100,
        200,
        250,
        200,
        200,
        100,
        200,
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
    };

    private void upgrade_tower_logic()
    {
        var tower = MapInfo.instance.get_tower_in_cell(active_zone);
        if (check_action($"upgrade({tower.type})"))
        {
            if (!pressed)
            {
                if (tower.level < 3)
                    if (GameLogic.instance.add_coins(-towers_costs[tower.type]))
                    {
                        float rotation = tower.gun.eulerAngles.z;
                        MapInfo.instance.upgrade_tower(active_zone, rotation);
                    }
            }
            pressed = true;
            hide_buttons();
        }
    }

    private void sell_tower_logic()
    {
        if (check_action($"sell"))
        {
            if (!pressed)
            {
                var tower = MapInfo.instance.get_tower_in_cell(active_zone);
                GameLogic.instance.add_coins(tower.level * towers_cell_values[tower.type]);
                MapInfo.instance.remove_tower(active_zone);
            }
            pressed = true;
            hide_buttons();
        }
    }

    // Скрывает все кнопки, вызванные игроком.
    public override void hide_buttons()
    {
        if (tower_buttons_animator.gameObject.activeSelf)
        {
            tower_buttons_animator.Play("Disapear");
        }
        if (tower_options_animator.gameObject.activeSelf)
        {
            tower_options_animator.Play("Disapear");
            range.SetActive(false);
        }
        if (specials_animator.gameObject.activeSelf)
        {
            specials_animator.Play("Disapear");
        }
    }

    public override bool asktutorial(string action)
    {
        return check_action(action);
    }
#if UNITY_EDITOR
    public void setTutorial(string[] ac, int[] inte, Vector2Int[] fo, int[] w)
    {
        tutorial_actions = ac;
        intervals = inte;
        focus_places = fo;
        actions_waves = w;
    }
#endif

    public override void nextWave()
    {
        tutorial_timer = 0;
    }
}
