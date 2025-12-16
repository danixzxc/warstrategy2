using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

public class MapInfo : MonoBehaviour
{
    public static MapInfo instance;
    private void Awake()
    {
        gameObject.AddComponent<GameAuido>();
        instance = this;
    }
    [SerializeField] private SpriteRenderer map_sprite;
    public Tower[] towers = new Tower[36];
    public int towers_count = 0;

    [SerializeField] private Transform envoronments_container;

    private MapInformation mapInformation;

    private int[] environments_healths;

    private GameObject[] environments_game_objects;

    private SpriteRenderer[] health_bars;

    [SerializeField] private string map_file_name = "";

    [SerializeField] private Color health_line_color;

    private void Start()
    {
        loadMapInformation();
        load_game_map_sprite();
    }

    // Загружает о расположении дорог, объектов окружения, свободных мест для башень.
    private void loadMapInformation()
    {
        if (map_file_name == "") map_file_name = "map_" + GameLogic.instance.current_level;
        string path = Path.Combine(Application.streamingAssetsPath, map_file_name + ".json");
        string _save;

        if (Application.platform == RuntimePlatform.Android)
        {
            UnityWebRequest www = UnityWebRequest.Get(path);
            www.SendWebRequest();
            while (!www.isDone) ;
            _save = www.downloadHandler.text;
        }
        else _save = File.ReadAllText(path);


        mapInformation = JsonUtility.FromJson<MapInformation>(_save);
        int map_size = (int)mapInformation.size.x * (int)mapInformation.size.y;
        environments_healths = new int[map_size];
        environments_game_objects = new GameObject[map_size];
        health_bars = new SpriteRenderer[map_size];

        if (envoronments_container)
            for (int i = 0; i < envoronments_container.childCount; i++)
            {
                var environment = envoronments_container.GetChild(i);
                int x = Mathf.RoundToInt(environment.position.x) + (int)(mapInformation.size.x - 1) / 2;
                int y = -(Mathf.RoundToInt(environment.position.y) - (int)(mapInformation.size.y - 1) / 2);
                int index = x + y * (int)mapInformation.size.x;
                if (environment.name == "[ENV]rock")
                {
                    mapInformation.map[index] = 22;
                    environments_healths[index] = 700;
                }
                else if (environment.name == "[ENV]bush")
                {
                    mapInformation.map[index] = 23;
                    environments_healths[index] = 10;
                }
                else
                {
                    environments_healths[index] = 100;
                    mapInformation.map[index] = 21;
                }
                environments_game_objects[index] = environment.gameObject;
            }
        Container.instance.loaded();

        if (towers_count == 0) towers = new Tower[36];
    }

    private void load_game_map_sprite()
    {
        string path = Path.Combine(Application.streamingAssetsPath, $"level_{FirebaseManager.instance.get_string_num(GameLogic.instance.current_level)}.jpg");
        byte[] bytes;

        if (Application.platform == RuntimePlatform.Android)
        {
            UnityWebRequest www = UnityWebRequest.Get(path);
            www.SendWebRequest();
            while (!www.isDone) ;
            bytes = www.downloadHandler.data;
        }
        else bytes = File.ReadAllBytes(path);

        Texture2D texture = new Texture2D((int)mapInformation.size.x * 150, (int)mapInformation.size.y * 150);
        Rect rec = new Rect(0, 0, texture.width, texture.height);
        texture.LoadImage(bytes);
        var sprite = Sprite.Create(texture, rec, new Vector2(.5f, .5f), 150);
        map_sprite.sprite = sprite;
        print("Sprite Loaded.");
    }

    public void asignTowers()
    {
        int count = towers_count;
        towers_count = 0;
        for (int i = 0; i < count; i++)
        {
            towers_count++;
            EnemiesLogic.instance.create_new_fragments(towers[towers_count - 1], true, true);
            Vector2Int pos = new Vector2Int(Mathf.RoundToInt(towers[i].transform.position.x), Mathf.RoundToInt(towers[i].transform.position.y));
            int x = pos.x + (int)(mapInformation.size.x - 1) / 2;
            int y = -(pos.y - (int)(mapInformation.size.y - 1) / 2);
            mapInformation.map[x + y * (int)mapInformation.size.x] = 40 + towers[i].type;

            calculate_range_multipliers();
        }
    }

