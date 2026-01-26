using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(WaveTracker))]
public class WaveTrackerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();  // рисует все поля класса

        WaveTracker tracker = (WaveTracker)target;

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Отладка волны", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Живых врагов:", tracker.activeEnemies.Count.ToString());

        if (GUILayout.Button("Симулировать конец волны"))
        {
            tracker.OnWaveCompleted.Invoke();
        }

        if (GUILayout.Button("Очистить список вручную"))
        {
            tracker.activeEnemies.Clear();
        }
    }
}