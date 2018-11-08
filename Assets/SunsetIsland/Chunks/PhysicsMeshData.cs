using System.Collections.Generic;
using UnityEngine;

namespace Assets.SunsetIsland.Chunks
{
    public class PhysicsMeshData
    {
        public PhysicsMeshData()
        {
            Verticies = new List<Vector3>();
            Indexes = new List<int>();
        }

        public List<Vector3> Verticies { get; }
        public List<int> Indexes { get; }

        public void Clear()
        {
            Verticies.Clear();
            Indexes.Clear();
        }

        public void AddQuad(Vector3 topLeft, Vector3 topRight, Vector3 bottomRight, Vector3 bottomLeft, bool backFace)
        {
            var index = Verticies.Count;

            Verticies.Add(topLeft);
            Verticies.Add(topRight);
            Verticies.Add(bottomRight);
            Verticies.Add(bottomLeft);

            if (backFace)
            {
                Indexes.Add(index);
                Indexes.Add(index + 1);
                Indexes.Add(index + 2);
                Indexes.Add(index + 2);
                Indexes.Add(index + 3);
                Indexes.Add(index);
            }
            else
            {
                Indexes.Add(index);
                Indexes.Add(index + 3);
                Indexes.Add(index + 2);
                Indexes.Add(index + 2);
                Indexes.Add(index + 1);
                Indexes.Add(index);
            }
        }

        public void UpdateMesh(Mesh mesh)
        {
            if(mesh == null)
                return;
            mesh.Clear();
            mesh.SetVertices(Verticies);
            mesh.SetTriangles(Indexes, 0);
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
        }
    }
}