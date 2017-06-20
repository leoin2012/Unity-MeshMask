/* ==============================================================================
 * 功能描述：
 * 创 建 者：shuchangliu
 * ==============================================================================*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ImageUtil {

    /// <summary>
    /// 使用RayCrossing算法判断点击点是否在封闭多边形里
    /// https://www.zhihu.com/question/26551754
    /// </summary>
    /// <param name="p"></param>
    /// <param name="vertices"></param>
    /// <param name="crossNumber"></param>
    public static bool Contains(Vector2 p, List<Vector2> vertices)
    {
        int crossNumber = 0;

        for (int i = 0, count = vertices.Count; i < count; i++)
        {
            var v1 = vertices[i];
            var v2 = vertices[(i + 1) % count];

            //点击点水平线必须与两顶点线段相交
            if (((v1.y <= p.y) && (v2.y > p.y))
                || ((v1.y > p.y) && (v2.y <= p.y)))
            {
                //只考虑点击点右侧方向，点击点水平线与线段相交，且交点x > 点击点x，则crossNumber+1
                if (p.x < v1.x + (p.y - v1.y) / (v2.y - v1.y) * (v2.x - v1.x))
                {
                    crossNumber += 1;
                }
            }
        }

        return (crossNumber & 1) == 1;
    }

    public static bool Between(float a, float X0, float X1)
    {
        float temp1 = a - X0;
        float temp2 = a - X1;
        if ((temp1 < 1e-8 && temp2 > -1e-8) || (temp2 < 1e-6 && temp1 > -1e-8))
        {
            return true;
        }
        else
        {
            return false;
        }
    }


    // 判断两条直线段是否有交点，有则计算交点的坐标
    // p1,p2是直线一的端点坐标
    // p3,p4是直线二的端点坐标
    public static bool GetIntersection(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, out Vector2 interPoint)
    {
        interPoint = Vector2.zero;
        float line_x, line_y; //交点
        if ((Mathf.Abs(p1.x - p2.x) < 1e-6) && (Mathf.Abs(p3.x - p4.x) < 1e-6))
        {
            return false;
        }
        else if ((Mathf.Abs(p1.x - p2.x) < 1e-6)) //如果直线段p1p2垂直与y轴
        {
            if (Between(p1.x, p3.x, p4.x))
            {
                float k = (p4.y - p3.y) / (p4.x - p3.x);
                line_x = p1.x;
                line_y = k * (line_x - p3.x) + p3.y;

                if (Between(line_y, p1.y, p2.y))
                {
                    interPoint.x = line_x;
                    interPoint.y = line_y;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        else if ((Mathf.Abs(p3.x - p4.x) < 1e-6)) //如果直线段p3p4垂直与y轴
        {
            if (Between(p3.x, p1.x, p2.x))
            {
                float k = (p2.y - p1.y) / (p2.x - p1.x);
                line_x = p3.x;
                line_y = k * (line_x - p2.x) + p2.y;

                if (Between(line_y, p3.y, p4.y))
                {
                    interPoint.x = line_x;
                    interPoint.y = line_y;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        else
        {
            float k1 = (p2.y - p1.y) / (p2.x - p1.x);
            float k2 = (p4.y - p3.y) / (p4.x - p3.x);

            if (Mathf.Abs(k1 - k2) < 1e-6)
            {
                return false;
            }
            else
            {
                line_x = ((p3.y - p1.y) - (k2 * p3.x - k1 * p1.x)) / (k1 - k2);
                line_y = k1 * (line_x - p1.x) + p1.y;
            }

            if (Between(line_x, p1.x, p2.x) && Between(line_x, p3.x, p4.x))
            {
                interPoint.x = line_x;
                interPoint.y = line_y;
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    /// <summary>
    /// 求两个多边形差集
    /// </summary>
    /// <param name="poly1"></param>
    /// <param name="poly2"></param>
    /// <param name="interPoly"></param>
    /// <returns></returns>
    public static bool PolygonClip(List<Vector2> poly1, List<Vector2> poly2, out List<Vector2> interPoly)
    {
        interPoly = new List<Vector2>();

        if (poly1.Count < 3 || poly2.Count < 3)
        {
            return false;
        }

        Vector2 point;
        //计算多边形交点
        for (int i = 0; i < poly1.Count; i++)
        {
            int poly1_next_idx = (i + 1) % poly1.Count;
            for (int j = 0; j < poly2.Count; j++)
            {
                int poly2_next_idx = (j + 1) % poly2.Count;
                if (ImageUtil.GetIntersection(poly1[i], poly1[poly1_next_idx],
                    poly2[j], poly2[poly2_next_idx],
                    out point))
                {
                    interPoly.Add(point);
                }
            }
        }

        //计算多边形内部点
        for (int i = 0; i < poly1.Count; i++)
        {
            if (ImageUtil.Contains(poly1[i], poly2))
            {
                interPoly.Add(poly1[i]);
            }
        }
        for (int i = 0; i < poly2.Count; i++)
        {
            if (ImageUtil.Contains(poly2[i], poly1))
            {
                interPoly.Add(poly2[i]);
            }
        }

        if (interPoly.Count <= 0)
            return false;

        //点集排序 
        interPoly = EdgeUtil.CreateConvexOutside(interPoly);
        return true;
    }



}
