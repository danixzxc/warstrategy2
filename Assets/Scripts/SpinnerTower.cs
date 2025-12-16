using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinnerTower : Tower
{
    // Cсылка на вращающуюся часть башни.

    [SerializeField] private Animator[] animators;

    [SerializeField] private float spinner_rotation_speed = 2;

    [SerializeField] private int damage = 1;

    [SerializeField] private int reload_time = 10;

    [SerializeField] private float rotation_speed = 10;

    private Vector2 target = new Vector2();

    // Текущее вращение по z в рамках 0 - 360.
    private float[] rotations_z = new float[3];

    // Говорит когда стрелять.
    private bool target_found = false;

    private Enemy target_enemy;

    // Таймер. Если его значение больше reload_time, то можно стрелять.
    private int timer = 0;

    // Индекс пушки, из которой башня стреляет следующей.
    private int current_gun = 0;

    private void Start()
    {
        animators[0].transform.rotation = Quaternion.Euler(0, 0, 0);
        animators[1].transform.rotation = Quaternion.Euler(0, 0, 0);
        animators[2].transform.rotation = Quaternion.Euler(0, 0, 0);

        timer = reload_time;

        target = (Vector2)transform.position + new Vector2(0, 2);
    }
    private void Update()
    {
       // EnemiesLogic.instance.visualize_range(this);
    }
    private void FixedUpdate()
    {
        gun.transform.rotation = Quaternion.Euler(0, 0, gun.transform.rotation.eulerAngles.z + spinner_rotation_speed);

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

                rotate_guns();



            // Если в своих фрагментах находится враг, то target_dist != 1000.
            if (target_dist != 1000)
            {
                rotate_guns();
                shoot_logic();
            }

            // Увеличение значения таймера.
            timer++;
        }
    }

    // Вращает пушку по направлению к цели.
    private void rotate_guns()
    {
        for (int gun_index = 0; gun_index < 3; gun_index++)
        {
            Transform current_gun_transform = animators[gun_index].transform;
            Vector2 dir = new Vector2(target.x - current_gun_transform.position.x, target.y - current_gun_transform.position.y);
            bool set_target_found = gun_index == current_gun;
            rotations_z[gun_index] += get_rotation_offset(correct_rotation(Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90), rotations_z[gun_index], set_target_found);
            rotations_z[gun_index] = correct_rotation(rotations_z[gun_index]);
            current_gun_transform.rotation = Quaternion.Euler(0.0f, 0.0f, rotations_z[gun_index]);
        }
    }

    // Логика стрельбы. Вызывается когда есть во что стрелять.
    private void shoot_logic()
    {
        // Если пушка смотрит прямо на цель.
        if (target_found)
        {
            if (timer >= reload_time)
            {
                animators[current_gun].Play("shoot");
                GameAuido.instance.Play(0);
                timer = 0;

                if (environment_target)
                {
                    MapInfo.instance.damage_environment(target, damage);
                }
                else
                {
                    EnemiesLogic.instance.damage_enemy(target_enemy, damage);
                }

                // Создает эффекты пуль и вспышек.
                var effect = Instantiate(Container.instance.objects[0]);
                var trail = Instantiate(Container.instance.objects[1]);
                effect.transform.position = target;
                trail.transform.position = target;
                effect.transform.rotation = Quaternion.Euler(0, 0, Random.Range(0, 360));
                trail.transform.localScale = new Vector3(1, Vector2.Distance(target, transform.position),1);
                trail.transform.rotation = Quaternion.Euler(0, 0, rotations_z[current_gun]);

                current_gun = (current_gun + 1) % 3;
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
    private float get_rotation_offset(float target_rot,float rotation_z, bool set_target_found)
    {
        float target_rot2 = (rotation_z >= 180) ? target_rot + 360 : target_rot - 360;
        float additional_rot1 = target_rot - rotation_z;
        float additional_rot2 = target_rot2 - rotation_z;
        if (Mathf.Abs(additional_rot1) < Mathf.Abs(additional_rot2))
        {
            if (Mathf.Abs(additional_rot1) > rotation_speed)
            {
                if (set_target_found) target_found = false;
                if (additional_rot1 >= 0) return rotation_speed;
                else return -rotation_speed;
            }
            else
            {
                if (set_target_found) target_found = true;
                return additional_rot1;
            }
        }
        else
        {
            if (Mathf.Abs(additional_rot2) > rotation_speed)
            {
                if (set_target_found) target_found = false;
                if (additional_rot2 >= 0) return rotation_speed;
                else return -rotation_speed;
            }
            else
            {
                if (set_target_found) target_found = true;
                return additional_rot2;
            }
        }
    }
}
