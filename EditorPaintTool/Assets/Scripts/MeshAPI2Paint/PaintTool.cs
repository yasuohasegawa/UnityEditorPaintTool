using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace EditorPaintTool
{
    public class PaintTool : MonoBehaviour
    {
        [SerializeField]
        private MeshAPIType m_meshAPIType = MeshAPIType.V2;

        [SerializeField]
        private float m_lineThickness = 0.05f;

        [SerializeField]
        private float m_polygonMinDistance = .3f;

        [SerializeField]
        private Camera m_camera;

        [SerializeField]
        private Color m_lineColor = Color.red;


        private List<ILineMesh> m_lines = new List<ILineMesh>();

        private ILineMesh m_currentLine;

        private PaintToolFactory m_paintToolFactory;

        private bool m_isFocusOut = false; // This one is to avoid instantiating the extra GameObjects.

#if UNITY_EDITOR
        void Awake()
        {
            m_paintToolFactory = new PaintToolFactory();
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (m_isFocusOut)
                {
                    m_isFocusOut = false;
                    return;
                }
                m_currentLine = AddLine();
            }

            if (Input.GetMouseButton(0))
            {
                if (m_currentLine != null) m_currentLine.Draw();
            }
        }

        void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
            {
                m_isFocusOut = true;
            }
        }

        void OnDestroy()
        {
            Clear();
        }
#endif
        private ILineMesh AddLine()
        {
            var line = m_paintToolFactory.Create(m_meshAPIType);
            line.StartDraw(m_lineColor, m_camera, m_lineThickness, m_polygonMinDistance, m_lines.Count);
            m_lines.Add(line);
            return line;
        }

        [ContextMenu("Clear")]
        private void Clear()
        {
            if (m_lines.Count != 0)
            {
                for (int i = 0; i < m_lines.Count; i++)
                {
                    var line = m_lines[i];
                    if (line != null)
                    {
                        var go = line.Self();
                        if(go != null) Destroy(go);
                        line = null;
                    }
                }
            }
            m_lines.Clear();
        }

        [ContextMenu("CaptureScreenshot")]
        private void CaptureScreenshot()
        {
            var filename = System.DateTime.Now.ToString("ddMMyyyy-HHmmss") + ".png";
            ScreenCapture.CaptureScreenshot(filename);
            var assembly = typeof(EditorWindow).Assembly;
            var type = assembly.GetType("UnityEditor.GameView");
            var gameview = EditorWindow.GetWindow(type);

            gameview.Repaint();
        }
    }
}
