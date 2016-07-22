using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AssetRule))]
public class AssetRuleInspector : Editor
{

    private AssetRule orig;
    [MenuItem("Assets/Create Asset Auditing Rule")]
    public static void CreateAssetRule()
    {
        var newRule = CreateInstance<AssetRule>();
        newRule.ApplyDefaults();

        string selectionpath = "Assets";
        foreach (Object obj in Selection.GetFiltered(typeof(Object), SelectionMode.Assets))
        {
            selectionpath = AssetDatabase.GetAssetPath(obj);
            if (File.Exists(selectionpath))
            {
                selectionpath = Path.GetDirectoryName(selectionpath);
            }
            break;
        }

        string newRuleFileName = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(selectionpath, "New Asset Rule.asset"));
        newRuleFileName = newRuleFileName.Replace("\\", "/");
        AssetDatabase.CreateAsset(newRule, newRuleFileName);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = newRule;
        changed = true;
    }

    // TODO: Custom inspector for asset rules
    public override void OnInspectorGUI()
    {
        var t = (AssetRule)target;

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Asset Rule Type");

        t.filter.type = (AssetFilterType) EditorGUILayout.EnumPopup(t.filter.type);

        EditorGUILayout.EndHorizontal();

        if (EditorGUI.EndChangeCheck ()) 
        {
            changed = true;
        }
        
        switch (t.filter.type)
        {
            case AssetFilterType.kAny:
                DrawTextureSettings(t);
                DrawMeshSettings(t);
            break;

            case AssetFilterType.kMesh:
                DrawMeshSettings(t);
            break;

            case AssetFilterType.kTexture:
                DrawTextureSettings(t);
            break;
        }


        if (changed) 
        {
          if (GUILayout.Button("Apply")) 
              Apply(t);
        }
    }

    private void Apply(AssetRule assetRule)
    {

        // get the directories that we do not want to apply changes to 
        List<string> dontapply = new List<string>();
        var assetrulepath = AssetDatabase.GetAssetPath(assetRule).Replace(assetRule.name +".asset","").TrimEnd('/');
        string projPath = Application.dataPath;
        projPath = projPath.Remove(projPath.Length - 6);
 
        string[] directories = Directory.GetDirectories( Path.GetDirectoryName(projPath + AssetDatabase.GetAssetPath(assetRule)) ,"*",  SearchOption.AllDirectories);
        foreach (var directory in directories)
        {
            var d = directory.Replace(Application.dataPath, "Assets");
            var appDirs = AssetDatabase.FindAssets("t:AssetRule", new[] {d});
            if (appDirs.Length != 0)
            {
                d = d.TrimEnd('/');
                d = d.Replace('\\', '/');
                dontapply.Add(d);
            }
        }


        List<string> finalAssetList = new List<string>();
        foreach (var findAsset in AssetDatabase.FindAssets("", new[] {assetrulepath}))
        {
            var asset = AssetDatabase.GUIDToAssetPath(findAsset);
            if (!File.Exists(asset)) continue;
            if (dontapply.Contains(Path.GetDirectoryName(asset))) continue;
            if (!assetRule.IsMatch(AssetImporter.GetAtPath(asset))) continue;
            if (finalAssetList.Contains(asset)) continue;
            if (asset == AssetDatabase.GetAssetPath(assetRule)) continue;
            finalAssetList.Add(asset);
        }

        int i = 1;
        foreach (var asset in finalAssetList)
        {
            AssetImporter.GetAtPath(asset).SaveAndReimport();
            i++;
        }

        changed = false;

    }

    private void DrawMeshSettings(AssetRule assetRule)
    {
        GUILayout.Space(20);
        GUILayout.Label("MESH SETTINGS ");

        // read write enabled
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Read/Write Enabled");
        assetRule.settings.meshSettings.readWriteEnabled = EditorGUILayout.Toggle(assetRule.settings.meshSettings.readWriteEnabled);
        EditorGUILayout.EndHorizontal();

        // optimise mesh
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Optimise Mesh");
        assetRule.settings.meshSettings.optimiseMesh = EditorGUILayout.Toggle(assetRule.settings.meshSettings.optimiseMesh);
        EditorGUILayout.EndHorizontal();

        // optimise mesh
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Blend Shapes");
        assetRule.settings.meshSettings.ImportBlendShapes = EditorGUILayout.Toggle(assetRule.settings.meshSettings.ImportBlendShapes);
        EditorGUILayout.EndHorizontal();



    }

    private int[] sizes = new[] {32, 64, 128, 256, 512, 1024, 2048, 4096};
    private string[] sizeStrings = new[] {"32", "64", "128", "256", "512", "1024", "2048", "4096" };
    private static bool changed = false;

    private void DrawTextureSettings(AssetRule assetRule)
    {
        GUILayout.Space(20);
        GUILayout.Label(" TEXTURE SETTINGS ");
        GUILayout.Space(20);

        GetValidPlatforms();
        // mip maps
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Generate Mip Maps");
        assetRule.settings.textureSettings.mipmapEnabled = EditorGUILayout.Toggle(assetRule.settings.textureSettings.mipmapEnabled);
        EditorGUILayout.EndHorizontal();

        //read write enabled
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Read/Write Enabled");
        assetRule.settings.textureSettings.readable = EditorGUILayout.Toggle(assetRule.settings.textureSettings.readable);
        EditorGUILayout.EndHorizontal();

        // per platform settings
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Max Texture Size");
        assetRule.settings.textureSettings.maxTextureSize =
        EditorGUILayout.IntPopup(assetRule.settings.textureSettings.maxTextureSize, sizeStrings, sizes);
        EditorGUILayout.EndHorizontal();


    }

    private void GetValidPlatforms()
    {
        
    }

    void OnEnable()
    {
        changed = false;
        orig = (AssetRule) target;
       Undo.RecordObject(target,"assetruleundo");
    }

    void OnDisable()
    {
        if (changed)
        {
            EditorUtility.SetDirty(target);
            if (EditorUtility.DisplayDialog("Unsaved Settings", "Unsaved AssetRule Changes", "Apply", "Revert"))
            {
                Apply((AssetRule)target);
            }
            else
            {
                Undo.PerformUndo();
                //SerializedObject so = new SerializedObject(target);
                //so.SetIsDifferentCacheDirty();
                //so.Update();
            }

        }
        changed = false;


    }
}