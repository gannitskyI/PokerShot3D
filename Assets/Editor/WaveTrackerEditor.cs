using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(WaveTracker))]
public class WaveTrackerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        WaveTracker tracker = (WaveTracker)target;

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Отладка волны", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Живых врагов:", tracker.LiveEnemies.ToString());

        if (GUILayout.Button("Симулировать конец волны"))
        {
            tracker.OnWaveCompleted.Invoke();
        }
    }
}