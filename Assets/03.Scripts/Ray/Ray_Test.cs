using System.Collections;
using UnityEngine;
public enum CapsuleAxis
{
    Up,
    Right,
    Forward
}
public class Ray_Test : MonoBehaviour
{
    [SerializeField] private float rayDistance = 100f;
    [SerializeField] private LayerMask mask;
    [SerializeField] private WaitForSeconds delay = new WaitForSeconds(2f);


    private RaycastHit hit;

    [SerializeField] private Transform[] owners = new Transform[6];

    [SerializeField] float radius = 1f;
    [SerializeField] private Vector3 boxHalfSize = new Vector3(1f, 1f, 1f);
    [SerializeField] float distance = 15f;

    [Space(20)]
    [Header("Capsule")]
    [SerializeField] float radius_Capsule = 1f;
    [SerializeField] float height_Capsule = 4f;
    [SerializeField] float value = 0.5f;
    [SerializeField] private CapsuleAxis capsuleAxis;


    [Space(20)]
    [Header("Overlap")]
    [SerializeField] float radius_Overlap = 7f;

    [Space(20)]
    [Header("Overlap Box")]
    [SerializeField] private Vector3 overlapBoxHalfSize = new Vector3(2f, 2f, 2f);

    [Space(20)]
    [Header("Overlap Capsule")]
    [SerializeField] private float overlapCapsuleRadius = 1f;
    [SerializeField] private float overlapCapsuleHeight = 4f;
    [SerializeField] private float overlapCapsuleValue = 0.5f;

    private bool drawSphere;
    private bool drawBox;
    private bool drawCapsule;
    private bool drawOverlapSphere;
    private bool drawOverlapBox;
    private bool drawOverlapCapsule;
    private void Start()
    {
        StartCoroutine(TestRoutine());
    }
    private void Draw_Ray_Line() 
    {
        (Vector3 origin, Vector3 direction) = (Get_Owner_T(0).pos, Get_Owner_T(0).dir);

        if (Physics.Raycast(origin, direction, out hit, rayDistance, mask))
        {
            Debug.Log("맞은 오브젝트 : " + hit.collider.name);
            Debug.DrawLine(origin, hit.point, Color.red, 2f);
        }
        else 
        {
            Debug.DrawRay(origin, direction * rayDistance, Color.red, 2f);
        }

    }
    private void Draw_Ray_Line2() 
    {
        (Vector3 origin, Vector3 direction) = (Get_Owner_T(1).pos, Get_Owner_T(1).dir);

        Ray ray = new Ray(origin, direction);

        if (Physics.Raycast(ray,out hit, rayDistance, mask)) 
        {
            Debug.Log("맞은 오브젝트 : " + hit.collider.name);
            Debug.DrawRay(origin, hit.point - origin, Color.black, 2f);
        }
        else
        {
            Debug.DrawRay(origin, direction * rayDistance, Color.black, 2f);
        }
    }

    private void Draw_Ray_Sphere()
    {
        (Vector3 origin, Vector3 direction) = (Get_Owner_T(2).pos, Get_Owner_T(2).dir);

        if (Physics.SphereCast(origin, radius, direction, out hit, distance, mask))
        {
            Debug.Log("맞은 오브젝트 : " + hit.collider.name);
        }
    }

    private void Draw_Ray_Box()
    {
        (Vector3 origin, Vector3 direction) = (Get_Owner_T(3).pos, Get_Owner_T(3).dir);

        if (Physics.BoxCast(origin, boxHalfSize, direction, out hit, owners[3].rotation, distance, mask))
        {
            Debug.Log("맞은 오브젝트 : " + hit.collider.name);
        }
    }

    private void Draw_Ray_Capsule()
    {
        (Vector3 origin, Vector3 direction) = (Get_Owner_T(4).pos, Get_Owner_T(4).dir);

        Vector3 axis = GetCapsuleAxis();

        Vector3 point1 = origin + axis * (height_Capsule * value - radius_Capsule);

        Vector3 point2 = origin - axis * (height_Capsule * value - radius_Capsule);

        if (Physics.CapsuleCast(point1, point2, radius, direction, out hit, distance, mask))
        {
            Debug.Log("맞은 오브젝트 : " + hit.collider.name);
        }
    }

    private Vector3 GetCapsuleAxis()
    {
        switch (capsuleAxis)
        {
            case CapsuleAxis.Right:
                return transform.right;

            case CapsuleAxis.Forward:
                return transform.forward;

            default:
                return transform.up;
        }
    }
    private void Draw_Overlap_Sphere()
    {
        Vector3 origin = owners[5].position;

        Collider[] cols = Physics.OverlapSphere(origin, radius_Overlap, mask);

        foreach (Collider col in cols)
        {
            Debug.Log(col.name);
        }
    }
    private void Draw_Overlap_Box()
    {
        Vector3 origin = owners[5].position;

        Collider[] cols = Physics.OverlapBox(origin, overlapBoxHalfSize, owners[5].rotation, mask);

        foreach (Collider col in cols)
        {
            Debug.Log("OverlapBox 감지 : " + col.name);
        }
    }

