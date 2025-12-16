using UnityEngine;
using UnityEngine.AddressableAssets;

public class Container : MonoBehaviour
{
    public static Container instance;

    private void Awake()
    {
        instance = this;
    }

    public GameObject[] objects;

    public bool game_loaded = false;

    public bool enemies_loaded = false;

    private int loaded_count = 0;

    private int[] enemies_healths = new int[]
    {
        3, // soldier
        18, // soldier 2
        22, // car
        100, // track
        260, // tank 1
        750, // tank 2
        1850, // tank 3
        3500, // tank 4
        7300, // tank 5
        30000 // tank 6
    };

    private float[] enemies_speeds = new float[]
    {
        0.02317f, // soldier
        0.01817f, // soldier 2
        0.04627f, // car
        0.02647f, // track
        0.01987f, // tank 1
        0.0157f, // tank 2
        0.01387f, // tank 3
        0.01157f, // tank 4
        0.01157f, // tank 5
        0.01157f // tank 6
    };

    private int[] enemies_die_effects = new int[]
    {
        6, // soldier
        7, // soldier 2
        2, // car
        2, // track
        2, // tank 1
        2, // tank 2
        2, // tank 3
        2, // tank 4
        2, // tank 5
        2 // tank 6
    };

    private int[] enemies_coins = new int[]
    {
        3, // soldier
        5, // soldier 2
        7, // car
        8, // track
        15, // tank 1
        20, // tank 2
        25, // tank 3
        30, // tank 4
        35, // tank 5
        50 // tank 6
    };

    [SerializeField] private GameObject[] enemies_coins_effects;
    [SerializeField] private GameObject[] environments_coins_effects;

    private GameObject[] current_wave_enemies_objects;
    private int[] groups_enemies_types;
    public float[] groups_speeds;
    public int[] groups_healths;

    [SerializeField] private bool asignTowers = false;

    public void loaded()
    {
        loaded_count++;
        if (loaded_count == 3)
        {
            game_loaded = true;
            if (Checkpoints.instance)  Checkpoints.instance.load_checkpoint_to_game();
            if (asignTowers) MapInfo.instance.asignTowers();
        }
    }

    private int loaded_enemies_count = 0;

    // Начинает загрузку врагов в следующей волне.
    public void load_enemies_in_current_wave(int[] wave_enemies_types)
    {
        current_wave_enemies_objects = new GameObject[wave_enemies_types.Length];
        groups_healths = new int[wave_enemies_types.Length];
        groups_speeds = new float[wave_enemies_types.Length];
        groups_enemies_types = new int[wave_enemies_types.Length];
        int index = 0;
        loaded_enemies_count = 0;

        foreach (var type in wave_enemies_types)
        {
            load_enemy_group(type, index);
            groups_speeds[index] = enemies_speeds[type];
            groups_healths[index] = enemies_healths[type];
            groups_enemies_types[index] = type;
            index++;
        }
    }

    private void load_enemy_group(int group, int enemy_index)
    {
        string name = (GameLogic.instance.game_pack == 1) ? "enemy_" + group : $"pack_{GameLogic.instance.game_pack}_enemy_{group}";
        Addressables.LoadAssetAsync<GameObject>(name).Completed += handle =>
        {
            current_wave_enemies_objects[enemy_index] = handle.Result;
            loaded_enemies_count++;
            if (loaded_enemies_count >= current_wave_enemies_objects.Length) enemies_loaded = true;
        };
    }

    public Enemy get_enemy_from_group(int group)
    {
        var enemy = new Enemy();
        var new_enemy = Instantiate(current_wave_enemies_objects[group]);
        enemy.transform = new_enemy.transform;
        enemy.group = group;
        enemy.health = groups_healths[group];
        return enemy;
    }

    public void spawn_enemy_die_effect(int group, Vector2 position, float rotation)
    {
        GameLogic.instance.add_coins(enemies_coins[groups_enemies_types[group]]);
        var effect = Instantiate(objects[enemies_die_effects[groups_enemies_types[group]]]);
        var coin_effect = Instantiate(enemies_coins_effects[groups_enemies_types[group]]);
        effect.transform.position = position;
        coin_effect.transform.position = position;
        effect.transform.eulerAngles = new Vector3(0, 0, rotation);
        if (enemies_die_effects[groups_enemies_types[group]] == 2) GameAuido.instance.Play(3);
    }

    public void spawn_environment_effect(int type, Vector2 pos)
    {
        var effect = Instantiate(objects[2]);
        effect.transform.position = pos;

        GameObject coin_effect;

        if (type == 0) coin_effect = Instantiate(environments_coins_effects[0]);
        else if (type == 1) coin_effect = Instantiate(environments_coins_effects[1]);
        else coin_effect = Instantiate(environments_coins_effects[2]);

        coin_effect.transform.position = pos;
    }
}
