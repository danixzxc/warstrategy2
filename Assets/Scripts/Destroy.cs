using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destroy : MonoBehaviour
{
    [SerializeField] private int time;
    int timer = 0;
    private void FixedUpdate()
    {
        if (time != 0)
        {
            timer++;
            if (timer == time)
            {
                destroy();
            }
        }
    }
    private void destroy()
    {
        Destroy(gameObject);
    }
}
