using System;
using System.Collections.Generic;
using Assets.SunsetIsland.Blocks;
using Assets.SunsetIsland.Common;
using UnityEngine;

namespace Assets.SunsetIsland.Chunks
{
    //TODO: Implement POP Buffers for LOD
    public class RenderMeshData
    {
        public RenderMeshData()
        {
            _verticies = new List<Vector3>();
            _colors = new List<Color32>();
            _uvs = new List<Vector3>();
            _lights = new List<Vector2>();
            _indexes = new List<int>();
        }

        private readonly List<Vector3> _verticies;
        private readonly List<Color32> _colors;
        private readonly List<Vector3> _uvs;
        private readonly List<Vector2> _lights;
        private readonly List<int> _indexes;

        public void Clear()
        {
            _verticies.Clear();
            _colors.Clear();
            _uvs.Clear();
            _lights.Clear();
            _indexes.Clear();
        }

        public void AddQuad(Vector3 topLeft, Vector3 topRight, Vector3 bottomRight, Vector3 bottomLeft,
                            float dV, float dU, BlockFace block, int side)
        {
            var backFace = side > 2;
            var index = _verticies.Count;

            _verticies.Add(topLeft * 0.5f);
            _verticies.Add(topRight * 0.5f);
            _verticies.Add(bottomRight * 0.5f);
            _verticies.Add(bottomLeft * 0.5f);

            var color = block.Tint.Convert32();
            _colors.Add(color);
            _colors.Add(color);
            _colors.Add(color);
            _colors.Add(color);
            switch (side)
            {
                case 0:
                case 1:
                    _uvs.Add(new Vector4(dV, dU, block.TextureIndex));
                    _uvs.Add(new Vector4(0, dU, block.TextureIndex));
                    _uvs.Add(new Vector4(0, 0, block.TextureIndex));
                    _uvs.Add(new Vector4(dV, 0, block.TextureIndex));
                    break;
                case 2:
                    _uvs.Add(new Vector4(dU, 0, block.TextureIndex));
                    _uvs.Add(new Vector4(dU, dV, block.TextureIndex));
                    _uvs.Add(new Vector4(0, dV, block.TextureIndex));
                    _uvs.Add(new Vector4(0, 0, block.TextureIndex));
                    break;
                case 3:
                case 4:
                    _uvs.Add(new Vector4(0, dU, block.TextureIndex));
                    _uvs.Add(new Vector4(dV, dU, block.TextureIndex));
                    _uvs.Add(new Vector4(dV, 0, block.TextureIndex));
                    _uvs.Add(new Vector4(0, 0, block.TextureIndex));
                    break;
                case 5:
                    _uvs.Add(new Vector4(0, 0, block.TextureIndex));
                    _uvs.Add(new Vector4(0, dV, block.TextureIndex));
                    _uvs.Add(new Vector4(dU, dV, block.TextureIndex));
                    _uvs.Add(new Vector4(dU, 0, block.TextureIndex));
                    break;
                default:
                    throw new IndexOutOfRangeException();
            }

            var lightTopLeft = new Vector2((block.LightTopLeft >> 15) & 0x7FFFu, block.LightTopLeft & 0x7FFFu);
            _lights.Add(lightTopLeft);
            var lightTopRight = new Vector2((block.LightTopRight >> 15) & 0x7FFFu, block.LightTopRight & 0x7FFFu);
            _lights.Add(lightTopRight);
            var lightBottomRight = new Vector2((block.LightBottomRight >> 15) & 0x7FFFu, block.LightBottomRight & 0x7FFFu);
            _lights.Add(lightBottomRight);
            var lightBottomLeft = new Vector2((block.LightBottomLeft >> 15) & 0x7FFFu, block.LightBottomLeft & 0x7FFFu);
            _lights.Add(lightBottomLeft);
            

            if (Mathf.Abs(SumComponents(block.LightTopLeft) - SumComponents(block.LightBottomRight)) <
                Mathf.Abs(SumComponents(block.LightTopRight) - SumComponents(block.LightBottomLeft)))
            {
                if (backFace)
                {
                    _indexes.Add(index);
                    _indexes.Add(index + 1);
                    _indexes.Add(index + 2);
                    _indexes.Add(index + 2);
                    _indexes.Add(index + 3);
                    _indexes.Add(index);
                }
                else
                {
                    _indexes.Add(index);
                    _indexes.Add(index + 3);
                    _indexes.Add(index + 2);
                    _indexes.Add(index + 2);
                    _indexes.Add(index + 1);
                    _indexes.Add(index);
                }
            }
            else
            {
                if (backFace)
                {
                    _indexes.Add(index + 1);
                    _indexes.Add(index + 2);
                    _indexes.Add(index + 3);
                    _indexes.Add(index + 3);
                    _indexes.Add(index);
                    _indexes.Add(index + 1);
                }
                else
                {
                    _indexes.Add(index + 1);
                    _indexes.Add(index);
                    _indexes.Add(index + 3);
                    _indexes.Add(index + 3);
                    _indexes.Add(index + 2);
                    _indexes.Add(index + 1);
                }
            }
        }

        private int SumComponents(uint light)
        {
            return (int)(((light >> 25) & 31) +
                   ((light >> 20) & 31) +
                   ((light >> 15) & 31) +
                   ((light >> 10) & 31) +
                   ((light >> 5) & 31) +
                   (light & 31));
        }

        public void UpdateMesh(Mesh mesh)
        {
            if(mesh == null)
                return;
            mesh.Clear();
            mesh.SetVertices(_verticies);
            mesh.SetColors(_colors);
            mesh.SetUVs(0, _uvs);
            mesh.SetUVs(1, _lights);
            mesh.SetTriangles(_indexes, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
        }
    }
}