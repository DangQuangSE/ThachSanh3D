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
        GameObject caveRoot = FindCaveRoot();
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
        Undo.RegisterCreatedObjectUndo(containerObj, "Create Torches Container");
        GameObjectUtility.SetParentAndAlign(containerObj, caveRoot);
        containerObj.transform.localPosition = Vector3.zero;
        containerObj.transform.localRotation = Quaternion.identity;

        // 3. SPAWN TORCHES
        SpawnTorchRing(containerObj.transform, torchPrefab, 8, 0.4f, 16f, 0f, "Ground_Torch");
        SpawnTorchRing(containerObj.transform, torchPrefab, 6, 28.0f, 28f, 45f, "Upper_Wall_Torch");

        // 4. AUTOMATIC ATMOSPHERE ENHANCEMENT
        ApplyAtmosphereEnhancement(caveRoot);

        Undo.CollapseUndoOperations(group);
        Selection.activeGameObject = containerObj;
        EditorGUIUtility.PingObject(containerObj);

        EditorUtility.DisplayDialog("Success!", "Added Torches to [" + caveRoot.name + "].", "I see it!");
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
            
            Vector3 centerInWorld = parent.TransformPoint(new Vector3(0, height, 0));
            torch.transform.LookAt(centerInWorld);
            
            if (tiltAngle != 0)
            {
                torch.transform.Rotate(-tiltAngle, 0, 0, Space.Self);
            }

            // ADD TORCH LIGHT
            GameObject lightObj = new GameObject("Torch_Light");
            lightObj.transform.SetParent(torch.transform);
            lightObj.transform.localPosition = new Vector3(0, 4.5f, 0); // Position at the flame point
            Light l = lightObj.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = new Color(1f, 0.55f, 0.15f); // Warm fire glow
            l.intensity = 5.0f;
            l.range = 15f;
            l.shadows = LightShadows.Soft;

            Undo.RegisterCreatedObjectUndo(torch, "Add Torch");
        }
    }

    // ──────────────────────────────────────────────────────────────────
    //  DECORATION & AESTHETICS
    // ──────────────────────────────────────────────────────────────────
    [MenuItem("HuuAnh/Decorate Boss Arena")]
    public static void DecorateBossArena()
    {
        Undo.IncrementCurrentGroup();
        Undo.SetCurrentGroupName("Decorate Boss Arena");
        int group = Undo.GetCurrentGroup();

        GameObject caveRoot = FindCaveRoot();
        if (caveRoot == null)
        {
            EditorUtility.DisplayDialog("Cave Not Found", "Please select your Cave root first.", "OK");
            return;
        }

        Transform existingDecon = caveRoot.transform.Find("Decoration_Container");
        if (existingDecon != null) Undo.DestroyObjectImmediate(existingDecon.gameObject);

        GameObject container = new GameObject("Decoration_Container");
        Undo.RegisterCreatedObjectUndo(container, "Create Decoration Container");
        GameObjectUtility.SetParentAndAlign(container, caveRoot);
        container.transform.localPosition = Vector3.zero;
        container.transform.localRotation = Quaternion.identity;

        string pebblePath = "Assets/ThachSanhGeneral/HuuAnh/Models/PolishedSurfaces/System_RockSet_Sample/Art/Prefabs/6. Pebbles/SM_Pebbles_02_Sample.prefab";
        string smallRockPath = "Assets/ThachSanhGeneral/HuuAnh/Models/PolishedSurfaces/System_RockSet_Sample/Art/Prefabs/1. Small/SM_Small_01_Sample.prefab";
        
        GameObject pebblePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(pebblePath);
        GameObject smallRockPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(smallRockPath);

        // Additional assets
        string largeRockPath = "Assets/ThachSanhGeneral/HuuAnh/Models/PolishedSurfaces/System_RockSet_Sample/Art/Prefabs/3. Large/SM_Large_01_Sample.prefab";
        string treePath = "Assets/ThachSanhGeneral/Quang/Models/Item/cayda-real.prefab";
        
        GameObject largeRockPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(largeRockPath);
        GameObject treePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(treePath);

        float arenaRadius = 35f;
        float groundY = 10.05f;

        ScatterObjects(container.transform, pebblePrefab, 60, arenaRadius, groundY, 0.5f, 1.5f, "Pebble");
        ScatterObjects(container.transform, smallRockPrefab, 25, arenaRadius, groundY, 0.8f, 2.2f, "SmallRock");
        
        // Scatter some large boulders near edges
        ScatterObjects(container.transform, largeRockPrefab, 8, arenaRadius - 5, groundY, 1.5f, 3.0f, "LargeBoulder");

        // Add specific wall decorations
        AddWallCharms(container.transform, arenaRadius);
        AddBanyanTrees(container.transform, treePrefab, arenaRadius);
        AddMonoliths(container.transform, arenaRadius);

        ApplyAtmosphereEnhancement(caveRoot);

        Undo.CollapseUndoOperations(group);
        Selection.activeGameObject = container;
        EditorUtility.DisplayDialog("Success!", "Arena decorated with pebbles, rocks, and lighting.", "Awesome");
    }

    private static void ScatterObjects(Transform parent, GameObject prefab, int count, float radius, float y, float minScale, float maxScale, string namePrefix)
    {
        if (prefab == null) return;
        for (int i = 0; i < count; i++)
        {
            Vector2 randCircle = Random.insideUnitCircle * radius;
            // Push small rocks away from center where player fight happens
            if (randCircle.magnitude < 5f) randCircle = randCircle.normalized * 5f;

            Vector3 pos = new Vector3(randCircle.x, y, randCircle.y);
            GameObject obj = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            obj.name = namePrefix + "_" + i;
            obj.transform.SetParent(parent);
            obj.transform.localPosition = pos;
            obj.transform.localRotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
            obj.transform.localScale = Vector3.one * Random.Range(minScale, maxScale);
            Undo.RegisterCreatedObjectUndo(obj, "Scatter " + namePrefix);
        }
    }

    private static void AddWallCharms(Transform parent, float radius)
    {
        // Use Magic Circle texture for "Bùa" on walls
        string texPath = "Assets/ThachSanhGeneral/Phat/VFX/Hovl Studio/Magic effects pack/Textures/MagicCircle.png";
        Texture2D charmTex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
        if (charmTex == null) return;

        Material charmMat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        charmMat.SetTexture("_BaseMap", charmTex);
        charmMat.SetFloat("_Surface", 1); // Transparent
        charmMat.SetInt("_ZWrite", 0);
        charmMat.renderQueue = 3000;
        charmMat.name = "M_WallCharm";

        int count = 12;
        for (int i = 0; i < count; i++)
        {
            float angle = i * Mathf.PI * 2 / count;
            Vector3 dir = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
            Vector3 pos = dir * (radius + 2f) + Vector3.up * Random.Range(15f, 25f);

            GameObject charm = GameObject.CreatePrimitive(PrimitiveType.Quad);
            charm.name = "WallCharm_" + i;
            charm.transform.SetParent(parent);
            charm.transform.localPosition = pos;
            charm.transform.localScale = Vector3.one * 5f;
            charm.transform.LookAt(parent.position + Vector3.up * pos.y);
            charm.transform.Rotate(0, 180, 0); // Face the center

            charm.GetComponent<Renderer>().sharedMaterial = charmMat;
            Undo.RegisterCreatedObjectUndo(charm, "Add Wall Charm");
        }
    }

    private static void AddBanyanTrees(Transform parent, GameObject prefab, float radius)
    {
        if (prefab == null) return;
        int count = 4;
        for (int i = 0; i < count; i++)
        {
            float angle = (i * 90f + 45f) * Mathf.Deg2Rad;
            Vector3 pos = new Vector3(Mathf.Cos(angle) * (radius + 5f), 9.5f, Mathf.Sin(angle) * (radius + 5f));
            
            GameObject tree = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            tree.name = "BanyanTree_" + i;
            tree.transform.SetParent(parent);
            tree.transform.localPosition = pos;
            tree.transform.localScale = Vector3.one * 0.5f;
            tree.transform.localRotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
            Undo.RegisterCreatedObjectUndo(tree, "Add Tree");
        }
    }

    private static void AddMonoliths(Transform parent, float radius)
    {
        string monolithPath = "Assets/ThachSanhGeneral/Phat/VFX/Hovl Studio/Magic effects pack/Models/Crystal1.fbx";
        string stoneMatPath = "Assets/ThachSanhGeneral/Phat/VFX/Hovl Studio/Magic effects pack/Materials/Stone.mat";
        
        GameObject monolithModel = AssetDatabase.LoadAssetAtPath<GameObject>(monolithPath);
        Material stoneMat = AssetDatabase.LoadAssetAtPath<Material>(stoneMatPath);
        
        if (monolithModel == null) return;

        int count = 6;
        for (int i = 0; i < count; i++)
        {
            float angle = (i * 60f) * Mathf.Deg2Rad;
            Vector3 pos = new Vector3(Mathf.Cos(angle) * (radius - 5f), 10.05f, Mathf.Sin(angle) * (radius - 5f));
            
            GameObject monolith = (GameObject)PrefabUtility.InstantiatePrefab(monolithModel);
            monolith.name = "MysticalMonolith_" + i;
            monolith.transform.SetParent(parent);
            monolith.transform.localPosition = pos;
            monolith.transform.localScale = new Vector3(2f, 5f, 2f); // Make them tall and pillar-like
            monolith.transform.localRotation = Quaternion.Euler(0, Random.Range(0, 360), i * 15f);
            
            if (stoneMat != null)
            {
                Renderer[] renderers = monolith.GetComponentsInChildren<Renderer>();
                foreach (var r in renderers) r.sharedMaterial = stoneMat;
            }
            
            Undo.RegisterCreatedObjectUndo(monolith, "Add Monolith");
        }
    }

    // ──────────────────────────────────────────────────────────────────
    //  GROUND FLOOR GENERATOR (SINGLE FLAT PIECE)
    // ──────────────────────────────────────────────────────────────────
    [MenuItem("HuuAnh/Generate Ground Floor")]
    public static void GenerateGroundFloor()
    {
        Undo.IncrementCurrentGroup();
        Undo.SetCurrentGroupName("Generate Flat Ground Floor");
        int group = Undo.GetCurrentGroup();

        GameObject caveRoot = FindCaveRoot();
        if (caveRoot == null)
        {
            EditorUtility.DisplayDialog("Cave Not Found", "Select Cave root first.", "OK");
            return;
        }

        // --- Material Setup (Using your new Ravine Floor) ---
        string customMatPath = "Assets/ThachSanhGeneral/HuuAnh/Models/M_RavineFloor.mat";
        string customTexPath = "Assets/ThachSanhGeneral/HuuAnh/Models/ravine_cliff_floor_texture_1773156108552.png";
        
        Material groundMat = AssetDatabase.LoadAssetAtPath<Material>(customMatPath);
        Texture2D groundTex = AssetDatabase.LoadAssetAtPath<Texture2D>(customTexPath);

        if (groundMat != null && groundTex != null)
        {
            // Auto-link texture to material if it's empty (handles both URP and Standard shaders)
            if (groundMat.HasProperty("_BaseMap") && groundMat.GetTexture("_BaseMap") == null) 
                groundMat.SetTexture("_BaseMap", groundTex);
            else if (groundMat.HasProperty("_MainTex") && groundMat.GetTexture("_MainTex") == null)
                groundMat.SetTexture("_MainTex", groundTex);
                
            EditorUtility.SetDirty(groundMat);
            
            // Ensure Tiling is detailed for the large arena
            groundMat.mainTextureScale = new Vector2(25, 25);
        }
        else if (groundMat == null)
        {
            // Fallback to sample ground if custom one isn't found
            string fallbackMatPath = "Assets/ThachSanhGeneral/HuuAnh/Models/PolishedSurfaces/System_RockSet_Sample/Art/Materials/2. URP/5. Ground/M_RockSet_01_Ground_01_Sample.mat";
            groundMat = AssetDatabase.LoadAssetAtPath<Material>(fallbackMatPath);
        }

        Transform existingGround = caveRoot.transform.Find("Ground_Container");
        if (existingGround != null) Undo.DestroyObjectImmediate(existingGround.gameObject);

        GameObject container = new GameObject("Ground_Container");
        Undo.RegisterCreatedObjectUndo(container, "Create Ground Container");
        GameObjectUtility.SetParentAndAlign(container, caveRoot);
        container.transform.localPosition = Vector3.zero;
        container.transform.localRotation = Quaternion.identity;

        GameObject flatFloor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        flatFloor.name = "Arena_FlatFloor_SinglePiece";
        flatFloor.transform.SetParent(container.transform);
        flatFloor.transform.localPosition = new Vector3(0, 10.0f, 0); // Absolute flat Y=10
        flatFloor.transform.localRotation = Quaternion.identity;
        
        // Scale 20x20 covers a huge area (200x200 units)
        flatFloor.transform.localScale = new Vector3(20f, 1f, 20f);

        if (groundMat != null)
        {
            Renderer r = flatFloor.GetComponent<Renderer>();
            Material instanceMat = new Material(groundMat);
            instanceMat.name = "M_Arena_Floor_Instanced";
            instanceMat.mainTextureScale = new Vector2(20, 20); // Repeat texture
            r.sharedMaterial = instanceMat;
        }

        Undo.RegisterCreatedObjectUndo(flatFloor, "Create Flat Floor");

        // Disable any other objects named "Ground" or "Plane" to avoid overlap
        GameObject[] allObs = GameObject.FindObjectsOfType<GameObject>();
        foreach (var obj in allObs)
        {
            string n = obj.name.ToLower();
            if ((n.Contains("ground_") || n.Contains("plane") || n.Contains("floor")) && 
                obj != flatFloor && obj != container &&  
                (obj.transform.parent == null || !obj.transform.IsChildOf(container.transform)))
            {
                obj.SetActive(false);
            }
        }

        Undo.CollapseUndoOperations(group);
        Selection.activeGameObject = container;
        EditorUtility.DisplayDialog("Success", "Đã tạo 1 MIẾNG SÀN DUY NHẤT phẳng tại Y=10!", "OK");
    }

    [MenuItem("HuuAnh/Add Light Shaft to Cave")]
    public static void AddLightShaftToCave()
    {
        Undo.IncrementCurrentGroup();
        Undo.SetCurrentGroupName("Add Light Shaft");
        int group = Undo.GetCurrentGroup();
        GameObject caveRoot = FindCaveRoot();
        if (caveRoot == null) return;
        ApplyAtmosphereEnhancement(caveRoot);
        Undo.CollapseUndoOperations(group);
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

        Transform existingShaft = caveRoot.transform.Find("Cave_TopShaftLight");
        if (existingShaft != null) Undo.DestroyObjectImmediate(existingShaft.gameObject);

        GameObject shaftObj = new GameObject("Cave_TopShaftLight");
        shaftObj.transform.SetParent(caveRoot.transform);
        shaftObj.transform.localPosition = new Vector3(0, 50f, 0);
        shaftObj.transform.localRotation = Quaternion.Euler(90, 0, 0);
        Light shaftLight = shaftObj.AddComponent<Light>();
        shaftLight.type = LightType.Spot;
        shaftLight.intensity = 30f;
        shaftLight.color = new Color(0.7f, 0.85f, 1f);
        shaftLight.range = 100f;
        shaftLight.spotAngle = 40f;
        shaftLight.shadows = LightShadows.Soft;
        Undo.RegisterCreatedObjectUndo(shaftObj, "Add Shaft Light");
    }
}
