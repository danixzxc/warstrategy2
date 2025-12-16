using System.Collections.Generic;
using UnityEngine;

public class Intersections
{
    public Vector2[] get_intersections_of_two_circles(Vector2 pos1, float radius1, Vector2 pos2, float radius2, bool up, bool right) // Выдает точки песечения окружности 2 с одной из четвертей окружности 1.
    {
        List<Vector2> intersections = new List<Vector2>();
        // Вычисление пересечений по заготовленной формуле.
        pos2 -= pos1;
        var u1 = pos2.x * pos2.x + pos2.y * pos2.y;
        var u2 = radius2 * radius2 - radius1 * radius1;

        var a = 4 * u1;
        var b = 4 * pos2.y * (u2 - u1);
        var c = u2 * u2 + u1 * u1 - 2 * pos2.y * pos2.y * u2 - 2 * pos2.x * pos2.x * (radius2 * radius2 + radius1 * radius1);

        var d = b * b - 4 * a * c;

        if (d >= 0)
        {
            var y1 = (-b + Mathf.Sqrt(d)) / (2 * a);
            var y2 = (-b - Mathf.Sqrt(d)) / (2 * a);

            var x1 = (u1 - u2 - 2 * y1 * pos2.y) / (2 * pos2.x);
            var x2 = (u1 - u2 - 2 * y2 * pos2.y) / (2 * pos2.x);

            var point_1 = new Vector2(x1, y1) + pos1;
            var point_2 = new Vector2(x2, y2) + pos1;

            if ((point_1.x > pos1.x) == right && (point_1.y > pos1.y) == up) intersections.Add(point_1); // Если точка пересечения находится в нужной четверти.
            if ((point_2.x > pos1.x) == right && (point_2.y > pos1.y) == up) intersections.Add(point_2); // Если вторая точка пересечения находится в нужной четверти.
        }
        if (intersections.Count != 0)
        {
            foreach (var i in intersections)
            {
                //     var p = Instantiate(test);
                //     p.transform.position = i;
                //     tests.Add(p);
            }
        }
        return intersections.ToArray();
    }
    public Vector2[] get_intersections_of_circle_and_line(float radius, Vector2 circle_pos, bool horizontal, float line_offset, float start, float end) // Выдает точки песечения окружности с отрезком.
    {
        if (start > end) // Исправления случаев, в которых начало отрезка и конец поменялись местами.
        {
            var start2 = start;
            start = end;
            end = start2;
        }
        List<Vector2> intersections = new List<Vector2>();
        if (horizontal) // Если линия горизонтальна.
        {
            // Вычисление пересечений по заготовленной формуле.
            var a = line_offset - circle_pos.y;
            var b = radius * radius - a * a;
            if (b >= 0)
            {
                var x = Mathf.Sqrt(b) + circle_pos.x;
                if (x >= start && x <= end) intersections.Add(new Vector2(x, line_offset));

                var x2 = -Mathf.Sqrt(b) + circle_pos.x;
                if (x2 >= start && x2 <= end) intersections.Add(new Vector2(x2, line_offset));
            }
            else
            {
                a = circle_pos.y - line_offset;
                b = radius * radius - a * a;
                if (b >= 0)
                {
                    var x = Mathf.Sqrt(b) + circle_pos.x;
                    if (x >= start && x <= end) intersections.Add(new Vector2(x, line_offset));

                    var x2 = -Mathf.Sqrt(b) + circle_pos.x;
                    if (x2 >= start && x2 <= end) intersections.Add(new Vector2(x2, line_offset));
                }
            }
        }
        else // Если линия вертикальна.
        {
            var a = line_offset - circle_pos.x;
            var b = radius * radius - a * a;
            if (b >= 0)
            {
                var y = Mathf.Sqrt(b) + circle_pos.y;
                if (y >= start && y <= end) intersections.Add(new Vector2(line_offset, y));

                var y2 = -Mathf.Sqrt(b) + circle_pos.y;
                if (y2 >= start && y2 <= end) intersections.Add(new Vector2(line_offset, y2));
            }
            else
            {
                a = circle_pos.x - line_offset;
                b = radius * radius - a * a;
                if (b >= 0)
                {
                    var y = Mathf.Sqrt(b) + circle_pos.y;
                    if (y >= start && y <= end) intersections.Add(new Vector2(line_offset, y));

                    var y2 = -Mathf.Sqrt(b) + circle_pos.y;
                    if (y2 >= start && y2 <= end) intersections.Add(new Vector2(line_offset, y2));
                }
            }
        }
        if (intersections.Count != 0)
        {
            foreach (var i in intersections)
            {
                //     var p = Instantiate(test);
                //     p.transform.position = i;
                //     tests.Add(p);
            }
        }
        return intersections.ToArray();
    }
}
