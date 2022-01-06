using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace EditorPaintTool
{
    public class LineMeshAPI2 : MonoBehaviour, ILineMesh
    {
        private float m_lineThickness = 1;
        private float m_polygonMinDistance = .1f;
        private Camera m_camera;

        private Material m_lineMaterial;

        private Vector3 m_prevMousePos = Vector2.zero;

        private Vector3 m_backVec = new Vector3(0, 0, -1); // view from the default camera setup.

        private Mesh m_mesh;
        private MeshFilter m_meshFilter;
        private VertexAttributeDescriptor[] m_layout = new[]
        {
            new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
            new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 3),
            new VertexAttributeDescriptor(VertexAttribute.Tangent, VertexAttributeFormat.Float32, 4),
            new VertexAttributeDescriptor(VertexAttribute.Color, VertexAttributeFormat.UNorm8, 4),
            new VertexAttributeDescriptor(VertexAttribute.TexCoord0,VertexAttributeFormat.Float32, 2),
            new VertexAttributeDescriptor(VertexAttribute.TexCoord1,VertexAttributeFormat.Float32, 2),
        };

        private NativeArray<PlaneVertex> m_vertexBuffer;
        private NativeArray<int> m_indexBuffer;
        private Bounds m_meshBounds = new Bounds(Vector3.zero, Vector3.one * 100000);

        public float lineThickness => m_lineThickness;

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

            DisposeBuffers();
        }

        #region private methods
        private void InitializeMesh(Color col)
        {
            DisposeBuffers();

            m_lineMaterial = new Material(Shader.Find("Custom/PaintToolEffect"));
            m_lineMaterial.SetColor("_Color", col);

            m_mesh = new Mesh();
            m_meshFilter = this.gameObject.AddComponent<MeshFilter>();
            m_meshFilter.mesh = m_mesh;

            var render = this.gameObject.AddComponent<MeshRenderer>();
            render.material = m_lineMaterial;
        }

        private void GenerateMesh()
        {
            var vertexCount = m_vertexBuffer.Length;
            m_mesh.SetVertexBufferParams(vertexCount, m_layout);

            int indexCount = m_indexBuffer.Length - 6;
            m_mesh.SetIndexBufferParams(indexCount, IndexFormat.UInt32);

            m_mesh.SetVertexBufferData(m_vertexBuffer, 0, 0, vertexCount);
            m_mesh.SetIndexBufferData(m_indexBuffer, 0, 0, indexCount);

            // Submesh definition
            var meshDesc = new SubMeshDescriptor(0, indexCount, MeshTopology.Triangles);
            m_mesh.subMeshCount = 1;
            m_mesh.SetSubMesh(0, meshDesc);
            m_mesh.bounds = m_meshBounds;
            m_mesh.RecalculateNormals();
            m_mesh.RecalculateBounds();
        }

        private void DisposeBuffers()
        {
            if (m_vertexBuffer.Length != 0)
            {
                m_vertexBuffer.Dispose();
                m_indexBuffer.Dispose();
            }
        }

        private unsafe void CopyNativeArray<T>(NativeArray<T> src, int srcIndex, NativeArray<T> dst, int dstIndex, int length) where T : struct
        {
            UnsafeUtility.MemCpy(
                                  destination: (void*)((IntPtr)dst.GetUnsafePtr()
                                             + dstIndex * UnsafeUtility.SizeOf<T>()),
                                   source: (void*)((IntPtr)src.GetUnsafePtr()
                                             + srcIndex * UnsafeUtility.SizeOf<T>()),
                                   size: length * UnsafeUtility.SizeOf<T>()
                                                );
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

            var vertexCount = 4;
            m_vertexBuffer = new NativeArray<PlaneVertex>(vertexCount, Allocator.Persistent,
                NativeArrayOptions.UninitializedMemory);

            int indexCount = 6;
            m_indexBuffer = new NativeArray<int>(indexCount, Allocator.Persistent,
                NativeArrayOptions.UninitializedMemory);

            PlaneVertex pv0 = new PlaneVertex();
            pv0.pos = InputUtils.GetMouseWorldPosition(m_camera);
            pv0.uv = Vector2.zero;
            m_vertexBuffer[0] = pv0;

            PlaneVertex pv1 = new PlaneVertex();
            pv1.pos = InputUtils.GetMouseWorldPosition(m_camera);
            pv1.uv = Vector2.zero;
            m_vertexBuffer[1] = pv1;

            PlaneVertex pv2 = new PlaneVertex();
            pv2.pos = InputUtils.GetMouseWorldPosition(m_camera);
            pv2.uv = Vector2.zero;
            m_vertexBuffer[2] = pv2;

            PlaneVertex pv3 = new PlaneVertex();
            pv3.pos = InputUtils.GetMouseWorldPosition(m_camera);
            pv3.uv = Vector2.zero;
            m_vertexBuffer[3] = pv3;

            m_indexBuffer[0] = 0;
            m_indexBuffer[1] = 3;
            m_indexBuffer[2] = 1;

            m_indexBuffer[3] = 1;
            m_indexBuffer[4] = 3;
            m_indexBuffer[5] = 2;

            GenerateMesh();
        }

        public void Draw()
        {
            Vector3 currentPos = InputUtils.GetMouseWorldPosition(m_camera);
            if (Vector3.Distance(currentPos, m_prevMousePos) > m_polygonMinDistance)
            {
                var vertexCount = m_vertexBuffer.Length + 2;
                var vertexBuffer = new NativeArray<PlaneVertex>(vertexCount, Allocator.Persistent,
                    NativeArrayOptions.UninitializedMemory);

                int indexCount = m_indexBuffer.Length + 6;
                var indexBuffer = new NativeArray<int>(indexCount, Allocator.Persistent,
                    NativeArrayOptions.UninitializedMemory);

                CopyNativeArray(m_vertexBuffer, 0, vertexBuffer, 0, m_vertexBuffer.Length);
                CopyNativeArray(m_indexBuffer, 0, indexBuffer, 0, m_indexBuffer.Length);

                int vIndex = vertexBuffer.Length - 4;
                int vIndex0 = vIndex + 0;
                int vIndex1 = vIndex + 1;
                int vIndex2 = vIndex + 2;
                int vIndex3 = vIndex + 3;

                // foward normalized
                Vector3 foward = (currentPos - m_prevMousePos).normalized;

                Vector3 left = currentPos + Vector3.Cross(foward, m_backVec * -1) * lineThickness;
                Vector3 right = currentPos + Vector3.Cross(foward, m_backVec) * lineThickness;

                PlaneVertex pv0 = new PlaneVertex();
                pv0.pos = right;
                pv0.uv = Vector2.zero;
                vertexBuffer[vIndex2] = pv0;

                PlaneVertex pv1 = new PlaneVertex();
                pv1.pos = left;
                pv1.uv = Vector2.zero;
                vertexBuffer[vIndex3] = pv1;

                int tIndex = m_indexBuffer.Length - 6;
                indexBuffer[tIndex + 0] = vIndex0;
                indexBuffer[tIndex + 1] = vIndex2;
                indexBuffer[tIndex + 2] = vIndex1;

                indexBuffer[tIndex + 3] = vIndex1;
                indexBuffer[tIndex + 4] = vIndex2;
                indexBuffer[tIndex + 5] = vIndex3;

                DisposeBuffers();
                m_vertexBuffer = new NativeArray<PlaneVertex>(vertexCount, Allocator.Persistent,
                    NativeArrayOptions.UninitializedMemory);

                m_indexBuffer = new NativeArray<int>(indexCount, Allocator.Persistent,
                    NativeArrayOptions.UninitializedMemory);

                m_vertexBuffer.CopyFrom(vertexBuffer);
                m_indexBuffer.CopyFrom(indexBuffer);

                vertexBuffer.Dispose();
                indexBuffer.Dispose();

                GenerateMesh();

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