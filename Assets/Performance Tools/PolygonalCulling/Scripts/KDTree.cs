using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NGS.PolygonalCulling
{
    public class KDTree
    {
        public KDTreeNode rootNode { get; private set; }
        public List<KDTreeNode> leafs { get; private set; }
        public MeshFilter[] srcFilters { get; private set; }
        public int maxStack { get; private set; }

        public void CreateTree(MeshFilter[] filters, int maxStack, int minTrianglesCount)
        {
            if (filters.Length == 0)
                throw new Exception("No filter's found");

            srcFilters = filters;

            this.maxStack = maxStack;

            Vector3 min, max;

            Triangle[] triangles = GetTriangles(srcFilters, out min, out max);

            Vector3 center = (max + min) / 2;
            Vector3 size = (max - min);

            rootNode = new KDTreeNode(center, size);
            rootNode.triangles = triangles;

            leafs = new List<KDTreeNode>();

            ComputeNode(rootNode, 1, maxStack, minTrianglesCount);
        }


        private Triangle[] GetTriangles(MeshFilter[] filters, out Vector3 AABBmin, out Vector3 AABBmax)
        {
            List<Triangle> triangles = new List<Triangle>();

            AABBmin = new Vector3(Mathf.Infinity, Mathf.Infinity, Mathf.Infinity);
            AABBmax = new Vector3(Mathf.NegativeInfinity, Mathf.NegativeInfinity, Mathf.NegativeInfinity);

            for (int i = 0; i < filters.Length; i++)
            {
                Mesh mesh = filters[i].sharedMesh;
                Vector3[] vertices = mesh.vertices;

                for (int c = 0; c < vertices.Length; c++)
                    vertices[c] = filters[i].transform.TransformPoint(vertices[c]);

                for (int c = 0; c < mesh.subMeshCount; c++)
                {
                    int[] meshTriangles = mesh.GetTriangles(c);

                    for (int p = 0; p < meshTriangles.Length; p += 3)
                    {
                        Vector3 vertex1 = vertices[meshTriangles[p]];
                        Vector3 vertex2 = vertices[meshTriangles[p + 1]];
                        Vector3 vertex3 = vertices[meshTriangles[p + 2]];

                        Triangle triangle = new Triangle(vertex1, vertex2, vertex3, i, c, meshTriangles[p], meshTriangles[p + 1], meshTriangles[p + 2]);
                        triangles.Add(triangle);

                        AABBmin.x = Mathf.Min(triangle.vertex1.x, triangle.vertex2.x, triangle.vertex3.x, AABBmin.x);
                        AABBmin.y = Mathf.Min(triangle.vertex1.y, triangle.vertex2.y, triangle.vertex3.y, AABBmin.y);
                        AABBmin.z = Mathf.Min(triangle.vertex1.z, triangle.vertex2.z, triangle.vertex3.z, AABBmin.z);

                        AABBmax.x = Mathf.Max(triangle.vertex1.x, triangle.vertex2.x, triangle.vertex3.x, AABBmax.x);
                        AABBmax.y = Mathf.Max(triangle.vertex1.y, triangle.vertex2.y, triangle.vertex3.y, AABBmax.y);
                        AABBmax.z = Mathf.Max(triangle.vertex1.z, triangle.vertex2.z, triangle.vertex3.z, AABBmax.z);
                    }
                }
            }

            return triangles.ToArray();
        }

        private void ComputeNode(KDTreeNode parent, int currentStack, int maxStack, int minTrianglesCount)
        {
            if (currentStack >= maxStack || parent.triangles.Length <= minTrianglesCount)
            {
                leafs.Add(parent);
                return;
            }

            Vector3 size = SplitNode(parent);

            KDTreeNode left = new KDTreeNode(parent.min + size / 2, size);
            KDTreeNode right = new KDTreeNode(parent.max - size / 2, size);

            Triangle[] leftSideTriangles, rightSideTriangles;

            SetTriangles(parent, left.bounds, right.bounds, out leftSideTriangles, out rightSideTriangles);

            left.triangles = leftSideTriangles;
            right.triangles = rightSideTriangles;

            parent.SetChilds(left, right);

            ComputeNode(parent.left, currentStack + 1, maxStack, minTrianglesCount);
            ComputeNode(parent.right, currentStack + 1, maxStack, minTrianglesCount);
        }

        private Vector3 SplitNode(KDTreeNode node)
        {
            Vector3 size;

            if (node.size.x >= node.size.y && node.size.x >= node.size.z)
                size = new Vector3(node.size.x / 2, node.size.y, node.size.z);

            else if (node.size.y >= node.size.x && node.size.y >= node.size.z)
                size = new Vector3(node.size.x, node.size.y / 2, node.size.z);

            else
                size = new Vector3(node.size.x, node.size.y, node.size.z / 2);

            return size;
        }

        private void SetTriangles(KDTreeNode parent, Bounds bound1, Bounds bound2, out Triangle[] leftSideTriangles, out Triangle[] rightSideTriangles)
        {
            List<Triangle> leftTriangles = new List<Triangle>();
            List<Triangle> rightTriangles = new List<Triangle>();

            for (int i = 0; i < parent.triangles.Length; i++)
            {
                Vector3 vertex1 = parent.triangles[i].vertex1;
                Vector3 vertex2 = parent.triangles[i].vertex2;
                Vector3 vertex3 = parent.triangles[i].vertex3;

                if (AdvancedMath.IsTriangleIntersectNode(bound1, vertex1, vertex2, vertex3))
                    leftTriangles.Add(parent.triangles[i]);

                else
                    rightTriangles.Add(parent.triangles[i]);
            }

            leftSideTriangles = leftTriangles.ToArray();
            rightSideTriangles = rightTriangles.ToArray();
        }
    }

    public class KDTreeNode
    {
        public KDTreeNode left { get; private set; }
        public KDTreeNode right { get; private set; }

        private Triangle[] _triangles = null;
        public Triangle[] triangles
        {
            get
            {
                return _triangles;
            }

            set
            {
                _triangles = value.Distinct().ToArray();
            }
        }

        public Vector3 center { get; private set; }
        public Vector3 size { get; private set; }

        public Bounds bounds
        {
            get
            {
                return new Bounds(center, size);
            }
        }
        public Vector3 min
        {
            get
            {
                return center - size / 2;
            }
        }
        public Vector3 max
        {
            get
            {
                return center + size / 2;
            }
        }


        public KDTreeNode(Vector3 center, Vector3 size)
        {
            this.center = center;

            this.size = size;
        }

        public void SetChilds(KDTreeNode left, KDTreeNode right)
        {
            this.left = left;

            this.right = right;
        }
    }

    public class Triangle
    {
        public Vector3 vertex1 { get; private set; }
        public Vector3 vertex2 { get; private set; }
        public Vector3 vertex3 { get; private set; }

        public int meshFilterIndex { get; private set; }
        public int subMeshIndex { get; private set; }

        public int triangle1 { get; private set; }
        public int triangle2 { get; private set; }
        public int triangle3 { get; private set; }

        public Triangle(Vector3 vertex1, Vector3 vertex2, Vector3 vertex3, int meshFilterIndex, int subMeshIndex, int triangle1, int triangle2, int triangle3)
        {
            this.vertex1 = vertex1;
            this.vertex2 = vertex2;
            this.vertex3 = vertex3;

            this.meshFilterIndex = meshFilterIndex;
            this.subMeshIndex = subMeshIndex;

            this.triangle1 = triangle1;
            this.triangle2 = triangle2;
            this.triangle3 = triangle3;
        }
    }

    public class AdvancedMath
    {
        public static bool IsSomeTriangleIntersectNode(Bounds node, Triangle[] triangles)
        {
            for (int i = 0; i < triangles.Length; i++)
                if (IsTriangleIntersectNode(node, triangles[i].vertex1, triangles[i].vertex2, triangles[i].vertex3))
                    return true;

            return false;
        }

        public static bool IsTriangleIntersectNode(Bounds node, Vector3 vertex1, Vector3 vertex2, Vector3 vertex3)
        {
            if (node.center.x + node.size.x / 2 < Mathf.Min(vertex1.x, vertex2.x, vertex3.x))
                return false;

            if (node.center.x - node.size.x / 2 > Mathf.Max(vertex1.x, vertex2.x, vertex3.x))
                return false;

            if (node.center.y + node.size.y / 2 < Mathf.Min(vertex1.y, vertex2.y, vertex3.y))
                return false;

            if (node.center.y - node.size.y / 2 > Mathf.Max(vertex1.y, vertex2.y, vertex3.y))
                return false;

            if (node.center.z + node.size.z / 2 < Mathf.Min(vertex1.z, vertex2.z, vertex3.z))
                return false;

            if (node.center.z - node.size.z / 2 > Mathf.Max(vertex1.z, vertex2.z, vertex3.z))
                return false;

            if (IsPointInNode(node, vertex1)) return true;
            if (IsPointInNode(node, vertex2)) return true;
            if (IsPointInNode(node, vertex3)) return true;

            if (IsTriangleBoxOverlapping
                (
                new float[3] { node.center.x, node.center.y, node.center.z },
                new float[3] { node.size.x / 2, node.size.y / 2, node.size.z / 2 },
                new float[3][]
                {
                    new float[3] { vertex1.x, vertex1.y, vertex1.z },
                    new float[3] { vertex2.x, vertex2.y, vertex2.z },
                    new float[3] { vertex3.x, vertex3.y, vertex3.z }
                }
                ))
                return true;

            return false;
        }

        public static bool IsPointInNode(Bounds node, Vector3 vertex)
        {
            if (vertex.x > node.min.x || vertex.x.IsEqual(node.min.x))
                if (vertex.x < node.max.x || vertex.x.IsEqual(node.max.x))
                    if (vertex.y > node.min.y || vertex.y.IsEqual(node.min.y))
                        if (vertex.y < node.max.y || vertex.y.IsEqual(node.max.y))
                            if (vertex.z > node.min.z || vertex.z.IsEqual(node.min.z))
                                if (vertex.z < node.max.z || vertex.z.IsEqual(node.max.z))
                                    return true;

            return false;
        }

        private static bool IsTriangleBoxOverlapping(float[] boxcenter, float[] boxhalfsize, float[][] triverts)
        {
            float[] v0 = new float[3], v1 = new float[3], v2 = new float[3];

            float min, max, p0, p1, p2, rad, fex, fey, fez;

            float[] normal = new float[3], e0 = new float[3], e1 = new float[3], e2 = new float[3];

            SUB(ref v0, triverts[0], boxcenter);
            SUB(ref v1, triverts[1], boxcenter);
            SUB(ref v2, triverts[2], boxcenter);

            SUB(ref e0, v1, v0);
            SUB(ref e1, v2, v1);
            SUB(ref e2, v0, v2);

            fex = Math.Abs(e0[0]);
            fey = Math.Abs(e0[1]);
            fez = Math.Abs(e0[2]);

            p0 = e0[2] * v0[1] - e0[1] * v0[2];
            p2 = e0[2] * v2[1] - e0[1] * v2[2];
            if (p0 < p2) { min = p0; max = p2; } else { min = p2; max = p0; }
            rad = fez * boxhalfsize[1] + fey * boxhalfsize[2];
            if (min > rad || max < -rad) return false;

            p0 = -e0[2] * v0[0] + e0[0] * v0[2];
            p2 = -e0[2] * v2[0] + e0[0] * v2[2];
            if (p0 < p2) { min = p0; max = p2; } else { min = p2; max = p0; }
            rad = fez * boxhalfsize[0] + fex * boxhalfsize[2];
            if (min > rad || max < -rad) return false;

            p1 = e0[1] * v1[0] - e0[0] * v1[1];
            p2 = e0[1] * v2[0] - e0[0] * v2[1];
            if (p2 < p1) { min = p2; max = p1; } else { min = p1; max = p2; }
            rad = fey * boxhalfsize[0] + fex * boxhalfsize[1];
            if (min > rad || max < -rad) return false;

            fex = Math.Abs(e1[0]);
            fey = Math.Abs(e1[1]);
            fez = Math.Abs(e1[2]);

            p0 = e1[2] * v0[1] - e1[1] * v0[2];
            p2 = e1[2] * v2[1] - e1[1] * v2[2];
            if (p0 < p2) { min = p0; max = p2; } else { min = p2; max = p0; }
            rad = fez * boxhalfsize[1] + fey * boxhalfsize[2];
            if (min > rad || max < -rad) return false;

            p0 = -e1[2] * v0[0] + e1[0] * v0[2];
            p2 = -e1[2] * v2[0] + e1[0] * v2[2];
            if (p0 < p2) { min = p0; max = p2; } else { min = p2; max = p0; }
            rad = fez * boxhalfsize[0] + fex * boxhalfsize[2];
            if (min > rad || max < -rad) return false;

            p0 = e1[1] * v0[0] - e1[0] * v0[1];
            p1 = e1[1] * v1[0] - e1[0] * v1[1];
            if (p0 < p1) { min = p0; max = p1; } else { min = p1; max = p0; }
            rad = fey * boxhalfsize[0] + fex * boxhalfsize[1];
            if (min > rad || max < -rad) return false;

            fex = Math.Abs(e2[0]);
            fey = Math.Abs(e2[1]);
            fez = Math.Abs(e2[2]);

            p0 = e2[2] * v0[1] - e2[1] * v0[2];
            p1 = e2[2] * v1[1] - e2[1] * v1[2];
            if (p0 < p1) { min = p0; max = p1; } else { min = p1; max = p0; }
            rad = fez * boxhalfsize[1] + fey * boxhalfsize[2];
            if (min > rad || max < -rad) return false;

            p0 = -e2[2] * v0[0] + e2[0] * v0[2];
            p1 = -e2[2] * v1[0] + e2[0] * v1[2];
            if (p0 < p1) { min = p0; max = p1; } else { min = p1; max = p0; }
            rad = fez * boxhalfsize[0] + fex * boxhalfsize[2];
            if (min > rad || max < -rad) return false;

            p1 = e2[1] * v1[0] - e2[0] * v1[1];
            p2 = e2[1] * v2[0] - e2[0] * v2[1];
            if (p2 < p1) { min = p2; max = p1; } else { min = p1; max = p2; }
            rad = fey * boxhalfsize[0] + fex * boxhalfsize[1];
            if (min > rad || max < -rad) return false;

            min = Mathf.Min(v0[0], v1[0], v2[0]);
            max = Mathf.Max(v0[0], v1[0], v2[0]);

            if (min > boxhalfsize[0] || max < -boxhalfsize[0]) return false;

            min = Mathf.Min(v0[1], v1[1], v2[1]);
            max = Mathf.Max(v0[1], v1[1], v2[1]);

            if (min > boxhalfsize[1] || max < -boxhalfsize[1]) return false;

            min = Mathf.Min(v0[2], v1[2], v2[2]);
            max = Mathf.Max(v0[2], v1[2], v2[2]);

            if (min > boxhalfsize[2] || max < -boxhalfsize[2]) return false;

            CROSS(ref normal, e0, e1);

            if (!IsPlaneBoxOverlap(normal, v0, boxhalfsize)) return false;

            return true;
        }

        private static bool IsPlaneBoxOverlap(float[] normal, float[] vert, float[] maxbox)
        {
            int q;

            float[] vmin = new float[3], vmax = new float[3];
            float v;

            for (q = 0; q <= 2; q++)
            {
                v = vert[q];

                if (normal[q] > 0.0f)
                {
                    vmin[q] = -maxbox[q] - v;
                    vmax[q] = maxbox[q] - v;
                }
                else
                {
                    vmin[q] = maxbox[q] - v;
                    vmax[q] = -maxbox[q] - v;
                }

            }

            if (DOT(normal, vmin) > 0.0f) return false;
            if (DOT(normal, vmax) >= 0.0f) return true;

            return false;
        }


        public static void CROSS(ref float[] dest, float[] v1, float[] v2)
        {
            dest[0] = v1[1] * v2[2] - v1[2] * v2[1];
            dest[1] = v1[2] * v2[0] - v1[0] * v2[2];
            dest[2] = v1[0] * v2[1] - v1[1] * v2[0];
        }

        public static void CROSS(ref Vector3 dest, Vector3 v1, Vector3 v2)
        {
            dest[0] = v1[1] * v2[2] - v1[2] * v2[1];
            dest[1] = v1[2] * v2[0] - v1[0] * v2[2];
            dest[2] = v1[0] * v2[1] - v1[1] * v2[0];
        }


        public static void SUB(ref float[] dest, float[] v1, float[] v2)
        {
            dest[0] = v1[0] - v2[0];
            dest[1] = v1[1] - v2[1];
            dest[2] = v1[2] - v2[2];
        }

        public static void SUB(ref Vector3 dest, Vector3 v1, Vector3 v2)
        {
            dest[0] = v1[0] - v2[0];
            dest[1] = v1[1] - v2[1];
            dest[2] = v1[2] - v2[2];
        }


        public static float DOT(float[] v1, float[] v2)
        {
            return v1[0] * v2[0] + v1[1] * v2[1] + v1[2] * v2[2];
        }

        public static float DOT(Vector3 v1, Vector3 v2)
        {
            return v1[0] * v2[0] + v1[1] * v2[1] + v1[2] * v2[2];
        }
    }
}
