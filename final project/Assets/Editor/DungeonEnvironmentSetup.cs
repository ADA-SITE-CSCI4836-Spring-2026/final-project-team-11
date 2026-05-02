using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class DungeonEnvironmentSetup
{
    private const string DungeonRootName = "DungedonAssets";
    private const string SampleScenePath = "Assets/Scenes/SampleScene.unity";
    private const float TargetScale = 1.3f;
    private const float FloorPaddingWorld = 0.12f;
    private const float FloorThicknessWorld = 0.22f;
    private const float FloorLiftWorld = 0.02f;
    private const float SolidPaddingWorld = 0.05f;
    private const float MinimumSolidThicknessWorld = 0.1f;

    private static readonly string[] ComplexCorridorNames =
    {
        "CornerCorridor",
        "Corridor X2",
        "Corridor X4",
        "CrossCorridor",
        "TCorridor",
    };

    private static readonly string[] SolidBoxNames =
    {
        "Ind.Asset.Pillar",
        "Ind.Asset.SquareWall",
        "Ind.Asset.Statue",
        "Ind.Asset.Wall",
    };

    static DungeonEnvironmentSetup()
    {
        EditorApplication.delayCall += ApplyToLoadedScenes;
        EditorSceneManager.sceneOpened += OnSceneOpened;
    }

    [MenuItem("Tools/Dungeon/Apply Environment Setup")]
    public static void ApplyToLoadedScenes()
    {
        for (var i = 0; i < SceneManager.sceneCount; i++)
        {
            var scene = SceneManager.GetSceneAt(i);
            if (!scene.IsValid() || !scene.isLoaded)
            {
                continue;
            }

            ApplyToScene(scene);
        }
    }

    [MenuItem("Tools/Dungeon/Apply Setup To Sample Scene")]
    public static void ApplyToSampleScene()
    {
        var scene = EditorSceneManager.OpenScene(SampleScenePath, OpenSceneMode.Single);
        var changed = ApplyToScene(scene);
        if (changed)
        {
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.ForceReserializeAssets(new[] { SampleScenePath });
        }
    }

    [MenuItem("Tools/Dungeon/Log Sample Scene Setup")]
    public static void LogSampleSceneSetup()
    {
        var scene = EditorSceneManager.OpenScene(SampleScenePath, OpenSceneMode.Single);
        foreach (var root in scene.GetRootGameObjects())
        {
            if (root.name != DungeonRootName)
            {
                continue;
            }

            var meshColliders = root.GetComponentsInChildren<MeshCollider>(true).Length;
            var boxColliders = root.GetComponentsInChildren<BoxCollider>(true).Length;
            Debug.Log(
                $"Dungeon setup summary: scale={root.transform.localScale}, boxColliders={boxColliders}, meshColliders={meshColliders}");
        }
    }

    private static void OnSceneOpened(Scene scene, OpenSceneMode mode)
    {
        ApplyToScene(scene);
    }

    private static bool ApplyToScene(Scene scene)
    {
        var changed = false;
        foreach (var root in scene.GetRootGameObjects())
        {
            if (root.name != DungeonRootName)
            {
                continue;
            }

            changed |= ApplyToDungeonRoot(root.transform);
        }

        if (changed)
        {
            EditorSceneManager.MarkSceneDirty(scene);
        }

        return changed;
    }

    private static bool ApplyToDungeonRoot(Transform dungeonRoot)
    {
        var changed = false;
        if (Approximately(dungeonRoot.localScale, Vector3.one))
        {
            dungeonRoot.localScale = Vector3.one * TargetScale;
            changed = true;
        }

        foreach (var meshFilter in dungeonRoot.GetComponentsInChildren<MeshFilter>(true))
        {
            if (meshFilter.sharedMesh == null)
            {
                continue;
            }

            var objectName = meshFilter.gameObject.name;
            if (Matches(objectName, ComplexCorridorNames))
            {
                changed |= EnsureFloorBoxCollider(meshFilter);
                changed |= EnsureMeshCollider(meshFilter);
                continue;
            }

            if (objectName == "Ind.Asset.Floor")
            {
                changed |= EnsureFloorBoxCollider(meshFilter);
                continue;
            }

            if (Matches(objectName, SolidBoxNames))
            {
                changed |= EnsureSolidBoxCollider(meshFilter);
            }
        }

        return changed;
    }

    private static bool EnsureMeshCollider(MeshFilter meshFilter)
    {
        var meshCollider = meshFilter.GetComponent<MeshCollider>();
        if (meshCollider == null)
        {
            meshCollider = meshFilter.gameObject.AddComponent<MeshCollider>();
        }

        var changed = false;
        if (meshCollider.sharedMesh != meshFilter.sharedMesh)
        {
            meshCollider.sharedMesh = meshFilter.sharedMesh;
            changed = true;
        }

        if (meshCollider.convex)
        {
            meshCollider.convex = false;
            changed = true;
        }

        return changed;
    }

    private static bool EnsureFloorBoxCollider(MeshFilter meshFilter)
    {
        var collider = meshFilter.GetComponent<BoxCollider>();
        if (collider == null)
        {
            collider = meshFilter.gameObject.AddComponent<BoxCollider>();
        }

        var bounds = meshFilter.sharedMesh.bounds;
        var scale = meshFilter.transform.localScale;
        var expectedCenter = new Vector3(
            bounds.center.x,
            bounds.center.y,
            bounds.min.z + (FloorThicknessWorld / Mathf.Abs(scale.z) * 0.5f) + (FloorLiftWorld / Mathf.Abs(scale.z)));
        var expectedSize = new Vector3(
            bounds.size.x + FloorPaddingWorld / Mathf.Abs(scale.x),
            bounds.size.y + FloorPaddingWorld / Mathf.Abs(scale.y),
            Mathf.Max(FloorThicknessWorld / Mathf.Abs(scale.z), bounds.size.z * 0.15f));

        return ApplyBoxCollider(collider, expectedCenter, expectedSize);
    }

    private static bool EnsureSolidBoxCollider(MeshFilter meshFilter)
    {
        var collider = meshFilter.GetComponent<BoxCollider>();
        if (collider == null)
        {
            collider = meshFilter.gameObject.AddComponent<BoxCollider>();
        }

        var bounds = meshFilter.sharedMesh.bounds;
        var scale = meshFilter.transform.localScale;
        var expectedCenter = bounds.center;
        var expectedSize = new Vector3(
            Mathf.Max(bounds.size.x + SolidPaddingWorld / Mathf.Abs(scale.x), MinimumSolidThicknessWorld / Mathf.Abs(scale.x)),
            Mathf.Max(bounds.size.y + SolidPaddingWorld / Mathf.Abs(scale.y), MinimumSolidThicknessWorld / Mathf.Abs(scale.y)),
            Mathf.Max(bounds.size.z + SolidPaddingWorld / Mathf.Abs(scale.z), MinimumSolidThicknessWorld / Mathf.Abs(scale.z)));

        return ApplyBoxCollider(collider, expectedCenter, expectedSize);
    }

    private static bool ApplyBoxCollider(BoxCollider collider, Vector3 expectedCenter, Vector3 expectedSize)
    {
        var changed = false;
        if (!Approximately(collider.center, expectedCenter))
        {
            collider.center = expectedCenter;
            changed = true;
        }

        if (!Approximately(collider.size, expectedSize))
        {
            collider.size = expectedSize;
            changed = true;
        }

        if (collider.isTrigger)
        {
            collider.isTrigger = false;
            changed = true;
        }

        return changed;
    }

    private static bool Matches(string value, string[] candidates)
    {
        for (var i = 0; i < candidates.Length; i++)
        {
            if (value == candidates[i])
            {
                return true;
            }
        }

        return false;
    }

    private static bool Approximately(Vector3 a, Vector3 b)
    {
        return Vector3.SqrMagnitude(a - b) < 0.0001f;
    }
}
