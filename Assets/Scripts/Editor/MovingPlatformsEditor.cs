using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

[CustomEditor(typeof(MovingPlatforms))]
public class MovingPlatformsEditor : Editor
{
    private Vector3 originalPosition;
    private bool isEditingWaypoints = false;
    private ReorderableList waypointList;
    
    // Grid constants based on your scale system
    private const float ONE_PIXEL = 0.03f;      // 1 pixel in world units (at 3x scale)
    private const float ONE_TILE = 0.48f;       // 16 pixels / 1 tile in world units
    private const float GRID_SIZE = 0.96f;      // World grid size
    
    // Platform sprite alignment offsets (48x8 pixel sprite = 1.44x0.24 units = 3x0.5 tiles)
    private const float PLATFORM_GRID_OFFSET_X = 0.24f;  // Half tile offset for 3-tile-wide sprite
    private const float PLATFORM_GRID_OFFSET_Y = -0.6f;  // Offset for half-tile-tall sprite sitting flush

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
            
            float lineHeight = EditorGUIUtility.singleLineHeight;
            float spacing = 2f;
            
            // Display relative position from first waypoint (not current platform position)
            Vector3 absolutePos = element.vector3Value;
            Vector3 relativePos = Vector3.zero;
            
            if (waypointList.serializedProperty.arraySize > 0)
            {
                SerializedProperty firstWaypoint = waypointList.serializedProperty.GetArrayElementAtIndex(0);
                relativePos = absolutePos - firstWaypoint.vector3Value;
                // Round to 0.01
                relativePos.x = Mathf.Round(relativePos.x * 100f) / 100f;
                relativePos.y = Mathf.Round(relativePos.y * 100f) / 100f;
                relativePos.z = Mathf.Round(relativePos.z * 100f) / 100f;
            }
            
            string controlName = "Waypoint_" + index;
            GUI.SetNextControlName(controlName);
            
            EditorGUI.BeginChangeCheck();
            Vector3 newRelativePos = EditorGUI.Vector3Field(
                new Rect(rect.x, rect.y, rect.width, lineHeight),
                $"Point {index} (Offset)",
                relativePos
            );
            
            if (EditorGUI.EndChangeCheck())
            {
                // Convert relative back to absolute (from first waypoint) and store
                if (waypointList.serializedProperty.arraySize > 0)
                {
                    SerializedProperty firstWaypoint = waypointList.serializedProperty.GetArrayElementAtIndex(0);
                    Vector3 newAbsolutePos = newRelativePos + firstWaypoint.vector3Value;
                    // Round to 0.01
                    newAbsolutePos.x = Mathf.Round(newAbsolutePos.x * 100f) / 100f;
                    newAbsolutePos.y = Mathf.Round(newAbsolutePos.y * 100f) / 100f;
                    newAbsolutePos.z = Mathf.Round(newAbsolutePos.z * 100f) / 100f;
                    element.vector3Value = newAbsolutePos;
                    serializedObject.ApplyModifiedProperties();
                }
            }
            
            if (GUI.GetNameOfFocusedControl() == controlName)
            {
                isEditingWaypoints = true;
            }
            
            // Movement buttons on next line
            rect.y += lineHeight + spacing;
            float buttonWidth = 30f;
            float buttonSpacing = 2f;
            float startX = rect.x + 20f; // Indent for visual grouping
            
            // Snap to grid button
            if (GUI.Button(new Rect(startX, rect.y, 60f, lineHeight), "Snap"))
            {
                if (index == 0)
                {
                    // Snapping waypoint 0 snaps the platform itself
                    Vector3 platformPos = platform.transform.position;
                    platformPos.x = Mathf.Round((platformPos.x - PLATFORM_GRID_OFFSET_X) / ONE_TILE) * ONE_TILE + PLATFORM_GRID_OFFSET_X;
                    platformPos.y = Mathf.Round((platformPos.y - PLATFORM_GRID_OFFSET_Y) / ONE_TILE) * ONE_TILE + PLATFORM_GRID_OFFSET_Y;
                    // Round to 0.01
                    platformPos.x = Mathf.Round(platformPos.x * 100f) / 100f;
                    platformPos.y = Mathf.Round(platformPos.y * 100f) / 100f;
                    platform.transform.position = platformPos;
                    
                    // Update waypoint 0 to match (it's at platform position)
                    element.vector3Value = platformPos;
                }
                else
                {
                    // For other waypoints, snap relative to platform's grid
                    Vector3 pos = element.vector3Value;
                    pos.x = Mathf.Round((pos.x - PLATFORM_GRID_OFFSET_X) / ONE_TILE) * ONE_TILE + PLATFORM_GRID_OFFSET_X;
                    pos.y = Mathf.Round((pos.y - PLATFORM_GRID_OFFSET_Y) / ONE_TILE) * ONE_TILE + PLATFORM_GRID_OFFSET_Y;
                    // Round to 0.01
                    pos.x = Mathf.Round(pos.x * 100f) / 100f;
                    pos.y = Mathf.Round(pos.y * 100f) / 100f;
                    element.vector3Value = pos;
                    // Update preview to show this waypoint
                    if (!Application.isPlaying)
                    {
                        platform.transform.position = pos;
                    }
                }
                serializedObject.ApplyModifiedProperties();
            }
            