    // Выдает то, что находится в ячейке.
    public int get_cell_object(Vector2Int pos)
    {
        int x = pos.x + (int)(mapInformation.size.x - 1) / 2;
        int y = -(pos.y - (int)(mapInformation.size.y - 1) / 2);

        if (x >= 0 && x < mapInformation.size.x && y >= 0 && y < mapInformation.size.y)
        {
            int value = 0;
            int index = x + y * (int)mapInformation.size.x;
            if (index < mapInformation.map.Length)
            {
                value = mapInformation.map[index];
            }

            return value - (value % 10);
        }
        return 30;
    }

    public Vector2Int get_map_size()
    {
        return new Vector2Int( (int)mapInformation.size.x, (int)mapInformation.size.y);
    }

    public void spawn_tower(int type, Vector2Int pos, int level = 1, float gun_rotation = 0)
    {
        if (towers_count < 25)
        {
            string name = (GameLogic.instance.game_pack == 1) ? "tower_" + type + "_L" + level : $"pack_{GameLogic.instance.game_pack}_tower_{type}_L{level}";
            Addressables.InstantiateAsync(name).Completed += handle =>
            {
                if (towers_count < 35)
                {
                    var new_tower_go = handle.Result;
                    new_tower_go.transform.position = (Vector2)pos;
                    towers[towers_count] = new_tower_go.GetComponent<Tower>();
                    towers_count++;
                    EnemiesLogic.instance.create_new_fragments(towers[towers_count - 1], true, true);

                    int x = pos.x + (int)(mapInformation.size.x - 1) / 2;
                    int y = -(pos.y - (int)(mapInformation.size.y - 1) / 2);
                    mapInformation.map[x + y * (int)mapInformation.size.x] = 40 + type;

                    calculate_range_multipliers();


                    if (environment_arrow.activeSelf && Vector2.Distance(environment_target_pos, towers[towers_count - 1].transform.position) - .35f < towers[towers_count - 1].range * towers[towers_count - 1].range_multiplier)
                    {
                        towers[towers_count - 1].environment_target = true;
                    }

                    if (towers[towers_count - 1].gun) towers[towers_count - 1].gun.transform.rotation = Quaternion.Euler(0, 0, gun_rotation);
                }
                else
                {
                    Addressables.ReleaseInstance(handle.Result);
                }
            };
        }
    }
    public Tower get_tower_in_cell(Vector2Int pos)
    {
        int cell_tower_index = 0;
        bool found = false;
        for (int tower_index = 0; tower_index < towers_count; tower_index++)
        {
            if (Mathf.RoundToInt(towers[tower_index].transform.position.x) == pos.x && Mathf.RoundToInt(towers[tower_index].transform.position.y) == pos.y)
            {
                cell_tower_index = tower_index;
                found = true;
                break;
            }
        }
        if (found)
            return towers[cell_tower_index];
        else
            return null;
    }
    public void remove_tower(Vector2Int pos)
    {
        int remove_tower_index = 0;
        for (int tower_index = 0; tower_index < towers_count; tower_index++)
        {
            if (Mathf.RoundToInt(towers[tower_index].transform.position.x) == pos.x && Mathf.RoundToInt(towers[tower_index].transform.position.y) == pos.y)
            {
                remove_tower_index = tower_index;
                break;
            }
        }
        Tower remove_tower = towers[remove_tower_index];
        towers = remove_tower_from_array(remove_tower_index);
        EnemiesLogic.instance.create_new_fragments(remove_tower, false, true);
        int x = pos.x + (int)(mapInformation.size.x - 1) / 2;
        int y = -(pos.y - (int)(mapInformation.size.y - 1) / 2);
        mapInformation.map[x + y * (int)mapInformation.size.x] = 0;
        Destroy(remove_tower.gameObject);

        calculate_range_multipliers();
    }
    public void upgrade_tower(Vector2Int pos, float gun_rotation = 0)
    {
        int cell_tower_index = 0;
        for (int tower_index = 0; tower_index < towers_count; tower_index++)
        {
            if (Mathf.RoundToInt(towers[tower_index].transform.position.x) == pos.x && Mathf.RoundToInt(towers[tower_index].transform.position.y) == pos.y)
            {
                cell_tower_index = tower_index;
                break;
            }
        }
        Tower remove_tower = towers[cell_tower_index];

        int type = remove_tower.type;
        int level = remove_tower.level + 1;
        string name = (GameLogic.instance.game_pack == 1) ? "tower_" + type + "_L" + level : $"pack_{GameLogic.instance.game_pack}_tower_{type}_L{level}";
        Addressables.InstantiateAsync(name).Completed += handle =>
        {
            var new_tower_go = handle.Result;
            new_tower_go.transform.position = remove_tower.transform.position;
            EnemiesLogic.instance.create_new_fragments(remove_tower, false, false);
            towers[cell_tower_index] = new_tower_go.GetComponent<Tower>();
            if (towers[cell_tower_index].gun) towers[cell_tower_index].gun.transform.rotation = Quaternion.Euler(0, 0, gun_rotation);
            EnemiesLogic.instance.create_new_fragments(towers[cell_tower_index], true, true);
            Addressables.ReleaseInstance(remove_tower.gameObject);

            calculate_range_multipliers();

            if (environment_arrow.activeSelf && Vector2.Distance(environment_target_pos, towers[cell_tower_index].transform.position) - .35f < towers[cell_tower_index].range * towers[cell_tower_index].range_multiplier)
            {
                towers[cell_tower_index].environment_target = true;
            }
        };
    }
    private Tower[] remove_tower_from_array(int index)
    {
        Tower[] _return = new Tower[towers.Length];
        for (int i = 0; i < towers_count - 1; i++)
        {
            if (i >= index)
            {
                _return[i] = towers[i + 1];
            }
            else
            {
                _return[i] = towers[i];
            }
        }
        towers_count--;
        return _return;
    }

