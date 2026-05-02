using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class DungeonEnvironmentSetup
{
    private const string DungeonRootName = "DungedonAssets";
    private const string BatteryContainerName = "BatteryPickups";
    private const string BatteryAssetPath = "Assets/9V_Battery.fbx";
    private const string SampleScenePath = "Assets/Scenes/SampleScene.unity";
    private const float TargetScale = 1.3f;
    private const float FloorPaddingWorld = 0.18f;
    private const float FloorThicknessWorld = 0.38f;
    private const float FloorLiftWorld = 0.08f;
    private const float WallThicknessWorld = 0.28f;
    private const float SolidPaddingWorld = 0.06f;
    private const float MinimumSolidThicknessWorld = 0.1f;
    private const float BatteryHoverWorld = 0.16f;
    private const float BatteryTriggerHeight = 1.2f;
    private const float BatteryTriggerWidth = 0.9f;

    private static readonly string[] StraightCorridorNames =
    {
        "Corridor X2",
        "Corridor X4",
    };

    private static readonly string[] ComplexCorridorNames =
    {
        "CornerCorridor",
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

    private static readonly BatteryPlacement[] BatteryPlacements =
    {
        new("CrossCorridor", 0f, 0f),
        new("Corridor X2", 0.28f, 0f),
        new("Corridor X4", -0.35f, 0f),
        new("CornerCorridor", -0.2f, -0.2f),
        new("TCorridor", 0f, -0.2f),
    };

    static DungeonEnvironmentSetup()
    {
        EditorApplication.delayCall += ApplyToLoadedScenes;
        EditorSceneManager.sceneOpened += OnSceneOpened;
    }

    [MenuItem("Tools/Dungeon/Apply Environment Setup")]
    public static void ApplyToLoadedScenes()
    {
        if (!CanModifyScenes())
        {
            return;
        }

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
        if (!CanModifyScenes())
        {
            return;
        }

        var scene = EditorSceneManager.OpenScene(SampleScenePath, OpenSceneMode.Single);
        var changed = ApplyToScene(scene);
        if (!changed && !scene.isDirty)
        {
            return;
        }

        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        AssetDatabase.ForceReserializeAssets(new[] { SampleScenePath });
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
            var batteries = Object.FindObjectsOfType<BatteryPickup>(true).Length;
            Debug.Log(
                $"Dungeon setup summary: scale={root.transform.localScale}, boxColliders={boxColliders}, meshColliders={meshColliders}, batteries={batteries}");
        }
    }

    private static void OnSceneOpened(Scene scene, OpenSceneMode mode)
    {
        if (!CanModifyScenes())
        {
            return;
        }

        ApplyToScene(scene);
    }

    private static bool ApplyToScene(Scene scene)
    {
        if (!CanModifyScenes() || !scene.IsValid() || !scene.isLoaded)
        {
            return false;
        }

        var changed = false;
        Transform dungeonRoot = null;

        foreach (var root in scene.GetRootGameObjects())
        {
            if (root.name != DungeonRootName)
            {
                continue;
            }

            dungeonRoot = root.transform;
            changed |= ApplyToDungeonRoot(dungeonRoot);
        }

        if (dungeonRoot != null)
        {
            changed |= ApplyBatteryPickups(scene, dungeonRoot);
        }

        if (changed)
        {
            EditorSceneManager.MarkSceneDirty(scene);
        }

        return changed;
    }

    private static bool CanModifyScenes()
    {
        return !EditorApplication.isPlaying && !EditorApplication.isPlayingOrWillChangePlaymode;
    }

    private static bool ApplyToDungeonRoot(Transform dungeonRoot)
    {
        var changed = false;
        if (!Approximately(dungeonRoot.localScale, Vector3.one * TargetScale))
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
            if (Matches(objectName, StraightCorridorNames))
            {
                changed |= EnsureStraightCorridorCollision(meshFilter);
                continue;
            }

            if (Matches(objectName, ComplexCorridorNames))
            {
                changed |= EnsureComplexCorridorCollision(meshFilter);
                continue;
            }

            if (objectName == "Ind.Asset.Floor")
            {
                changed |= EnsureFloorOnlyCollision(meshFilter);
                continue;
            }

            if (Matches(objectName, SolidBoxNames))
            {
                changed |= EnsureSolidBoxCollider(meshFilter);
            }
        }

        return changed;
    }

    private static bool EnsureStraightCorridorCollision(MeshFilter meshFilter)
    {
        var changed = false;
        var collisionRoot = GetOrCreateGeneratedRoot(meshFilter.transform);
        var bounds = meshFilter.sharedMesh.bounds;
        var scale = meshFilter.transform.localScale;

        changed |= EnsureBoxChild(
            collisionRoot,
            "Floor",
            new Vector3(
                bounds.center.x,
                bounds.center.y,
                bounds.min.z + LocalDistance(FloorThicknessWorld * 0.5f + FloorLiftWorld, scale.z)),
            new Vector3(
                bounds.size.x + LocalDistance(FloorPaddingWorld, scale.x),
                bounds.size.y + LocalDistance(FloorPaddingWorld, scale.y),
                Mathf.Max(LocalDistance(FloorThicknessWorld, scale.z), bounds.size.z * 0.25f)));

        changed |= RemoveGeneratedChild(collisionRoot, "WallLeft");
        changed |= RemoveGeneratedChild(collisionRoot, "WallRight");
        changed |= RemoveParentBoxColliders(meshFilter.gameObject);
        changed |= RemoveMeshCollider(meshFilter.gameObject);
        return changed;
    }

    private static bool EnsureComplexCorridorCollision(MeshFilter meshFilter)
    {
        var changed = false;
        var collisionRoot = GetOrCreateGeneratedRoot(meshFilter.transform);
        var bounds = meshFilter.sharedMesh.bounds;
        var scale = meshFilter.transform.localScale;

        changed |= EnsureBoxChild(
            collisionRoot,
            "Floor",
            new Vector3(
                bounds.center.x,
                bounds.center.y,
                bounds.min.z + LocalDistance(FloorThicknessWorld * 0.5f + FloorLiftWorld, scale.z)),
            new Vector3(
                bounds.size.x + LocalDistance(FloorPaddingWorld, scale.x),
                bounds.size.y + LocalDistance(FloorPaddingWorld, scale.y),
                Mathf.Max(LocalDistance(FloorThicknessWorld, scale.z), bounds.size.z * 0.25f)));

        changed |= RemoveParentBoxColliders(meshFilter.gameObject);
        changed |= RemoveMeshCollider(meshFilter.gameObject);
        return changed;
    }

    private static bool EnsureFloorOnlyCollision(MeshFilter meshFilter)
    {
        var changed = false;
        var collisionRoot = GetOrCreateGeneratedRoot(meshFilter.transform);
        var bounds = meshFilter.sharedMesh.bounds;
        var scale = meshFilter.transform.localScale;

        changed |= EnsureBoxChild(
            collisionRoot,
            "Floor",
            new Vector3(
                bounds.center.x,
                bounds.center.y,
                bounds.min.z + LocalDistance(FloorThicknessWorld * 0.5f, scale.z)),
            new Vector3(
                bounds.size.x + LocalDistance(FloorPaddingWorld, scale.x),
                bounds.size.y + LocalDistance(FloorPaddingWorld, scale.y),
                Mathf.Max(LocalDistance(FloorThicknessWorld, scale.z), 0.001f)));

        changed |= RemoveParentBoxColliders(meshFilter.gameObject);
        changed |= RemoveMeshCollider(meshFilter.gameObject);
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

    private static bool RemoveMeshCollider(GameObject target)
    {
        var meshCollider = target.GetComponent<MeshCollider>();
        if (meshCollider == null)
        {
            return false;
        }

        Object.DestroyImmediate(meshCollider);
        return true;
    }

    private static bool RemoveParentBoxColliders(GameObject target)
    {
        var colliders = target.GetComponents<BoxCollider>();
        if (colliders.Length == 0)
        {
            return false;
        }

        for (var i = 0; i < colliders.Length; i++)
        {
            Object.DestroyImmediate(colliders[i]);
        }

        return true;
    }

    private static bool RemoveGeneratedChild(Transform parent, string childName)
    {
        var child = parent.Find(childName);
        if (child == null)
        {
            return false;
        }

        Object.DestroyImmediate(child.gameObject);
        return true;
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
        return ApplyBoxCollider(
            collider,
            bounds.center,
            new Vector3(
                Mathf.Max(bounds.size.x + LocalDistance(SolidPaddingWorld, scale.x), LocalDistance(MinimumSolidThicknessWorld, scale.x)),
                Mathf.Max(bounds.size.y + LocalDistance(SolidPaddingWorld, scale.y), LocalDistance(MinimumSolidThicknessWorld, scale.y)),
                Mathf.Max(bounds.size.z + LocalDistance(SolidPaddingWorld, scale.z), LocalDistance(MinimumSolidThicknessWorld, scale.z))));
    }

    private static bool ApplyBatteryPickups(Scene scene, Transform dungeonRoot)
    {
        var batteryAsset = AssetDatabase.LoadAssetAtPath<GameObject>(BatteryAssetPath);
        if (batteryAsset == null)
        {
            Debug.LogError($"Missing battery asset at {BatteryAssetPath}");
            return false;
        }

        var changed = false;
        var container = FindRoot(scene, BatteryContainerName);
        if (container == null)
        {
            container = new GameObject(BatteryContainerName);
            SceneManager.MoveGameObjectToScene(container, scene);
            changed = true;
        }

        var pickups = CollectPickupRoots(scene, container.transform);
        while (pickups.Count < BatteryPlacements.Length)
        {
            var pickup = new GameObject("BatteryPickup");
            pickup.transform.SetParent(container.transform, false);
            pickups.Add(pickup);
            changed = true;
        }

        for (var i = 0; i < BatteryPlacements.Length; i++)
        {
            changed |= ConfigurePickup(pickups[i], dungeonRoot, batteryAsset, BatteryPlacements[i]);
        }

        return changed;
    }

    private static bool ConfigurePickup(GameObject pickup, Transform dungeonRoot, GameObject batteryAsset, BatteryPlacement placement)
    {
        var changed = false;
        if (pickup.name != "BatteryPickup")
        {
            pickup.name = "BatteryPickup";
            changed = true;
        }

        var anchor = FindChildRecursive(dungeonRoot, placement.AnchorName);
        if (anchor == null)
        {
            return changed;
        }

        var anchorMesh = anchor.GetComponent<MeshFilter>();
        if (anchorMesh == null || anchorMesh.sharedMesh == null)
        {
            return changed;
        }

        var bounds = anchorMesh.sharedMesh.bounds;
        var localPoint = new Vector3(
            bounds.center.x + bounds.extents.x * placement.NormalizedOffset.x,
            bounds.center.y + bounds.extents.y * placement.NormalizedOffset.y,
            bounds.min.z + LocalDistance(BatteryHoverWorld, anchor.localScale.z));
        var worldPoint = anchor.TransformPoint(localPoint);
        if (!Approximately(pickup.transform.position, worldPoint))
        {
            pickup.transform.position = worldPoint;
            changed = true;
        }

        if (!Approximately(pickup.transform.rotation.eulerAngles, Vector3.zero))
        {
            pickup.transform.rotation = Quaternion.identity;
            changed = true;
        }

        if (!Approximately(pickup.transform.localScale, Vector3.one))
        {
            pickup.transform.localScale = Vector3.one;
            changed = true;
        }

        var boxCollider = pickup.GetComponent<BoxCollider>();
        if (boxCollider == null)
        {
            boxCollider = pickup.AddComponent<BoxCollider>();
            changed = true;
        }

        changed |= ApplyBoxCollider(
            boxCollider,
            new Vector3(0f, BatteryTriggerHeight * 0.5f, 0f),
            new Vector3(BatteryTriggerWidth, BatteryTriggerHeight, BatteryTriggerWidth),
            isTrigger: true);

        var pickupScript = pickup.GetComponent<BatteryPickup>();
        if (pickupScript == null)
        {
            pickup.AddComponent<BatteryPickup>();
            changed = true;
        }

        if (pickup.GetComponent<MeshCollider>() != null)
        {
            Object.DestroyImmediate(pickup.GetComponent<MeshCollider>());
            changed = true;
        }

        var model = pickup.transform.childCount > 0 ? pickup.transform.GetChild(0).gameObject : null;
        if (model == null)
        {
            model = (GameObject)PrefabUtility.InstantiatePrefab(batteryAsset);
            model.transform.SetParent(pickup.transform, false);
            changed = true;
        }

        if (!Approximately(model.transform.localPosition, Vector3.zero))
        {
            model.transform.localPosition = Vector3.zero;
            changed = true;
        }

        if (!model.activeSelf)
        {
            model.SetActive(true);
            changed = true;
        }

        return changed;
    }

    private static List<GameObject> CollectPickupRoots(Scene scene, Transform container)
    {
        var pickups = new List<GameObject>();
        foreach (var root in scene.GetRootGameObjects())
        {
            if (root.name != "BatteryPickup")
            {
                continue;
            }

            root.transform.SetParent(container, true);
            pickups.Add(root);
        }

        for (var i = 0; i < container.childCount; i++)
        {
            var child = container.GetChild(i).gameObject;
            if (child.name == "BatteryPickup" && !pickups.Contains(child))
            {
                pickups.Add(child);
            }
        }

        return pickups;
    }

    private static Transform GetOrCreateGeneratedRoot(Transform parent)
    {
        var generated = parent.Find("__GeneratedCollision");
        if (generated != null)
        {
            return generated;
        }

        var collisionRoot = new GameObject("__GeneratedCollision");
        collisionRoot.transform.SetParent(parent, false);
        return collisionRoot.transform;
    }

    private static bool EnsureBoxChild(Transform parent, string childName, Vector3 center, Vector3 size)
    {
        var child = parent.Find(childName);
        if (child == null)
        {
            var childObject = new GameObject(childName);
            childObject.transform.SetParent(parent, false);
            child = childObject.transform;
        }

        if (!Approximately(child.localPosition, Vector3.zero))
        {
            child.localPosition = Vector3.zero;
        }

        if (!Approximately(child.localEulerAngles, Vector3.zero))
        {
            child.localRotation = Quaternion.identity;
        }

        if (!Approximately(child.localScale, Vector3.one))
        {
            child.localScale = Vector3.one;
        }

        var boxCollider = child.GetComponent<BoxCollider>();
        if (boxCollider == null)
        {
            boxCollider = child.gameObject.AddComponent<BoxCollider>();
        }

        return ApplyBoxCollider(boxCollider, center, size);
    }

    private static bool ApplyBoxCollider(BoxCollider collider, Vector3 expectedCenter, Vector3 expectedSize, bool isTrigger = false)
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

        if (collider.isTrigger != isTrigger)
        {
            collider.isTrigger = isTrigger;
            changed = true;
        }

        return changed;
    }

    private static GameObject FindRoot(Scene scene, string name)
    {
        foreach (var root in scene.GetRootGameObjects())
        {
            if (root.name == name)
            {
                return root;
            }
        }

        return null;
    }

    private static Transform FindChildRecursive(Transform root, string name)
    {
        if (root.name == name)
        {
            return root;
        }

        for (var i = 0; i < root.childCount; i++)
        {
            var result = FindChildRecursive(root.GetChild(i), name);
            if (result != null)
            {
                return result;
            }
        }

        return null;
    }

    private static float LocalDistance(float worldDistance, float localScaleAxis)
    {
        return worldDistance / Mathf.Max(Mathf.Abs(localScaleAxis), 0.0001f);
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

    private readonly struct BatteryPlacement
    {
        public BatteryPlacement(string anchorName, float offsetX, float offsetY)
        {
            AnchorName = anchorName;
            NormalizedOffset = new Vector2(offsetX, offsetY);
        }

        public string AnchorName { get; }
        public Vector2 NormalizedOffset { get; }
    }
}
