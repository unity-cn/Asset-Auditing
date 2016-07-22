using System;
using UnityEditor;

[System.Serializable]
public struct MeshImporterSettings
{
    public bool readWriteEnabled;
    public bool optimiseMesh;
    public bool ImportBlendShapes;

    public static MeshImporterSettings Extract(ModelImporter importer)
    {
        if (importer == null)
            throw new ArgumentException();

        MeshImporterSettings settings = new MeshImporterSettings();
        settings.readWriteEnabled = importer.isReadable;
        settings.optimiseMesh = importer.optimizeMesh;
        settings.ImportBlendShapes = importer.importBlendShapes;
        return settings;
    }

    public static bool Equal(MeshImporterSettings a, MeshImporterSettings b)
    {
        return (a.readWriteEnabled == b.readWriteEnabled) && (a.optimiseMesh == b.optimiseMesh) && ( a.ImportBlendShapes == b.ImportBlendShapes);
    }

    public void ApplyDefaults()
    {
        readWriteEnabled = false;
        optimiseMesh = true;
        ImportBlendShapes = false;
    }
}