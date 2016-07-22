using System.IO;
using UnityEditor;
using UnityEngine;

// TODO compile to dll
public class AssetImportCop : AssetPostprocessor
{
    AssetRule FindRuleForAsset(string path)
    {
        return SearchRecursive(path);
    }

    private AssetRule SearchRecursive(string path)
    {
        foreach (var findAsset in AssetDatabase.FindAssets("t:AssetRule", new[] {Path.GetDirectoryName(path)}))
        {
            var p = Path.GetDirectoryName(AssetDatabase.GUIDToAssetPath(findAsset));
            if (p == Path.GetDirectoryName(path))
            {
                Debug.Log("Found AssetRule for Asset Rule" + AssetDatabase.GUIDToAssetPath(findAsset));
                {
                    return AssetDatabase.LoadAssetAtPath<AssetRule>(AssetDatabase.GUIDToAssetPath(findAsset));
                }
            }
        }
        //no match so go up a level
        path = Directory.GetParent(path).FullName;
        path = path.Replace('\\','/');
        path = path.Remove(0, Application.dataPath.Length);
        path = path.Insert(0, "Assets");
        Debug.Log("Searching: " + path);
       if (path != "Assets")
            return SearchRecursive(path);

        //no matches
        return null;
    }

    private void OnPreprocessTexture()
    {
        AssetRule rule = FindRuleForAsset(assetImporter.assetPath);

        if (rule == null)
        {
            Debug.Log("No asset rules found for asset");
            return;
        }

        Debug.Log("Modifying Texture settings");
        rule.ApplySettings(assetImporter);
    }

    private void OnPreprocessModel()
    {
        AssetRule rule = FindRuleForAsset(assetImporter.assetPath);

        if (rule == null)
        {
            Debug.Log("No asset rules found for asset");
            return;
        }
        rule.ApplySettings(assetImporter);
    }
}