            startX += 62f + buttonSpacing;
            
            // 1 pixel movement buttons
            EditorGUI.LabelField(new Rect(startX, rect.y, 40f, lineHeight), "1px:");
            startX += 35f;
            
            if (GUI.Button(new Rect(startX, rect.y, buttonWidth, lineHeight), "←"))
            {
                Vector3 pos = element.vector3Value;
                pos.x -= ONE_PIXEL;
                pos.x = Mathf.Round(pos.x * 100f) / 100f;
                element.vector3Value = pos;
                serializedObject.ApplyModifiedProperties();
                if (!Application.isPlaying)
                {
                    platform.transform.position = pos;
                }
            }
            startX += buttonWidth + buttonSpacing;
            
            if (GUI.Button(new Rect(startX, rect.y, buttonWidth, lineHeight), "→"))
            {
                Vector3 pos = element.vector3Value;
                pos.x += ONE_PIXEL;
                pos.x = Mathf.Round(pos.x * 100f) / 100f;
                element.vector3Value = pos;
                serializedObject.ApplyModifiedProperties();
                if (!Application.isPlaying)
                {
                    platform.transform.position = pos;
                }
            }
            startX += buttonWidth + buttonSpacing;
            
            if (GUI.Button(new Rect(startX, rect.y, buttonWidth, lineHeight), "↓"))
            {
                Vector3 pos = element.vector3Value;
                pos.y -= ONE_PIXEL;
                pos.y = Mathf.Round(pos.y * 100f) / 100f;
                element.vector3Value = pos;
                serializedObject.ApplyModifiedProperties();
                if (!Application.isPlaying)
                {
                    platform.transform.position = pos;
                }
            }
            startX += buttonWidth + buttonSpacing;
            
            if (GUI.Button(new Rect(startX, rect.y, buttonWidth, lineHeight), "↑"))
            {
                Vector3 pos = element.vector3Value;
                pos.y += ONE_PIXEL;
                pos.y = Mathf.Round(pos.y * 100f) / 100f;
                element.vector3Value = pos;
                serializedObject.ApplyModifiedProperties();
                if (!Application.isPlaying)
                {
                    platform.transform.position = pos;
                }
            }
            
            // 16 pixel (1 tile) movement buttons on next line
            rect.y += lineHeight + spacing;
            startX = rect.x + 20f;
            
            EditorGUI.LabelField(new Rect(startX, rect.y, 50f, lineHeight), "1 tile:");
            startX += 35f;
            
            if (GUI.Button(new Rect(startX, rect.y, buttonWidth, lineHeight), "←"))
            {
                Vector3 pos = element.vector3Value;
                pos.x -= ONE_TILE;
                pos.x = Mathf.Round(pos.x * 100f) / 100f;
                element.vector3Value = pos;
                serializedObject.ApplyModifiedProperties();
                if (!Application.isPlaying)
                {
                    platform.transform.position = pos;
                }
            }
            startX += buttonWidth + buttonSpacing;
            
            if (GUI.Button(new Rect(startX, rect.y, buttonWidth, lineHeight), "→"))
            {
                Vector3 pos = element.vector3Value;
                pos.x += ONE_TILE;
                pos.x = Mathf.Round(pos.x * 100f) / 100f;
                element.vector3Value = pos;
                serializedObject.ApplyModifiedProperties();
                if (!Application.isPlaying)
                {
                    platform.transform.position = pos;
                }
            }
            startX += buttonWidth + buttonSpacing;
            
            if (GUI.Button(new Rect(startX, rect.y, buttonWidth, lineHeight), "↓"))
            {
                Vector3 pos = element.vector3Value;
                pos.y -= ONE_TILE;
                pos.y = Mathf.Round(pos.y * 100f) / 100f;
                element.vector3Value = pos;
                serializedObject.ApplyModifiedProperties();
                if (!Application.isPlaying)
                {
                    platform.transform.position = pos;
                }
            }
            startX += buttonWidth + buttonSpacing;
            
            if (GUI.Button(new Rect(startX, rect.y, buttonWidth, lineHeight), "↑"))
            {
                Vector3 pos = element.vector3Value;
                pos.y += ONE_TILE;
                pos.y = Mathf.Round(pos.y * 100f) / 100f;
                element.vector3Value = pos;
                serializedObject.ApplyModifiedProperties();
                if (!Application.isPlaying)
                {
                    platform.transform.position = pos;
                }
            }
        };
        
        // Adjust element height to accommodate buttons
        waypointList.elementHeightCallback = (int index) =>
        {
            return EditorGUIUtility.singleLineHeight * 3 + 10f; // 3 lines + spacing
        };
        
        // On add
        waypointList.onAddCallback = (ReorderableList list) =>
        {
            int index = list.serializedProperty.arraySize;
            list.serializedProperty.InsertArrayElementAtIndex(index);
            SerializedProperty newElement = list.serializedProperty.GetArrayElementAtIndex(index);
            
            if (index == 0)
            {
                // First waypoint is at platform's current position
                newElement.vector3Value = platform.transform.position;
            }
            else
            {
                // New waypoints offset from previous by 1 tile to the right
                SerializedProperty lastElement = list.serializedProperty.GetArrayElementAtIndex(index - 1);
                newElement.vector3Value = lastElement.vector3Value + Vector3.right * ONE_TILE;
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
