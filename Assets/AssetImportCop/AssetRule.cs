using UnityEngine;
using UnityEditor;
using System.IO;
using System;

[System.Serializable]
public enum AssetFilterType
{
	kAny,
	kTexture,
	kMesh
}

[System.Serializable]
public class AssetRule : ScriptableObject
{
	public AssetFilter filter;
	public AssetRuleImportSettings settings;

    public static AssetRule CreateAssetRule()
    {
        var assetRule = AssetRule.CreateInstance<AssetRule>();

        assetRule.ApplyDefaults();

        return assetRule;
    }

    public void ApplyDefaults()
    {
        filter.ApplyDefaults();
        settings.ApplyDefaults();
    }

    public bool IsMatch(AssetImporter importer)
	{
		return filter.IsMatch(importer);
	}

	public bool AreSettingsCorrect(AssetImporter importer)
	{
		return settings.AreSettingsCorrect(importer);
	}

	public void ApplySettings(AssetImporter importer)
	{
		settings.Apply(importer);
	}
}



[System.Serializable]
public struct AssetFilter
{
	public AssetFilterType type;
	public string path;

	public bool IsMatch(AssetImporter importer)
	{
	    if (importer == null) return false;

		AssetFilterType filterType = GetAssetFilterType(importer);
		return IsMatch(filterType, importer.assetPath);
	}

	public bool IsMatch(string path)
	{
	    if (string.IsNullOrEmpty(this.path)) return true;
		if(string.IsNullOrEmpty(path)) { return string.IsNullOrEmpty(this.path); }

		string fullPath = Path.Combine(Application.dataPath, path);

		string[] files = Directory.GetFiles(Application.dataPath, this.path);
		if(files == null)
			return false;

		for(int i = 0; i < files.Length; i++)
		{
			if(fullPath.Equals(files[i]))
				return true;
		}

		return false;
	}

	public bool IsMatch(AssetFilterType type, string path)
	{
		return (this.type == AssetFilterType.kAny || type == this.type) &&
			IsMatch(path);
	}

	public static AssetFilterType GetAssetFilterType(AssetImporter importer)
	{
		if(importer is TextureImporter)
			return AssetFilterType.kTexture;
		else if(importer is ModelImporter)
			return AssetFilterType.kMesh;

		return AssetFilterType.kAny;
	}

    public void ApplyDefaults()
    {
        type = AssetFilterType.kAny;
        path = "";
    }
}


[System.Serializable]
public struct AssetRuleImportSettings
{
	public TextureImporterSettings textureSettings;
	public MeshImporterSettings meshSettings;

	public bool AreSettingsCorrect(AssetImporter importer)
	{
		if(importer is TextureImporter)
			return AreSettingsCorrectTexture((TextureImporter)importer);
		else if(importer is ModelImporter)
			return AreSettingsCorrectModel((ModelImporter)importer);

		return true;
	}

	bool AreSettingsCorrectTexture(TextureImporter importer)
	{
		TextureImporterSettings currentSettings = new TextureImporterSettings();
		importer.ReadTextureSettings(currentSettings);

		return TextureImporterSettings.Equal(currentSettings, this.textureSettings);
	}

	bool AreSettingsCorrectModel(ModelImporter importer)
	{
		var currentSettings = MeshImporterSettings.Extract(importer);
		return MeshImporterSettings.Equal(currentSettings, this.meshSettings);
	}

	public void Apply(AssetImporter importer)
	{
		if(importer is TextureImporter)
			ApplyTextureSettings((TextureImporter)importer);
		else if(importer is ModelImporter)
			ApplyMeshSettings((ModelImporter)importer);
	}

	void ApplyTextureSettings(TextureImporter importer)
	{
	    bool dirty = false;
	    TextureImporterSettings tis = new TextureImporterSettings();
	    importer.ReadTextureSettings(tis);
	    if (!tis.mipmapEnabled == textureSettings.mipmapEnabled)
	    {
	        tis.mipmapEnabled = textureSettings.mipmapEnabled;    
	        dirty = true;
	    }
	    if (!tis.readable == textureSettings.readable)
	    {
	        tis.readable = textureSettings.readable;
	        dirty = true;
	    }
	    if (tis.maxTextureSize != textureSettings.maxTextureSize)
	    {
	        tis.maxTextureSize = textureSettings.maxTextureSize;
	        dirty = true;
	    }

	    

        // add settings as needed


	    if (dirty)
	    {
            Debug.Log("Modifying texture settings");
            importer.SetTextureSettings(tis);
	        importer.SaveAndReimport();
	    }
	    else
	    {
            Debug.Log("Texture Import Settings are Ok");	        
	    }
	}

	void ApplyMeshSettings(ModelImporter importer)
	{
		bool dirty = false;
		if(importer.isReadable != meshSettings.readWriteEnabled)
		{
			importer.isReadable = meshSettings.readWriteEnabled;
			dirty = true;
		}

	    if (importer.optimizeMesh != meshSettings.optimiseMesh)
	    {
	        importer.optimizeMesh = meshSettings.optimiseMesh;
	        dirty = true;
	    }

	    if (importer.importBlendShapes != meshSettings.ImportBlendShapes)
	    {
	        importer.importBlendShapes = meshSettings.ImportBlendShapes;
	        dirty = true;
	    }


		// Add more settings in here that you might need

	    if (dirty)
	    {
            Debug.Log("Modifying Model Import Settings, An Import will now occur and the settings will be checked to be OK again during that import");
	        importer.SaveAndReimport();
	    }
	    else
	    {
	        Debug.Log("Model Import Settings OK");
	    }
	}

    public void ApplyDefaults()
    {
        meshSettings.ApplyDefaults();
        ApplyTextureSettingDefaults();
       
    }

    private void ApplyTextureSettingDefaults()
    {
        textureSettings = new TextureImporterSettings();
        Debug.Log("texture setting defaults");
        textureSettings.maxTextureSize = 2048;
        textureSettings.mipmapEnabled = true;
    }
}
