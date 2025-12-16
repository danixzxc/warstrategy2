using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadiationTower : Tower
{
    [SerializeField] private int damage_interval = 15;
    [SerializeField] private int damage = 5;
    [SerializeField] private bool examplae = false;

    private int time = 0;

    private int env_damage_rate = 4;

    private List<Vector2Int> environments;

    private void Start()
    {
        if (!examplae) environments = find_environment_cells(count_effected_cells());
        else environments = new List<Vector2Int>();
    }

    private void FixedUpdate()
    {
        time++;
        if (time % damage_interval == 0)
        {
            print("damage");
            if (enemies_array_range != null)
            {
                for (int way = 0; way < enemies_array_range.Length; way++)
                {
                    for (int fragment_index = enemies_array_range[way].Length - 1; fragment_index >= 0; fragment_index--)
                    {
                        int fragment = enemies_array_range[way][fragment_index];

                        foreach (var enemy in EnemiesLogic.instance.enemies[way][fragment])
                        {
                            if (enemy == null) break;
                            EnemiesLogic.instance.damage_enemy(enemy, damage);
                        }
                    }
                }
            }

            if (environments.Count != 0 && time / damage_interval == env_damage_rate)
            {
                List<Vector2Int> new_environments = new List<Vector2Int>(environments);
                print("count");
                foreach (var environment in environments)
                {
                    int cell_object = MapInfo.instance.get_cell_object(environment);
                    if (cell_object == 20) MapInfo.instance.damage_environment(environment, damage);
                    else new_environments.Remove(environment);
                }

                environments = new_environments;
            }

            if (time / damage_interval == env_damage_rate) time = 0;
        }
    }

    // Находит клетки на которые будет действовать башня.
    private Vector2Int[] count_effected_cells()
    {
        List<Vector2Int> _return = new List<Vector2Int>();
        var mapSize = MapInfo.instance.get_map_size();
        for (int x = (int)(transform.position.x - 1); x < (int)(transform.position.x + 2); x++)
        {
            for (int y = (int)(transform.position.y - 1); y < (int)(transform.position.y + 2); y++)
            {
                if (y >= -mapSize.y / 2 && y < mapSize.y / 2 + 1 && x >= -mapSize.x / 2 && x < mapSize.x / 2 + 1)
                {
                    _return.Add(new Vector2Int(x, y));
                }
            }
        }
        return _return.ToArray();
    }

    // Возвращает список с координатами клеток с объектами окружения.
    private List<Vector2Int> find_environment_cells(Vector2Int[] all_cells)
    {
        List<Vector2Int> environments = new List<Vector2Int>();

        foreach (var cell in all_cells)
        {
            int cell_object = MapInfo.instance.get_cell_object(cell);
            if (cell_object == 20) // env
            {
                environments.Add(cell);
            }
        }

        return environments;
    }
}
