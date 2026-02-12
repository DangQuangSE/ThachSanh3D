/*
 * ============================================
 * H??NG D?N SETUP SLASH EFFECTS CHO RÌU
 * ============================================
 * 
 * B??C 1: CHU?N B? SLASH EFFECT PREFABS
 * --------------------------------------
 * 1. Vào th? m?c: Assets/ThachSanhGeneral/Quang/Matthew Guz/Slash Effects FREE/Prefabs
 * 2. Ch?n các Slash Effect prefabs b?n mu?n dùng cho m?i ?òn t?n công
 *    Ví d?:
 *    - Attack 1: Slash_01 (?òn chém ngang)
 *    - Attack 2: Slash_02 (?òn chém chéo)
 *    - Attack 3: Slash_03 (?òn chém m?nh)
 *    - Ultimate: Slash_05 (?òn chém c?c m?nh)
 * 
 * 
 * B??C 2: T?O VFX SPAWN POINT
 * --------------------------------------
 * 1. Tìm weapon (rìu) trong Hierarchy c?a Player:
 *    PlayerArmature ? Hips ? Spine ? ... ? Hand_R ? Weapon
 * 
 * 2. T?o Empty GameObject con trong Weapon:
 *    - Click chu?t ph?i vào Weapon ? Create Empty
 *    - ??t tên: "VFX_SpawnPoint"
 * 
 * 3. Di chuy?n VFX_SpawnPoint ??n v? trí phù h?p:
 *    - ??t ? ??u l??i rìu ho?c gi?a l??i rìu
 *    - ?i?u ch?nh rotation ?? VFX h??ng ?úng chi?u chém
 *    - Tip: B?t Gizmos ?? th?y h??ng tr?c (X=??, Y=xanh lá, Z=xanh d??ng)
 * 
 * 
 * B??C 3: THÊM COMPONENT VÀO PLAYER
 * --------------------------------------
 * 1. Ch?n Player GameObject trong Hierarchy
 * 
 * 2. Add Component ? Search "AttackVFXManager" ? Add
 * 
 * 3. Trong Inspector c?a AttackVFXManager:
 * 
 *    [VFX Prefabs - Slash Effects]
 *    - Attack 1 VFX: Kéo prefab Slash_01 vào ?ây
 *    - Attack 2 VFX: Kéo prefab Slash_02 vào ?ây
 *    - Attack 3 VFX: Kéo prefab Slash_03 vào ?ây
 *    - Ultimate VFX: Kéo prefab Slash_05 vào ?ây
 * 
 *    [VFX Spawn Settings]
 *    - VFX Spawn Point: Kéo "VFX_SpawnPoint" GameObject vào ?ây
 *    - Spawn Offset: (0, 0, 0) - ho?c ?i?u ch?nh n?u c?n
 *    - Rotation Offset: (0, 0, 0) - ?i?u ch?nh n?u VFX h??ng sai
 *    - VFX Scale: 1 - t?ng/gi?m n?u VFX quá nh?/l?n
 *    - VFX Lifetime: 2 - th?i gian VFX t?n t?i (giây)
 * 
 *    [Timing Settings]
 *    - Attack 1 Spawn Time: 0.4 (40% animation = khi rìu chém)
 *    - Attack 2 Spawn Time: 0.4
 *    - Attack 3 Spawn Time: 0.4
 *    - Ultimate Spawn Time: 0.5
 * 
 *    [Debug]
 *    - Show Debug Logs: ? (?? xem log khi test)
 * 
 * 
 * B??C 4: ?I?U CH?NH TIMING (QUAN TR?NG!)
 * --------------------------------------
 * 1. Ch?y game (Play mode)
 * 
 * 2. Th?c hi?n ?òn t?n công và quan sát:
 *    - VFX spawn quá s?m? ? T?ng Spawn Time (vd: 0.4 ? 0.5)
 *    - VFX spawn quá mu?n? ? Gi?m Spawn Time (vd: 0.4 ? 0.3)
 * 
 * 3. ?i?u ch?nh cho ??n khi VFX spawn ?úng lúc rìu chém
 * 
 * 4. Tips:
 *    - M? Window ? Animation ?? xem animation timeline
 *    - Quan sát khi nào rìu chém ?? tính timing chính xác
 * 
 * 
 * B??C 5: ?I?U CH?NH V? TRÍ VÀ H??NG VFX
 * --------------------------------------
 * 1. Ch?n Player trong Hierarchy
 * 
 * 2. Trong Scene view, b?t Gizmos
 * 
 * 3. Khi ch?n Player, b?n s? th?y:
 *    - Hình c?u ?? = v? trí spawn VFX
 *    - M?i tên vàng = h??ng VFX
 * 
 * 4. ?i?u ch?nh VFX_SpawnPoint:
 *    - Position: Di chuy?n ??n v? trí t?t h?n
 *    - Rotation: Xoay ?? VFX h??ng ?úng chi?u chém
 * 
 * 5. Ho?c ?i?u ch?nh trong AttackVFXManager:
 *    - Spawn Offset: Offset thêm t? spawn point
 *    - Rotation Offset: Xoay VFX thêm (??)
 * 
 * 
 * B??C 6: SETUP POST-PROCESSING (TÙY CH?N)
 * --------------------------------------
 * ?? VFX ??p h?n, làm theo h??ng d?n trong file:
 * Assets/ThachSanhGeneral/Quang/Matthew Guz/Slash Effects FREE/Documentation/Readme IMPORTANT.txt
 * 
 * Tóm t?t:
 * 1. Package Manager ? Install Post Processing
 * 2. T?o layer "PostProcessing"
 * 3. Main Camera ? Add "Post Processing Layer"
 * 4. Create Empty "PP Volume" ? Add "Post Processing Volume"
 * 5. Add effects: Ambient Occlusion + Bloom
 * 
 * 
 * TROUBLESHOOTING
 * --------------------------------------
 * ? VFX không spawn:
 *    ? Ki?m tra prefabs ?ã assign vào AttackVFXManager ch?a
 *    ? Ki?m tra VFX Spawn Point ?ã assign ch?a
 *    ? B?t Debug Logs ?? xem log
 * 
 * ? VFX spawn sai v? trí:
 *    ? ?i?u ch?nh VFX_SpawnPoint position/rotation
 *    ? ?i?u ch?nh Spawn Offset và Rotation Offset
 * 
 * ? VFX spawn sai timing:
 *    ? ?i?u ch?nh Attack Spawn Time (0-1)
 *    ? M? Animation window ?? xem timeline
 * 
 * ? VFX quá nh?/l?n:
 *    ? ?i?u ch?nh VFX Scale
 * 
 * ? VFX spawn nhi?u l?n:
 *    ? Script t? ??ng ch?ng spam, nh?ng n?u v?n b?:
 *    ? Ki?m tra animation name có ?úng không (Attack_1, Attack_2, Attack_3)
 * 
 * 
 * TIPS B? SUNG
 * --------------------------------------
 * 1. Có th? dùng VFX khác nhau cho m?i ?òn:
 *    - ?òn 1: Slash nh?
 *    - ?òn 2: Slash v?a
 *    - ?òn 3: Slash l?n (k?t thúc combo)
 *    - Ultimate: Slash c?c l?n v?i nhi?u trail
 * 
 * 2. Có th? t?o nhi?u spawn points:
 *    - M?t cho tay trái, m?t cho tay ph?i (n?u dùng 2 rìu)
 *    - Script có th? m? r?ng ?? support nhi?u spawn points
 * 
 * 3. N?u mu?n dùng Animation Events:
 *    - S? d?ng các hàm: SpawnAttack1VFX(), SpawnAttack2VFX(), etc.
 *    - Thêm vào Animation Events trong Animation window
 * 
 * 
 * H?I ?ÁP
 * --------------------------------------
 * Q: Có th? dùng nhi?u VFX cho 1 ?òn không?
 * A: Có, t?o thêm bi?n public GameObject[] attack1VFXArray và spawn t?t c?
 * 
 * Q: VFX có follow weapon không?
 * A: Không, VFX spawn r?i ??ng yên. N?u mu?n follow, set parent = vfxSpawnPoint
 * 
 * Q: Có th? dùng particle systems không?
 * A: Có, prefab có th? là b?t k? GameObject nào (Particle System, VFX Graph, etc.)
 * 
 */

using UnityEngine;

namespace StarterAssets
{
    /// <summary>
    /// This is a documentation file - No actual code here
    /// See AttackVFXManager.cs for implementation
    /// </summary>
    public class VFXSetupGuide : MonoBehaviour
    {
        // This class is just for documentation purposes
        // Attach AttackVFXManager to your Player GameObject instead
    }
}
