using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WavesCreator : MonoBehaviour
{
    [SerializeField] private GameObject edit_panel;

    [SerializeField] private InputField waves_count_inputField;
    [SerializeField] private InputField current_wave_inputField;
    [SerializeField] private InputField groups_count_inputField;
    [SerializeField] private InputField current_group_inputField;
    [SerializeField] private InputField enemies_count_inputField;
    [SerializeField] private InputField interval_inputField;
    [SerializeField] private InputField start_delay_inputField;
    [SerializeField] private InputField enemy_type_inputField;
    [SerializeField] private InputField group_way_inputField;

    [SerializeField] private Button open_button;
    [SerializeField] private Button save_to_clipboard_button;
    [SerializeField] private Button load_from_clipboard_button;
    [SerializeField] private Button copy_button;
    [SerializeField] private Button paste_button;
    [SerializeField] private Button close_button;

    [SerializeField] private Text timer_text;

    private float time_scale_before_open = 1;

    public static WavesCreator instance;

    public class Group
    {
        public int start_delay;
        public int intervals;
        public int way;
        public int enemy_type;
        public int enemies_count;
    }

    public static Group[][] waves;

    private void Awake()
    {
        instance = this;
        create_default_waves();
        waves_count_inputField.text = ""+waves.Length;
        switch_to_wave(0);
    }

    private int timer = 0;

    private void FixedUpdate()
    {
        timer++;
    }
    private void Update()
    {
        int all_seconds = (int)(timer * .033f);
        int seconds = all_seconds % 60;
        int minutes = all_seconds / 60;
        if (timer_text) timer_text.text = minutes + ":" + seconds;
        if (Input.GetKeyDown(KeyCode.W))
        {
            if (!edit_panel.activeSelf) open();
            else close();
        }
    }

    private void Start()
    {
        if (open_button) open_button.onClick.AddListener(open);
        close_button.onClick.AddListener(close);
        copy_button.onClick.AddListener(copy_group);
        paste_button.onClick.AddListener(pasete_group);
        save_to_clipboard_button.onClick.AddListener(save_to_clipboard);
        load_from_clipboard_button.onClick.AddListener(load_from_clipboard);
        waves_count_inputField.onEndEdit.AddListener(delegate { waves_count_changed();});
        current_wave_inputField.onEndEdit.AddListener(delegate { switch_to_wave(int.Parse(current_wave_inputField.text)); });
        groups_count_inputField.onEndEdit.AddListener(delegate { groups_count_changed(); });
        current_group_inputField.onEndEdit.AddListener(delegate { switch_to_group(int.Parse(current_wave_inputField.text), int.Parse(current_group_inputField.text)); });
        enemies_count_inputField.onEndEdit.AddListener(delegate { enemies_count_changed(); });
        interval_inputField.onEndEdit.AddListener(delegate { enemies_intervals_changed(); });
        start_delay_inputField.onEndEdit.AddListener(delegate { enemies_start_delay_changed(); });
        enemy_type_inputField.onEndEdit.AddListener(delegate { enemies_enemy_type_changed(); });
        group_way_inputField.onEndEdit.AddListener(delegate { enemies_way_changed(); });
    }

    #region open close window

    private void open()
    {
        time_scale_before_open = Time.timeScale; 
        Time.timeScale = 0;
        CameraMovement.instance.moving = false;
        GameController.instance.buttons_active = false;

        print("current wave: " + WavesLogic.instance.current_wave);
        current_wave_inputField.text = ""+ WavesLogic.instance.current_wave;
        switch_to_wave(WavesLogic.instance.current_wave);

        edit_panel.SetActive(true);
    }

    private void close()
    {
        Time.timeScale = time_scale_before_open;
        CameraMovement.instance.moving = true;
        GameController.instance.buttons_active = true;

        edit_panel.SetActive(false);
    }

    #endregion
    #region helpers

    public Waves get_in_waves_format()
    {
        List<int> group = new List<int>();
        List<int> enemies_number = new List<int>();
        List<int> groups_way = new List<int>();
        List<int> group_start_delay = new List<int>();
        List<int> intervals = new List<int>();
        int[] waves_lengths = new int[waves.Length];

        for (int wave_index = 0; wave_index < waves.Length; wave_index++)
        {
            waves_lengths[wave_index] = waves[wave_index].Length;
            for (int group_index = 0; group_index < waves[wave_index].Length; group_index++)
            {
                group.Add(waves[wave_index][group_index].enemy_type);
                enemies_number.Add(waves[wave_index][group_index].enemies_count);
                groups_way.Add(waves[wave_index][group_index].way);
                group_start_delay.Add(waves[wave_index][group_index].start_delay);
                intervals.Add(waves[wave_index][group_index].intervals);
            }
        }

        Waves _return = new Waves();
        _return.group = group.ToArray();
        _return.enemies_number = enemies_number.ToArray();
        _return.groups_way = groups_way.ToArray();
        _return.group_start_delay = group_start_delay.ToArray();
        _return.intervals = intervals.ToArray();
        _return.waves_lengths = waves_lengths;

        return _return;
    }

    private void switch_to_wave(int wave)
    {
        if (wave >= waves.Length)
        {
            wave = waves.Length - 1;
            current_wave_inputField.text = "" + wave;
        }
        else if (wave < 0)
        {
            wave = 0;
            current_wave_inputField.text = "0";
        }

        groups_count_inputField.text = "" + waves[wave].Length;
        current_group_inputField.text = "0";
        switch_to_group(wave, 0);
    }
    private void switch_to_group(int wave,int group)
    {
        if (group >= waves[wave].Length)
        {
            group = waves[wave].Length - 1;
            current_group_inputField.text = "" + group;
        }
        else if (group < 0)
        {
            group = 0;
            current_group_inputField.text = "0";
        }

        enemies_count_inputField.text = "" + waves[wave][group].enemies_count;
        interval_inputField.text = "" + waves[wave][group].intervals;
        start_delay_inputField.text = "" + waves[wave][group].start_delay;
        enemy_type_inputField.text = "" + waves[wave][group].enemy_type;
        group_way_inputField.text = "" + waves[wave][group].way;
    }
    private Group get_default_group()
    {
        Group default_group = new Group();
        default_group.start_delay = 300;
        default_group.intervals = 50;
        default_group.way = 0;
        default_group.enemy_type = 0;
        default_group.enemies_count = 10;

        return default_group;
    }

    #endregion

    private void create_default_waves()
    {
        if (waves == null)
        {
            waves = new Group[2][];

            waves[0] = new Group[1];

            waves[0][0] = new Group();

            waves[1] = new Group[1];

            waves[1][0] = new Group();

            waves[0][0].start_delay = 20;
            waves[0][0].intervals = 50;
            waves[0][0].way = 0;
            waves[0][0].enemy_type = 0;
            waves[0][0].enemies_count = 10;

            waves[1][0].start_delay = 20;
            waves[1][0].intervals = 120;
            waves[1][0].way = 0;
            waves[1][0].enemy_type = 2;
            waves[1][0].enemies_count = 15;
        }
    }

    private void waves_count_changed()
    {
        int size = int.Parse(waves_count_inputField.text);
        if (size < 1)
        {
            size = 1;
            waves_count_inputField.text = "1";
        }

        Group[][] new_array = new Group[size][];

        for (int i = 0; i < size; i++)
        {
            if (i < waves.Length)
                new_array[i] = waves[i];
            else new_array[i] = new Group[1] { get_default_group() };
        }
        waves = new_array;
        switch_to_wave(waves.Length);
        current_wave_inputField.text = "" + (waves.Length - 1);
    }

    private void groups_count_changed()
    {
        int wave = int.Parse(current_wave_inputField.text);
        int size = int.Parse(groups_count_inputField.text);
        if (size < 1)
        {
            size = 1;
            groups_count_inputField.text = "1";
        }

        Group[] new_array = new Group[size];

        for (int i = 0; i < size; i++)
        {
            if (i < waves[wave].Length)
                new_array[i] = waves[wave][i];
            else new_array[i] = get_default_group();
        }
        waves[wave] = new_array;
        switch_to_group(wave,0);
    }

    private void enemies_count_changed()
    {
        int wave = int.Parse(current_wave_inputField.text);
        int group = int.Parse(current_group_inputField.text);
        int new_value = int.Parse(enemies_count_inputField.text);
        if (new_value <= 0)
        {
            new_value = 1;
            enemies_count_inputField.text = "1";
        }
        waves[wave][group].enemies_count = new_value;
    }

    private void enemies_intervals_changed()
    {
        int wave = int.Parse(current_wave_inputField.text);
        int group = int.Parse(current_group_inputField.text);
        int new_value = int.Parse(interval_inputField.text);
        if (new_value <= 0)
        {
            new_value = 1;
            interval_inputField.text = "1";
        }
        waves[wave][group].intervals = new_value;
    }

    private void enemies_start_delay_changed()
    {
        int wave = int.Parse(current_wave_inputField.text);
        int group = int.Parse(current_group_inputField.text);
        int new_value = int.Parse(start_delay_inputField.text);
        if (new_value < 0)
        {
            new_value = 0;
            start_delay_inputField.text = "0";
        }
        waves[wave][group].start_delay = new_value;
    }

    private void enemies_enemy_type_changed()
    {
        int wave = int.Parse(current_wave_inputField.text);
        int group = int.Parse(current_group_inputField.text);
        int new_value = int.Parse(enemy_type_inputField.text);
        if (new_value < 0)
        {
            new_value = 0;
            enemy_type_inputField.text = "0";
        }
        waves[wave][group].enemy_type = new_value;
    }

    private void enemies_way_changed()
    {
        int wave = int.Parse(current_wave_inputField.text);
        int group = int.Parse(current_group_inputField.text);
        int new_value = int.Parse(group_way_inputField.text);
        if (new_value < 0)
        {
            new_value = 0;
            group_way_inputField.text = "0";
        }
        waves[wave][group].way = new_value;
    }
    Group current_copy;
    private void copy_group()
    {
        current_copy = new Group();

        int current_wave = int.Parse(current_wave_inputField.text);
        int current_group = int.Parse(current_group_inputField.text);

        current_copy.enemies_count = waves[current_wave][current_group].enemies_count;
        current_copy.enemy_type = waves[current_wave][current_group].enemy_type;
        current_copy.intervals = waves[current_wave][current_group].intervals;
        current_copy.start_delay = waves[current_wave][current_group].start_delay;
        current_copy.way = waves[current_wave][current_group].way;
    }

    private void pasete_group()
    {
        if (current_copy != null)
        {
            Group new_grup = new Group();

            int current_wave = int.Parse(current_wave_inputField.text);
            int current_group = int.Parse(current_group_inputField.text);

            new_grup.enemies_count = current_copy.enemies_count;
            new_grup.enemy_type = current_copy.enemy_type;
            new_grup.intervals = current_copy.intervals;
            new_grup.start_delay = current_copy.start_delay;
            new_grup.way = current_copy.way;

            waves[current_wave][current_group] = new_grup;

            switch_to_group(current_wave, current_group);
        }
    }

    private void save_to_clipboard()
    {
        CopyToClipboard(JsonUtility.ToJson(get_in_waves_format()));
    }

    private void load_from_clipboard()
    {
        Waves new_waves = JsonUtility.FromJson<Waves>(get_from_clipboard());
        waves = new Group[new_waves.waves_lengths.Length][];
        int cur_group_index = 0;

        for (int wave_index = 0; wave_index < new_waves.waves_lengths.Length; wave_index++)
        {
            waves[wave_index] = new Group[new_waves.waves_lengths[wave_index]];
            for (int group_index = cur_group_index; group_index < cur_group_index + new_waves.waves_lengths[wave_index]; group_index++)
            {
                Group new_group = new Group();

                new_group.start_delay = new_waves.group_start_delay[group_index];
                new_group.intervals = new_waves.intervals[group_index];
                new_group.way = new_waves.groups_way[group_index];
                new_group.enemy_type = new_waves.group[group_index];
                new_group.enemies_count = new_waves.enemies_number[group_index];

                waves[wave_index][group_index - cur_group_index] = new_group;

            }
            cur_group_index += new_waves.waves_lengths[wave_index];
        }
    }

    void CopyToClipboard(string str)
    {
        TextEditor textEditor = new TextEditor();
        textEditor.text = str;
        textEditor.SelectAll();
        textEditor.Copy();
    }

    private string get_from_clipboard()
    {
        TextEditor textEditor = new TextEditor();
        textEditor.isMultiline = true;
        textEditor.Paste();
        return textEditor.text;
    }
}