    public Vector2 environment_target_pos = new Vector2(100,100);

    [SerializeField] private GameObject environment_arrow;

    public void set_environment_target(Vector2 pos)
    {
        if (pos != environment_target_pos)
        {
            environment_target_pos = pos;
            for (int tower_index = 0; tower_index < towers_count; tower_index++)
            {
                var tower = towers[tower_index];
                if (Vector2.Distance(environment_target_pos, tower.transform.position) - .35f < tower.range * tower.range_multiplier) tower.environment_target = true;
                else tower.environment_target = false;
            }
            environment_arrow.SetActive(true);
            environment_arrow.transform.position = pos;
        }
        else
        {
            for (int tower_index = 0; tower_index < towers_count; tower_index++)
            {
                var tower = towers[tower_index];
                tower.environment_target = false;
            }
            environment_arrow.SetActive(false);
            environment_target_pos = new Vector2(100, 100);
        }
    }

    public void damage_environment(Vector2 pos, int damage)
    {
        int x = Mathf.RoundToInt(pos.x) + (int)(mapInformation.size.x - 1) / 2;
        int y = -(Mathf.RoundToInt(pos.y) - (int)(mapInformation.size.y - 1) / 2);
        int index = x + y * (int)mapInformation.size.x;
        environments_healths[index] -= damage;
        int max_health = get_max_health(index);

        if (environments_healths[index] <= 0)
        {
            remove_environment(index);
            if (pos == environment_target_pos) set_environment_target(pos);
            var effect = Instantiate(Container.instance.objects[2]);
            effect.transform.position = pos;
            GameAuido.instance.Play(3);

            int type = 0;
            if (max_health >= 500)
            {
                GameLogic.instance.add_coins(35);
                type = 2;
            }
            else if (max_health >= 100)
            {
                GameLogic.instance.add_coins(10);
                type = 1;
            }
            else
            {
                GameLogic.instance.add_coins(5);
            }

            Container.instance.spawn_environment_effect(type, pos);
        }
        else
        {

            if (!health_bars[index])
            {
                var new_health_bar = Instantiate(Container.instance.objects[3], environments_game_objects[index].transform);
                health_bars[index] = new_health_bar.transform.GetChild(0).GetChild(0).GetComponent<SpriteRenderer>();
                new_health_bar.transform.position = environments_game_objects[index].transform.position;
            }
            float health_coef = (float)environments_healths[index] / max_health;
            health_bars[index].transform.parent.localScale = new Vector3(health_coef, 1);
            health_bars[index].color = health_line_color * health_coef + Color.red * (1 - health_coef);
        }
    }

    private void remove_environment(int index)
    {
        environments_healths[index] = 0;
        mapInformation.map[index] = 0;
        environments_healths[index] = 0;
        Destroy(environments_game_objects[index]);
    }

    private int get_max_health(int map_index)
    {
        int max_health = 100;
        if (mapInformation.map[map_index] == 22)
        {
            max_health = 700;
        }
        else if (mapInformation.map[map_index] == 23)
        {
            max_health = 10;
        }
        return max_health;
    }

    private class MapInformation
    {
        // Размер карты.
        public Vector2 size;

