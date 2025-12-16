using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DelayTower : Tower
{
    [SerializeField] private float slow_effect = .5f;

    private void FixedUpdate()
    {
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
                        if (slow_effect < enemy.speed_multiplier) enemy.speed_multiplier = slow_effect;
                       // print("set");
                    }
                }
            }
        }
    }
}
