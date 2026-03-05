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
            Debug.LogWarning("Không tìm thấy Prefab mặt đất (Ground). Vui lòng kiểm tra lại đường dẫn.");
        }

        // Spawn Cave Walls (Circle of Large Rocks)
        GameObject rockPrefab1 = AssetDatabase.LoadAssetAtPath<GameObject>(largeRockPaths[0]);
        GameObject rockPrefab2 = AssetDatabase.LoadAssetAtPath<GameObject>(largeRockPaths[1]);

        if (rockPrefab1 == null && rockPrefab2 == null)
        {
            Debug.LogError("Không tìm thấy Prefab đá lớn (Large Rock) để làm vách hang!");
            return;
        }

        int numberOfRocks = 24; // Tăng số lượng đá
        float radius = 35f; // Mở rộng bán kính gấp đôi
        float heightScale = 2.5f;

        // VÁCH HANG (Tạo hình móng ngựa để hở lối vào ở phía Nam Z = -radius)
        for (int i = 0; i < numberOfRocks; i++)
        {
            float angle = i * Mathf.PI * 2 / numberOfRocks;
            
            // Bỏ qua một góc 60 độ để tạo lối vào (từ 240 độ đến 300 độ)
            float degrees = angle * Mathf.Rad2Deg;
            if (degrees > 230f && degrees < 310f) continue;

            // Thêm độ xê dịch ngẫu nhiên (jitter) để vách không thẳng băng
            float jitterX = Random.Range(-3f, 3f);
            float jitterZ = Random.Range(-3f, 3f);

            float x = Mathf.Cos(angle) * radius + jitterX;
            float z = Mathf.Sin(angle) * radius + jitterZ;

            GameObject selectedPrefab = (i % 2 == 0 && rockPrefab2 != null) ? rockPrefab2 : rockPrefab1;
            if (selectedPrefab == null) selectedPrefab = rockPrefab1 ?? rockPrefab2;

            GameObject wallRock = (GameObject)PrefabUtility.InstantiatePrefab(selectedPrefab);
            wallRock.transform.SetParent(caveRoot.transform);
            wallRock.transform.position = new Vector3(x, -3f, z); // Dìm đá xuống một chút cho lún xuống đất
            
            // Xoay ngẫu nhiên mọi mặt để tạo vách đá tự nhiên
            wallRock.transform.rotation = Quaternion.Euler(Random.Range(-15f, 15f), Random.Range(0, 360f), Random.Range(-15f, 15f));
            
            // Xoay mặt đá lớn hướng vào trong Arena
            wallRock.transform.LookAt(caveRoot.transform.position + new Vector3(0, wallRock.transform.position.y, 0));
            // Tilt ra sau một chút
            wallRock.transform.Rotate(-15, 0, 0);

            float randomScaleX = Random.Range(1.8f, 3.5f);
            float randomScaleY = Random.Range(2.5f, 3.5f) * heightScale;
            wallRock.transform.localScale = new Vector3(randomScaleX, randomScaleY, randomScaleX);

            Undo.RegisterCreatedObjectUndo(wallRock, "Spawn Cave Wall");
        }

        // TRẦN HANG (Nhiều lớp đá chụm lại cao hơn)
        for (int j = 0; j < 2; j++) // 2 vòng tròn đồng tâm tạo mái vòm
        {
            float ceilingRadius = radius * (0.8f - j * 0.4f);
            float ceilingHeight = 25f + j * 10f; // Vòng trong cao hơn vòng ngoài
            
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
                
                // Nghiêng đá cắm thẳng vào tâm trên không
                Vector3 centerPoint = new Vector3(0, ceilingHeight + 20f, 0);
                ceilingRock.transform.rotation = Quaternion.LookRotation(centerPoint - ceilingRock.transform.position) * Quaternion.Euler(90, 0, Random.Range(0,360f));
                
                float randomScale = Random.Range(3.0f, 4.5f); // Đá mái to hơn để che cho kín
                ceilingRock.transform.localScale = new Vector3(randomScale, randomScale * 0.5f, randomScale);

                Undo.RegisterCreatedObjectUndo(ceilingRock, "Spawn Cave Ceiling");
            }
        }

        Selection.activeGameObject = caveRoot;
        Debug.Log("Đã tạo xong Arena Hang Động Boss! Bạn có thể xem trong Scene.");
    }

    [MenuItem("HuuAnh/Fix Dark Scene (Restore Default Brightness)")]
    public static void RestoreBrightEnvironment()
    {
        // 1. Chỉnh Mặt trời sáng lên
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

        // 2. Tắt sương mù đi
        RenderSettings.fog = false;

        // 3. Khôi phục ánh sáng phản chiếu (Ambient) sáng sủa
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.8f, 0.8f, 0.8f);

        Debug.Log("Đã khôi phục ánh sáng bình thường! Nếu nền trời vẫn đen, hãy vào Window > Rendering > Lighting, kéo một Material vào ô Skybox Material.");
    }
}
