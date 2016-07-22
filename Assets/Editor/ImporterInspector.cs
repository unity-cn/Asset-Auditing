using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(TextureImporter))]
public class ImporterInspector : Editor {

	

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
    }

}
