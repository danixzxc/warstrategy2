using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SniperTower : Tower
{

    [SerializeField] private Animator animator;

    [SerializeField] private int damage = 20;

    [SerializeField] private int reload_time = 50;

    [SerializeField] private float rotation_speed = 10;

    [SerializeField] private int guns_count = 1;

    // Текущая цель.
    private Vector2 target;

    // Текущее вращение по z в рамках 0 - 360.
    private float rotation_z = 0;

    // Говорит когда стрелять.
    private bool target_found = false;

    // Таймер. Если его значение больше reload_time, то можно стрелять.
    private int timer = 0;

    private void Start()
    {
        rotation_z = correct_rotation(gun.transform.rotation.eulerAngles.z);
        timer = reload_time;
    }

    private void FixedUpdate()
    {
        if (enemies_array_range != null)
        {

            // Расстояние до ближайшего врага.
            float target_dist = 1000;

            // Пробегается по своим фрагментам пути в каждом пути и находит ближайшего к
            // финишу врага если он есть.
            if (!environment_target)
                for (int way = 0; way < enemies_array_range.Length; way++)
                {
                    for (int fragment_index = enemies_array_range[way].Length - 1; fragment_index >= 0; fragment_index--)
                    {
                        int fragment = enemies_array_range[way][fragment_index];
                        if (EnemiesLogic.instance.enemies[way][fragment][0] != null)
                        {
                            if (EnemiesLogic.instance.enemies[way][fragment][0].distance_to_finish < target_dist)
                            {
                                target_dist = EnemiesLogic.instance.enemies[way][fragment][0].distance_to_finish;
                                target = EnemiesLogic.instance.enemies[way][fragment][0].transform.position;
                                break;
                            }
                        }
                    }
                }
            else
            {
                target = MapInfo.instance.environment_target_pos;
                target_dist = 0;
            }

            // Если в своих фрагментах находится враг, то target_dist != 1000.
            if (target_dist != 1000)
            {
                shoot_logic();
            }

            // Увеличение значения таймера.
            timer++;
        }
    }
    // Логика стрельбы. Вызывается когда есть во что стрелять.
    private void shoot_logic()
    {
        // Вращает пушку по направлению к цели.
        Vector2 dir = new Vector2(target.x - transform.position.x, target.y - transform.position.y);
        rotation_z += get_rotation_offset(correct_rotation(Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90));
        rotation_z = correct_rotation(rotation_z);
        gun.rotation = Quaternion.Euler(0.0f, 0.0f, rotation_z);

        // Если пушка смотрит прямо на цель.
        if (target_found)
        {
            if (timer >= reload_time)
            {

                animator.Play("shoot_0");
                GameAuido.instance.Play(2);
                timer = 0;

                var line_kb = get_line_kb(transform.position, target);
                var all_cells = get_all_cells_in_bullet_trajectorie(line_kb);
                var roads_environments_cells = get_road_environment_cells(all_cells);
                var all_enemies_in_cells = get_all_enemies_in_given_cells(roads_environments_cells[0]);
                damage_enemies(line_kb, all_enemies_in_cells);
                damage_environments(line_kb, roads_environments_cells[1]);

                // Создает эффекты пуль и вспышек.
                var trail = Instantiate(Container.instance.objects[5]);
                trail.transform.position = gun.transform.up * 10 + transform.position;
                trail.transform.localScale = new Vector3(1, Vector2.Distance(gun.transform.up * 10 + transform.position, transform.position), 1);
                trail.transform.rotation = Quaternion.Euler(0, 0, rotation_z);
            }
        }
    }

    // Загоняет значение в рамки 0 - 360.
    private float correct_rotation(float rotation)
    {
        rotation %= 360;
        if (rotation < 0) rotation += 360;
        return rotation;
    }

    // Решает в какую сторону и на сколько повернуться пушке.
    private float get_rotation_offset(float target_rot)
    {
        float target_rot2 = (rotation_z >= 180) ? target_rot + 360 : target_rot - 360;
        float additional_rot1 = target_rot - rotation_z;
        float additional_rot2 = target_rot2 - rotation_z;
        if (Mathf.Abs(additional_rot1) < Mathf.Abs(additional_rot2))
        {
            if (Mathf.Abs(additional_rot1) > rotation_speed)
            {
                target_found = false;
                if (additional_rot1 >= 0) return rotation_speed;
                else return -rotation_speed;
            }
            else
            {
                target_found = true;
                return additional_rot1;
            }
        }
        else
        {
            if (Mathf.Abs(additional_rot2) > rotation_speed)
            {
                target_found = false;
                if (additional_rot2 >= 0) return rotation_speed;
                else return -rotation_speed;
            }
            else
            {
                target_found = true;
                return additional_rot2;
            }
        }
    }

    // Возвращает параметры функции прямой по двум точкам.
    private float[] get_line_kb(Vector2 point1, Vector2 point2)
    {
        float[] _return = new float[3];
        _return[0] = (point1.y - point2.y) / (point1.x - point2.x);
        _return[1] = point1.y - _return[0] * point1.x;
        if (point1.x == point2.x)
        {
            _return[2] = 1;
            _return[0] = point2.x;
        }


        return _return;
    }

    // Выдает массив с координатами клеток, в которых пролетит пуля.
    private Vector2Int[] get_all_cells_in_bullet_trajectorie(float[] line_kb)
    {
        var mapSize = MapInfo.instance.get_map_size();

        List<Vector2Int> _return = new List<Vector2Int>();

        if (line_kb[2] != 1)
            if ((line_kb[0] >= 0 && line_kb[0] <= 1) || (line_kb[0] <= 0 && line_kb[0] >= -1))
            {
                bool target_right = target.x > transform.position.x;
                int min = target_right ? (int)transform.position.x + 1 : -mapSize.x / 2;
                int max = target_right ? mapSize.x / 2 + 1 : (int)transform.position.x;

                for (int x = min; x < max; x++)
                {
                    float real_y = line_kb[0] * x + line_kb[1];
                    int y = Mathf.RoundToInt(real_y);
                    int y2 = (real_y - y > 0) ? y + 1 : y - 1;
                    if (y >= -mapSize.y / 2 && y < mapSize.y / 2 + 1)
                    {
                     //   var a = Instantiate(test);
                     //   a.transform.position = new Vector3(x, y);
                        _return.Add(new Vector2Int(x, y));
                    }
                    if (y2 >= -mapSize.y / 2 && y2 < mapSize.y / 2 + 1 && real_y - y != 0)
                    {
                    //    var a = Instantiate(test);
                    //    a.transform.position = new Vector3(x, y2);
                        _return.Add(new Vector2Int(x, y2));

                    }
                }
            }
            else
            {
                bool target_up = target.y > transform.position.y;
                int min = target_up ? (int)transform.position.y + 1 : -mapSize.y / 2;
                int max = target_up ? mapSize.y / 2 + 1 : (int)transform.position.y;

                for (int y = min; y < max; y++)
                {
                    float real_x = (y - line_kb[1]) / line_kb[0];
                    int x = Mathf.RoundToInt(real_x);
                    int x2 = (real_x - x > 0) ? x + 1 : x - 1;
                    if (x >= -mapSize.x / 2 && x < mapSize.x / 2 + 1)
                    {
                      //  var a = Instantiate(test);
                     //   a.transform.position = new Vector3(x, y);
                        _return.Add(new Vector2Int(x, y));
                    }
                    if (x2 >= -mapSize.x / 2 && x2 < mapSize.x / 2 + 1)
                    {
                    //    var a = Instantiate(test);
                     //   a.transform.position = new Vector3(x2, y);
                        _return.Add(new Vector2Int(x2, y));
                    }
                }
            }
        else
        {
            bool target_up = target.y > transform.position.y;
            int min = target_up ? (int)transform.position.y + 1 : -mapSize.y / 2;
            int max = target_up ? mapSize.y / 2 + 1 : (int)transform.position.y;
            int x = (int)line_kb[0];
            for (int y = min; y < max; y++)
            {
              //  var a = Instantiate(test);
             //   a.transform.position = new Vector3(x, y);
                _return.Add(new Vector2Int(x, y));
            }
        }
        return _return.ToArray();
    }

    // Создает массив с разделенными координатами клеток дорог и объектов окружения.
    private Vector2Int[][] get_road_environment_cells(Vector2Int[] all_cells)
    {
        List<Vector2Int> roads = new List<Vector2Int>();
        List<Vector2Int> environments = new List<Vector2Int>();

        foreach (var cell in all_cells)
        {
            int cell_object = MapInfo.instance.get_cell_object(cell);
            if (cell_object == 10) //road
            {
                roads.Add(cell);
            }
            else if (cell_object == 20) // env
            {
                environments.Add(cell);
            }

        }

        return new Vector2Int[2][] { roads.ToArray(), environments.ToArray() };
    }

    // Выдает всех врагов, находящихся в данных клетках.
    private Enemy[] get_all_enemies_in_given_cells(Vector2Int[] road_cells)
    {
        List<Enemy> _return = new List<Enemy>();

        var mapSize = MapInfo.instance.get_map_size();
        var enemies = EnemiesLogic.instance.get_all_enemies();
        if (enemies != null)
            foreach (var enemy in enemies)
            {
                int enemy_cell_x = Mathf.RoundToInt(enemy.transform.position.x);
                int enemy_cell_y = Mathf.RoundToInt(enemy.transform.position.y);

                foreach (var cell in road_cells)
                {
                    if (cell.x == enemy_cell_x && cell.y == enemy_cell_y)
                    {
                        _return.Add(enemy);
                        break;
                    }
                }

                if (enemy_cell_x > mapSize.x / 2 && rotation_z > 180)
                {
                    _return.Add(enemy);
                }
                else if (enemy_cell_x < -mapSize.x / 2 && rotation_z < 180)
                {
                    _return.Add(enemy);
                }

                else if (enemy_cell_y > mapSize.y / 2 && (rotation_z > 270 || rotation_z < 90))
                {
                    _return.Add(enemy);
                }
                else if (enemy_cell_y < -mapSize.y / 2 && rotation_z > 90 && rotation_z < 270)
                {
                    _return.Add(enemy);
                }
            }

        return _return.ToArray();
    }

    private void damage_enemies(float[] line_kb, Enemy[] enemies)
    {
        for (int enemy_index = 0; enemy_index < enemies.Length; enemy_index++)
        {
            Enemy enemy = enemies[enemy_index];

            float r = .15f;

            if (line_kb[2] != 1)
            {

                float b = line_kb[1] - enemy.transform.position.y + line_kb[0] * enemy.transform.position.x;

                float d = 4 * (line_kb[0] * line_kb[0] * r * r + r * r - b * b);

                if (d > 0)
                {
                    float x1 = (-2 * line_kb[0] * b + Mathf.Sqrt(d)) / (2 * (line_kb[0] * line_kb[0] + 1));
                    float y1 = line_kb[0] * x1 + b;

                    float x2 = (-2 * line_kb[0] * b - Mathf.Sqrt(d)) / (2 * (line_kb[0] * line_kb[0] + 1));
                    float y2 = line_kb[0] * x2 + b;

                    var effect_ref = Container.instance.objects[4];

                    var effect1 = Instantiate(effect_ref);
                    effect1.transform.position = new Vector2(x1, y1) + (Vector2)enemy.transform.position;

                    var effect2 = Instantiate(effect_ref);
                    effect2.transform.position = new Vector2(x2, y2) + (Vector2)enemy.transform.position;

                    EnemiesLogic.instance.damage_enemy(enemy, damage);
                }
            }
            else
            {
                float x = transform.position.x - enemy.transform.position.x;

                float u = r * r - x * x;

                if (u > 0)
                {
                    float y1 = Mathf.Sqrt(u);
                    float y2 = -Mathf.Sqrt(u);

                    var effect_ref = Container.instance.objects[4];

                    var effect1 = Instantiate(effect_ref);
                    effect1.transform.position = new Vector2(x, y1) + (Vector2)enemy.transform.position;

                    var effect2 = Instantiate(effect1);
                    effect2.transform.position = new Vector2(x, y2) + (Vector2)enemy.transform.position;

                    EnemiesLogic.instance.damage_enemy(enemy, damage);
                }
            }
        }
    }
    private void damage_environments(float[] line_kb, Vector2Int[] environments)
    {
        for (int environment_index = 0; environment_index < environments.Length; environment_index++)
        {
            var environment = environments[environment_index];

            float r = .35f;

            if (line_kb[2] != 1)
            {

                float b = line_kb[1] - environment.y + line_kb[0] * environment.x;

                float d = 4 * (line_kb[0] * line_kb[0] * r * r + r * r - b * b);

                if (d > 0)
                {
                    float x1 = (-2 * line_kb[0] * b + Mathf.Sqrt(d)) / (2 * (line_kb[0] * line_kb[0] + 1));
                    float y1 = line_kb[0] * x1 + b;

                    float x2 = (-2 * line_kb[0] * b - Mathf.Sqrt(d)) / (2 * (line_kb[0] * line_kb[0] + 1));
                    float y2 = line_kb[0] * x2 + b;

                    var effect_ref = Container.instance.objects[4];

                    var effect1 = Instantiate(effect_ref);
                    effect1.transform.position = new Vector2(x1, y1) + environment;

                    var effect2 = Instantiate(effect_ref);
                    effect2.transform.position = new Vector2(x2, y2) + environment;

                    MapInfo.instance.damage_environment(environment, damage);
                }
            }
            else
            {
                float x = transform.position.x - environment.x;

                float u = r * r - x * x;

                if (u > 0)
                {
                    float y1 = Mathf.Sqrt(u);
                    float y2 = -Mathf.Sqrt(u);

                    var effect_ref = Container.instance.objects[4];

                    var effect1 = Instantiate(effect_ref);
                    effect1.transform.position = new Vector2(x, y1) + environment;

                    var effect2 = Instantiate(effect_ref);
                    effect2.transform.position = new Vector2(x, y2) + environment;

                    MapInfo.instance.damage_environment(environment, damage);
                }
            }
        }
    }
}
