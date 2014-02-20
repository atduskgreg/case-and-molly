// Version 1.2
// ©2013 Reindeer Games
// All rights reserved
// Redistribution of source code without permission not allowed

using System;
using System.Collections.Generic;
using UnityEngine;
using Poly2Tri;

namespace Exploder.MeshCutter
{
    /// <summary>
    /// represents a polygon
    /// </summary>
    public class Polygon
    {
        public Vector2[] Points;
        public readonly float Area;
        public Vector2 Min, Max;

        private readonly List<Polygon> holes; 

        /// <summary>
        /// c-tor
        /// </summary>
        /// <param name="pnts">points of the polygon</param>
        public Polygon(Vector2[] pnts)
        {
            ExploderUtils.Assert(pnts.Length >= 3, "Invalid polygon!");

            Points = pnts;
            Area = GetArea();

            holes = new List<Polygon>();
        }

        /// <summary>
        /// compute area of the polygon
        /// </summary>
        /// <returns>area of the polygon</returns>
        public float GetArea()
        {
            Min.x = float.MaxValue;
            Min.y = float.MaxValue;
            Max.x = float.MinValue;
            Max.y = float.MinValue;

            int n = Points.Length;
            float A = 0.0f;
            for (int p = n - 1, q = 0; q < n; p = q++)
            {
                var pval = Points[p];
                var qval = Points[q];
                A += pval.x * qval.y - qval.x * pval.y;

                if (pval.x < Min.x)
                {
                    Min.x = pval.x;
                }
                if (pval.y < Min.y)
                {
                    Min.y = pval.y;
                }
                if (pval.x > Max.x)
                {
                    Max.x = pval.x;
                }
                if (pval.y > Max.y)
                {
                    Max.y = pval.y;
                }
            }

            return A*0.5f;
        }

        /// <summary>
        /// return true if a point is inside the polygon
        /// </summary>
        /// <param name="p">tested point</param>
        /// <returns>true if point is inside</returns>
        public bool IsPointInside(Vector3 p)
        {
            int numVerts = Points.Length;

            var p0 = Points[numVerts - 1];

            bool bYFlag0 = (p0.y >= p.y);
            var p1 = Vector2.zero;

            bool bInside = false;
            for (int j = 0; j < numVerts; ++j)
            {
                p1 = Points[j];
                bool bYFlag1 = (p1.y >= p.y);
                if (bYFlag0 != bYFlag1)
                {
                    if (((p1.y - p.y) * (p0.x - p1.x) >= (p1.x - p.x) * (p0.y - p1.y)) == bYFlag1)
                    {
                        bInside = !bInside;
                    }
                }

                // Move to the next pair of vertices, retaining info as possible.
                bYFlag0 = bYFlag1;
                p0 = p1;
            }

            return bInside;
        }

        /// <summary>
        /// quick test if another polygon is inside this polygon
        /// </summary>
        /// <param name="polygon">testing polygon</param>
        /// <returns></returns>
        public bool IsPolygonInside(Polygon polygon)
        {
            if (Area > polygon.Area)
            {
                return IsPointInside(polygon.Points[0]);
            }

            return false;
        }

        /// <summary>
        /// add hole (polygon inside this polygon)
        /// </summary>
        /// <param name="polygon">polygon representing the hole</param>
        public void AddHole(Polygon polygon)
        {
            holes.Add(polygon);
        }

