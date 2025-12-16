using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bomb : MonoBehaviour
{
    [SerializeField] private int time = 100;
    [SerializeField] private float range = 1;
    [SerializeField] private int damage = 100;

    private int current_time = 0;

    private void FixedUpdate()
    {
        if (current_time >= time)
        {
            damage_enemies();
            damage_environments();

            var effect = Instantiate(Container.instance.objects[2]);
            effect.transform.position = transform.position;
            effect.transform.localScale = new Vector3(1.1f, 1.1f,1.1f);
            GameAuido.instance.Play(3);
            Destroy(gameObject);
        }
        current_time++;
    }

    private void damage_enemies()
    {
        var enemies = EnemiesLogic.instance.get_all_enemies();
        if (enemies != null)
            foreach (var enemy in enemies)
            {
                float distance = Vector2.Distance(transform.position, enemy.transform.position);
                if (distance <= range)
                {
                    int enemy_damage = (int)(1 / distance * damage);
                    EnemiesLogic.instance.damage_enemy(enemy, enemy_damage);
                }
            }
    }

    private void damage_environments()
    {
        int rounded_range = Mathf.RoundToInt(range);
        for (int x = (int)transform.position.x - rounded_range; x < (int)transform.position.x + rounded_range; x++)
        {
            for (int y = (int)transform.position.y - rounded_range; y < (int)transform.position.y + rounded_range; y++)
            {
                Vector2Int position = new Vector2Int(x, y);
                if (MapInfo.instance.get_cell_object(position) == 20)
                {
                    float distance = Vector2.Distance(transform.position, position);
                    if (distance <= range)
                    {
                        int environment_damage = (int)(1 / distance * damage);
                        MapInfo.instance.damage_environment(position,environment_damage);
                    }
                }
            }
        }
    }
}
