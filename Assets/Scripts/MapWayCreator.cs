using UnityEngine;
using UnityEditor;
using System.IO;
#if UNITY_EDITOR
public class MapWayCreator : EditorWindow
{
    [MenuItem("Window/files creator")]
    public static void ShowWindow()
    {
        GetWindow<MapWayCreator>();
    }

    // Массив объектов хранящих точки передвижения каждого из путей.
    public Transform[] Ways = new Transform[0];
    SerializedObject serializedObject;

    // Рисунок карты. Нужен для определения размера карты.
    private SpriteRenderer map_sprite;

    private void OnEnable()
    {
        ScriptableObject target = this;
        serializedObject = new SerializedObject(target);
    }

    void OnGUI()
    {
        // Ввод данных в массив Ways.
        serializedObject.Update();
        SerializedProperty stringsProperty = serializedObject.FindProperty("Ways");
        EditorGUILayout.PropertyField(stringsProperty, true);
        serializedObject.ApplyModifiedProperties();

        // Ввод рисунока карты.
        map_sprite = EditorGUILayout.ObjectField("Map sprite", map_sprite, typeof(SpriteRenderer), true) as SpriteRenderer;

        // При нажатии на кнопку создания файла пути.
        if (GUILayout.Button("Cоздания файла пути"))
        {
            WaysSave waysSave = new WaysSave();
            waysSave.ways_length = new int[Ways.Length];
            int way_points_count = 0;
            for (int way = 0; way < Ways.Length; way++)
            {
                way_points_count += Ways[way].childCount;
                waysSave.ways_length[way] = Ways[way].childCount;
            }
            waysSave.way_points = new Vector2[way_points_count];

            int point_index = 0;
            for (int way = 0; way < Ways.Length; way++)
            {
                for (int point_index_in_way = 0; point_index_in_way < Ways[way].childCount; point_index_in_way++)
                {
                    var point = Ways[way].GetChild(point_index_in_way);
                    waysSave.way_points[point_index] = point.position;
                    point_index++;
                }
            }
            File.WriteAllText(Application.dataPath + $"/New files/ways.json", JsonUtility.ToJson(waysSave));
            Debug.Log("Way file created.");
        }

        // При нажатии на кнопку создания файла объектов на карте.
        if (GUILayout.Button("Cоздания файла объектов на карте"))
        {
            MapInformation mapInformation = new MapInformation();

            mapInformation.size = new Vector2(map_sprite.bounds.size.x, map_sprite.bounds.size.y);
            mapInformation.map = new int[(int)map_sprite.bounds.size.x * (int)map_sprite.bounds.size.y];

            if ((int)mapInformation.size.x % 2 != 1 || (int)mapInformation.size.y % 2 != 1) Debug.LogError("Wrong map size");

            var roads = GameObject.FindGameObjectsWithTag("Road");
            var employeds = GameObject.FindGameObjectsWithTag("Employed");

            foreach (var road in roads)
            {
                int x = Mathf.RoundToInt(road.transform.position.x) + (int)(mapInformation.size.x - 1) / 2;
                int y = -(Mathf.RoundToInt(road.transform.position.y) - (int)(mapInformation.size.y - 1) / 2);
                if (mapInformation.map[x + y * (int)mapInformation.size.x] != 0) Debug.LogError("Duplicate on " + Mathf.RoundToInt(road.transform.position.x) + ":" + Mathf.RoundToInt(road.transform.position.y));
                mapInformation.map[x + y * (int)mapInformation.size.x] = 10;
            }
            foreach (var employed in employeds)
            {
                int x = Mathf.RoundToInt(employed.transform.position.x) + (int)(mapInformation.size.x - 1) / 2;
                int y = -(Mathf.RoundToInt(employed.transform.position.y) - (int)(mapInformation.size.y - 1) / 2);
                if (mapInformation.map[x + y * (int)mapInformation.size.x] != 0) Debug.LogError("Duplicate on " + Mathf.RoundToInt(employed.transform.position.x) + ":" + Mathf.RoundToInt(employed.transform.position.y));
                mapInformation.map[x + y * (int)mapInformation.size.x] = 30;
            }
            File.WriteAllText(Application.dataPath + $"/New files/map.json", JsonUtility.ToJson(mapInformation));
            Debug.Log("Map file created.");
        }
    }

    private class WaysSave
    {
        // Хранит все точки передвижения последовательно.
        public Vector2[] way_points;

        // Хранит число точек передвижения из которых состоит путь.
        public int[] ways_length;
    }

    private class MapInformation
    {
        // Размер карты.
        public Vector2 size;

        // Показывает что хранится на каждом из мест на карте.
        // 0 - пусто, 10 - дорога, 20 - объект окружения, 30 - занято.
        public int[] map;
    }

}
#endif
