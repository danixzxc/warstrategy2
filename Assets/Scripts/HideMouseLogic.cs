using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideMouseLogic : MonoBehaviour
{
    [SerializeField] private float mouse_time = 0.3f;
    private float mouse_current_time = 0;
    private Vector2 mouse_previous_position = new Vector2();
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        mouse_current_time += Time.unscaledDeltaTime;
        print(Input.GetAxis("Mouse X"));

        bool moved = mouse_previous_position != (Vector2)Input.mousePosition;
        //print(moved);


        if (moved)
        {
            Cursor.visible = true;
            mouse_current_time = 0;
        }
        else if (mouse_current_time >= mouse_time)
        {
            Cursor.visible = false;
        }

        mouse_previous_position = (Vector2)Input.mousePosition;
    }
}