        /// <summary>
        /// triangulate polygon, algorithm from wiki is fast enough
        /// http://wiki.unity3d.com/index.php?title=Triangulator
        /// </summary>
        /// <returns></returns>
        public List<int> Triangulate()
        {
            // no holes supported
            if (holes.Count == 0)
            {
                var indices = new List<int>(Points.Length);

                int n = Points.Length;
                if (n < 3)
                    return indices;

                var V = new int[n];
                if (Area > 0)
                {
                    for (int v = 0; v < n; v++)
                        V[v] = v;
                }
                else
                {
                    for (int v = 0; v < n; v++)
                        V[v] = (n - 1) - v;
                }

                int nv = n;
                int count = 2*nv;
                for (int m = 0, v = nv - 1; nv > 2;)
                {
                    if ((count--) <= 0)
                        return indices;

                    int u = v;
                    if (nv <= u)
                        u = 0;
                    v = u + 1;
                    if (nv <= v)
                        v = 0;
                    int w = v + 1;
                    if (nv <= w)
                        w = 0;

                    if (Snip(u, v, w, nv, V))
                    {
                        int a, b, c, s, t;
                        a = V[u];
                        b = V[v];
                        c = V[w];
                        indices.Add(a);
                        indices.Add(b);
                        indices.Add(c);
                        m++;
                        for (s = v, t = v + 1; t < nv; s++, t++)
                            V[s] = V[t];
                        nv--;
                        count = 2*nv;
                    }
                }

                indices.Reverse();
                return indices;
            }
            else
            {
                // use poly2tri library to triangulate mesh with holes

                var p2tPoints = new List<Poly2Tri.PolygonPoint>(Points.Length);

                foreach (var point in Points)
                {
                    p2tPoints.Add(new Poly2Tri.PolygonPoint(point.x, point.y));
                }

                // create p2t polygon
                var p2tPolygon = new Poly2Tri.Polygon(p2tPoints);

                // add holes
                foreach (var polygonHole in holes)
                {
                    var p2tHolePoints = new List<Poly2Tri.PolygonPoint>(polygonHole.Points.Length);

                    foreach (var polygonPoint in polygonHole.Points)
                    {
                        p2tHolePoints.Add(new Poly2Tri.PolygonPoint(polygonPoint.x, polygonPoint.y));
                    }

                    p2tPolygon.AddHole(new Poly2Tri.Polygon(p2tHolePoints));
                }

                try
                {
                    Poly2Tri.P2T.Triangulate(p2tPolygon);
                }
                catch (Exception ex)
                {
                    ExploderUtils.Log("P2T Exception: " + ex);
                    return null;
                }

                var triangles = p2tPolygon.Triangles.Count;

                var indices = new List<int>(triangles*3);
                Points = new Vector2[triangles*3];
                var j = 0;

                // recalc min max
                Min.x = float.MaxValue;
                Min.y = float.MaxValue;
                Max.x = float.MinValue;
                Max.y = float.MinValue;

                for (int i = 0; i < triangles; i++)
                {
                    indices.Add((j + 0));
                    indices.Add((j + 1));
                    indices.Add((j + 2));

                    Points[j + 2].x = (float)p2tPolygon.Triangles[i].Points._0.X;
                    Points[j + 2].y = (float)p2tPolygon.Triangles[i].Points._0.Y;

                    Points[j + 1].x = (float)p2tPolygon.Triangles[i].Points._1.X;
                    Points[j + 1].y = (float)p2tPolygon.Triangles[i].Points._1.Y;

                    Points[j + 0].x = (float)p2tPolygon.Triangles[i].Points._2.X;
                    Points[j + 0].y = (float)p2tPolygon.Triangles[i].Points._2.Y;

                    // recalc min max
                    for (int k = 0; k < 3; k++)
                    {
                        if (Points[j + k].x < Min.x)
                        {
                            Min.x = Points[j + k].x;
                        }
                        if (Points[j + k].y < Min.y)
                        {
                            Min.y = Points[j + k].y;
                        }
                        if (Points[j + k].x > Max.x)
                        {
                            Max.x = Points[j + k].x;
                        }
                        if (Points[j + k].y > Max.y)
                        {
                            Max.y = Points[j + k].y;
                        }
                    }

                    j += 3;
                }

                return indices;
            }
        }

        /// <summary>
        /// from http://wiki.unity3d.com/index.php?title=Triangulator
        /// </summary>
        private bool Snip(int u, int v, int w, int n, int[] V)
        {
            int p;
            var A = Points[V[u]];
            var B = Points[V[v]];
            var C = Points[V[w]];
            if (Mathf.Epsilon > (((B.x - A.x) * (C.y - A.y)) - ((B.y - A.y) * (C.x - A.x))))
                return false;
            for (p = 0; p < n; p++)
            {
                if ((p == u) || (p == v) || (p == w))
                    continue;
                var P = Points[V[p]];
                if (InsideTriangle(A, B, C, P))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// from http://wiki.unity3d.com/index.php?title=Triangulator
        /// </summary>
        private bool InsideTriangle(Vector2 A, Vector2 B, Vector2 C, Vector2 P)
        {
            float ax, ay, bx, by, cx, cy, apx, apy, bpx, bpy, cpx, cpy;
            float cCROSSap, bCROSScp, aCROSSbp;

            ax = C.x - B.x; ay = C.y - B.y;
            bx = A.x - C.x; by = A.y - C.y;
            cx = B.x - A.x; cy = B.y - A.y;
            apx = P.x - A.x; apy = P.y - A.y;
            bpx = P.x - B.x; bpy = P.y - B.y;
            cpx = P.x - C.x; cpy = P.y - C.y;

            aCROSSbp = ax * bpy - ay * bpx;
            cCROSSap = cx * apy - cy * apx;
            bCROSScp = bx * cpy - by * cpx;

            return ((aCROSSbp >= 0.0f) && (bCROSScp >= 0.0f) && (cCROSSap >= 0.0f));
        }
    }
}
