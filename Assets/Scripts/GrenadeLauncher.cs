using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrenadeLauncher : Tower
{
    // Cсылка на вращающуюся часть башни.

    [SerializeField] private Animator animator;

    [SerializeField] private int damage = 1;

    [SerializeField] private float bomb_range = 1;

    [SerializeField] private int reload_time = 10;

    [SerializeField] private float rotation_speed = 10;

    [SerializeField] private int guns_count = 1;

    // Текущая цель.
    private Vector2 target;

    // Текущее вращение по z в рамках 0 - 360.
    private float rotation_z = 0;

    // Говорит когда стрелять.
    private bool target_found = false;

    private Enemy target_enemy;

    // Таймер. Если его значение больше reload_time, то можно стрелять.
    private int timer = 0;


    private void Start()
    {
        rotation_z = correct_rotation(gun.transform.rotation.eulerAngles.z);
        timer = reload_time;
    }
    private void Update()
    {
       // EnemiesLogic.instance.visualize_range(this);
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
                                target_enemy = EnemiesLogic.instance.enemies[way][fragment][0];
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
                animator.Play("shoot");
                timer = 0;

                damage_enemies(target);
                damage_environments(target);

                // Создает эффекты пуль и вспышек.
                var trail = Instantiate(Container.instance.objects[1]);
                trail.transform.position = target;
                trail.transform.localScale = new Vector3(1, Vector2.Distance(target, transform.position),1);
                trail.transform.rotation = Quaternion.Euler(0, 0, rotation_z);

                var effect = Instantiate(Container.instance.objects[2]);
                effect.transform.position = target;
                effect.transform.localScale = new Vector3(.6f, .6f, .6f);

                GameAuido.instance.Play(3);
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

    private void damage_enemies(Vector2 pos)
    {
        var enemies = EnemiesLogic.instance.get_all_enemies();
        if (enemies != null)
            foreach (var enemy in enemies)
            {
                float distance = Vector2.Distance(pos, enemy.transform.position);
                if (distance <= bomb_range)
                {
                    int enemy_damage = (int)(1 / (distance*2) * damage);
                    if (enemy_damage > damage || distance == 0) enemy_damage = damage;
                    EnemiesLogic.instance.damage_enemy(enemy, enemy_damage);
                }
            }
    }

    private void damage_environments(Vector2 pos)
    {
        int rounded_range = Mathf.RoundToInt(bomb_range);
        int rounded_x = Mathf.RoundToInt(pos.x);
        int rounded_y = Mathf.RoundToInt(pos.y);
        for (int x = rounded_x - rounded_range; x < rounded_x + rounded_range; x++)
        {
            for (int y = rounded_y - rounded_range; y < rounded_y + rounded_range; y++)
            {
                Vector2Int position = new Vector2Int(x, y);
                if (MapInfo.instance.get_cell_object(position) == 20)
                {
                    float distance = Vector2.Distance(pos, position);
                    if (distance <= bomb_range)
                    {
                        int environment_damage = (int)(1 / (distance * 4) * damage);
                        if (environment_damage > damage || distance == 0) environment_damage = damage;
                        MapInfo.instance.damage_environment(position, environment_damage);
                    }
                }
            }
        }
    }
}
