using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;

// EnemiesLogic выполняет логику связанную с врагами в игре.
public class EnemiesLogic : MonoBehaviour
{

    [SerializeField] private string way_file = "";

    // Хранит всех врагов. Сортируются по фрагментам пути, на котором находятся. enemies[путь][фрагмент][враг].
    public Enemy[][][] enemies;

    // Число врагов в каждом фрагменте. enemies_in_fragments_count[путь][фрагмент].
    private int[][] enemies_in_fragments_count;

    // Расстояеие на котором точка начала фрагмента находится от финиша.
    private float[][] enemies_arrays_steps;

    // Число врагов.
    private int enemies_count;

    // Ссылка на класс.
    public static EnemiesLogic instance;

    private const float pi_div_two = 1.570796326794897f;

    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        if (way_file == "") way_file = "ways_" + GameLogic.instance.current_level;

        // Загружает заготовленные пути.
        Addressables.LoadAssetAsync<TextAsset>(way_file).Completed += handle =>
        {
            // Загружает и сохраняет пути.
            var _save = handle.Result;
            level_ways = JsonUtility.FromJson<Ways>(_save.text);

            // Далее вычисляет и сохраняет индекс последней точки каждого пути.
            ways_ends = new int[level_ways.ways_length.Length];
            int current_way_end = 0;
            for (int i = 0; i < level_ways.ways_length.Length; i++)
            {
                current_way_end += level_ways.ways_length[i];
                ways_ends[i] = current_way_end;
            }

            // Далее вычисляет и сохраняет длины всех путей.
            find_distance_to_finish();

            // Делит путь на фрагменты.
            points_sorted_by_way_fragments = new Vector2[level_ways.ways_length.Length][][];
            int way_start = 0;
            for (int way = 0; way < level_ways.ways_length.Length; way++)
            {
                points_sorted_by_way_fragments[way] = new Vector2[ways_ends[way] - way_start][];
                for (int fragment = 0; fragment < points_sorted_by_way_fragments[way].Length; fragment++)
                {
                    points_sorted_by_way_fragments[way][fragment] = new Vector2[0];

                }
                way_start = ways_ends[way];
            }
            Container.instance.loaded();
            if (MapInfo.instance.towers_count == 0) calculate_new_enemy_arrays();
        };
    }
    private void FixedUpdate()
    {
        if (Container.instance.game_loaded)
        {
            enemies_movement();
        }
    }

    // Логика взаимодействия врагов с другими игровыми объектами.
    #region Enemies interaction

    // Спавн нового врага.
    public void spawn_enemy(Enemy enemy)
    {
        int index = enemies_in_fragments_count[enemy.enemy_way][0];
        enemies[enemy.enemy_way][0][index] = enemy;
        enemy.current_way_point = (enemy.enemy_way == 0) ? 0 : ways_ends[enemy.enemy_way - 1];
        enemy.transform.position = level_ways.way_points[enemy.current_way_point];
        enemy.distance_to_finish = ways_distance[enemy.enemy_way];
        set_start_rotation(enemy);
        enemies_in_fragments_count[enemy.enemy_way][0]++;
        enemies_count++;
    }

    // Нанесение урона врагу.
    public void damage_enemy(Enemy enemy, int damage)
    {
        enemy.health -= damage;
        if (enemy.health_bar == null)
        {
            var health_bar = Instantiate(Container.instance.objects[8]);
            health_bar.transform.position = enemy.transform.position;
            enemy.health_bar = health_bar.transform;
        }
        enemy.health_bar.GetChild(0).localScale = new Vector3((float)enemy.health / Container.instance.groups_healths[enemy.group], 1);

        if (enemy.health <= 0)
        {
            Container.instance.spawn_enemy_die_effect(enemy.group, enemy.transform.position, enemy.transform.eulerAngles.z);
            remove_enemy(enemy);
        }
    }

    // Когда игрок спавнет башню или радиус действия башени меняется, то пути нужно снова делить на фрагменты.
    // Переменная "add_or_remove" определяет добавлять фрагменты дороги, на которые реагирет введенная башня, или нет.
    // Переменная "calculate_enemy_arrays" определяет надо ли именно сейчас распределить всех врагов по фрагментам и указать нужные фрагменты башням.
    public void create_new_fragments(Tower tower, bool add_or_remove, bool calculate_enemy_arrays)
    {
        if (add_or_remove)
        {
            add_new_tower_fragments(tower);
        }
        else
        {
            remove_tower_fragments(tower);
        }

        if (calculate_enemy_arrays)
        {
            calculate_new_enemy_arrays();
        }
    }

    // Возвращает один массив со всеми врагами.
    public Enemy[] get_all_enemies()
    {
        if (enemies_count != 0)
        {
            var _return = new Enemy[enemies_count];
            int enemy_index = 0;
            for (int way = 0; way < enemies.Length; way++)
            {
                for (int fragment = enemies[way].Length - 1; fragment >= 0; fragment--)
                {
                    foreach (var enemy in enemies[way][fragment])
                    {
                        if (enemy == null) break;
                        _return[enemy_index] = enemy;
                        enemy_index++;
                    }
                }
            }
            return _return;
        }
        else return null;
    }

    #endregion

    // Логика движения и вращения врага.
    #region Movement and rotation

    // Cкорость врага.
    // private float speed = .02f;

    // Показывает в какую сторону перемещался враг перед тем как пошел на поворот. 0 это вверх, 1 это вправо, 2 это вниз, 3 это влево.
    private int direction_before_turn = 0;

    private float enemy_line_movement(Enemy enemy, float speed, bool vertical)
    {
        float _return = 0;
        if (vertical)
        {
            if (level_ways.way_points[enemy.current_way_point + 1].y > enemy.transform.position.y)
            {
                enemy.transform.position += new Vector3(0, speed);
                if (enemy.transform.position.y >= level_ways.way_points[enemy.current_way_point + 1].y)
                {
                    direction_before_turn = 0;
                    _return = enemy.transform.position.y - level_ways.way_points[enemy.current_way_point + 1].y;
                    enemy.transform.position = level_ways.way_points[enemy.current_way_point + 1];
                    enemy.current_way_point++;
                }
            }
            else
            {
                enemy.transform.position += new Vector3(0, -speed);
                if (enemy.transform.position.y <= level_ways.way_points[enemy.current_way_point + 1].y)
                {
                    direction_before_turn = 2;
                    _return = level_ways.way_points[enemy.current_way_point + 1].y - enemy.transform.position.y;
                    enemy.transform.position = level_ways.way_points[enemy.current_way_point + 1];
                    enemy.current_way_point++;
                }
            }
        }
        else 
        {
            if (level_ways.way_points[enemy.current_way_point + 1].x >= enemy.transform.position.x)
            {
                enemy.transform.position += new Vector3(speed, 0);
                if (enemy.transform.position.x >= level_ways.way_points[enemy.current_way_point + 1].x)
                {
                    direction_before_turn = 1;
                    _return = enemy.transform.position.x - level_ways.way_points[enemy.current_way_point + 1].x;
                    enemy.transform.position = level_ways.way_points[enemy.current_way_point + 1];
                    enemy.current_way_point++;
                }
            }
            else
            {
                enemy.transform.position += new Vector3(-speed, 0);
                if (enemy.transform.position.x <= level_ways.way_points[enemy.current_way_point + 1].x)
                {
                    direction_before_turn = 3;
                    _return = level_ways.way_points[enemy.current_way_point + 1].x - enemy.transform.position.x;
                    enemy.transform.position = level_ways.way_points[enemy.current_way_point + 1];
                    enemy.current_way_point++;
                }
            }
        }
        return _return;
    }

    private float enemy_turn_movement(Enemy enemy, float speed)
    {
        if (enemy.turn_value == 0)
        {
            enemy.circle_centre = get_circle_centre(direction_before_turn, level_ways.way_points[enemy.current_way_point + 1]);
        }

        float _return = 0;
        enemy.turn_value += speed * 2;

        if (level_ways.way_points[enemy.current_way_point + 1].x > level_ways.way_points[enemy.current_way_point].x)
        {
            if (level_ways.way_points[enemy.current_way_point + 1].y > level_ways.way_points[enemy.current_way_point].y)
            {
                if (enemy.circle_centre.y == level_ways.way_points[enemy.current_way_point].y)
                {
                    enemy.transform.position = enemy.circle_centre + new Vector2(-(float)System.Math.Cos(enemy.turn_value) / 2, (float)System.Math.Sin(enemy.turn_value) / 2);
                    enemy.transform.eulerAngles = new Vector3(0, 0, -enemy.turn_value * 57.296f);
                    if (enemy.turn_value >= pi_div_two)
                    {
                        enemy.transform.eulerAngles = new Vector3(0, 0, -90);
                        enemy.transform.position = level_ways.way_points[enemy.current_way_point + 1];
                        enemy.current_way_point++;
                        _return = (enemy.turn_value - pi_div_two) / 2;
                        direction_before_turn = 1;
                        enemy.turn_value = 0;
                    }
                }
                else
                {
                    enemy.transform.position = enemy.circle_centre + new Vector2((float)System.Math.Sin(enemy.turn_value) / 2, -(float)System.Math.Cos(enemy.turn_value) / 2);
                    enemy.transform.eulerAngles = new Vector3(0, 0, +enemy.turn_value * 57.296f - 90);
                    if (enemy.turn_value >= pi_div_two)
                    {
                        enemy.transform.eulerAngles = new Vector3(0, 0, 0);
                        enemy.transform.position = level_ways.way_points[enemy.current_way_point + 1];
                        enemy.current_way_point++;
                        _return = (enemy.turn_value - pi_div_two) / 2;
                        direction_before_turn = 0;
                        enemy.turn_value = 0;
                    }
                }
            }
            else
            {
                if (enemy.circle_centre.y == level_ways.way_points[enemy.current_way_point].y)
                {
                    enemy.transform.position = enemy.circle_centre + new Vector2(-(float)System.Math.Cos(enemy.turn_value) / 2, -(float)System.Math.Sin(enemy.turn_value) / 2);
                    enemy.transform.eulerAngles = new Vector3(0, 0, +enemy.turn_value * 57.296f - 180);
                    if (enemy.turn_value >= pi_div_two)
                    {
                        enemy.transform.eulerAngles = new Vector3(0, 0, -90);
                        enemy.transform.position = level_ways.way_points[enemy.current_way_point + 1];
                        enemy.current_way_point++;
                        _return = (enemy.turn_value - pi_div_two) / 2;
                        direction_before_turn = 1;
                        enemy.turn_value = 0;
                    }
                }
                else
                {
                    enemy.transform.position = enemy.circle_centre + new Vector2((float)System.Math.Sin(enemy.turn_value) / 2, (float)System.Math.Cos(enemy.turn_value) / 2);
                    enemy.transform.eulerAngles = new Vector3(0, 0, -enemy.turn_value * 57.296f - 90);
                    if (enemy.turn_value >= pi_div_two)
                    {
                        enemy.transform.eulerAngles = new Vector3(0, 0, 180);
                        enemy.transform.position = level_ways.way_points[enemy.current_way_point + 1];
                        enemy.current_way_point++;
                        _return = (enemy.turn_value - pi_div_two) / 2;
                        direction_before_turn = 2;
                        enemy.turn_value = 0;
                    }
                }
            }
        }
        else
        {
            if (level_ways.way_points[enemy.current_way_point + 1].y > level_ways.way_points[enemy.current_way_point].y)
            {
                if (enemy.circle_centre.y == level_ways.way_points[enemy.current_way_point].y)
                {
                    enemy.transform.position = enemy.circle_centre + new Vector2((float)System.Math.Cos(enemy.turn_value) / 2, (float)System.Math.Sin(enemy.turn_value) / 2);
                    enemy.transform.eulerAngles = new Vector3(0, 0, +enemy.turn_value * 57.296f);
                    if (enemy.turn_value >= pi_div_two)
                    {
                        enemy.transform.eulerAngles = new Vector3(0, 0, 90);
                        enemy.transform.position = level_ways.way_points[enemy.current_way_point + 1];
                        enemy.current_way_point++;
                        _return  = (enemy.turn_value - pi_div_two) / 2;
                        direction_before_turn = 3;
                        enemy.turn_value = 0;
                    }
                }
                else
                {
                    enemy.transform.position = enemy.circle_centre + new Vector2(-(float)System.Math.Sin(enemy.turn_value) / 2, -(float)System.Math.Cos(enemy.turn_value) / 2);
                    enemy.transform.eulerAngles = new Vector3(0, 0, -enemy.turn_value * 57.296f + 90);
                    if (enemy.turn_value >= pi_div_two)
                    {
                        enemy.transform.eulerAngles = new Vector3(0, 0, 0);
                        enemy.transform.position = level_ways.way_points[enemy.current_way_point + 1];
                        enemy.current_way_point++;
                        _return = (enemy.turn_value - pi_div_two) / 2;
                        direction_before_turn = 0;
                        enemy.turn_value = 0;
                    }
                }
            }
            else
            {
                if (enemy.circle_centre.y == level_ways.way_points[enemy.current_way_point].y)
                {
                    enemy.transform.position = enemy.circle_centre + new Vector2((float)System.Math.Cos(enemy.turn_value) / 2, -(float)System.Math.Sin(enemy.turn_value) / 2);
                    enemy.transform.eulerAngles = new Vector3(0, 0, -enemy.turn_value * 57.296f + 180);
                    if (enemy.turn_value >= pi_div_two)
                    {
                        enemy.transform.eulerAngles = new Vector3(0, 0, 90);
                        enemy.transform.position = level_ways.way_points[enemy.current_way_point + 1];
                        enemy.current_way_point++;
                        _return = (enemy.turn_value - pi_div_two) / 2;
                        direction_before_turn = 3;
                        enemy.turn_value = 0;
                    }
                }
                else
                {
                    enemy.transform.position = enemy.circle_centre + new Vector2(-(float)System.Math.Sin(enemy.turn_value) / 2, (float)System.Math.Cos(enemy.turn_value) / 2);
                    enemy.transform.eulerAngles = new Vector3(0, 0, enemy.turn_value * 57.296f + 90);
                    if (enemy.turn_value >= pi_div_two)
                    {
                        enemy.transform.eulerAngles = new Vector3(0, 0, 180);
                        enemy.transform.position = level_ways.way_points[enemy.current_way_point + 1];
                        enemy.current_way_point++;
                        _return  = (enemy.turn_value - pi_div_two) / 2;
                        direction_before_turn = 2;
                        enemy.turn_value = 0;
                    }
                }
            }
        }
        return _return;
    }

    // Функция выполняющая движение врага.
    private void moveEnemy(Enemy enemy)
    {
        float speed = Container.instance.groups_speeds[enemy.group] * enemy.speed_multiplier;
        enemy.speed_multiplier = 1;


        bool sameX = level_ways.way_points[enemy.current_way_point + 1].x == enemy.transform.position.x;
        bool sameY = level_ways.way_points[enemy.current_way_point + 1].y == enemy.transform.position.y;
        if (enemy.turn_value != 0)
        {
            sameX = false; sameY = false;
        }
        bool vertical_movement = sameX && !sameY;
        bool horizontal_movement = !sameX && sameY;

        enemy.distance_to_finish -= speed;

        if (vertical_movement || horizontal_movement)
        {
            float supplement_to_turn = enemy_line_movement(enemy, speed, vertical_movement);
            if (supplement_to_turn != 0 && enemy.current_way_point + 1 < ways_ends[enemy.enemy_way])
            {
                enemy_turn_movement(enemy, supplement_to_turn);
            }
        }
        else
        {
            float supplement_to_turn = enemy_turn_movement(enemy, speed);
            if (supplement_to_turn != 0)
            {
                sameX = level_ways.way_points[enemy.current_way_point + 1].x == enemy.transform.position.x;
                sameY = level_ways.way_points[enemy.current_way_point + 1].y == enemy.transform.position.y;
                if (enemy.turn_value != 0)
                {
                    sameX = false; sameY = false;
                }
                vertical_movement = sameX && !sameY;
                horizontal_movement = !sameX && sameY;

                if (vertical_movement || horizontal_movement) enemy_line_movement(enemy, supplement_to_turn, vertical_movement);
                else
                {
                    enemy_turn_movement(enemy, supplement_to_turn);
                }
            }
        }

        if (enemy.health_bar) enemy.health_bar.position = enemy.transform.position;
    }

    // Выдает центр круга при заходе в поворот.
    private Vector2 get_circle_centre(int direction_before_turn, Vector2 point)
    {
        if (direction_before_turn == 0) // Eсли враг до поворота двигался вверх.
        {
            return point + new Vector2(0, -.5f); // Выстовляем центр окружности опираясь на цель движения врага.
        }
        else if (direction_before_turn == 1) // Eсли враг до поворота двигался вправо.
        {
            return point + new Vector2(-.5f, 0); // Выстовляем центр окружности опираясь на цель движения врага.
        }
        else if (direction_before_turn == 2) // Eсли враг до поворота двигался вниз.
        {
            return point + new Vector2(0, .5f); // Выстовляем центр окружности опираясь на цель движения врага.
        }
        else // Eсли враг до поворота двигался влево.
        {
            return point + new Vector2(.5f, 0); // Выстовляем центр окружности опираясь на цель движения врага.
        }
    }

    // Устанавливает правильное вращение врага на старте.
    private void set_start_rotation(Enemy enemy)
    {
        if (level_ways.way_points[enemy.current_way_point].x == level_ways.way_points[enemy.current_way_point + 1].x)
        {
            if (level_ways.way_points[enemy.current_way_point].y > level_ways.way_points[enemy.current_way_point + 1].y)
            {
                enemy.transform.eulerAngles = new Vector3(0, 0, 180);
            }
            else
            {
                enemy.transform.eulerAngles = new Vector3(0, 0, 0);
            }
        }
        else
        {
            if (level_ways.way_points[enemy.current_way_point].x > level_ways.way_points[enemy.current_way_point + 1].x)
            {
                enemy.transform.eulerAngles = new Vector3(0, 0, 90);
            }
            else
            {
                enemy.transform.eulerAngles = new Vector3(0, 0, -90);
            }
        }
    }
    #endregion

    // Движение и сортировка врагов в массивах.
    #region Enemies movement and sorting

    // Выполняет логику движения для каждого врага и сортирует их.
    private void enemies_movement()
    {
        for (int way = 0; way < enemies.Length; way++)
        {
            for (int fragment = enemies[way].Length - 1; fragment >= 0; fragment--)
            {
                float distance_to_finish_last = 0;
                for (int enemy = 0; enemy < enemies_in_fragments_count[way][fragment]; enemy++)
                {
                    var cur_enemy = enemies[way][fragment][enemy];
                    if (cur_enemy.current_way_point + 1 < ways_ends[enemies[way][fragment][enemy].enemy_way]) ///////////////////////////////////////////////////////////////////////////////////////////
                    {

                        moveEnemy(cur_enemy);
                        float dist_to_finish = cur_enemy.distance_to_finish;

                        if (portal_active)
                        {
                            if (dist_to_finish > portal_zones[cur_enemy.enemy_way][0] && dist_to_finish < portal_zones[cur_enemy.enemy_way][1]) ///////////////////////////////////////////////////////////////////////////////////////////
                            {
                                int start_point_index = (way == 0) ? 0 : ways_ends[way - 1];

                                cur_enemy.transform.position = level_ways.way_points[start_point_index];
                                move_to_fragment(way, fragment, 0, enemy);

                                cur_enemy.current_way_point = start_point_index;
                                cur_enemy.distance_to_finish = ways_distance[way];
                                cur_enemy.turn_value = 0;
                                set_start_rotation(cur_enemy);

                                var trials = cur_enemy.transform.GetComponentsInChildren<TrailRenderer>();
                                foreach (var trail in trials)
                                {
                                    trail.Clear();
                                }
                            }
                        }

                        if (distance_to_finish_last != 0)
                        {
                            if (dist_to_finish < distance_to_finish_last)
                            {
                                swap_places(way, fragment, enemy, enemy - 1);
                            }
                        }
                        if (dist_to_finish < enemies_arrays_steps[way][fragment] && dist_to_finish > 0)
                        {
                            move_to_fragment(way, fragment, fragment + 1, enemy);
                            enemy--;
                        }
                        else distance_to_finish_last = dist_to_finish;
                    }
                    else
                    {
                        GameLogic.instance.damage_base();
                        remove_enemy(cur_enemy); enemy--;
                    }
                }
            }
        }
    }

    // Выдает индексы ячеек в которых есть данный враг.
    private int[][] get_enemy_fragments(Enemy enemy)
    {
        List<int[]> fragments = new List<int[]>();
        for (int way = 0; way < enemies.Length; way++)
        {
            bool found = false;
            for (int fragment = enemies[way].Length - 1; fragment >= 0; fragment--)
            {
                for (int enemy_index = 0; enemy_index < enemies_in_fragments_count[way][fragment]; enemy_index++)
                {
                    if (enemies[way][fragment][enemy_index] == enemy)
                    {
                        fragments.Add(new int[] { way, fragment, enemy_index });
                        found = true;
                        break;
                    }
                }
                if (found) break;
            }
        }
        return fragments.ToArray();
    }

    // Удаление врага.
    private void remove_enemy(Enemy enemy)
    {
        var fragments = get_enemy_fragments(enemy);

        if (enemy.health_bar != null) Destroy(enemy.health_bar.gameObject);
        Destroy(enemies[fragments[0][0]][fragments[0][1]][fragments[0][2]].transform.gameObject);
        enemies_count--;

        for (int way_index = 0; way_index < fragments.Length; way_index++)
        {
            int way = fragments[way_index][0];
            int fragment = fragments[way_index][1];
            int enemy_index = fragments[way_index][2];
            enemies[way][fragment][enemy_index] = null;
            enemies_in_fragments_count[way][fragment]--;

            for (int i = enemy_index; i < enemies_in_fragments_count[way][fragment] + 1; i++)
            {
                enemies[way][fragment][i] = enemies[way][fragment][i + 1];
            }
            enemies[way][fragment][enemies_in_fragments_count[way][fragment] + 1] = null;
        }
        WavesLogic.instance.enemy_destroyed();
    }

    // Перемещает врага в следующий фрагмент.
    private void move_to_fragment(int way, int fragment1, int fragment2, int enemy_index)
    {
        enemies[way][fragment2][enemies_in_fragments_count[way][fragment2]] = enemies[way][fragment1][enemy_index];
        enemies[way][fragment1][enemy_index] = null;
        enemies_in_fragments_count[way][fragment1]--;
        enemies_in_fragments_count[way][fragment2]++;

        // Смещение врагов в массиве идущих после убранного.
        for (int i = enemy_index; i < enemies_in_fragments_count[way][fragment1] + 1; i++)
        {
            enemies[way][fragment1][i] = enemies[way][fragment1][i + 1];
        }
        enemies[way][fragment1][enemies_in_fragments_count[way][fragment1] + 1] = null;
    }

    // Меняет местами врагов.
    private void swap_places(int way, int fragment, int index_1, int index_2)
    {
        var enemie_1 = enemies[way][fragment][index_1];
        enemies[way][fragment][index_1] = enemies[way][fragment][index_2];
        enemies[way][fragment][index_2] = enemie_1;
    }
    #endregion

    // Логика, связанная с путями врагов.
    #region Ways Loigic

    // Хранит информацию о путях.
    private Ways level_ways;

    // Длины каждого из путей.
    private float[] ways_distance;

    // Концечные точки каждого из путей.
    private int[] ways_ends;

    // Класс, описвыающий пути.
    private class Ways
    {
        // Хранит все точки передвижения последовательно.
        public Vector2[] way_points;

        // Хранит число точек передвижения из которых состоит путь.
        public int[] ways_length;
    }

    // Находит длину каждого пути от начала до финиша. 
    private void find_distance_to_finish()
    {
        // Пробегается по всем точкам и складывает расстояния между ними.
        // В поворотах прибавляет длину дугового движения.
        ways_distance = new float[level_ways.ways_length.Length];
        int start_point = 0;
        for (int way = 0; way < level_ways.ways_length.Length; way++)
        {
            float way_distance = 0;
            for (int point = start_point; point < ways_ends[way] - 1; point++)
            {
                var cur_point = level_ways.way_points[point];
                var next_point = level_ways.way_points[point + 1];
                if (cur_point.x != next_point.x && next_point.y != cur_point.y)
                {
                    way_distance += .785f;
                }
                else
                {
                    way_distance += Vector2.Distance(cur_point, next_point);
                }
            }
            ways_distance[way] = way_distance;
            start_point = ways_ends[way];
        }
    }
    #endregion

    #region Path Sepurator

    // Все пресeчения пути с окружностью башни, сортировынные по фрагментам пути.
    private Vector2[][][] points_sorted_by_way_fragments;

    private void add_new_tower_fragments(Tower tower)
    {
        int way_start = 0;
        float range = (tower.type != 3 && tower.type != 4) ? tower.range * tower.range_multiplier : tower.range;
        tower.points = new Vector2[level_ways.ways_length.Length][];
        for (int way = 0; way < level_ways.ways_length.Length; way++)
        {
            bool include_start_point = Vector2.Distance(level_ways.way_points[way_start], tower.transform.position) < range;
            bool incude_end_point = Vector2.Distance(level_ways.way_points[ways_ends[way] - 1], tower.transform.position) < range;
            tower.points[way] = include_start_point ? new Vector2[] { level_ways.way_points[way_start] } : new Vector2[0];
            if (include_start_point) points_sorted_by_way_fragments[way][0] = points_sorted_by_way_fragments[way][0].Concat(tower.points[way]).ToArray();
            if (incude_end_point) points_sorted_by_way_fragments[way][ways_ends[way] - 1 - way_start] = points_sorted_by_way_fragments[way][ways_ends[way] - 1 - way_start].Concat(new Vector2[] { level_ways.way_points[ways_ends[way] - 1] }).ToArray();
            int way_direction_before_turn = 0;
            for (int point = way_start + 1; point < ways_ends[way]; point++)
            {
                var current_fragment_points = new Vector2[0];
                var point1 = level_ways.way_points[point - 1];
                var point2 = level_ways.way_points[point];
                if (point1.x == point2.x)
                {
                    if (point1.y < point2.y) way_direction_before_turn = 0;
                    else way_direction_before_turn = 2;

                    Vector2[] points = get_intersections_of_circle_and_line(range, tower.transform.position, false, point1.x, point1.y, point2.y);
                    if (points != null)
                    {
                        points_sorted_by_way_fragments[way][point - 1 - way_start] = points_sorted_by_way_fragments[way][point - 1 - way_start].Concat(points).ToArray();
                        current_fragment_points = current_fragment_points.Concat(points).ToArray();
                    }
                }
                else if (point1.y == point2.y)
                {
                    if (point1.x < point2.x) way_direction_before_turn = 1;
                    else way_direction_before_turn = 3;

                    Vector2[] points = get_intersections_of_circle_and_line(range, tower.transform.position, true, point1.y, point1.x, point2.x);
                    if (points != null)
                    {
                        points_sorted_by_way_fragments[way][point - 1 - way_start] = points_sorted_by_way_fragments[way][point - 1 - way_start].Concat(points).ToArray();
                        current_fragment_points = current_fragment_points.Concat(points).ToArray();
                    }
                }
                else
                {
                    // 0 это вверх, 1 это вправо, 2 это вниз, 3 это влево.
                    var circle_centre = get_circle_centre(way_direction_before_turn, point2);
                    bool up = false;
                    bool right = false;
                    if (way_direction_before_turn == 0 || (way_direction_before_turn == 1 && point1.y > point2.y) || (way_direction_before_turn == 3 && point1.y > point2.y)) up = true;
                    if (way_direction_before_turn == 1 || (way_direction_before_turn == 0 && point1.x > point2.x) || (way_direction_before_turn == 2 && point1.x > point2.x)) right = true;

                    if (up && right)
                    {
                        if (point1.y < point2.y) way_direction_before_turn = 3;
                        else way_direction_before_turn = 2;
                    }
                    else if (up && !right)
                    {
                        if (point1.y < point2.y) way_direction_before_turn = 1;
                        else way_direction_before_turn = 2;
                    }
                    else if (!up && right)
                    {
                        if (point1.y < point2.y) way_direction_before_turn = 0;
                        else way_direction_before_turn = 3;
                    }
                    else if (!up && !right)
                    {
                        if (point1.y < point2.y) way_direction_before_turn = 0;
                        else way_direction_before_turn = 1;
                    }

                    Vector2[] points = get_intersections_of_two_circles(circle_centre, .5f, tower.transform.position, range, up, right);
                    if (points != null)
                    {
                        points_sorted_by_way_fragments[way][point - 1 - way_start] = points_sorted_by_way_fragments[way][point - 1 - way_start].Concat(points).ToArray();
                        current_fragment_points = current_fragment_points.Concat(points).ToArray();
                    }
                }
                points_sorted_by_way_fragments[way][point - 1 - way_start] = sort_points(points_sorted_by_way_fragments[way][point - 1 - way_start], point2);
                current_fragment_points = sort_points(current_fragment_points, point2);
                tower.points[way] = tower.points[way].Concat(current_fragment_points).ToArray();
            }
            if (incude_end_point) tower.points[way] = tower.points[way].Concat(new Vector2[] { level_ways.way_points[ways_ends[way] - 1] }).ToArray();
            way_start = ways_ends[way];
        }
        tower.enemies_array_range = new int[level_ways.ways_length.Length][];

    }

    public void visualize_range(Tower tower)
    {
        int way_start = 0;
        float range = (tower.type != 4 && tower.type != 5) ? tower.range * tower.range_multiplier : tower.range;
        for (int way = 0; way < level_ways.ways_length.Length; way++)
        {
            int way_direction_before_turn = 0;
            for (int point = way_start + 1; point < ways_ends[way]; point++)
            {
                var point1 = level_ways.way_points[point - 1];
                var point2 = level_ways.way_points[point];
                if (point1.x == point2.x)
                {
                    if (point1.y < point2.y) way_direction_before_turn = 0;
                    else way_direction_before_turn = 2;

                    get_intersections_of_circle_and_line(range, tower.transform.position, false, point1.x, point1.y, point2.y);
                }
                else if (point1.y == point2.y)
                {
                    if (point1.x < point2.x) way_direction_before_turn = 1;
                    else way_direction_before_turn = 3;

                    get_intersections_of_circle_and_line(range, tower.transform.position, true, point1.y, point1.x, point2.x);
                }
                else
                {
                    // 0 это вверх, 1 это вправо, 2 это вниз, 3 это влево.
                    var circle_centre = get_circle_centre(way_direction_before_turn, point2);
                    bool up = false;
                    bool right = false;
                    if (way_direction_before_turn == 0 || (way_direction_before_turn == 1 && point1.y > point2.y) || (way_direction_before_turn == 3 && point1.y > point2.y)) up = true;
                    if (way_direction_before_turn == 1 || (way_direction_before_turn == 0 && point1.x > point2.x) || (way_direction_before_turn == 2 && point1.x > point2.x)) right = true;

                    if (up && right)
                    {
                        if (point1.y < point2.y) way_direction_before_turn = 3;
                        else way_direction_before_turn = 2;
                    }
                    else if (up && !right)
                    {
                        if (point1.y < point2.y) way_direction_before_turn = 1;
                        else way_direction_before_turn = 2;
                    }
                    else if (!up && right)
                    {
                        if (point1.y < point2.y) way_direction_before_turn = 0;
                        else way_direction_before_turn = 3;
                    }
                    else if (!up && !right)
                    {
                        if (point1.y < point2.y) way_direction_before_turn = 0;
                        else way_direction_before_turn = 1;
                    }

                    get_intersections_of_two_circles(circle_centre, .5f, tower.transform.position, range, up, right);
                }
            }
            way_start = ways_ends[way];
        }
    }

    private void calculate_new_enemy_arrays()
    {
        var points_count_in_way = new int[level_ways.ways_length.Length];

        int way_start = 0;
        for (int way = 0; way < level_ways.ways_length.Length; way++)
        {
            for (int point = way_start + 1; point < ways_ends[way]; point++)
            {
                points_count_in_way[way] += points_sorted_by_way_fragments[way][point - 1 - way_start].Length;
            }
            way_start = ways_ends[way];
        }


        Vector2[][][] points_sorted_by_way_fragments_no_duplicates = new Vector2[points_sorted_by_way_fragments.Length][][];
        for (int way = 0; way < points_sorted_by_way_fragments.Length; way++)
        {
            points_sorted_by_way_fragments_no_duplicates[way] = new Vector2[points_sorted_by_way_fragments[way].Length][];
            for (int fragment = 0; fragment < points_sorted_by_way_fragments_no_duplicates[way].Length; fragment++)
            {
                points_sorted_by_way_fragments_no_duplicates[way][fragment] = remove_duplicate_points((Vector2[])points_sorted_by_way_fragments[way][fragment].Clone());
            }
        }

        Vector2[][] all_way_points = new Vector2[level_ways.ways_length.Length][];
        for (int way = 0; way < level_ways.ways_length.Length; way++)
        {
            all_way_points[way] = new Vector2[0];
            for (int fragment = 0; fragment < points_sorted_by_way_fragments_no_duplicates[way].Length; fragment++)
            {
                all_way_points[way] = all_way_points[way].Concat(points_sorted_by_way_fragments_no_duplicates[way][fragment]).ToArray();
            }
        }

        for (int i = 0; i < MapInfo.instance.towers_count; i++)
        {
            var _tower = MapInfo.instance.towers[i];
            for (int way = 0; way < level_ways.ways_length.Length; way++)
            {
                print(_tower);
                _tower.enemies_array_range[way] = new int[0];
                if (_tower.points[way].Length != 0)
                {
                    for (int tower_point = 0; tower_point < _tower.points[way].Length / 2; tower_point++)
                    {
                        int index1 = System.Array.IndexOf(all_way_points[way], _tower.points[way][tower_point * 2]);
                        int index2 = System.Array.IndexOf(all_way_points[way], _tower.points[way][tower_point * 2 + 1]);
                        for (int fragment = index1 + 1; fragment <= index2; fragment++)
                        {
                            if (fragment < all_way_points[way].Length) _tower.enemies_array_range[way] = _tower.enemies_array_range[way].Concat(new int[] { fragment }).ToArray();
                        }
                    }
                }
            }
        }

        enemies_arrays_steps = new float[level_ways.ways_length.Length][];
        way_start = 0;

        for (int way = 0; way < level_ways.ways_length.Length; way++)
        {
            enemies_arrays_steps[way] = new float[points_count_in_way[way] + 1];
            float current_distance = 0;
            int array_steps_index = 0;
            for (int fragment = way_start; fragment < ways_ends[way] - 1; fragment++)
            {
                var point_pos1 = level_ways.way_points[fragment];
                var point_pos2 = level_ways.way_points[fragment + 1];
                bool moving_on_line = point_pos1.x == point_pos2.x || point_pos1.y == point_pos2.y;
                if (points_sorted_by_way_fragments_no_duplicates[way][fragment - way_start] != null)
                {
                    if (moving_on_line)
                        for (int point = 0; point < points_sorted_by_way_fragments_no_duplicates[way][fragment - way_start].Length; point++)
                        {
                            var distance_in_this_point = Vector2.Distance(points_sorted_by_way_fragments_no_duplicates[way][fragment - way_start][point], point_pos1) + current_distance;
                            enemies_arrays_steps[way][array_steps_index] = ways_distance[way] - distance_in_this_point;
                            array_steps_index++;
                        }
                    else
                    {
                        for (int point = 0; point < points_sorted_by_way_fragments_no_duplicates[way][fragment - way_start].Length; point++)
                        {
                            var distance_in_this_point = get_point_turn_distance(.5f, Vector2.Distance(point_pos1, points_sorted_by_way_fragments_no_duplicates[way][fragment - way_start][point])) + current_distance;
                            enemies_arrays_steps[way][array_steps_index] = ways_distance[way] - distance_in_this_point;
                            array_steps_index++;
                        }
                    }

                }
                if (moving_on_line) current_distance += Vector2.Distance(point_pos1, point_pos2);
                else current_distance += pi_div_two / 2;
            }
            way_start = ways_ends[way];
        }


        var all_enemies = get_all_enemies();

        // Создает ячейку в массиве с врагами для каждого фрагмента пути.
        enemies = new Enemy[level_ways.ways_length.Length][][];
        enemies_in_fragments_count = new int[enemies.Length][];
        for (int way = 0; way < enemies.Length; way++)
        {
            enemies[way] = new Enemy[enemies_arrays_steps[way].Length][];
            enemies_in_fragments_count[way] = new int[enemies[way].Length];
            for (int fragment = 0; fragment < enemies[way].Length; fragment++)
            {
                enemies[way][fragment] = new Enemy[100];
            }
        }

        // Если фрагменты создаются когда враги уже идут по пути, то нужно их рассортировать.
        if (enemies_count != 0)
        {
            foreach (var enemy in all_enemies)
            {
                for (int fragment = 0; fragment < enemies_arrays_steps[enemy.enemy_way].Length; fragment++)
                {
                    if (enemy.distance_to_finish > enemies_arrays_steps[enemy.enemy_way][fragment])
                    {
                        enemies[enemy.enemy_way][fragment][enemies_in_fragments_count[enemy.enemy_way][fragment]] = enemy;
                        enemies_in_fragments_count[enemy.enemy_way][fragment]++;
                        break;
                    }
                }
            }
        }

    }

    private void remove_tower_fragments(Tower tower)
    {
        for (int way = 0; way < tower.points.Length; way++)
        {
            foreach (var tower_point in tower.points[way])
            {
                bool removed = false;
                for (int fragment = 0; fragment < points_sorted_by_way_fragments[way].Length; fragment++)
                {
                    for (int fragment_point_index = 0; fragment_point_index < points_sorted_by_way_fragments[way][fragment].Length; fragment_point_index++)
                    {
                        if (tower_point == points_sorted_by_way_fragments[way][fragment][fragment_point_index])
                        {
                            points_sorted_by_way_fragments[way][fragment] = remove_point_from_array(points_sorted_by_way_fragments[way][fragment], fragment_point_index);
                            removed = true;
                            break;
                        }
                    }
                    if (removed) break;
                }
            }
        }
    }

    private Vector2[] remove_point_from_array(Vector2[] array, int index)
    {
        Vector2[] _return = new Vector2[array.Length - 1];
        for (int i = 0; i < array.Length - 1; i++)
        {
            if (i >= index)
            {
                _return[i] = array[i + 1];
            }
            else
            {
                _return[i] = array[i];
            }
        }
        return _return;
    }

    // Сортирует заданные точки по близости к "target".
    private Vector2[] sort_points(Vector2[] points, Vector2 target)
    {
        if (points != null)
            for (int i = 0; i < points.Length; i++)
            {
                for (int j = 0; j < points.Length; j++)
                {
                    if (i != j && points[i].x != 1000 && points[j].x != 1000)
                    {
                        float dist1 = Vector2.Distance(points[i], target);
                        float dist2 = Vector2.Distance(points[j], target);

                        if (dist1 > dist2)
                        {
                            Vector2 copy = points[i];
                            points[i] = points[j];
                            points[j] = copy;
                        }
                    }
                }
            }
        return points;
    }

    private Vector2[] remove_duplicate_points(Vector2[] points)
    {
        if (points != null)
        {
            int duplicates = 0;
            for (int i = 0; i < points.Length; i++)
            {
                for (int j = 0; j < points.Length; j++)
                {
                    if (i != j && points[i].x != 1000 && points[j].x != 1000)
                    {
                        if (points[i] == points[j])
                        {
                            points[i] = new Vector2(1000, 0);
                            duplicates++;
                        }
                    }
                }
            }
            Vector2[] _return = new Vector2[points.Length - duplicates];
            int index = 0;
            for (int i = 0; i < points.Length; i++)
            {
                if (points[i].x != 1000)
                {
                    _return[index] = points[i];
                    index++;
                }
            }
            return _return;
        }
        return null;
    }

    // Выдает точки песечения окружности с отрезком.
    private Vector2[] get_intersections_of_circle_and_line(float radius, Vector2 circle_pos, bool horizontal, float line_offset, float start, float end)
    {
        if (start > end) // Исправления случаев, в которых начало отрезка и конец поменялись местами.
        {
            var start2 = start;
            start = end;
            end = start2;
        }
        List<Vector2> intersections = new List<Vector2>();
        if (horizontal) // Если линия горизонтальна.
        {
            // Вычисление пересечений по заготовленной формуле.
            var a = line_offset - circle_pos.y;
            var b = radius * radius - a * a;
            if (b >= 0)
            {
                var x = Mathf.Sqrt(b) + circle_pos.x;
                if (x >= start && x <= end) intersections.Add(new Vector2(x, line_offset));

                var x2 = -Mathf.Sqrt(b) + circle_pos.x;
                if (x2 >= start && x2 <= end) intersections.Add(new Vector2(x2, line_offset));
            }
            else
            {
                a = circle_pos.y - line_offset;
                b = radius * radius - a * a;
                if (b >= 0)
                {
                    var x = Mathf.Sqrt(b) + circle_pos.x;
                    if (x >= start && x <= end) intersections.Add(new Vector2(x, line_offset));

                    var x2 = -Mathf.Sqrt(b) + circle_pos.x;
                    if (x2 >= start && x2 <= end) intersections.Add(new Vector2(x2, line_offset));
                }
            }
        }
        else // Если линия вертикальна.
        {
            var a = line_offset - circle_pos.x;
            var b = radius * radius - a * a;
            if (b >= 0)
            {
                var y = Mathf.Sqrt(b) + circle_pos.y;
                if (y >= start && y <= end) intersections.Add(new Vector2(line_offset, y));

                var y2 = -Mathf.Sqrt(b) + circle_pos.y;
                if (y2 >= start && y2 <= end) intersections.Add(new Vector2(line_offset, y2));
            }
            else
            {
                a = circle_pos.x - line_offset;
                b = radius * radius - a * a;
                if (b >= 0)
                {
                    var y = Mathf.Sqrt(b) + circle_pos.y;
                    if (y >= start && y <= end) intersections.Add(new Vector2(line_offset, y));

                    var y2 = -Mathf.Sqrt(b) + circle_pos.y;
                    if (y2 >= start && y2 <= end) intersections.Add(new Vector2(line_offset, y2));
                }
            }
        }
        if ((intersections.Count == 2 && intersections[0] == intersections[1]) || intersections.Count == 0) return null;
        return intersections.ToArray();
    }

    // Выдает точки песечения окружности 2 с одной из четвертей окружности 1.
    private Vector2[] get_intersections_of_two_circles(Vector2 pos1, float radius1, Vector2 pos2, float radius2, bool up, bool right)
    {
        List<Vector2> intersections = new List<Vector2>();
        // Вычисление пересечений по заготовленной формуле.
        pos2 -= pos1;
        var u1 = pos2.x * pos2.x + pos2.y * pos2.y;
        var u2 = radius2 * radius2 - radius1 * radius1;

        var a = 4 * u1;
        var b = 4 * pos2.y * (u2 - u1);
        var c = u2 * u2 + u1 * u1 - 2 * pos2.y * pos2.y * u2 - 2 * pos2.x * pos2.x * (radius2 * radius2 + radius1 * radius1);

        var d = b * b - 4 * a * c;

        if (d >= 0)
        {
            var y1 = (-b + Mathf.Sqrt(d)) / (2 * a);
            var y2 = (-b - Mathf.Sqrt(d)) / (2 * a);

            var x1 = (u1 - u2 - 2 * y1 * pos2.y) / (2 * pos2.x);
            var x2 = (u1 - u2 - 2 * y2 * pos2.y) / (2 * pos2.x);

            var point_1 = new Vector2(x1, y1) + pos1;
            var point_2 = new Vector2(x2, y2) + pos1;

            if ((point_1.x > pos1.x) == right && (point_1.y > pos1.y) == up) intersections.Add(point_1); // Если точка пересечения находится в нужной четверти.
            if ((point_2.x > pos1.x) == right && (point_2.y > pos1.y) == up) intersections.Add(point_2); // Если вторая точка пересечения находится в нужной четверти.
        }
        if ((intersections.Count == 2 && intersections[0] == intersections[1]) || intersections.Count == 0) return null;
        return intersections.ToArray();
    }

    // Выдает путь проделанный при незаконченном повороте. Принимает радиус и расстояние от начала поворота и текущим положением.
    private float get_point_turn_distance(float radius, float distance_to_start_turn_point)
    {
        var u1 = 2 * radius * radius;
        var u2 = distance_to_start_turn_point * distance_to_start_turn_point;
        return Mathf.Acos((u1 - u2) / u1) / 2;
    }
    #endregion

    #region Portal interaction

    private float[][] portal_zones;
    private bool portal_active = false;


    public void SetPortal(bool active, Vector2Int position = new Vector2Int())
    {
        portal_zones = new float[level_ways.ways_length.Length][];
        portal_active = active;

        if (active)
        {
            int start_point = 0;
            for (int way = 0; way < level_ways.ways_length.Length; way++)
            {
                portal_zones[way] = new float[2];
                float way_distance = 0;
                for (int point = start_point; point < ways_ends[way] - 1; point++)
                {
                    var cur_point = level_ways.way_points[point];
                    var next_point = level_ways.way_points[point + 1];

                    if (cur_point.x != next_point.x && next_point.y != cur_point.y)
                    {
                        if (Vector2.Distance(cur_point, position) == .5f && Vector2.Distance(next_point, position) == .5f)
                        {
                            float portal_pos = ways_distance[way] - (way_distance + pi_div_two / 4);
                            portal_zones[way][0] = portal_pos - .2f;
                            portal_zones[way][1] = portal_pos + .35f;

                            break;
                        }

                        way_distance += pi_div_two / 2;
                    }
                    else
                    {
                        if (cur_point.x == next_point.x && cur_point.x == position.x)
                        {
                            if ((position.y > cur_point.y && position.y < next_point.y) || (position.y < cur_point.y && position.y > next_point.y))
                            {
                                float portal_pos = ways_distance[way] - (way_distance + Vector2.Distance(cur_point, position));
                                portal_zones[way][0] = portal_pos - .2f;
                                portal_zones[way][1] = portal_pos + .2f;

                                break;
                            }

                        }
                        else if (cur_point.y == next_point.y && cur_point.y == position.y)
                        {
                            if ((position.x > cur_point.x && position.x < next_point.x) || (position.x < cur_point.x && position.x > next_point.x))
                            {
                                float portal_pos = ways_distance[way] - (way_distance + Vector2.Distance(cur_point, position));
                                portal_zones[way][0] = portal_pos - .2f;
                                portal_zones[way][1] = portal_pos + .2f;

                                break;
                            }
                        }

                        way_distance += Vector2.Distance(cur_point, next_point);
                    }
                }
                start_point = ways_ends[way];
            }
        }
    }

    #endregion
}

// Класс, описывающий врага.
public class Enemy
{
    public Transform transform;
    public Transform health_bar;
    public int current_way_point; // Посдледняя контрольная точка, которую преодалел враг.
    public int enemy_way; // Показывает по какому из путей движется враг.
    public int group;
    public float turn_value; // Угол поворота врага в радианах.
    public float distance_to_finish; // Путь, который остался преодолеть врагу.
    public Vector2 circle_centre = new Vector2(); // Центр круга, по которому производится вращение врага.
    public int health;
    public float speed_multiplier = 1;
}
