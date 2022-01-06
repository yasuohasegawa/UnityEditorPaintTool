using UnityEngine;

namespace EditorPaintTool
{
    public class LineMesh : MonoBehaviour, ILineMesh
    {
        private Mesh m_mesh;
        private MeshFilter m_meshFilter;

        private float m_lineThickness = 1;
        private float m_polygonMinDistance = .1f;
        private Camera m_camera;

        private Material m_lineMaterial;

        private Vector3 m_prevMousePos = Vector2.zero;

        private Vector3 m_backVec = new Vector3(0, 0, -1); // view from the default camera setup.

        public float lineThickness => m_lineThickness;

        void Awake()
        {

        }

        void OnDestroy()
        {
            if (m_mesh != null)
            {
                Destroy(m_mesh);
                m_mesh = null;
            }

            if (m_lineMaterial != null)
            {
                Destroy(m_lineMaterial);
                m_lineMaterial = null;
            }
        }

        #region private methods
        private void InitializeMesh(Color col)
        {
            m_lineMaterial = new Material(Shader.Find("Custom/PaintToolEffect"));
            m_lineMaterial.SetColor("_Color", col);

            m_mesh = new Mesh();
            m_meshFilter = this.gameObject.AddComponent<MeshFilter>();
            m_meshFilter.mesh = m_mesh;

            var render = this.gameObject.AddComponent<MeshRenderer>();
            render.material = m_lineMaterial;
        }
        #endregion

        #region public methods
        public void StartDraw(Color col, Camera cam, float lineThickness = 1, float polygonMinDistance = .1f, int renderQueue = 0)
        {
            m_camera = (cam == null) ? Camera.main : cam;
            m_lineThickness = lineThickness;
            m_polygonMinDistance = polygonMinDistance;
            InitializeMesh(col);
            m_lineMaterial.renderQueue = 9999 + renderQueue;

            Vector3[] vertices = new Vector3[4];
            Vector2[] uv = new Vector2[4];
            int[] triangles = new int[6];

            vertices[0] = InputUtils.GetMouseWorldPosition(m_camera);
            vertices[1] = InputUtils.GetMouseWorldPosition(m_camera);
            vertices[2] = InputUtils.GetMouseWorldPosition(m_camera);
            vertices[3] = InputUtils.GetMouseWorldPosition(m_camera);

            uv[0] = Vector2.zero;
            uv[1] = Vector2.zero;
            uv[2] = Vector2.zero;
            uv[3] = Vector2.zero;

            triangles[0] = 0;
            triangles[1] = 3;
            triangles[2] = 1;

            triangles[3] = 1;
            triangles[4] = 3;
            triangles[5] = 2;

            m_mesh.vertices = vertices;
            m_mesh.uv = uv;
            m_mesh.triangles = triangles;
            m_mesh.MarkDynamic();

            GetComponent<MeshFilter>().mesh = m_mesh;
        }

        // line drawing code reference: https://www.youtube.com/watch?v=XozHdfHrb1U&t=177s
        // The following drawing method will use the Mesh API2 version as well.
        public void Draw()
        {
            Vector3 currentPos = InputUtils.GetMouseWorldPosition(m_camera);
            if (Vector3.Distance(currentPos, m_prevMousePos) > m_polygonMinDistance)
            {
                Vector3[] vertices = new Vector3[m_mesh.vertices.Length + 2];
                Vector2[] uv = new Vector2[m_mesh.uv.Length + 2];
                int[] triangles = new int[m_mesh.triangles.Length + 6];

                m_mesh.vertices.CopyTo(vertices, 0);
                m_mesh.uv.CopyTo(uv, 0);
                m_mesh.triangles.CopyTo(triangles, 0);

                int vIndex = vertices.Length - 4;
                int vIndex0 = vIndex + 0;
                int vIndex1 = vIndex + 1;
                int vIndex2 = vIndex + 2;
                int vIndex3 = vIndex + 3;

                // foward normalized
                Vector3 foward = (currentPos - m_prevMousePos).normalized;

                Vector3 left = currentPos + Vector3.Cross(foward, m_backVec * -1) * lineThickness;
                Vector3 right = currentPos + Vector3.Cross(foward, m_backVec) * lineThickness;

                vertices[vIndex2] = right;
                vertices[vIndex3] = left;

                uv[vIndex2] = Vector2.zero;
                uv[vIndex3] = Vector2.zero;

                int tIndex = triangles.Length - 6;
                triangles[tIndex + 0] = vIndex0;
                triangles[tIndex + 1] = vIndex2;
                triangles[tIndex + 2] = vIndex1;

                triangles[tIndex + 3] = vIndex1;
                triangles[tIndex + 4] = vIndex2;
                triangles[tIndex + 5] = vIndex3;

                m_mesh.vertices = vertices;
                m_mesh.uv = uv;
                m_mesh.triangles = triangles;

                m_prevMousePos = currentPos;
            }
        }

        public GameObject Self()
        {
            if (this == null) return null;
            return this.gameObject;
        }

        #endregion
    }
}