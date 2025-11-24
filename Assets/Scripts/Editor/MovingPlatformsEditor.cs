using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

[CustomEditor(typeof(MovingPlatforms))]
public class MovingPlatformsEditor : Editor
{
    private Vector3 originalPosition;
    private bool isEditingWaypoints = false;
    private ReorderableList waypointList;

    void OnEnable()
    {
        MovingPlatforms platform = (MovingPlatforms)target;
        originalPosition = platform.transform.position;
        
        // Create reorderable list
        SerializedProperty waypointsProperty = serializedObject.FindProperty("waypoints");
        waypointList = new ReorderableList(serializedObject, waypointsProperty, true, true, true, true);
        
        // Header
        waypointList.drawHeaderCallback = (Rect rect) =>
        {
            EditorGUI.LabelField(rect, "Waypoints");
        };
        
        // Draw each element
        waypointList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            SerializedProperty element = waypointList.serializedProperty.GetArrayElementAtIndex(index);
            rect.y += 2;
            
            string controlName = "Waypoint_" + index;
            GUI.SetNextControlName(controlName);
            
            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(
                new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
                element,
                new GUIContent($"Point {index}")
            );
            
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                if (!Application.isPlaying)
                {
                    platform.transform.position = element.vector3Value;
                }
            }
            
            if (GUI.GetNameOfFocusedControl() == controlName)
            {
                isEditingWaypoints = true;
            }
        };
        
        // On add
        waypointList.onAddCallback = (ReorderableList list) =>
        {
            int index = list.serializedProperty.arraySize;
            list.serializedProperty.InsertArrayElementAtIndex(index);
            SerializedProperty newElement = list.serializedProperty.GetArrayElementAtIndex(index);
            
            if (index == 0)
            {
                newElement.vector3Value = platform.transform.position;
            }
            else
            {
                SerializedProperty lastElement = list.serializedProperty.GetArrayElementAtIndex(index - 1);
                newElement.vector3Value = lastElement.vector3Value + Vector3.right * 2f;
            }
            
            serializedObject.ApplyModifiedProperties();
        };
    }

    public override void OnInspectorGUI()
    {
        MovingPlatforms platform = (MovingPlatforms)target;
        
        serializedObject.Update();
        
        // Move Speed field
        EditorGUILayout.PropertyField(serializedObject.FindProperty("moveSpeed"));
        
        EditorGUILayout.Space();
        
        // Track if we were editing
        bool wasEditing = isEditingWaypoints;
        isEditingWaypoints = false;
        
        // Draw reorderable list
        waypointList.DoLayoutList();
        
        EditorGUILayout.Space();
        
        // Reset position button
        SerializedProperty waypointsProperty = serializedObject.FindProperty("waypoints");
        if (!Application.isPlaying && waypointsProperty.arraySize > 0)
        {
            if (GUILayout.Button("Reset to Start Position"))
            {
                platform.transform.position = waypointsProperty.GetArrayElementAtIndex(0).vector3Value;
                EditorUtility.SetDirty(platform);
            }
        }
        
        // If we just stopped editing, reset to start position
        if (wasEditing && !isEditingWaypoints && !Application.isPlaying && waypointsProperty.arraySize > 0)
        {
            platform.transform.position = waypointsProperty.GetArrayElementAtIndex(0).vector3Value;
            EditorUtility.SetDirty(platform);
        }
        
        serializedObject.ApplyModifiedProperties();
    }
    
    void OnSceneGUI()
    {
        MovingPlatforms platform = (MovingPlatforms)target;
        
        if (platform.waypoints == null || platform.waypoints.Count == 0)
            return;
        
        // Draw path between waypoints
        Handles.color = Color.cyan;
        for (int i = 0; i < platform.waypoints.Count; i++)
        {
            int nextIndex = (i + 1) % platform.waypoints.Count;
            Handles.DrawLine(platform.waypoints[i], platform.waypoints[nextIndex]);
            
            // Draw waypoint spheres
            Handles.color = Color.yellow;
            if (Handles.Button(platform.waypoints[i], Quaternion.identity, 0.15f, 0.15f, Handles.SphereHandleCap))
            {
                // Click on waypoint to move platform there in edit mode
                if (!Application.isPlaying)
                {
                    platform.transform.position = platform.waypoints[i];
                    EditorUtility.SetDirty(platform);
                }
            }
            
            // Draw label
            Handles.Label(platform.waypoints[i] + Vector3.up * 0.5f, $"Point {i}");
            Handles.color = Color.cyan;
        }
    }
}