    private void Draw_Overlap_Capsule()
    {
        Vector3 origin = owners[5].position;

        Vector3 axis = GetCapsuleAxis();

        Vector3 point1 = origin + axis * (overlapCapsuleHeight * overlapCapsuleValue - overlapCapsuleRadius);
        Vector3 point2 = origin - axis * (overlapCapsuleHeight * overlapCapsuleValue - overlapCapsuleRadius);

        Collider[] cols = Physics.OverlapCapsule(point1, point2, overlapCapsuleRadius, mask);

        foreach (Collider col in cols)
        {
            Debug.Log("OverlapCapsule 감지 : " + col.name);
        }
    }

    private void OnDrawGizmos()
    {
        if (drawSphere)
        {
            Vector3 origin = owners[2].position;
            Vector3 direction = owners[2].forward;

            Vector3 endPoint = origin + direction * distance;

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(origin, radius);

            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(endPoint, radius);

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(origin, endPoint);
        }

        if (drawBox)
        {
            Vector3 origin = owners[3].position;
            Vector3 direction = owners[3].forward;

            Vector3 endPoint = origin + direction * distance;

            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(origin, boxHalfSize * 2f);

            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(endPoint, boxHalfSize * 2f);

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(origin, endPoint);
        }

        if (drawCapsule)
        {
            Vector3 origin = owners[4].position;
            Vector3 direction = owners[4].forward;

            Vector3 axis = GetCapsuleAxis();

            Vector3 point1 = origin + axis * (height_Capsule * value - radius_Capsule);

            Vector3 point2 = origin - axis * (height_Capsule * value - radius_Capsule);

            Vector3 endPoint1 = point1 + direction * distance;
            Vector3 endPoint2 = point2 + direction * distance;

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(point1, radius_Capsule);
            Gizmos.DrawWireSphere(point2, radius_Capsule);
            Gizmos.DrawLine(point1, point2);

            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(endPoint1, radius_Capsule);
            Gizmos.DrawWireSphere(endPoint2, radius_Capsule);
            Gizmos.DrawLine(endPoint1, endPoint2);

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(point1, endPoint1);
            Gizmos.DrawLine(point2, endPoint2);
        }

        if (drawOverlapSphere)
        {
            Vector3 origin = owners[5].position;

            Gizmos.color = Color.cyan;

            Gizmos.DrawWireSphere(origin, radius_Overlap);
        }

        if (drawOverlapBox)
        {
            Vector3 origin = owners[5].position;

            Gizmos.color = Color.magenta;

            Gizmos.matrix = Matrix4x4.TRS(origin, owners[5].rotation, Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, overlapBoxHalfSize * 2f);
            Gizmos.matrix = Matrix4x4.identity;
        }

        if (drawOverlapCapsule)
        {
            Vector3 origin = owners[5].position;

            Vector3 axis = GetCapsuleAxis();

            Vector3 point1 = origin + axis * (overlapCapsuleHeight * overlapCapsuleValue - overlapCapsuleRadius);
            Vector3 point2 = origin - axis * (overlapCapsuleHeight * overlapCapsuleValue - overlapCapsuleRadius);

            Gizmos.color = Color.green;

            Gizmos.DrawWireSphere(point1, overlapCapsuleRadius);
            Gizmos.DrawWireSphere(point2, overlapCapsuleRadius);
            Gizmos.DrawLine(point1, point2);
        }
    }


    private (Vector3 pos, Vector3 dir) Get_Owner_T(int index) 
    {
        Vector3 origin = owners[index].position;
        Vector3 direction = owners[index].forward;

        return (origin, direction);
    }
    private IEnumerator TestRoutine()
    {
        while (true)
        {
            // Raycast
            Draw_Ray_Line();
            yield return delay;

            // Ray
            Draw_Ray_Line2();
            yield return delay;

            // SphereCast
            drawSphere = true;
            Draw_Ray_Sphere();
            yield return delay;
            drawSphere = false;

            // BoxCast
            drawBox = true;
            Draw_Ray_Box();
            yield return delay;
            drawBox = false;

            // CapsuleCast
            drawCapsule = true;
            Draw_Ray_Capsule();
            yield return delay;
            drawCapsule = false;

            // OverlapSphere
            drawOverlapSphere = true;
            Draw_Overlap_Sphere();
            yield return delay;
            drawOverlapSphere = false;

            // OverlapBox
            drawOverlapBox = true;
            Draw_Overlap_Box();
            yield return delay;
            drawOverlapBox = false;

            // OverlapCapsule
            drawOverlapCapsule = true;
            Draw_Overlap_Capsule();
            yield return delay;
            drawOverlapCapsule = false;
        }
    }
}