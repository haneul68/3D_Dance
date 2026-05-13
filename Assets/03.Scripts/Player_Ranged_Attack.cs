using UnityEngine;

public class Player_Ranged_Attack : MonoBehaviour
{
    [Header("Ref")]
    [SerializeField] private Camera aim_Camera;
    [SerializeField] private Transform muzzle;

    [Header("Range")]
    [SerializeField] private float aim_Range = 100f;
    [SerializeField] private float shot_Range = 100f;

    [Header("Attack")]
    [SerializeField] private int damage = 10;
    [SerializeField] private float shot_Radius = 0f;

    [Header("Layer")]
    [SerializeField] private LayerMask aim_Mask;
    [SerializeField] private LayerMask shot_Mask;
    [SerializeField] private LayerMask muzzle_BlockMask;

    [Header("Muzzle Block")]
    [SerializeField] private bool check_Muzzle_Blocked = true;
    [SerializeField] private float muzzle_Block_Radius = 0.25f;

    [Header("Effect")]
    [SerializeField] private GameObject hit_Effect_Prefab;

    private struct AimResult
    {
        public Ray ray;
        public bool didHit;
        public Vector3 point;
        public RaycastHit hit;
    }

    private struct ShotResult
    {
        public Ray ray;
        public float distance;
        public bool did_Hit;
        public RaycastHit hit;
    }

    private void Awake()
    {
        if (aim_Camera == null)
            aim_Camera = Camera.main;
    }

    public void Attack()
    {
        if (aim_Camera == null || muzzle == null)
        {
            Debug.LogWarning("aim_Camera == null || muzzle == null");
            return;
        }

        if (Is_Muzzle_Blocked())
        {
            Debug.Log("발사 불가 : 총구가 장애물에 너무 가까움");
            return;
        }

        AimResult aim_Result = Resolve_Aim_Point();
        ShotResult shot_Result = Fire_From_Muzzle(aim_Result);

        Draw_Debug_Rays(aim_Result, shot_Result);

        if (shot_Result.did_Hit)
            Handle_Hit(shot_Result.hit, aim_Result);
    }

    private AimResult Resolve_Aim_Point()
    {
        Ray aim_Ray = aim_Camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        AimResult result = new AimResult
        {
            ray = aim_Ray,
            didHit = false,
            point = aim_Ray.GetPoint(aim_Range)
        };

        if (Physics.Raycast(aim_Ray, out RaycastHit hit, aim_Range, aim_Mask, QueryTriggerInteraction.Ignore))
        {
            result.didHit = true;
            result.hit = hit;
            result.point = hit.point;
        }

        return result;
    }

    private ShotResult Fire_From_Muzzle(AimResult aimResult)
    {
        Vector3 to_Aim_Point = aimResult.point - muzzle.position;

        if (to_Aim_Point.sqrMagnitude < 0.0001f)
            to_Aim_Point = aim_Camera.transform.forward;

        Vector3 shot_Direction = to_Aim_Point.normalized;
        float distance_To_Aim_Point = to_Aim_Point.magnitude;
        float cast_Distance = aimResult.didHit ? Mathf.Min(shot_Range, distance_To_Aim_Point + 0.05f) : shot_Range;

        ShotResult result = new ShotResult
        {
            ray = new Ray(muzzle.position, shot_Direction),
            distance = cast_Distance,
            did_Hit = false
        };

        if (Cast_Shot(result.ray, result.distance, out RaycastHit shotHit))
        {
            result.did_Hit = true;
            result.hit = shotHit;
        }

        return result;
    }

    private bool Cast_Shot(Ray ray, float distance, out RaycastHit hit)
    {
        if (shot_Radius > 0f)
            return Physics.SphereCast(ray.origin, shot_Radius, ray.direction, out hit, distance, shot_Mask, QueryTriggerInteraction.Ignore);

        return Physics.Raycast(ray, out hit, distance, shot_Mask, QueryTriggerInteraction.Ignore);
    }

    private bool Is_Muzzle_Blocked()
    {
        if (!check_Muzzle_Blocked)
            return false;

        return Physics.CheckSphere(muzzle.position, muzzle_Block_Radius, muzzle_BlockMask, QueryTriggerInteraction.Ignore);
    }

    private void Handle_Hit(RaycastHit hit, AimResult aimResult)
    {
        string aim_Name = aimResult.didHit ? aimResult.hit.collider.name : "없음";
        string shot_Name = hit.collider.name;

        Debug.Log($"카메라 조준 : {aim_Name} / 실제 피격 : {shot_Name}");

        IDamageable damageable = hit.collider.GetComponentInParent<IDamageable>();

        if (damageable != null)
            damageable.Take_Damage(damage);

        if (hit_Effect_Prefab != null)
            Instantiate(hit_Effect_Prefab, hit.point, Quaternion.LookRotation(hit.normal));
    }

    private void Draw_Debug_Rays(AimResult aim, ShotResult shot)
    {
        float aim_Distance = aim.didHit ? aim.hit.distance : aim_Range;
        float shot_Distance = shot.did_Hit ? shot.hit.distance : shot.distance;

        Debug.DrawRay(aim.ray.origin, aim.ray.direction * aim_Distance, Color.cyan, 1f);
        Debug.DrawRay(shot.ray.origin, shot.ray.direction * shot_Distance, shot.did_Hit ? Color.red : Color.yellow, 1f);
    }

    private void OnDrawGizmosSelected()
    {
        if (muzzle == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(muzzle.position, muzzle_Block_Radius);
    }
}