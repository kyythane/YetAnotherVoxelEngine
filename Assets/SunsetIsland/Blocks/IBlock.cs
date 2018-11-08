using Assets.SunsetIsland.Common.Enums;
using UnityEngine;

namespace Assets.SunsetIsland.Blocks
{
    public interface IBlock
    {
        int BlockId { get; }
        
        ushort[] Tints { get; }
        byte Rotation { get; }

        bool RenderOpaque { get; }

        bool AddToRenderMesh { get; }
        bool AddToPhysicsMesh { get; }

        uint Opacity { get; }
        uint Emissivity { get; }
        
        int GetTextureMapping(FaceDirection faceDir);

        Color32 GetColor(FaceDirection faceDir);
    }
}