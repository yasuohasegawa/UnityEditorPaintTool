using UnityEngine;

namespace EditorPaintTool
{
    public enum MeshAPIType
    {
        V1,
        V2
    }

    public class PaintToolFactory
    {
        public PaintToolFactory()
        {

        }

        public ILineMesh Create(MeshAPIType apiType)
        {
            var go = new GameObject();
            go.hideFlags = HideFlags.HideInHierarchy;
            switch (apiType)
            {
                case MeshAPIType.V1:
                default:
                    return go.AddComponent<LineMesh>();
                case MeshAPIType.V2:
                    return go.AddComponent<LineMeshAPI2>();
            }
        }
    }
}
