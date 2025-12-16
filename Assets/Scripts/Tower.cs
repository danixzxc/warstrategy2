using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tower : MonoBehaviour
{
    // Cсылка на вращающуюся часть башни.
    public Transform gun;

    public int cost;
    public float range = 2;
    public float range_multiplier = 1;
    public int type = 0;
    public int level = 1;
    public int[][] enemies_array_range;
    public Vector2[][] points;
    public bool environment_target = false;
}
