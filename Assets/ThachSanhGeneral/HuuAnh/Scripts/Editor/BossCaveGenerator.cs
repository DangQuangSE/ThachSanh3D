using UnityEngine;
using UnityEditor;

public class BossCaveGenerator : MonoBehaviour
{
    [MenuItem("HuuAnh/Generate Boss Cave (Polished Surfaces)")]
    public static void GenerateCave()
    {
        // Define prefab paths
        string basePath = "Assets/ThachSanhGeneral/HuuAnh/Models/PolishedSurfaces/System_RockSet_Sample/Art/Prefabs/";
        string[] largeRockPaths = new string[]
        {
            basePath + "3. Large/SM_Large_01_Sample.prefab",
            basePath + "8. Structures/PF_Sample_Large_01.prefab"
        };
        string groundPath = basePath + "5. Ground/SM_Ground_01_Sample.prefab";
        string structureGroundPath = basePath + "8. Structures/PF_Sample_Ground_01.prefab";

        // Create main container
        GameObject caveRoot = new GameObject("BossCaveArena_DaiBang");
        Undo.RegisterCreatedObjectUndo(caveRoot, "Generate Boss Cave");

        // Spawn Ground
        GameObject groundPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(structureGroundPath);
        if (groundPrefab == null) groundPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(groundPath);

        if (groundPrefab != null)
        {
            for (int x = -1; x <= 1; x++)
            {
                for (int z = -1; z <= 1; z++)
                {
                    GameObject ground = (GameObject)PrefabUtility.InstantiatePrefab(groundPrefab);
                    ground.transform.SetParent(caveRoot.transform);
                    // Adjust spacing based on ground scale if necessary, assuming 10 units for now
                    ground.transform.position = new Vector3(x * 15f, 0, z * 15f);
                    Undo.RegisterCreatedObjectUndo(ground, "Spawn Ground");
                }
            }
        }
        else
        {
            Debug.LogWarning("Ground Prefab not found. Please check the path.");
        }

        // Spawn Cave Walls (Circle of Large Rocks)
        GameObject rockPrefab1 = AssetDatabase.LoadAssetAtPath<GameObject>(largeRockPaths[0]);
        GameObject rockPrefab2 = AssetDatabase.LoadAssetAtPath<GameObject>(largeRockPaths[1]);

        if (rockPrefab1 == null && rockPrefab2 == null)
        {
            Debug.LogError("Large Rock Prefab not found for cave walls!");
            return;
        }

        int numberOfRocks = 24; // Increased number of rocks
        float radius = 35f; // Double the radius
        float heightScale = 2.5f;

        // CAVE WALLS (Create horseshoe shape with entrance gap at South Z = -radius)
        for (int i = 0; i < numberOfRocks; i++)
        {
            float angle = i * Mathf.PI * 2 / numberOfRocks;
            
            // Skip 60 degree angle to create entrance (from 240 to 300 degrees)
            float degrees = angle * Mathf.Rad2Deg;
            if (degrees > 230f && degrees < 310f) continue;

            // Add random jitter so walls aren't perfectly aligned
            float jitterX = Random.Range(-3f, 3f);
            float jitterZ = Random.Range(-3f, 3f);

            float x = Mathf.Cos(angle) * radius + jitterX;
            float z = Mathf.Sin(angle) * radius + jitterZ;

            GameObject selectedPrefab = (i % 2 == 0 && rockPrefab2 != null) ? rockPrefab2 : rockPrefab1;
            if (selectedPrefab == null) selectedPrefab = rockPrefab1 ?? rockPrefab2;

            GameObject wallRock = (GameObject)PrefabUtility.InstantiatePrefab(selectedPrefab);
            wallRock.transform.SetParent(caveRoot.transform);
            wallRock.transform.position = new Vector3(x, -3f, z); // Sink rocks slightly into ground
            
            // Random rotation on all axes for natural rock walls
            wallRock.transform.rotation = Quaternion.Euler(Random.Range(-15f, 15f), Random.Range(0, 360f), Random.Range(-15f, 15f));
            
            // Rotate large rock face toward arena center
            wallRock.transform.LookAt(caveRoot.transform.position + new Vector3(0, wallRock.transform.position.y, 0));
            // Tilt backward slightly
            wallRock.transform.Rotate(-15, 0, 0);

            float randomScaleX = Random.Range(1.8f, 3.5f);
            float randomScaleY = Random.Range(2.5f, 3.5f) * heightScale;
            wallRock.transform.localScale = new Vector3(randomScaleX, randomScaleY, randomScaleX);

            Undo.RegisterCreatedObjectUndo(wallRock, "Spawn Cave Wall");
        }

        // CAVE CEILING (Multiple layers of rocks converging higher)
        for (int j = 0; j < 2; j++) // 2 concentric circles create dome
        {
            float ceilingRadius = radius * (0.8f - j * 0.4f);
            float ceilingHeight = 25f + j * 10f; // Inner ring higher than outer ring
            
            for (int i = 0; i < numberOfRocks / (j + 1); i++)
            {
                float angle = i * Mathf.PI * 2 / (numberOfRocks / (j + 1));
                float x = Mathf.Cos(angle) * ceilingRadius;
                float z = Mathf.Sin(angle) * ceilingRadius;

                GameObject selectedPrefab = (i % 2 == 0 && rockPrefab2 != null) ? rockPrefab2 : rockPrefab1;
                if (selectedPrefab == null) selectedPrefab = rockPrefab1 ?? rockPrefab2;

                GameObject ceilingRock = (GameObject)PrefabUtility.InstantiatePrefab(selectedPrefab);
                ceilingRock.transform.SetParent(caveRoot.transform);
                ceilingRock.transform.position = new Vector3(x, ceilingHeight, z);
                
                // Tilt rocks pointing straight toward center overhead
                Vector3 centerPoint = new Vector3(0, ceilingHeight + 20f, 0);
                ceilingRock.transform.rotation = Quaternion.LookRotation(centerPoint - ceilingRock.transform.position) * Quaternion.Euler(90, 0, Random.Range(0,360f));
                
                float randomScale = Random.Range(3.0f, 4.5f); // Larger ceiling rocks for better coverage
                ceilingRock.transform.localScale = new Vector3(randomScale, randomScale * 0.5f, randomScale);

                Undo.RegisterCreatedObjectUndo(ceilingRock, "Spawn Cave Ceiling");
            }
        }

        Selection.activeGameObject = caveRoot;
        Debug.Log("Boss Cave Arena created! You can view it in the Scene.");
    }

    [MenuItem("HuuAnh/Fix Dark Scene (Restore Default Brightness)")]
    public static void RestoreBrightEnvironment()
    {
        // 1. Brighten Directional Light (Sun)
        Light[] allLights = GameObject.FindObjectsOfType<Light>();
        foreach (Light light in allLights)
        {
            if (light.type == LightType.Directional)
            {
                Undo.RecordObject(light, "Restore Directional Light");
                light.intensity = 1.0f; 
                light.color = Color.white; 
            }
        }

        // 2. Disable fog
        RenderSettings.fog = false;

        // 3. Restore bright ambient lighting (Ambient Reflection)
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.8f, 0.8f, 0.8f);

        Debug.Log("Default lighting restored! If sky is still black, go to Window > Rendering > Lighting and drag a Material into the Skybox Material slot.");
    }
}
