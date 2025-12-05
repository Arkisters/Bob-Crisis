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
        originalPosition = platform.transform.localPosition;
        
        // Migrate old waypoint system to new if needed
        SerializedProperty oldWaypoints = serializedObject.FindProperty("waypoints");
        SerializedProperty newWaypoints = serializedObject.FindProperty("waypointData");
        
        if (oldWaypoints.arraySize > 0 && newWaypoints.arraySize == 0)
        {
            // Migrate old waypoints to new system
            for (int i = 0; i < oldWaypoints.arraySize; i++)
            {
                newWaypoints.InsertArrayElementAtIndex(i);
                SerializedProperty newElement = newWaypoints.GetArrayElementAtIndex(i);
                SerializedProperty oldPos = oldWaypoints.GetArrayElementAtIndex(i);
                
                newElement.FindPropertyRelative("position").vector3Value = oldPos.vector3Value;
                newElement.FindPropertyRelative("speed").floatValue = platform.moveSpeed;
                newElement.FindPropertyRelative("rotation").floatValue = 0f;
            }
            serializedObject.ApplyModifiedProperties();
        }
        
        // Create reorderable list for new waypoint system
        SerializedProperty waypointsProperty = serializedObject.FindProperty("waypointData");
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
            SerializedProperty positionProp = element.FindPropertyRelative("position");
            SerializedProperty speedProp = element.FindPropertyRelative("speed");
            SerializedProperty rotationProp = element.FindPropertyRelative("rotation");
            
            rect.y += 2;
            
            float lineHeight = EditorGUIUtility.singleLineHeight;
            float spacing = 2f;
            
            // Display relative position from first waypoint
            Vector3 absolutePos = positionProp.vector3Value;
            Vector3 relativePos = Vector3.zero;
            
            if (waypointList.serializedProperty.arraySize > 0)
            {
                SerializedProperty firstWaypoint = waypointList.serializedProperty.GetArrayElementAtIndex(0);
                SerializedProperty firstPos = firstWaypoint.FindPropertyRelative("position");
                relativePos = absolutePos - firstPos.vector3Value;
                // Round to 0.01
                relativePos.x = Mathf.Round(relativePos.x * 100f) / 100f;
                relativePos.y = Mathf.Round(relativePos.y * 100f) / 100f;
                relativePos.z = Mathf.Round(relativePos.z * 100f) / 100f;
            }
            
            // Snap button on left side of position field
            float snapWidth = 50f;
            if (GUI.Button(new Rect(rect.x, rect.y, snapWidth, lineHeight), "Snap"))
            {
                Vector3 currentWorldPos = positionProp.vector3Value;
                
                if (index == 0)
                {
                    // Snapping waypoint 0: Get platform's current world position and snap it
                    Vector3 platformWorldPos = platform.transform.position;
                    if (platform.transform.parent != null)
                    {
                        platformWorldPos = platform.transform.parent.TransformPoint(platform.transform.localPosition);
                    }
                    
                    platformWorldPos.x = Mathf.Round((platformWorldPos.x - PLATFORM_GRID_OFFSET_X) / ONE_TILE) * ONE_TILE + PLATFORM_GRID_OFFSET_X;
                    platformWorldPos.y = Mathf.Round((platformWorldPos.y - PLATFORM_GRID_OFFSET_Y) / ONE_TILE) * ONE_TILE + PLATFORM_GRID_OFFSET_Y;
                    platformWorldPos.x = Mathf.Round(platformWorldPos.x * 100f) / 100f;
                    platformWorldPos.y = Mathf.Round(platformWorldPos.y * 100f) / 100f;
                    
                    positionProp.vector3Value = platformWorldPos;
                    UpdatePlatformPreview(platform, platformWorldPos, rotationProp.floatValue);
                }
                else
                {
                    // For other waypoints, snap the waypoint position
                    currentWorldPos.x = Mathf.Round((currentWorldPos.x - PLATFORM_GRID_OFFSET_X) / ONE_TILE) * ONE_TILE + PLATFORM_GRID_OFFSET_X;
                    currentWorldPos.y = Mathf.Round((currentWorldPos.y - PLATFORM_GRID_OFFSET_Y) / ONE_TILE) * ONE_TILE + PLATFORM_GRID_OFFSET_Y;
                    currentWorldPos.x = Mathf.Round(currentWorldPos.x * 100f) / 100f;
                    currentWorldPos.y = Mathf.Round(currentWorldPos.y * 100f) / 100f;
                    positionProp.vector3Value = currentWorldPos;
                    UpdatePlatformPreview(platform, currentWorldPos, rotationProp.floatValue);
                }
                serializedObject.ApplyModifiedProperties();
            }
            
            string controlName = "Waypoint_" + index;
            GUI.SetNextControlName(controlName);
            
            EditorGUI.BeginChangeCheck();
            Vector3 newRelativePos = EditorGUI.Vector3Field(
                new Rect(rect.x + snapWidth + 5f, rect.y, rect.width - snapWidth - 5f, lineHeight),
                $"Point {index} (Offset)",
                relativePos
            );
            
            if (EditorGUI.EndChangeCheck())
            {
                if (waypointList.serializedProperty.arraySize > 0)
                {
                    SerializedProperty firstWaypoint = waypointList.serializedProperty.GetArrayElementAtIndex(0);
                    Vector3 newAbsolutePos = newRelativePos + firstWaypoint.FindPropertyRelative("position").vector3Value;
                    newAbsolutePos.x = Mathf.Round(newAbsolutePos.x * 100f) / 100f;
                    newAbsolutePos.y = Mathf.Round(newAbsolutePos.y * 100f) / 100f;
                    newAbsolutePos.z = Mathf.Round(newAbsolutePos.z * 100f) / 100f;
                    positionProp.vector3Value = newAbsolutePos;
                    serializedObject.ApplyModifiedProperties();
                    UpdatePlatformPreview(platform, newAbsolutePos, rotationProp.floatValue);
                }
            }
            
            if (GUI.GetNameOfFocusedControl() == controlName)
            {
                isEditingWaypoints = true;
            }
            
            // Speed and Rotation fields on next line
            rect.y += lineHeight + spacing;
            float labelWidth = 50f;
            float fieldWidth = 60f;
            float rightSideX = rect.x + rect.width - labelWidth - fieldWidth;
            
            EditorGUI.LabelField(new Rect(rect.x + 20f, rect.y, labelWidth, lineHeight), "Speed:");
            speedProp.floatValue = EditorGUI.FloatField(new Rect(rect.x + 70f, rect.y, fieldWidth, lineHeight), speedProp.floatValue);
            
            EditorGUI.LabelField(new Rect(rightSideX, rect.y, labelWidth, lineHeight), "Rot:");
            EditorGUI.BeginChangeCheck();
            float newRotation = EditorGUI.FloatField(new Rect(rightSideX + labelWidth, rect.y, fieldWidth, lineHeight), rotationProp.floatValue);
            if (EditorGUI.EndChangeCheck())
            {
                rotationProp.floatValue = newRotation;
                serializedObject.ApplyModifiedProperties();
                if (!Application.isPlaying)
                {
                    Transform targetTransform = platform.transform.parent != null ? platform.transform.parent : platform.transform;
                    targetTransform.eulerAngles = new Vector3(0, 0, newRotation);
                }
            }
            
            // Movement buttons - 1 pixel row
            rect.y += lineHeight + spacing;
            float buttonWidth = 30f;
            float buttonSpacing = 2f;
            float startX = rect.x + 20f;
            
            EditorGUI.LabelField(new Rect(startX, rect.y, 40f, lineHeight), "1px:");
            startX += 35f;
            
            if (GUI.Button(new Rect(startX, rect.y, buttonWidth, lineHeight), "←"))
            {
                ApplyPositionOffset(positionProp, new Vector3(-ONE_PIXEL, 0, 0), rotationProp, platform);
            }
            startX += buttonWidth + buttonSpacing;
            
            if (GUI.Button(new Rect(startX, rect.y, buttonWidth, lineHeight), "→"))
            {
                ApplyPositionOffset(positionProp, new Vector3(ONE_PIXEL, 0, 0), rotationProp, platform);
            }
            startX += buttonWidth + buttonSpacing;
            
            if (GUI.Button(new Rect(startX, rect.y, buttonWidth, lineHeight), "↓"))
            {
                ApplyPositionOffset(positionProp, new Vector3(0, -ONE_PIXEL, 0), rotationProp, platform);
            }
            startX += buttonWidth + buttonSpacing;
            
            if (GUI.Button(new Rect(startX, rect.y, buttonWidth, lineHeight), "↑"))
            {
                ApplyPositionOffset(positionProp, new Vector3(0, ONE_PIXEL, 0), rotationProp, platform);
            }
            
            // 16 pixel (1 tile) movement buttons on next line
            rect.y += lineHeight + spacing;
            startX = rect.x + 20f;
            
            EditorGUI.LabelField(new Rect(startX, rect.y, 50f, lineHeight), "1 tile:");
            startX += 35f;
            
            if (GUI.Button(new Rect(startX, rect.y, buttonWidth, lineHeight), "←"))
            {
                ApplyPositionOffset(positionProp, new Vector3(-ONE_TILE, 0, 0), rotationProp, platform);
            }
            startX += buttonWidth + buttonSpacing;
            
            if (GUI.Button(new Rect(startX, rect.y, buttonWidth, lineHeight), "→"))
            {
                ApplyPositionOffset(positionProp, new Vector3(ONE_TILE, 0, 0), rotationProp, platform);
            }
            startX += buttonWidth + buttonSpacing;
            
            if (GUI.Button(new Rect(startX, rect.y, buttonWidth, lineHeight), "↓"))
            {
                ApplyPositionOffset(positionProp, new Vector3(0, -ONE_TILE, 0), rotationProp, platform);
            }
            startX += buttonWidth + buttonSpacing;
            
            if (GUI.Button(new Rect(startX, rect.y, buttonWidth, lineHeight), "↑"))
            {
                ApplyPositionOffset(positionProp, new Vector3(0, ONE_TILE, 0), rotationProp, platform);
            }
        };
        
        // Adjust element height to accommodate buttons
        waypointList.elementHeightCallback = (int index) =>
        {
            return EditorGUIUtility.singleLineHeight * 4 + 12f; // 4 lines: position, speed/rotation, 1px buttons, 1 tile buttons
        };
        
        // On add
        waypointList.onAddCallback = (ReorderableList list) =>
        {
            int index = list.serializedProperty.arraySize;
            list.serializedProperty.InsertArrayElementAtIndex(index);
            SerializedProperty newElement = list.serializedProperty.GetArrayElementAtIndex(index);
            SerializedProperty newPos = newElement.FindPropertyRelative("position");
            SerializedProperty newSpeed = newElement.FindPropertyRelative("speed");
            SerializedProperty newRotation = newElement.FindPropertyRelative("rotation");
            
            if (index == 0)
            {
                // First waypoint is at platform's current world position
                Vector3 worldPos = platform.transform.position;
                if (platform.transform.parent != null)
                {
                    worldPos = platform.transform.parent.TransformPoint(platform.transform.localPosition);
                }
                newPos.vector3Value = worldPos;
                
                // Get world rotation
                float worldRotation = platform.transform.eulerAngles.z;
                if (platform.transform.parent != null)
                {
                    worldRotation = platform.transform.parent.eulerAngles.z + platform.transform.localEulerAngles.z;
                }
                newRotation.floatValue = worldRotation;
                newSpeed.floatValue = platform.moveSpeed;
            }
            else
            {
                // New waypoints offset from previous by 1 tile to the right
                SerializedProperty lastElement = list.serializedProperty.GetArrayElementAtIndex(index - 1);
                SerializedProperty lastPos = lastElement.FindPropertyRelative("position");
                SerializedProperty lastSpeed = lastElement.FindPropertyRelative("speed");
                SerializedProperty lastRotation = lastElement.FindPropertyRelative("rotation");
                newPos.vector3Value = lastPos.vector3Value + Vector3.right * ONE_TILE;
                newSpeed.floatValue = lastSpeed.floatValue; // Copy speed from previous
                newRotation.floatValue = lastRotation.floatValue; // Copy rotation from previous
            }
            
            serializedObject.ApplyModifiedProperties();
        };
    }
    
    void OnDisable()
    {
        // Reset platform to start position when deselected
        MovingPlatforms platform = (MovingPlatforms)target;
        if (platform != null && !Application.isPlaying)
        {
            SerializedProperty waypointsProperty = serializedObject.FindProperty("waypointData");
            if (waypointsProperty != null && waypointsProperty.arraySize > 0)
            {
                SerializedProperty firstWaypoint = waypointsProperty.GetArrayElementAtIndex(0);
                Vector3 startPos = firstWaypoint.FindPropertyRelative("position").vector3Value;
                float startRot = firstWaypoint.FindPropertyRelative("rotation").floatValue;
                UpdatePlatformPreview(platform, startPos, startRot);
            }
        }
    }
    
    // Helper method to update platform preview position and rotation
    private void UpdatePlatformPreview(MovingPlatforms platform, Vector3 worldPosition, float worldRotation)
    {
        if (Application.isPlaying) return;
        
        if (platform.transform.parent != null)
        {
            // Convert world position to local space
            platform.transform.localPosition = platform.transform.parent.InverseTransformPoint(worldPosition);
            // Convert world rotation to local rotation
            float parentRotation = platform.transform.parent.eulerAngles.z;
            platform.transform.localEulerAngles = new Vector3(0, 0, worldRotation - parentRotation);
        }
        else
        {
            // No parent, use world space directly
            platform.transform.position = worldPosition;
            platform.transform.eulerAngles = new Vector3(0, 0, worldRotation);
        }
    }
    
    // Helper method to apply position offset
    private void ApplyPositionOffset(SerializedProperty positionProp, Vector3 offset, SerializedProperty rotationProp, MovingPlatforms platform)
    {
        Vector3 pos = positionProp.vector3Value + offset;
        pos.x = Mathf.Round(pos.x * 100f) / 100f;
        pos.y = Mathf.Round(pos.y * 100f) / 100f;
        positionProp.vector3Value = pos;
        serializedObject.ApplyModifiedProperties();
        UpdatePlatformPreview(platform, pos, rotationProp.floatValue);
    }

    public override void OnInspectorGUI()
    {
        MovingPlatforms platform = (MovingPlatforms)target;
        
        serializedObject.Update();
        
        // Move Speed field
        EditorGUILayout.PropertyField(serializedObject.FindProperty("moveSpeed"));
        
        // Constant Rotation Speed field
        EditorGUILayout.PropertyField(serializedObject.FindProperty("constantRotationSpeed"));
        
        EditorGUILayout.Space();
        
        // Track if we were editing
        bool wasEditing = isEditingWaypoints;
        isEditingWaypoints = false;
        
        // Draw reorderable list
        waypointList.DoLayoutList();
        
        EditorGUILayout.Space();
        
        // Reset position button
        SerializedProperty waypointsProperty = serializedObject.FindProperty("waypointData");
        if (!Application.isPlaying && waypointsProperty.arraySize > 0)
        {
            if (GUILayout.Button("Reset to Start Position"))
            {
                SerializedProperty firstWaypoint = waypointsProperty.GetArrayElementAtIndex(0);
                Vector3 startPos = firstWaypoint.FindPropertyRelative("position").vector3Value;
                UpdatePlatformPreview(platform, startPos, 0f);
                EditorUtility.SetDirty(platform);
            }
        }
        
        // If we just stopped editing, reset to start position
        if (wasEditing && !isEditingWaypoints && !Application.isPlaying && waypointsProperty.arraySize > 0)
        {
            SerializedProperty firstWaypoint = waypointsProperty.GetArrayElementAtIndex(0);
            Vector3 startPos = firstWaypoint.FindPropertyRelative("position").vector3Value;
            UpdatePlatformPreview(platform, startPos, 0f);
            EditorUtility.SetDirty(platform);
        }
        
        serializedObject.ApplyModifiedProperties();
    }
    
    void OnSceneGUI()
    {
        MovingPlatforms platform = (MovingPlatforms)target;
        
        if (platform.waypointData == null || platform.waypointData.Count == 0)
            return;
        
        // Draw path between waypoints
        Handles.color = Color.cyan;
        for (int i = 0; i < platform.waypointData.Count; i++)
        {
            int nextIndex = (i + 1) % platform.waypointData.Count;
            Handles.DrawLine(platform.waypointData[i].position, platform.waypointData[nextIndex].position);
            
            // Draw waypoint spheres
            Handles.color = Color.yellow;
            if (Handles.Button(platform.waypointData[i].position, Quaternion.identity, 0.15f, 0.15f, Handles.SphereHandleCap))
            {
                if (!Application.isPlaying)
                {
                    UpdatePlatformPreview(platform, platform.waypointData[i].position, platform.waypointData[i].rotation);
                    EditorUtility.SetDirty(platform);
                }
            }
            
            // Draw label with speed info
            string label = $"Point {i} (Spd:{platform.waypointData[i].speed:F1})";
            Handles.Label(platform.waypointData[i].position + Vector3.up * 0.5f, label);
            Handles.color = Color.cyan;
        }
    }
}
