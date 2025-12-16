using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class WavesLogic : MonoBehaviour
{

    public static WavesLogic instance;
    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        // Загрузка заготовленных данных о волнах.
        loadWavesInformation();
    }
    private void FixedUpdate()
    {
        // Главная логика волн.
        if (Container.instance.game_loaded && Container.instance.enemies_loaded)
        {
            waves_logic();
        }
    }

    // Будет хранить волны в текущем уровне.
    private Waves waves;

    [SerializeField] private string waves_file_name = "";

    // Загружает данные о волнах и задает нужные значения.
    private void loadWavesInformation()
    {
        if (!WavesCreator.instance)
        {
            if (waves_file_name == "") waves_file_name = "waves_" + GameLogic.instance.current_level;
            Addressables.LoadAssetAsync<TextAsset>(waves_file_name).Completed += handle =>
            {
                var _save = handle.Result;
                wave_start_logic(JsonUtility.FromJson<Waves>(_save.text));
            };
        }
        else
        {
            wave_start_logic(WavesCreator.instance.get_in_waves_format());
        }
    }

    private void wave_start_logic(Waves file)
    {
        waves = file;

        if (Checkpoints.current_save != null)
        {
            current_wave = Checkpoints.current_save.wave;
            current_group = 0;
            for (int wave_index = 0; wave_index < current_wave; wave_index++)
            {
                current_group += waves.waves_lengths[wave_index];
            }
        }

        enemies_spawned = new int[waves.waves_lengths[current_wave]];
        timers = new int[waves.waves_lengths[current_wave]];
        int[] wave_enemies_types = new int[waves.waves_lengths[current_wave]];
        for (int group = 0; group < waves.waves_lengths[current_wave]; group++)
        {
            enemies_number_in_wave += waves.enemies_number[current_group + group];
            wave_enemies_types[group] = waves.group[current_group + group];
        }


        Container.instance.load_enemies_in_current_wave(wave_enemies_types);

        GameLogic.instance.next_wave(current_wave, waves.waves_lengths.Length);

        Container.instance.loaded();
    }

    // Функционал волн.
    #region Waves logic

    // Текущаяя волна.
    public int current_wave;

    // Число полностью завершивших свой путь групп.
    private int current_group;

    // Число уже появившихся врагов в каждой из групп в волне.
    private int[] enemies_spawned;

    // Таймеры для каждой из групп.
    private int[] timers;

    // Число врагов, убитых в текущей волне.
    private int enemies_dead_in_wave;

    // Число врагов, которые появятся в текущей волне.
    private int enemies_number_in_wave;

    // Создает врагов согласео загруженным данным. Должен вызываться в Fixed update.
    private void waves_logic()
    {
        for (int group = 0; group < waves.waves_lengths[current_wave]; group++)
        {
            int way = waves.groups_way[current_group + group];
            if (enemies_spawned[group] < waves.enemies_number[current_group + group])
            {
                timers[group]++;
                if (enemies_spawned[group] > 0 && timers[group] >= waves.intervals[current_group + group])
                {
                    var enemy = Container.instance.get_enemy_from_group(group);
                    enemy.enemy_way = way;
                    EnemiesLogic.instance.spawn_enemy(enemy);
                    enemies_spawned[group]++;
                    timers[group] = 0;
                }
                else if (enemies_spawned[group] == 0 && timers[group] >= waves.group_start_delay[current_group + group] && Container.instance.enemies_loaded)
                {
                    var enemy = Container.instance.get_enemy_from_group(group);
                    enemy.enemy_way = way;
                    EnemiesLogic.instance.spawn_enemy(enemy);
                    enemies_spawned[group]++;
                    timers[group] = 0;
                }
            }
        }
    }
    #endregion

    // Вызывается когда враг был убит.
    public void enemy_destroyed()
    {
        // Увеличение значения убитых врагов в текущей волне.
        enemies_dead_in_wave++;

        if (enemies_dead_in_wave >= enemies_number_in_wave)
        {
            GameLogic.instance.next_wave(current_wave + 1, waves.waves_lengths.Length);
            if (current_wave + 1 < waves.waves_lengths.Length)
            {
                // Переключение к следующей волне.
                current_group += waves.waves_lengths[current_wave];
                current_wave++;
                enemies_spawned = new int[waves.waves_lengths[current_wave]]; 
                timers = new int[waves.waves_lengths[current_wave]]; 
                enemies_dead_in_wave = 0;
                enemies_number_in_wave = 0;
                int[] wave_enemies_types = new int[waves.waves_lengths[current_wave]];
                for (int group = 0; group < waves.waves_lengths[current_wave]; group++) 
                {
                    enemies_number_in_wave += waves.enemies_number[current_group + group];
                    wave_enemies_types[group] = waves.group[current_group + group];
                }
                Container.instance.enemies_loaded = false;
                Container.instance.load_enemies_in_current_wave(wave_enemies_types);
                if (Checkpoints.instance) Checkpoints.instance.save_wave_check_point();
                GameController.instance.nextWave();
            }
        }
    }
}

// Класс, описывающий волны.
public class Waves
{
    // Группа состоит только из одного вида врагов. Хранит вид врагов.
    public int[] group;

    // Хранит количество врагов в группе.
    public int[] enemies_number;

    // Путь по которому идет группа.
    public int[] groups_way;

    // Задержка перед выходом группы.
    public int[] group_start_delay;

    // Время, через которое появляются враги в группе.
    public int[] intervals;

    // Число групп, из которых состоит волна.
    public int[] waves_lengths;
}
