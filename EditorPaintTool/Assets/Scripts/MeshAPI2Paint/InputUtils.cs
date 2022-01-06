using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EditorPaintTool
{
    public class InputUtils : MonoBehaviour
    {
        public static Vector3 GetMouseWorldPosition(Camera cam)
        {
            var cPos = cam.transform.position;
            var mousePos = Input.mousePosition;
            mousePos.z = cPos.z * -1f;

            Vector3 vec = cam.ScreenToWorldPoint(mousePos);
            vec.z = 0f;
            return vec;
        }
    }
}
