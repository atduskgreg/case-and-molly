// Version 1.2
// ©2013 Reindeer Games
// All rights reserved
// Redistribution of source code without permission not allowed

//#define PROFILING

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Exploder.MeshCutter
{
    public struct CutterMesh
    {
        public Mesh mesh;
        public Vector3 centroid;
    }

    public class MeshCutter
    {
        private List<int>[] triangles;
        private List<Vector3>[] vertices;
        private List<Vector3>[] normals;
        private List<Vector2>[] uvs;
        private List<Vector4>[] tangents;
        private List<Color32>[] vertexColors;

        private List<int> cutTris;
        private int[] triCache;

        private Vector3[] centroid;
        private int[] triCounter;

        private Contour contour;
        private Dictionary<long, int>[] cutVertCache;
        private Dictionary<int, int>[] cornerVertCache;
        private int contourBufferSize;

        private Color crossSectionVertexColour;
        private Vector4 crossSectionUV;

        /// <summary>
        /// initialize cutter for faster start
        /// </summary>
        /// <param name="trianglesNum">potencial number of triangles</param>
        /// <param name="verticesNum">potencial number of vertices</param>
        public void Init(int trianglesNum, int verticesNum)
        {
            AllocateBuffers(trianglesNum, verticesNum, false, false);
            AllocateContours(trianglesNum/2);
        }

        void AllocateBuffers(int trianglesNum, int verticesNum, bool useMeshTangents, bool useVertexColors)
        {
            // pre-allocate mesh data for both sides
            if (triangles == null || triangles[0].Capacity < trianglesNum)
            {
                triangles = new[] { new List<int>(trianglesNum), new List<int>(trianglesNum) };
            }
            else
            {
                triangles[0].Clear();
                triangles[1].Clear();
            }

            if (vertices == null || vertices[0].Capacity < verticesNum || triCache.Length < verticesNum || 
                (useMeshTangents && (tangents == null || tangents[0].Capacity < verticesNum)) || 
                (useVertexColors && (vertexColors == null || vertexColors[0].Capacity < verticesNum)))
            {
                vertices = new[] { new List<Vector3>(verticesNum), new List<Vector3>(verticesNum) };
                normals = new[] { new List<Vector3>(verticesNum), new List<Vector3>(verticesNum) };
                uvs = new[] { new List<Vector2>(verticesNum), new List<Vector2>(verticesNum) };

                if (useMeshTangents)
                {
                    tangents = new[] {new List<Vector4>(verticesNum), new List<Vector4>(verticesNum)};
                }
                else
                {
                    tangents = new[] { new List<Vector4>(0), new List<Vector4>(0) };
                }

                if (useVertexColors)
                {
                    vertexColors = new[] {new List<Color32>(verticesNum), new List<Color32>(verticesNum)};
                }
                else
                {
                    vertexColors = new[] { new List<Color32>(0), new List<Color32>(0) };
                }

                centroid = new Vector3[2];

                triCache = new int[verticesNum + 1];
                triCounter = new int[2] { 0, 0 };
                cutTris = new List<int>(verticesNum / 3);
            }
            else
            {
                for (int i = 0; i < 2; i++)
                {
                    vertices[i].Clear();
                    normals[i].Clear();
                    uvs[i].Clear();
                    tangents[i].Clear();
                    vertexColors[i].Clear();
                    centroid[i] = Vector3.zero;
                    triCounter[i] = 0;
                }
                cutTris.Clear();
                for (int i = 0; i < triCache.Length; i++)
                {
                    triCache[i] = 0;
                }
            }
        }

        void AllocateContours(int cutTrianglesNum)
        {
            // pre-allocate contour data
            if (contour == null)
            {
//                Utils.Log("Allocating contours buffes: " + cutTrianglesNum);

                contour = new Contour(cutTrianglesNum);
                cutVertCache = new[] { new Dictionary<long, int>(cutTrianglesNum * 2), new Dictionary<long, int>(cutTrianglesNum * 2) };
                cornerVertCache = new[] { new Dictionary<int, int>(cutTrianglesNum), new Dictionary<int, int>(cutTrianglesNum) };
                contourBufferSize = cutTrianglesNum;
            }
            else
            {
                if (contourBufferSize < cutTrianglesNum)
                {
//                    Utils.Log("Re-allocating contours buffes: " + cutTrianglesNum);

                    cutVertCache = new[] { new Dictionary<long, int>(cutTrianglesNum * 2), new Dictionary<long, int>(cutTrianglesNum * 2) };
                    cornerVertCache = new[] { new Dictionary<int, int>(cutTrianglesNum), new Dictionary<int, int>(cutTrianglesNum) };

                    contourBufferSize = cutTrianglesNum;
                }
                else
                {
                    for (int i = 0; i < 2; i++)
                    {
                        cutVertCache[i].Clear();
                        cornerVertCache[i].Clear();
                    }
                }

                contour.AllocateBuffers(cutTrianglesNum);
            }
        }

        /// <summary>
        /// cut mesh by plane
        /// </summary>
        /// <param name="mesh">mesh to cut</param>
        /// <param name="meshTransform">transformation of the mesh</param>
        /// <param name="plane">cutting plane</param>
        /// <param name="mesh0">first part of the new mesh</param>
        /// <param name="mesh1">second part of the new mesh</param>
        /// <param name="triangulateHoles">flag for triangulation of holes</param>
        /// <param name="centroid0">center position of the new mesh0 - apply to transform.position to stay on the same position</param>
        /// <param name="centroid1">center position of the new mesh1 - apply to transform.position to stay on the same position</param>
        /// <param name="crossSectionVertexColor">this color will be assigned to cross section, valid only for vertex color shaders</param>
        /// <param name="crossUV">uv mapping area for cross section</param>
        /// <returns>processing time</returns>
        public float Cut(Mesh mesh, Transform meshTransform, Math.Plane plane, bool triangulateHoles, ref List<CutterMesh> meshes,
                         Color crossSectionVertexColor, Vector4 crossUV)
        {
            this.crossSectionVertexColour = crossSectionVertexColor;
            this.crossSectionUV = crossUV;
            return Cut(mesh, meshTransform, plane, triangulateHoles, ref meshes);
        }

        float Cut(Mesh mesh, Transform meshTransform, Math.Plane plane, bool triangulateHoles, ref List<CutterMesh> meshes)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

#if PROFILING
            MeasureIt.Begin("CutAllocations");
#endif

            // cache mesh data
            var trianglesNum = mesh.triangles.Length;
            var verticesNum = mesh.vertices.Length;
            var meshTriangles = mesh.triangles;
            var meshTangents = mesh.tangents;
            var meshColors = mesh.colors32;
            var meshVertices = mesh.vertices;
            var meshNormals = mesh.normals;
            var meshUV = mesh.uv;
            var useMeshTangents = meshTangents != null && meshTangents.Length > 0;
            var useVertexColors = meshColors != null && meshColors.Length > 0;
            var useNormals = meshNormals != null && meshNormals.Length > 0;

            // preallocate buffers
            AllocateBuffers(trianglesNum, verticesNum, useMeshTangents, useVertexColors);

            CutterMesh mesh0, mesh1;

#if PROFILING
            MeasureIt.End("CutAllocations");
            MeasureIt.Begin("CutCycleFirstPass");
#endif

            // inverse transform cutting plane
            plane.InverseTransform(meshTransform);

            // first pass - find complete triangles on both sides of the plane
            for (int i = 0; i < trianglesNum; i += 3)
            {
                // get triangle points
                var v0 = meshVertices[meshTriangles[i]];
                var v1 = meshVertices[meshTriangles[i + 1]];
                var v2 = meshVertices[meshTriangles[i + 2]];

                var side0 = plane.GetSideFix(ref v0);
                var side1 = plane.GetSideFix(ref v1);
                var side2 = plane.GetSideFix(ref v2);

                meshVertices[meshTriangles[i]] = v0;
                meshVertices[meshTriangles[i + 1]] = v1;
                meshVertices[meshTriangles[i + 2]] = v2;

//                Utils.Log(plane.Pnt + " " + v0 + " " + v1 + " " + " " + v2);

                // all points on one side
                if (side0 == side1 && side1 == side2)
                {
                    var idx = side0 ? 0 : 1;

                    if (meshTriangles[i] >= triCache.Length)
                    {
                        ExploderUtils.Log("TriCacheError " + meshTriangles[i] + " " + triCache.Length + " " + meshVertices.Length);
                    }

                    if (triCache[meshTriangles[i]] == 0)
                    {
                        triangles[idx].Add(triCounter[idx]);
                        vertices[idx].Add(meshVertices[meshTriangles[i]]);
                        uvs[idx].Add(meshUV[meshTriangles[i]]);

                        if (useNormals)
                        {
                            normals[idx].Add(meshNormals[meshTriangles[i]]);
                        }

                        if (useMeshTangents)
                        {
                            tangents[idx].Add(meshTangents[meshTriangles[i]]);
                        }

                        if (useVertexColors)
                        {
                            vertexColors[idx].Add(meshColors[meshTriangles[i]]);
                        }

                        centroid[idx] += meshVertices[meshTriangles[i]];

                        triCache[meshTriangles[i]] = triCounter[idx] + 1;
                        triCounter[idx]++;
                    }
                    else
                    {
                        triangles[idx].Add(triCache[meshTriangles[i]] - 1);
                    }

                    if (triCache[meshTriangles[i + 1]] == 0)
                    {
                        triangles[idx].Add(triCounter[idx]);
                        vertices[idx].Add(meshVertices[meshTriangles[i + 1]]);
                        uvs[idx].Add(meshUV[meshTriangles[i + 1]]);

                        if (useNormals)
                        {
                            normals[idx].Add(meshNormals[meshTriangles[i + 1]]);
                        }

                        if (useMeshTangents)
                        {
                            tangents[idx].Add(meshTangents[meshTriangles[i+1]]);
                        }

                        if (useVertexColors)
                        {
                            vertexColors[idx].Add(meshColors[meshTriangles[i+1]]);
                        }

                        centroid[idx] += meshVertices[meshTriangles[i + 1]];

                        triCache[meshTriangles[i + 1]] = triCounter[idx] + 1;
                        triCounter[idx]++;
                    }
                    else
                    {
                        triangles[idx].Add(triCache[meshTriangles[i + 1]] - 1);
                    }

                    if (triCache[meshTriangles[i + 2]] == 0)
                    {
                        triangles[idx].Add(triCounter[idx]);
                        vertices[idx].Add(meshVertices[meshTriangles[i + 2]]);
                        uvs[idx].Add(meshUV[meshTriangles[i + 2]]);

                        if (useNormals)
                        {
                            normals[idx].Add(meshNormals[meshTriangles[i + 2]]);
                        }

                        if (useMeshTangents)
                        {
                            tangents[idx].Add(meshTangents[meshTriangles[i+2]]);
                        }

                        if (useVertexColors)
                        {
                            vertexColors[idx].Add(meshColors[meshTriangles[i+2]]);
                        }

                        centroid[idx] += meshVertices[meshTriangles[i + 2]];

                        triCache[meshTriangles[i + 2]] = triCounter[idx] + 1;
                        triCounter[idx]++;
                    }
                    else
                    {
                        triangles[idx].Add(triCache[meshTriangles[i + 2]] - 1);
                    }
                }
                else
                {
                    // intersection triangles add to list and process it in second pass
                    cutTris.Add(i);
                }
            }

            if (vertices[0].Count == 0)
            {
                centroid[0] = meshVertices[0];
            }
            else
            {
                centroid[0] /= vertices[0].Count;
            }

            if (vertices[1].Count == 0)
            {
                centroid[1] = meshVertices[1];
            }
            else
            {
                centroid[1] /= vertices[1].Count;
            }

#if PROFILING
            MeasureIt.End("CutCycleFirstPass");
            MeasureIt.Begin("CutCycleSecondPass");
#endif

            mesh0.centroid = centroid[0];
            mesh1.centroid = centroid[1];

            mesh0.mesh = null;
            mesh1.mesh = null;

            if (cutTris.Count < 1)
            {
                stopWatch.Stop();
                return stopWatch.ElapsedMilliseconds;
            }

            AllocateContours(cutTris.Count);

            // second pass - cut intersecting triangles in half
            foreach (var cutTri in cutTris)
            {
                var triangle = new Triangle
                {
                    ids = new[] { meshTriangles[cutTri + 0], meshTriangles[cutTri + 1], meshTriangles[cutTri + 2] },
                    pos = new[] { meshVertices[meshTriangles[cutTri + 0]], meshVertices[meshTriangles[cutTri + 1]], meshVertices[meshTriangles[cutTri + 2]] },
                    normal = useNormals ? new[] { meshNormals[meshTriangles[cutTri + 0]], meshNormals[meshTriangles[cutTri + 1]], meshNormals[meshTriangles[cutTri + 2]] } : new[] { Vector3.zero, Vector3.zero, Vector3.zero },
                    uvs = new[] { meshUV[meshTriangles[cutTri + 0]], meshUV[meshTriangles[cutTri + 1]], meshUV[meshTriangles[cutTri + 2]] },
                    tangents = useMeshTangents ? new[] { meshTangents[meshTriangles[cutTri + 0]], meshTangents[meshTriangles[cutTri + 1]], meshTangents[meshTriangles[cutTri + 2]]} : new []{ Vector4.zero, Vector4.zero, Vector4.zero },
                    colors = useVertexColors ? new[] { meshColors[meshTriangles[cutTri + 0]], meshColors[meshTriangles[cutTri + 1]], meshColors[meshTriangles[cutTri + 2]] } : new Color32[] { Color.white, Color.white, Color.white },
                };

                // check points with a plane
                var side0 = plane.GetSide(triangle.pos[0]);
                var side1 = plane.GetSide(triangle.pos[1]);
                var side2 = plane.GetSide(triangle.pos[2]);

                float t0, t1;
                Vector3 s0 = Vector3.zero, s1 = Vector3.zero;

                var idxLeft = side0 ? 0 : 1;
                var idxRight = 1 - idxLeft;

                if (side0 == side1)
                {
                    var a = plane.IntersectSegment(triangle.pos[2], triangle.pos[0], out t0, ref s0);
                    var b = plane.IntersectSegment(triangle.pos[2], triangle.pos[1], out t1, ref s1);

                    ExploderUtils.Assert(a && b, "!!!!!!!!!!!!!!!");

                    // left side ... 2 triangles
                    var s0Left = AddIntersectionPoint(s0, triangle, triangle.ids[2], triangle.ids[0], cutVertCache[idxLeft], vertices[idxLeft], normals[idxLeft], uvs[idxLeft], tangents[idxLeft], vertexColors[idxLeft], useMeshTangents, useVertexColors, useNormals);
                    var s1Left = AddIntersectionPoint(s1, triangle, triangle.ids[2], triangle.ids[1], cutVertCache[idxLeft], vertices[idxLeft], normals[idxLeft], uvs[idxLeft], tangents[idxLeft], vertexColors[idxLeft], useMeshTangents, useVertexColors, useNormals);
                    var v0Left = AddTrianglePoint(triangle.pos[0], triangle.normal[0], triangle.uvs[0], triangle.tangents[0], triangle.colors[0], triangle.ids[0], triCache, cornerVertCache[idxLeft], vertices[idxLeft], normals[idxLeft], uvs[idxLeft], tangents[idxLeft], vertexColors[idxLeft], useMeshTangents, useVertexColors, useNormals);
                    var v1Left = AddTrianglePoint(triangle.pos[1], triangle.normal[1], triangle.uvs[1], triangle.tangents[1], triangle.colors[1], triangle.ids[1], triCache, cornerVertCache[idxLeft], vertices[idxLeft], normals[idxLeft], uvs[idxLeft], tangents[idxLeft], vertexColors[idxLeft], useMeshTangents, useVertexColors, useNormals);

                    // Triangle (s0, v0, s1)
                    triangles[idxLeft].Add(s0Left);
                    triangles[idxLeft].Add(v0Left);
                    triangles[idxLeft].Add(s1Left);

                    // Triangle (s1, v0, v1)
                    triangles[idxLeft].Add(s1Left);
                    triangles[idxLeft].Add(v0Left);
                    triangles[idxLeft].Add(v1Left);

                    // right side ... 1 triangle
                    var s0Right = AddIntersectionPoint(s0, triangle, triangle.ids[2], triangle.ids[0], cutVertCache[idxRight], vertices[idxRight], normals[idxRight], uvs[idxRight], tangents[idxRight], vertexColors[idxRight], useMeshTangents, useVertexColors, useNormals);
                    var s1Right = AddIntersectionPoint(s1, triangle, triangle.ids[2], triangle.ids[1], cutVertCache[idxRight], vertices[idxRight], normals[idxRight], uvs[idxRight], tangents[idxRight], vertexColors[idxRight], useMeshTangents, useVertexColors, useNormals);
                    var v2Right = AddTrianglePoint(triangle.pos[2], triangle.normal[2], triangle.uvs[2], triangle.tangents[2], triangle.colors[2], triangle.ids[2], triCache, cornerVertCache[idxRight], vertices[idxRight], normals[idxRight], uvs[idxRight], tangents[idxRight], vertexColors[idxRight], useMeshTangents, useVertexColors, useNormals);

                    // Triangle (v2, s0, s1)
                    triangles[idxRight].Add(v2Right);
                    triangles[idxRight].Add(s0Right);
                    triangles[idxRight].Add(s1Right);

                    // buffer intersection vertices for triangulation
                    if (triangulateHoles)
                    {
                        if (idxLeft == 0)
                        {
                            contour.AddTriangle(cutTri, s0Left, s1Left, s0, s1);
                        }
                        else
                        {
                            contour.AddTriangle(cutTri, s0Right, s1Right, s0, s1);
                        }
                    }
                }
                else if (side0 == side2)
                {
                    var a = plane.IntersectSegment(triangle.pos[1], triangle.pos[0], out t0, ref s1);
                    var b = plane.IntersectSegment(triangle.pos[1], triangle.pos[2], out t1, ref s0);

                    ExploderUtils.Assert(a && b, "!!!!!!!!!!!!!");

                    // left side ... 2 triangles
                    var s0Left = AddIntersectionPoint(s0, triangle, triangle.ids[1], triangle.ids[2], cutVertCache[idxLeft], vertices[idxLeft], normals[idxLeft], uvs[idxLeft], tangents[idxLeft], vertexColors[idxLeft], useMeshTangents, useVertexColors, useNormals);
                    var s1Left = AddIntersectionPoint(s1, triangle, triangle.ids[1], triangle.ids[0], cutVertCache[idxLeft], vertices[idxLeft], normals[idxLeft], uvs[idxLeft], tangents[idxLeft], vertexColors[idxLeft], useMeshTangents, useVertexColors, useNormals);
                    var v0Left = AddTrianglePoint(triangle.pos[0], triangle.normal[0], triangle.uvs[0], triangle.tangents[0], triangle.colors[0], triangle.ids[0], triCache, cornerVertCache[idxLeft], vertices[idxLeft], normals[idxLeft], uvs[idxLeft], tangents[idxLeft], vertexColors[idxLeft], useMeshTangents, useVertexColors, useNormals);
                    var v2Left = AddTrianglePoint(triangle.pos[2], triangle.normal[2], triangle.uvs[2], triangle.tangents[2], triangle.colors[2], triangle.ids[2], triCache, cornerVertCache[idxLeft], vertices[idxLeft], normals[idxLeft], uvs[idxLeft], tangents[idxLeft], vertexColors[idxLeft], useMeshTangents, useVertexColors, useNormals);

                    // Triangle (v2, s1, s0)
                    triangles[idxLeft].Add(v2Left);
                    triangles[idxLeft].Add(s1Left);
                    triangles[idxLeft].Add(s0Left);

                    // Triangle (v2, v0, s1)
                    triangles[idxLeft].Add(v2Left);
                    triangles[idxLeft].Add(v0Left);
                    triangles[idxLeft].Add(s1Left);

                    // right side ... 1 triangle
                    var s0Right = AddIntersectionPoint(s0, triangle, triangle.ids[1], triangle.ids[2], cutVertCache[idxRight], vertices[idxRight], normals[idxRight], uvs[idxRight], tangents[idxRight], vertexColors[idxRight], useMeshTangents, useVertexColors, useNormals);
                    var s1Right = AddIntersectionPoint(s1, triangle, triangle.ids[1], triangle.ids[0], cutVertCache[idxRight], vertices[idxRight], normals[idxRight], uvs[idxRight], tangents[idxRight], vertexColors[idxRight], useMeshTangents, useVertexColors, useNormals);
                    var v1Right = AddTrianglePoint(triangle.pos[1], triangle.normal[1], triangle.uvs[1], triangle.tangents[1], triangle.colors[1], triangle.ids[1], triCache, cornerVertCache[idxRight], vertices[idxRight], normals[idxRight], uvs[idxRight], tangents[idxRight], vertexColors[idxRight], useMeshTangents, useVertexColors, useNormals);

                    // Triangle (s0, s1, v1)
                    triangles[idxRight].Add(s0Right);
                    triangles[idxRight].Add(s1Right);
                    triangles[idxRight].Add(v1Right);

                    // buffer intersection vertices for triangulation
                    if (triangulateHoles)
                    {
                        if (idxLeft == 0)
                        {
                            contour.AddTriangle(cutTri, s0Left, s1Left, s0, s1);
                        }
                        else
                        {
                            contour.AddTriangle(cutTri, s0Right, s1Right, s0, s1);
                        }
                    }
                }
                else
                {
                    var a = plane.IntersectSegment(triangle.pos[0], triangle.pos[1], out t0, ref s0);
                    var b = plane.IntersectSegment(triangle.pos[0], triangle.pos[2], out t1, ref s1);

                    ExploderUtils.Assert(a && b, "!!!!!!!!!!!!!");

                    // right side ... 2 triangles
                    var s0Right = AddIntersectionPoint(s0, triangle, triangle.ids[0], triangle.ids[1], cutVertCache[idxRight], vertices[idxRight], normals[idxRight], uvs[idxRight], tangents[idxRight], vertexColors[idxRight], useMeshTangents, useVertexColors, useNormals);
                    var s1Right = AddIntersectionPoint(s1, triangle, triangle.ids[0], triangle.ids[2], cutVertCache[idxRight], vertices[idxRight], normals[idxRight], uvs[idxRight], tangents[idxRight], vertexColors[idxRight], useMeshTangents, useVertexColors, useNormals);
                    var v1Right = AddTrianglePoint(triangle.pos[1], triangle.normal[1], triangle.uvs[1], triangle.tangents[1], triangle.colors[1], triangle.ids[1], triCache, cornerVertCache[idxRight], vertices[idxRight], normals[idxRight], uvs[idxRight], tangents[idxRight], vertexColors[idxRight], useMeshTangents, useVertexColors, useNormals);
                    var v2Right = AddTrianglePoint(triangle.pos[2], triangle.normal[2], triangle.uvs[2], triangle.tangents[2], triangle.colors[2], triangle.ids[2], triCache, cornerVertCache[idxRight], vertices[idxRight], normals[idxRight], uvs[idxRight], tangents[idxRight], vertexColors[idxRight], useMeshTangents, useVertexColors, useNormals);

                    // Triangle (v2, s1, v1)
                    triangles[idxRight].Add(v2Right);
                    triangles[idxRight].Add(s1Right);
                    triangles[idxRight].Add(v1Right);

                    // Triangle (s1, s0, v1)
                    triangles[idxRight].Add(s1Right);
                    triangles[idxRight].Add(s0Right);
                    triangles[idxRight].Add(v1Right);

                    // left side ... 1 triangle
                    var s0Left = AddIntersectionPoint(s0, triangle, triangle.ids[0], triangle.ids[1], cutVertCache[idxLeft], vertices[idxLeft], normals[idxLeft], uvs[idxLeft], tangents[idxLeft], vertexColors[idxLeft], useMeshTangents, useVertexColors, useNormals);
                    var s1Left = AddIntersectionPoint(s1, triangle, triangle.ids[0], triangle.ids[2], cutVertCache[idxLeft], vertices[idxLeft], normals[idxLeft], uvs[idxLeft], tangents[idxLeft], vertexColors[idxLeft], useMeshTangents, useVertexColors, useNormals);
                    var v0Left = AddTrianglePoint(triangle.pos[0], triangle.normal[0], triangle.uvs[0], triangle.tangents[0], triangle.colors[0], triangle.ids[0], triCache, cornerVertCache[idxLeft], vertices[idxLeft], normals[idxLeft], uvs[idxLeft], tangents[idxLeft], vertexColors[idxLeft], useMeshTangents, useVertexColors, useNormals);

                    // Triangle (s1, v0, s0)
                    triangles[idxLeft].Add(s1Left);
                    triangles[idxLeft].Add(v0Left);
                    triangles[idxLeft].Add(s0Left);

                    // buffer intersection vertices for triangulation
                    if (triangulateHoles)
                    {
                        if (idxLeft == 0)
                        {
                            contour.AddTriangle(cutTri, s0Left, s1Left, s0, s1);
                        }
                        else
                        {
                            contour.AddTriangle(cutTri, s0Right, s1Right, s0, s1);
                        }
                    }
                }
            }

#if PROFILING
            MeasureIt.End("CutCycleSecondPass");
#endif

            if (triangulateHoles)
            {
#if PROFILING
                MeasureIt.Begin("FindContours");
#endif

                contour.FindContours();

                if (contour.contour.Count == 0 || contour.contour[0].Count < 3)
                {
                    stopWatch.Stop();
                    return stopWatch.ElapsedMilliseconds;
                }

#if PROFILING
                MeasureIt.End("FindContours");
#endif
            }

            List<int>[] trianglesCut = null;

            if (triangulateHoles)
            {
#if PROFILING
                MeasureIt.Begin("Triangulate");
#endif

                trianglesCut = new List<int>[2]
                    {new List<int>(contour.MidPointsCount), new List<int>(contour.MidPointsCount)};
                Triangulate(contour.contour, plane, vertices, normals, uvs, tangents, vertexColors, trianglesCut, true,
                            useMeshTangents, useVertexColors, useNormals);

#if PROFILING
                MeasureIt.End("Triangulate");
#endif
            }

            if (vertices[0].Count > 3 && vertices[1].Count > 3)
            {
#if PROFILING
                MeasureIt.Begin("CutEndCopyBack");
#endif

                mesh0.mesh = new Mesh();
                mesh1.mesh = new Mesh();

                var verticesArray0 = vertices[0].ToArray();
                var verticesArray1 = vertices[1].ToArray();

                mesh0.mesh.vertices = verticesArray0;
                mesh0.mesh.uv = uvs[0].ToArray();

                mesh1.mesh.vertices = verticesArray1;
                mesh1.mesh.uv = uvs[1].ToArray();

                if (useNormals)
                {
                    mesh0.mesh.normals = normals[0].ToArray();
                    mesh1.mesh.normals = normals[1].ToArray();
                }

                if (useMeshTangents)
                {
                    mesh0.mesh.tangents = tangents[0].ToArray();
                    mesh1.mesh.tangents = tangents[1].ToArray();
                }

                if (useVertexColors)
                {
                    mesh0.mesh.colors32 = vertexColors[0].ToArray();
                    mesh1.mesh.colors32 = vertexColors[1].ToArray();
                }

                if (trianglesCut != null && trianglesCut[0].Count > 3)
                {
                    triangles[0].AddRange(trianglesCut[0]);
                    triangles[1].AddRange(trianglesCut[1]);
                }

                mesh0.mesh.triangles = triangles[0].ToArray();
                mesh1.mesh.triangles = triangles[1].ToArray();

                if (!triangulateHoles)
                {
                    // don't triangulate holes means the mesh is a 2d plane
                    // it is better to recalculate centroid to get precise center not just an approximation

                    mesh0.centroid = Vector3.zero;
                    mesh1.centroid = Vector3.zero;

                    foreach (var p in vertices[0])
                    {
                        mesh0.centroid += p;
                    }
                    mesh0.centroid /= vertices[0].Count;

                    foreach (var p in vertices[1])
                    {
                        mesh1.centroid += p;
                    }
                    mesh1.centroid /= vertices[1].Count;
                }

#if PROFILING
                MeasureIt.End("CutEndCopyBack");
#endif

                meshes = new List<CutterMesh> { mesh0, mesh1 };

                stopWatch.Stop();
                return stopWatch.ElapsedMilliseconds;
            }

            stopWatch.Stop();

//            UnityEngine.Debug.Log("Empty cut! " + vertices[0].Count + " " + vertices[1].Count);

            return stopWatch.ElapsedMilliseconds;
        }

        struct Triangle
        {
            public int[] ids;
            public Vector3[] pos;
            public Vector3[] normal;
            public Vector2[] uvs;
            public Vector4[] tangents;
            public Color32[] colors;
        }

        int AddIntersectionPoint(Vector3 pos, Triangle tri, int edge0, int edge1, Dictionary<long, int> cache, List<Vector3> vertices, List<Vector3> normals, List<Vector2> uvs, List<Vector4> tangents, List<Color32> colors32, bool useTangents, bool useColors, bool useNormals)
        {
            //! TODO: figure out position hash for shared vertices
//            var key = pos.GetHashCode();
            var key = edge0 < edge1 ? (edge0 << 16) + edge1 : (edge1 << 16) + edge0;

            int result;
            if (cache.TryGetValue(key, out result))
            {
                // cache hit!
                return result;
            }

            // compute barycentric coordinates for a new point to interpolate normal and uv
            var baryCoord = MeshUtils.ComputeBarycentricCoordinates(tri.pos[0], tri.pos[1], tri.pos[2], pos);

            vertices.Add(pos);

            if (useNormals)
            {
                normals.Add(new Vector3(baryCoord.x * tri.normal[0].x + baryCoord.y * tri.normal[1].x + baryCoord.z * tri.normal[2].x,
                                        baryCoord.x * tri.normal[0].y + baryCoord.y * tri.normal[1].y + baryCoord.z * tri.normal[2].y,
                                        baryCoord.x * tri.normal[0].z + baryCoord.y * tri.normal[1].z + baryCoord.z * tri.normal[2].z));
            }

            uvs.Add(new Vector2(baryCoord.x * tri.uvs[0].x + baryCoord.y * tri.uvs[1].x + baryCoord.z * tri.uvs[2].x,
                                baryCoord.x * tri.uvs[0].y + baryCoord.y * tri.uvs[1].y + baryCoord.z * tri.uvs[2].y));

            if (useTangents)
            {
                tangents.Add(new Vector4(baryCoord.x * tri.tangents[0].x + baryCoord.y * tri.tangents[1].x + baryCoord.z * tri.tangents[2].x,
                                         baryCoord.x * tri.tangents[0].y + baryCoord.y * tri.tangents[1].y + baryCoord.z * tri.tangents[2].y,
                                         baryCoord.x * tri.tangents[0].z + baryCoord.y * tri.tangents[1].z + baryCoord.z * tri.tangents[2].z,
                                         baryCoord.x * tri.tangents[0].w + baryCoord.y * tri.tangents[1].w + baryCoord.z * tri.tangents[2].z));
            }

            if (useColors)
            {
                // by default copy color from first triangle vertex
                colors32.Add(tri.colors[0]);
            }

            var vertIndex = vertices.Count - 1;

            cache.Add(key, vertIndex);

            return vertIndex;
        }

        int AddTrianglePoint(Vector3 pos, Vector3 normal, Vector2 uv, Vector4 tangent, Color32 color, int idx, 
                             int[] triCache, Dictionary<int, int> cache, List<Vector3> vertices, List<Vector3> normals, 
                             List<Vector2> uvs, List<Vector4> tangents, List<Color32> colors, bool useTangents, bool useColors, bool useNormals)
        {
            // tricache
            if (triCache[idx] != 0)
            {
                // cache hit!
                return triCache[idx] - 1;
            }

            // second cache
            int result;
            if (cache.TryGetValue(idx, out result))
            {
                // cache hit!
                return result;
            }

            vertices.Add(pos);

            if (useNormals)
            {
                normals.Add(normal);
            }

            uvs.Add(uv);

            if (useTangents)
            {
                tangents.Add(tangent);
            }

            if (useColors)
            {
                colors.Add(color);
            }

            var vertIndex = vertices.Count - 1;

            cache.Add(idx, vertIndex);

            return vertIndex;
        }

        void Triangulate(List<Dictionary<int, int>> contours, Math.Plane plane, List<Vector3>[] vertices, List<Vector3>[] normals, List<Vector2>[] uvs, 
                         List<Vector4>[] tangents, List<Color32>[] colors, List<int>[] triangles, bool uvCutMesh, bool useTangents, bool useColors, bool useNormals)
        {
            if (contours.Count == 0 || contours[0].Count < 3)
            {
//                Utils.Log("Contour empty!: ");
                return;
            }

            // prepare plane matrix
            var m = plane.GetPlaneMatrix();
            var mInv = m.inverse;

            var zShit = 0.0f;

            var polygons = new List<Polygon>(contours.Count);

            // construct polygons from contours
            Polygon highAreaPoly = null;
            foreach (var ctr in contours)
            {
                var polygonPoints = new Vector2[ctr.Count];
                var j = 0;

                foreach (var i in ctr.Values)
                {
                    var p = mInv*vertices[0][i];
                    polygonPoints[j++] = p;

                    // save z-coordinate
                    zShit = p.z;
                }

                var polygon = new Polygon(polygonPoints);
                polygons.Add(polygon);

                if (highAreaPoly == null || Mathf.Abs(highAreaPoly.Area) < Mathf.Abs(polygon.Area))
                {
                    highAreaPoly = polygon;
                }
            }

            ExploderUtils.Assert(polygons.Count > 0, "Zero polygons!");

            // test for holes
            if (polygons.Count > 0)
            {
                var polyToRemove = new List<Polygon>();

                foreach (var polygon in polygons)
                {
                    if (polygon != highAreaPoly)
                    {
                        if (highAreaPoly.IsPointInside(polygon.Points[0]))
                        {
                            highAreaPoly.AddHole(polygon);
                            polyToRemove.Add(polygon);
                        }
                    }
                }

                foreach (var polygon in polyToRemove)
                {
                    polygons.Remove(polygon);
                }
            }

            var vertCounter0 = vertices[0].Count;
            var vertCounter1 = vertices[1].Count;

            foreach (var polygon in polygons)
            {
                var indices = polygon.Triangulate();
                if (indices == null)
                {
                    continue;
                }

                // get polygon bounding square size
                var min = Mathf.Min(polygon.Min.x, polygon.Min.y);
                var max = Mathf.Max(polygon.Max.x, polygon.Max.y);
                var polygonSize = max - min;

//                Utils.Log("PolygonSize: " + polygonSize + " " + polygon.Min + " " + polygon.Max);

//                Utils.Log("Triangulate polygons: " + polygon.Points.Length);

                foreach (var polyPoint in polygon.Points)
                {
                    var p = m * new Vector3(polyPoint.x, polyPoint.y, zShit);

                    vertices[0].Add(p);
                    vertices[1].Add(p);

                    if (useNormals)
                    {
                        normals[0].Add(-plane.Normal);
                        normals[1].Add(plane.Normal);
                    }

                    if (uvCutMesh)
                    {
                        var uv0 = new Vector2((polyPoint.x - min)/polygonSize,
                                              (polyPoint.y - min)/polygonSize);
                        var uv1 = new Vector2((polyPoint.x - min)/polygonSize,
                                              (polyPoint.y - min)/polygonSize);

                        // normalize uv to fit cross-section uv area
                        var areaSizeX = crossSectionUV.z - crossSectionUV.x;
                        var areaSizeY = crossSectionUV.w - crossSectionUV.y;

                        uv0.x = crossSectionUV.x + uv0.x * areaSizeX;
                        uv0.y = crossSectionUV.y + uv0.y * areaSizeY;
                        uv1.x = crossSectionUV.x + uv1.x * areaSizeX;
                        uv1.y = crossSectionUV.y + uv1.y * areaSizeY;

                        uvs[0].Add(uv0);
                        uvs[1].Add(uv1);
                    }
                    else
                    {
                        uvs[0].Add(Vector2.zero);
                        uvs[1].Add(Vector2.zero);
                    }

                    if (useTangents)
                    {
                        // fast and dirty way to create tangents
                        var v0 = plane.Normal;
                        MeshUtils.Swap(ref v0.x, ref v0.y);
                        MeshUtils.Swap(ref v0.y, ref v0.z);

                        Vector4 tangent = Vector3.Cross(plane.Normal, v0);
                        tangent.w = 1.0f;

                        tangents[0].Add(tangent);

                        tangent.w = -1.0f;
                        tangents[1].Add(tangent);
                    }

                    if (useColors)
                    {
                        colors[0].Add(crossSectionVertexColour);
                        colors[1].Add(crossSectionVertexColour);
                    }
                }

                var indicesCount = indices.Count;
                var j = indicesCount - 1;
                for (int i = 0; i < indicesCount; i++)
                {
                    triangles[0].Add(vertCounter0 + indices[i]);
                    triangles[1].Add(vertCounter1 + indices[j]);
                    j--;
                }

                vertCounter0 += polygon.Points.Length;
                vertCounter1 += polygon.Points.Length;
            }
        }
    }
}
