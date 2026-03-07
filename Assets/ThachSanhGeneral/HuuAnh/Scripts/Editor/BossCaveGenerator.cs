using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class BossCaveGenerator : MonoBehaviour
{
    [MenuItem("HuuAnh/Add Torches to Cave")]
    public static void AddTorchesToCave()
    {
        Undo.IncrementCurrentGroup();
        Undo.SetCurrentGroupName("Add Torches to Cave");
        int group = Undo.GetCurrentGroup();

        string torchPath = "Assets/ThachSanhGeneral/Quang/Models/Item/DuocFire.prefab";
        GameObject torchPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(torchPath);
        
        if (torchPrefab == null)
        {
            EditorUtility.DisplayDialog("Error", "Torch Prefab (DuocFire) not found at:\n" + torchPath, "OK");
            return;
        }

        // 1. Robust cave search
        GameObject caveRoot = GameObject.Find("CaveArena_PrincessRescue");
        if (caveRoot == null) caveRoot = GameObject.Find("BossCaveArena_PrincessRescue");
        
        // If not found by exact name, look for anything with "PrincessRescue"
        if (caveRoot == null)
        {
            GameObject[] allRoots = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
            foreach (var root in allRoots)
            {
                if (root.name.ToLower().Contains("princess") && root.name.ToLower().Contains("rescue"))
                {
                    caveRoot = root;
                    break;
                }
            }
        }

        // Fallback to selection
        if (caveRoot == null) caveRoot = Selection.activeGameObject;

        if (caveRoot == null)
        {
            EditorUtility.DisplayDialog("Cave Not Found", "Please CLICK on your Cave object in the Hierarchy first, then run this menu.", "OK");
            return;
        }

        Debug.LogWarning("--- STARTING TORCH SETUP FOR: " + caveRoot.name + " ---");

        // 2. Setup Hierarchy Container
        Transform existingContainer = caveRoot.transform.Find("Torches_Container");
        if (existingContainer != null) Undo.DestroyObjectImmediate(existingContainer.gameObject);

        GameObject containerObj = new GameObject("Torches_Container");
        // Use Selection-aware parenting for better editor behavior
        Undo.RegisterCreatedObjectUndo(containerObj, "Create Torches Container");
        GameObjectUtility.SetParentAndAlign(containerObj, caveRoot);
        containerObj.transform.localPosition = Vector3.zero;
        containerObj.transform.localRotation = Quaternion.identity;

        // 3. SPAWN TORCHES
        // Ground Ring: 8 torches around central area where Player/Boss stand
        SpawnTorchRing(containerObj.transform, torchPrefab, 8, 0.4f, 16f, 0f, "Ground_Torch");

        // Upper Ring: 6 torches high on the wall
        SpawnTorchRing(containerObj.transform, torchPrefab, 6, 28.0f, 28f, 45f, "Upper_Wall_Torch");

        // 4. AUTOMATIC ATMOSPHERE ENHANCEMENT
        ApplyAtmosphereEnhancement(caveRoot);

        Undo.CollapseUndoOperations(group);
        
        // SELECT THE FOLDER to show the user where it is
        Selection.activeGameObject = containerObj;
        EditorGUIUtility.PingObject(containerObj);

        EditorUtility.DisplayDialog("Success!", "Added Torches to [" + caveRoot.name + "].\nCheck the selected 'Torches_Container' in Hierarchy.", "I see it!");
        Debug.LogWarning("SUCCESS: Torches added under " + caveRoot.name + "/Torches_Container. OBJECT SELECTED.");
    }

    private static void SpawnTorchRing(Transform parent, GameObject prefab, int count, float height, float radius, float tiltAngle, string namePrefix)
    {
        for (int i = 0; i < count; i++)
        {
            float angle = i * Mathf.PI * 2 / count;
            Vector3 localPos = new Vector3(Mathf.Cos(angle) * radius, height, Mathf.Sin(angle) * radius);
            
            GameObject torch = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            torch.name = namePrefix + "_" + i;
            torch.transform.SetParent(parent);
            torch.transform.localPosition = localPos;
            
            // Point towards center
            Vector3 centerInWorld = parent.TransformPoint(new Vector3(0, height, 0));
            torch.transform.LookAt(centerInWorld);
            
            // Tilt if needed (Wall torches)
            if (tiltAngle != 0)
            {
                torch.transform.Rotate(-tiltAngle, 0, 0, Space.Self);
            }

            Undo.RegisterCreatedObjectUndo(torch, "Add Torch");
        }
    }

    [MenuItem("HuuAnh/Add Light Shaft to Cave")]
    public static void AddLightShaftToCave()
    {
        Undo.IncrementCurrentGroup();
        Undo.SetCurrentGroupName("Add Light Shaft");
        int group = Undo.GetCurrentGroup();

        GameObject caveRoot = FindCaveRoot();
        if (caveRoot == null)
        {
            EditorUtility.DisplayDialog("Cave Not Found", "Please select your Cave in the Hierarchy first.", "OK");
            return;
        }

        ApplyAtmosphereEnhancement(caveRoot);

        Undo.CollapseUndoOperations(group);
        EditorUtility.DisplayDialog("Success!", "Light Shaft and Atmosphere applied to [" + caveRoot.name + "].", "Brilliant!");
    }

    private static GameObject FindCaveRoot()
    {
        GameObject caveRoot = GameObject.Find("CaveArena_PrincessRescue");
        if (caveRoot == null) caveRoot = GameObject.Find("BossCaveArena_PrincessRescue");
        
        if (caveRoot == null)
        {
            GameObject[] allRoots = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
            foreach (var root in allRoots)
            {
                if (root.name.ToLower().Contains("princess") && root.name.ToLower().Contains("rescue"))
                    return root;
            }
        }
        return Selection.activeGameObject;
    }

    private static void ApplyAtmosphereEnhancement(GameObject caveRoot)
    {
        Undo.RecordObject(Unsupported.GetRenderSettings(), "Update Atmosphere");

        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.18f, 0.18f, 0.22f); 
        RenderSettings.fog = true;
        RenderSettings.fogColor = new Color(0.08f, 0.08f, 0.12f);
        RenderSettings.fogDensity = 0.012f;

        // Internal Fill Light
        Transform existingFill = caveRoot.transform.Find("Cave_InternalFillLight");
        if (existingFill != null) Undo.DestroyObjectImmediate(existingFill.gameObject);

        GameObject fillObj = new GameObject("Cave_InternalFillLight");
        fillObj.transform.SetParent(caveRoot.transform);
        fillObj.transform.localPosition = new Vector3(0, 15f, 0);
        Light fillLight = fillObj.AddComponent<Light>();
        fillLight.type = LightType.Point;
        fillLight.intensity = 1.6f;
        fillLight.range = 80f;
        fillLight.color = new Color(1f, 0.9f, 0.75f);
        Undo.RegisterCreatedObjectUndo(fillObj, "Add Fill Light");

        // Top Shaft Light
        Transform existingShaft = caveRoot.transform.Find("Cave_TopShaftLight");
        if (existingShaft != null) Undo.DestroyObjectImmediate(existingShaft.gameObject);

        GameObject shaftObj = new GameObject("Cave_TopShaftLight");
        shaftObj.transform.SetParent(caveRoot.transform);
        shaftObj.transform.localPosition = new Vector3(0, 50f, 0);
        shaftObj.transform.localRotation = Quaternion.Euler(90, 0, 0);
        Light shaftLight = shaftObj.AddComponent<Light>();
        shaftLight.type = LightType.Spot;
        shaftLight.intensity = 30f; // Extra power for the shaft logic
        shaftLight.color = new Color(0.7f, 0.85f, 1f);
        shaftLight.range = 100f;
        shaftLight.spotAngle = 40f;
        shaftLight.shadows = LightShadows.Soft;
        Undo.RegisterCreatedObjectUndo(shaftObj, "Add Shaft Light");
    }
}
