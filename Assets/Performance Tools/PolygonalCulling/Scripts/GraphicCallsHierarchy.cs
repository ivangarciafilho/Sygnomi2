using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Linq;

namespace NGS.PolygonalCulling
{
    [Serializable]
    public class GraphicCallsHierarchy
    {
        private GraphicCallsNode _topNode = null;
        public GraphicCallsNode topNode
        {
            get
            {
                return _topNode;
            }
        }

        private List<GraphicCallsNode> _leafs = new List<GraphicCallsNode>();
        public List<GraphicCallsNode> leafs
        {
            get
            {
                return _leafs;
            }
        }


        public void CreateHierarchy(MeshFilter[] filters, float minNodeSize)
        {
            Vector3 center, size;

            GetAABBInfo(filters, out center, out size);

            CreateHierarchy(center, size, minNodeSize);
        }

        public void CreateHierarchy(Vector3 center, Vector3 size, float minNodeSize)
        {
            _topNode = new GraphicCallsNode(center, size);

            ComputeNode(_topNode, minNodeSize, 1);
        }

        public GraphicCallsNode GetNodeByPoint(Vector3 point)
        {
            return GetNodeByPoint(_topNode, point);
        }

        public GraphicCallsNode GetNodeByPoint(GraphicCallsNode callsNode, Vector3 point)
        {
            if (callsNode.left == null)
                return callsNode;

            if (AdvancedMath.IsPointInNode(callsNode.left.bounds, point))
                return GetNodeByPoint(callsNode.left, point);

            if (AdvancedMath.IsPointInNode(callsNode.right.bounds, point))
                return GetNodeByPoint(callsNode.right, point);

            return null;
        }


        private void GetAABBInfo(MeshFilter[] filters, out Vector3 center, out Vector3 size)
        {
            Vector3 min = new Vector3(Mathf.Infinity, Mathf.Infinity, Mathf.Infinity);
            Vector3 max = new Vector3(-Mathf.Infinity, -Mathf.Infinity, -Mathf.Infinity);

            for (int i = 0; i < filters.Length; i++)
            {
                Mesh mesh = filters[i].sharedMesh;

                for (int c = 0; c < mesh.subMeshCount; c++)
                {
                    Vector3[] vertices = mesh.vertices;
                    int[] _triangles = mesh.GetTriangles(c);

                    for (int p = 0; p < _triangles.Length; p += 3)
                    {
                        Vector3 vertex1 = filters[i].transform.TransformPoint(vertices[_triangles[p]]);
                        Vector3 vertex2 = filters[i].transform.TransformPoint(vertices[_triangles[p + 1]]);
                        Vector3 vertex3 = filters[i].transform.TransformPoint(vertices[_triangles[p + 2]]);

                        min.x = Mathf.Min(vertex1.x, vertex2.x, vertex3.x, min.x);
                        min.y = Mathf.Min(vertex1.y, vertex2.y, vertex3.y, min.y);
                        min.z = Mathf.Min(vertex1.z, vertex2.z, vertex3.z, min.z);

                        max.x = Mathf.Max(vertex1.x, vertex2.x, vertex3.x, max.x);
                        max.y = Mathf.Max(vertex1.y, vertex2.y, vertex3.y, max.y);
                        max.z = Mathf.Max(vertex1.z, vertex2.z, vertex3.z, max.z);
                    }
                }
            }

            center = (min + max) / 2;
            size = (max - min);
        }

        private void ComputeNode(GraphicCallsNode parent, float minNodeSize, int stackIndex)
        {
            if (parent.size.x <= minNodeSize)
                if (parent.size.y <= minNodeSize)
                    if (parent.size.z <= minNodeSize)
                    {
                        _leafs.Add(parent);
                        return;
                    }

            Vector3 size = SplitNode(parent);

            GraphicCallsNode left = new GraphicCallsNode(parent.min + size / 2, size);
            GraphicCallsNode right = new GraphicCallsNode(parent.max - size / 2, size);

            parent.SetChilds(left, right);

            ComputeNode(left, minNodeSize, stackIndex + 1);
            ComputeNode(right, minNodeSize, stackIndex + 1);
        }

        private Vector3 SplitNode(GraphicCallsNode node)
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
    }

    [Serializable]
    public class GraphicCallsNode
    {
        private GraphicCallsNode _left;
        public GraphicCallsNode left
        {
            get
            {
                return _left;
            }
        }

        private GraphicCallsNode _right;
        public GraphicCallsNode right
        {
            get
            {
                return _right;
            }
        }

        private SVector3 _center;
        public Vector3 center
        {
            get
            {
                return _center;
            }
        }

        private SVector3 _size;
        public Vector3 size
        {
            get
            {
                return _size;
            }
        }

        private List<int> _renderersIndexes = new List<int>();
        public List<int> renderersIndexes
        {
            get
            {
                return _renderersIndexes;
            }
        }

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


        public GraphicCallsNode()
        {
            _center = Vector3.zero;

            _size = Vector3.one;
        }

        public GraphicCallsNode(Vector3 center, Vector3 size)
        {
            _center = center;

            _size = size;
        }

        public void SetChilds(GraphicCallsNode left, GraphicCallsNode right)
        {
            _left = left;

            _right = right;
        }

        public void AddRenderer(int index)
        {
            _renderersIndexes.DistinctAdd(index);
        }

        public void AddRenderers(IEnumerable<int> indexes)
        {
            foreach (var index in indexes)
                AddRenderer(index);
        }
    }

    [Serializable]
    public struct SVector3 : ISerializable
    {
        public Vector3 vector;

        public SVector3(Vector3 vector)
        {
            this.vector = vector;
        }

        private SVector3(SerializationInfo info, StreamingContext context)
        {
            vector = Vector3.zero;

            vector.x = info.GetSingle("x");

            vector.y = info.GetSingle("y");

            vector.z = info.GetSingle("z");
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("x", vector.x);

            info.AddValue("y", vector.y);

            info.AddValue("z", vector.z);
        }

        public static implicit operator Vector3(SVector3 sVector)
        {
            return sVector.vector;
        }

        public static implicit operator SVector3(Vector3 vector)
        {
            return new SVector3(vector);
        }
    }
}

