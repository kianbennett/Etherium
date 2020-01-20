using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CanEditMultipleObjects]
[CustomEditor(typeof(World))]
public class WorldEditor : Editor {

    public override void OnInspectorGUI() {
        World worldGen = (World) target;

        DrawDefaultInspector();

        if(Application.isPlaying) {
            EditorGUILayout.Space();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Generate")) worldGen.Build();
            GUILayout.EndHorizontal();
        }
    }
}