        // Показывает что хранится на каждом из мест на карте.
        // 0 - пусто, 1 - дорога, 2 - объект окружения, 3 - занято, 4 - башня.
        public int[] map; 
    }

    #region Ranges multipliers_calculation

    private void calculate_range_multipliers()
    {
        var radars_and_not_radars = get_radars_and_else_towers();
        var effected_towers = get_towers_with_changed_ranges(radars_and_not_radars[0], radars_and_not_radars[1]);

        for (int tower_index = 0; tower_index < effected_towers.Length; tower_index++)
        {
            var tower = effected_towers[tower_index];
            print("multip: "+tower.name);
            EnemiesLogic.instance.create_new_fragments(tower, false, false);
            if (tower_index != effected_towers.Length - 1) EnemiesLogic.instance.create_new_fragments(tower, true, false);//
            else EnemiesLogic.instance.create_new_fragments(tower, true, true);
        }
    }

    private Tower[][] get_radars_and_else_towers()
    {
        List<Tower> radars = new List<Tower>();
        List<Tower> not_radars = new List<Tower>();

        for (int tower_index = 0; tower_index < towers_count; tower_index++)
        {
            if (towers[tower_index].type == 4)
            {
                radars.Add(towers[tower_index]);
            }
            else if (towers[tower_index].type != 3 && towers[tower_index].type != 7)
            {
                not_radars.Add(towers[tower_index]);
            }
        }
        return new Tower[2][] { radars.ToArray(), not_radars.ToArray() };
    }

    private Tower[] get_towers_with_changed_ranges(Tower[] radars, Tower[] not_radars)
    {
        List<Tower> _return = new List<Tower>();

        for (int tower_index = 0; tower_index < not_radars.Length; tower_index++)
        {
            float old_range_multiplier = not_radars[tower_index].range_multiplier;
            not_radars[tower_index].range_multiplier = 1;
            foreach (var radar in radars)
            {
                if (Vector2.Distance(radar.transform.position, not_radars[tower_index].transform.position) - .35f < radar.range && not_radars[tower_index].range_multiplier < radar.range_multiplier)
                {
                    not_radars[tower_index].range_multiplier = radar.range_multiplier;
                }
            }
            if (not_radars[tower_index].range_multiplier != old_range_multiplier) _return.Add(not_radars[tower_index]);
        }

        return _return.ToArray();
    }

    #endregion

    public int[] get_map_info()
    {
        int[] _return = (int[])environments_healths.Clone();
        for (int env_index = 0; env_index < _return.Length; env_index++)
        {
            if (_return[env_index] != 0)
            {
                _return[env_index] = _return[env_index] * 10 + 3;
            }
        }
        foreach (var tower in towers)
        {
            if (tower)
            {
                int x = Mathf.RoundToInt(tower.transform.position.x) + (int)(mapInformation.size.x - 1) / 2;
                int y = -(Mathf.RoundToInt(tower.transform.position.y) - (int)(mapInformation.size.y - 1) / 2);
                int index = x + y * (int)mapInformation.size.x;

                int tower_roatation = (int)tower.gun.eulerAngles.z;
                if (tower_roatation <= 0) tower_roatation += 360;

                _return[index] = tower_roatation * 1000 + tower.level * 100 + tower.type * 10 + 4;
            }
        }

        return _return;
    }

    public void set_map_info(int[] info)
    {
        for (int index = 0; index < environments_healths.Length; index++)
        {
            int cell_type = info[index] % 10;

            if (environments_healths[index] != 0)
            {
                if (cell_type == 3)
                {
                    environments_healths[index] = (info[index] - cell_type) / 10;
                    int max_health = get_max_health(index);
                    if (environments_healths[index] != max_health)
                    {
                        print(environments_game_objects.Length);
                        damage_environment(environments_game_objects[index].transform.position, 0);
                    }
                }
                else
                {
                    remove_environment(index);
                }
            }

            if (cell_type == 4)
            {
                int tower_type = info[index] % 100 / 10;
                int tower_level = info[index]% 1000 / 100;
                int tower_rotation = info[index] / 1000;
                int x = index % (int)mapInformation.size.x - (int)mapInformation.size.x / 2;
                int y = (int)mapInformation.size.y / 2 - index / (int)mapInformation.size.x ;
                var pos = new Vector2Int(x,y);

                spawn_tower(tower_type, pos, tower_level, tower_rotation);
            }
        }
    }
}
