using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public bool moving = true;
    private float Zoom_max = 2.9f, Zoom_min = 1;

    [SerializeField] private float mapMaxX, mapMaxY;
    private Vector3 dragOrigin;
    private Camera cam;
    private int cur_touches = 0;

    private Vector3 Target;
    [HideInInspector] public bool moveToTarget = false;

    public static CameraMovement instance;
    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        cam = Camera.main;
        SetZoomMaxMin();
    }
    public void setTarget(Vector3 target)
    {
        Target = ClampCamera(target);
    }
    private void Update()
    {
        if (moveToTarget)
        {
            var targ = ClampCamera(Target);
            cam.transform.position = ClampCamera(Vector2.Lerp(cam.transform.position, targ, Time.unscaledDeltaTime * 6));
            if (Vector2.Distance(cam.transform.position, Target) < .01f)
            {
                cam.transform.position = targ;
                moveToTarget = false;
            }
        }
        if (moving)
        {
            if (Input.touchCount == 2)
            {
                cur_touches = 2;
                Touch touch_0 = Input.GetTouch(0);
                Touch touch_1 = Input.GetTouch(1);

                Vector2 touch_0_PrevPos = touch_0.position - touch_0.deltaPosition;
                Vector2 touch_1_PrevPos = touch_1.position - touch_1.deltaPosition;

                float PrevMagnitude = (touch_0_PrevPos - touch_1_PrevPos).magnitude;
                float CurMagnitude = (touch_0.position - touch_1.position).magnitude;

                float difference = CurMagnitude - PrevMagnitude;

                zoom(difference * 0.005f);// / Time.timeScale);
                cam.transform.position = ClampCamera(cam.transform.position);
            }
            else if (Input.touchCount > 2) { }
            else if (cur_touches == 0)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    dragOrigin = cam.ScreenToWorldPoint(Input.mousePosition);
                    GameController.instance.hide_buttons();
                }
                if (Input.GetMouseButton(0))
                {
                    Vector3 difference = dragOrigin - cam.ScreenToWorldPoint(Input.mousePosition);
                    cam.transform.position += difference;
                }
            }
            if (Input.GetMouseButtonUp(0))
            {
                dragOrigin = cam.ScreenToWorldPoint(Input.mousePosition);
                cur_touches = 0;
            }

            // Zoom
            zoom(Input.GetAxis("Mouse ScrollWheel"));
            cam.transform.position = ClampCamera(cam.transform.position);
        }
    }

    private Vector3 ClampCamera(Vector3 targetPosition)
    {
        float cam_height = cam.orthographicSize;
        float cam_width = cam.orthographicSize * cam.aspect;

        float minX = -mapMaxX + cam_width;
        float maxX = mapMaxX - cam_width;
        float minY = -mapMaxY + cam_height;
        float maxY = mapMaxY - cam_height;

        float newX = Mathf.Clamp(targetPosition.x, minX, maxX);
        float newY = Mathf.Clamp(targetPosition.y, minY, maxY);

        return new Vector3(newX, newY, -10);
    }

    private void zoom(float value)
    {
        if (value != 0)
        {
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize - value, Zoom_min, Zoom_max);
            GameController.instance.hide_buttons();
        }
    }
    private void SetZoomMaxMin()
    {
        float screen_proportions = (float)Screen.height / (float)Screen.width;
        float xSize = mapMaxX + mapMaxX;
        float ySize = mapMaxY + mapMaxY;
        float map_proportions = ySize / xSize;
        if (screen_proportions <= map_proportions)
        {
            Zoom_max = xSize / 2 * screen_proportions;
        }
        else
        {
            Zoom_max = ySize / 2;
        }
        Zoom_min = Zoom_max * .8f;
        cam.orthographicSize = Zoom_max;
    }
}
