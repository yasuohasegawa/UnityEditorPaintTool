using UnityEngine;

namespace EditorPaintTool
{
    public interface ILineMesh
    {
        void StartDraw(Color col, Camera cam, float lineThickness = 1, float polygonMinDistance = .1f, int renderQueue = 0);
        void Draw();
        GameObject Self();
    }
}
