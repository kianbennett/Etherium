using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MathUtil {

    public static float Root3 = Mathf.Sqrt(3);

    public static float MakePositive(float f) {
        return f < 0 ? f * -1 : f;
    }

    public static Vector2 MakePositive(Vector2 v) {
        v.x = MakePositive(v.x);
        v.y = MakePositive(v.y);
        return v;
    }

    public static Vector3 RandomVector3(float min, float max) {
        return new Vector3(Random.Range(min, max), Random.Range(min, max), Random.Range(min, max));
    }

    public static Vector3 RotateAroundPoint(Vector3 vector, Vector3 point, Vector3 angles) {
        Vector3 dir = vector - point; // get vector direction relative to point
        dir = Quaternion.Euler(angles) * dir; // rotate it
        vector = dir + point; // calculate rotated point
        return vector; // return it
    }

    public static Vector3 AverageVector3(params Vector3[] vectors) {
        Vector3 result = Vector3.zero;

        foreach(Vector3 v in vectors) {
            result += v;
        }
        result *= 1f / vectors.Length;
        return result;
    }

    public static float TotalVectorLengths(params Vector3[] vectors) {
        float length = 0;
        for(int i = 0; i < vectors.Length; i++) {
            if (i == 0) continue;
            length += Vector3.Distance(vectors[i], vectors[i - 1]);
        }
        return length;
    }

    //public static List<Vector3> GetGroupGridPoints(List<Character> characters, Vector3 target, float r, Vector3 anchor = default(Vector3)) {
    //    Vector3 dir = Vector3.zero;
    //    if (characters.Count == 2) dir = MathUtil.AverageVector3(characters[0].transform.position, characters[1].transform.position) - target;
    //    return GetGroupGridPoints(characters.Count, target, r, anchor, dir);
    //}

    /* Returns a list of points that a group of characters would form around a point 
       Anchor is the point at which the group is centred (1,0,1 is top right 0,0,0 is centre) */
    public static List<Vector3> GetGroupGridPoints(int count, Vector3 target, float r, Vector3 anchor = default(Vector3), Vector3 dir = default(Vector3)) {
        List<Vector3> gridPoints = new List<Vector3>();

        if (count == 1) {
            gridPoints.Add(target);
            return gridPoints;
        }

        if (count == 2) {
            Vector3 p1 = new Vector3(target.x - r, 0, target.z);
            Vector3 p2 = new Vector3(target.x + r, 0, target.z);
            if (dir.magnitude != 0) {
                p1 = MathUtil.RotateAroundPoint(p1, target, Quaternion.LookRotation(dir).eulerAngles);
                p2 = MathUtil.RotateAroundPoint(p2, target, Quaternion.LookRotation(dir).eulerAngles);
            }
            gridPoints.Add(p1);
            gridPoints.Add(p2);
            return gridPoints;
        }

        // Get the maximum number of points to test from
        int root = Mathf.CeilToInt(Mathf.Sqrt(count));

        // Arrange them in a hex grid
        float gapX = r * 2;
        float gapY = r * MathUtil.Root3;

        //Vector3 origin = new Vector3(root * gapX / 2f - gapX / 2, 0, root * gapY / 2f - gapY / 2);
        //if (root % 2 != 0) origin.z -= gapY / 2;

        for (int j = 0; j < root; j++) {
            for (int i = 0; i < root; i++) {
                Vector3 point = new Vector3(i * gapX, 0, j * gapY);
                if (j % 2 != 0) { // if odd row, offset x
                    point.x += r;
                }
                gridPoints.Add(point);
            }
        }

        // Get hex grid origin
        Vector3 origin = MathUtil.AverageVector3(gridPoints.Take(count).ToArray());

        // Offset origin to anchor
        origin = Vector3.Scale(origin, Vector3.one + anchor);

        // Order by distance to target
        gridPoints = gridPoints.OrderBy(o => Vector3.Distance(o, origin)).ToList();

        // Add offset
        for (int i = 0; i < gridPoints.Count; i++) gridPoints[i] += target - origin;

        return gridPoints;
    }
}
