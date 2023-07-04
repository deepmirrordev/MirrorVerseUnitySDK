#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.AI;
using UnityEditor;
using MirrorVerse.Options;

namespace MirrorVerse.Editor
{
    [CustomEditor(typeof(NavigationOptions))]
    public class NavigationOptionsEditor : UnityEditor.Editor
    {
        private NavigationOptions _navigationOptions;

        private void OnEnable()
        {
            _navigationOptions = (NavigationOptions)target;
        }

        private void NavAgentOverridesGUI()
        {
            // Draw title of the panel.
            EditorGUILayout.Space(20);
            GUILayout.Label("Nav Agent Radius Overriding", EditorStyles.boldLabel);
            GUILayout.Label("Overrides the Radius value for each Nav Agent defined in Navigation system settings.", EditorStyles.wordWrappedMiniLabel);

            var fontStyle = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Italic };
            int settingsCount = NavMesh.GetSettingsCount();

            // Loop for each agent type.
            for (int i = 0; i < settingsCount; i++)
            {
                var settings = NavMesh.GetSettingsByIndex(i);

                EditorGUILayout.Space(20);

                // Draw agent name
                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.Label("Name", GUILayout.Width(80));
                    GUILayout.Label(NavMesh.GetSettingsNameFromID(settings.agentTypeID), fontStyle, GUILayout.Width(100));
                }
                EditorGUILayout.EndHorizontal();

                // Draw radius and input box for overriding
                float newRadiusValue;
                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.Label("Radius", GUILayout.Width(80));
                    GUILayout.Label(settings.agentRadius.ToString(), GUILayout.Width(40));
                    GUILayout.Label("→", GUILayout.Width(20));
                    float radiusValue = settings.agentRadius;
                    float? radiusOverride = _navigationOptions.GetRadiusOverride(settings.agentTypeID);
                    if (radiusOverride.HasValue)
                    {
                        radiusValue = radiusOverride.Value;
                    }
                    newRadiusValue = EditorGUILayout.FloatField(radiusValue);
                    if (newRadiusValue != radiusValue)
                    {
                        _navigationOptions.SetRadiusOverride(settings.agentTypeID, newRadiusValue);
                        EditorUtility.SetDirty(target);
                    }
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                {
                    // Draw other values of the agent.
                    EditorGUILayout.BeginVertical(GUILayout.Width(150));
                    {
                        EditorGUILayout.BeginHorizontal();
                        Color backupColor = GUI.color;
                        GUI.color = Color.gray;
                        GUILayout.Label("Height", GUILayout.Width(80));
                        GUILayout.Label(settings.agentHeight.ToString(), GUILayout.Width(40));
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Label("Step Height", GUILayout.Width(80));
                        GUILayout.Label(settings.agentClimb.ToString(), GUILayout.Width(40));
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Label("Max Slope", GUILayout.Width(80));
                        GUILayout.Label(settings.agentSlope.ToString(), GUILayout.Width(40));
                        GUI.color = backupColor;
                        EditorGUILayout.EndHorizontal();
                    }
                    EditorGUILayout.EndVertical();

                    // Draw agent diagram.
                    Rect rect = EditorGUILayout.GetControlRect(false, 80, GUILayout.MaxWidth(160));
                    int backupSize = GUI.skin.label.fontSize;
                    GUI.skin.label.fontSize = (int)(backupSize * 0.6); // 60% fontsize for the agent diagram.
                    UnityEditor.AI.NavMeshEditorHelpers.DrawAgentDiagram(rect, newRadiusValue, settings.agentHeight, settings.agentClimb, settings.agentSlope);
                    GUI.skin.label.fontSize = backupSize;
                }
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.Space(20);
            
            // Draw button to open navigation setting window.
            if (GUILayout.Button("Open Navigation Settings"))
            {
                UnityEditor.AI.NavMeshEditorHelpers.OpenAgentSettings(0);
            }
        }


        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            NavAgentOverridesGUI();
        }
    }
}
#endif
