/* ==============================================================================
 * 功能描述：计算图形的有序点集
 * 创 建 者：shuchangliu
 * ==============================================================================*/

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Collections;

public class EdgeUtil
{

    private static Dictionary<Vector2, bool> pointCheckFlags;

    public static List<Vector2> GetPoints(Texture2D t2d)
    {
        List<Vector2> ret = new List<Vector2>();
        for (int i = 0; i < t2d.width; i++)
        {
            for (int j = 0; j < t2d.height; j++)
            {
                if (t2d.GetPixel(i, j).a > 0)
                    ret.Add(new Vector2(i, j));
            }
        }
        ret = CreateConcaveOutside(ret);
        return ret;
    }

    /// <summary>
    /// 按顺时针方向生成凹边形(效率较低)
    /// </summary>
    /// <param name="corners"></param>
    /// <returns></returns>
    public static List<Vector2> CreateConcaveOutside(List<Vector2> corners)
    {
        List<Vector2> orderedCorners = new List<Vector2>();
        List<Vector2> indexs = corners.OrderBy(a => a.x).ToList();
        Vector2 cur = indexs[0];
        Vector2 next = Vector2.zero;
        float minAngle;
        Vector2 vector = new Vector2(0, 1);
        Vector2 tmpVector = Vector2.zero;

        bool hasFindTarget;
        int targetDistance;

        orderedCorners.Add(cur);
        corners.Remove(cur);
        //生成外围边
        do
        {
            minAngle = 360;
            //泛洪法查找可能尽可能近的点组成边
            hasFindTarget = false;
            targetDistance = 0;
            while (!hasFindTarget)
            {
                targetDistance += 1;
                foreach (var temp in corners)
                {
                    if (temp == cur) continue;
                    if (Vector2.SqrMagnitude(temp - cur) > targetDistance) continue;//从身边8个格子开始查找起

                    tmpVector.x = temp.x - cur.x;
                    tmpVector.y = temp.y - cur.y;

                    float tmpAngle = Vector3.Angle(vector, tmpVector);
                    if (tmpAngle < minAngle)
                    {
                        minAngle = tmpAngle;
                        next = temp;
                        hasFindTarget = true;
                    }
                }
            }

            if (orderedCorners.Count >= 2)
            {
                vector = next - orderedCorners[orderedCorners.Count - 2];
                tmpVector = orderedCorners[orderedCorners.Count - 1] - orderedCorners[orderedCorners.Count - 2];

                //找到新顶点的角度与上次迭代的顶点角度相同，说明两顶点共边，可以删除上次迭代的顶点
                if (Vector3.Angle(vector, tmpVector) == 0)
                {
                    orderedCorners.RemoveAt(orderedCorners.Count - 1);
                }
            }

            //加入新顶点
            orderedCorners.Add(next);
            corners.Remove(next);
            //
            vector.x = next.x - cur.x;
            vector.y = next.y - cur.y;
            cur = next;
            next = Vector2.zero;
        } while (corners.Count > 0);

        return orderedCorners;
    }


    /// <summary>
    /// 按顺时针方向生成凸边形(效率较高)
    /// </summary>
    /// <param name="corners"></param>
    /// <returns></returns>
    public static List<Vector2> CreateConvexOutside(List<Vector2> corners)
    {
        List<Vector2> orderedCorners = new List<Vector2>();
        List<Vector2> indexs = corners.OrderBy(a => a.x).ToList();
        Vector2 cur = indexs[0];
        Vector2 next = Vector2.zero;
        Vector2 start = cur;
        float minAngle;
        Vector2 vector = new Vector2(0, 1);
        Vector2 tmpVector = Vector2.zero;

        orderedCorners.Add(cur);
        //生成外围边
        do
        {
            minAngle = 360;

            foreach (var temp in corners)
            {
                if (temp == cur) continue;

                tmpVector.x = temp.x - cur.x;
                tmpVector.y = temp.y - cur.y;

                float tmpAngle = Vector3.Angle(vector, tmpVector);
                if (tmpAngle < minAngle)
                {
                    minAngle = tmpAngle;
                    next = temp;
                }
            }

            if (orderedCorners.Count >= 2)
            {
                vector = next - orderedCorners[orderedCorners.Count - 2];
                tmpVector = orderedCorners[orderedCorners.Count - 1] - orderedCorners[orderedCorners.Count - 2];

                //找到新顶点的角度与上次迭代的顶点角度相同，说明两顶点共边，可以删除上次迭代的顶点
                if (Vector3.Angle(vector, tmpVector) == 0)
                {
                    orderedCorners.RemoveAt(orderedCorners.Count - 1);
                }
            }

            //加入新顶点
            orderedCorners.Add(next);
            corners.Remove(next);
            //
            vector.x = next.x - cur.x;
            vector.y = next.y - cur.y;
            cur = next;
            next = Vector2.zero;
        } while (cur != start);

        return orderedCorners;
    }

}
