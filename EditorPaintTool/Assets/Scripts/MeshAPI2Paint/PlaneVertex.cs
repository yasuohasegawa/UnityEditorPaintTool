using UnityEngine;

namespace EditorPaintTool
{
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public struct PlaneVertex
    {
        public Vector3 pos;
        public Vector3 normal;
        public Vector4 tangent;
        public Color32 color;
        public Vector2 uv;
        public Vector2 uv2;
    }
}