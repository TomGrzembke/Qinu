#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CamFollowAfterPixel))]
public class CamChangeEditorListener : Editor
{
    private Camera targetCamera;
    private float previousOrthoSize;

    void OnEnable()
    {
        CamFollowAfterPixel myScript = (CamFollowAfterPixel)target;
        if (myScript == null) return;

        targetCamera = myScript.GetComponent<Camera>();

        // Store the initial value if the camera exists
        if (targetCamera != null)
        {
            previousOrthoSize = targetCamera.orthographicSize;
        }
    }

    public override void OnInspectorGUI()
    {
        // Draw the default inspector for your script
        DrawDefaultInspector();

        if (targetCamera == null)
        {
            EditorGUILayout.HelpBox("Camera component not found on this GameObject.", MessageType.Warning);
            return;
        }

        // Draw the float field for orthographicSize directly
        float currentOrthoSize = EditorGUILayout.FloatField("Orthographic Size", targetCamera.orthographicSize);  

        // Check for a change
        if (currentOrthoSize != previousOrthoSize)
        {
            // Apply the new value and handle the change
            targetCamera.orthographicSize = currentOrthoSize;
            ((CamFollowAfterPixel)target).HandleCameraChange();

            // Update the previous value
            previousOrthoSize = currentOrthoSize;
        }
    }
}

#endif