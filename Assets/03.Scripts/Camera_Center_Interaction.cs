using UnityEngine;
using StarterAssets;

public class Camera_Center_Interaction : MonoBehaviour
{
    [Header("Ref")]
    [SerializeField] private Camera aim_Camera;

    [Header("Interaction")]
    [SerializeField] private float interact_Distance = 5f;
    [SerializeField] private float interact_Radius = 0f;
    [SerializeField] private LayerMask interact_Layer;

    [Header("Debug")]
    [SerializeField] private bool draw_Debug = true;

    private StarterAssetsInputs input;

    private Simple_Interactable current_Target;
    private bool has_Entered_Target;

    private struct InteractionResult
    {
        public Ray ray;
        public bool didHit;
        public RaycastHit hit;
        public Simple_Interactable target;
        public Vector3 point;
    }

    private void Awake()
    {
        input = GetComponent<StarterAssetsInputs>();

        if (aim_Camera == null)
            aim_Camera = Camera.main;
    }

    private void Update()
    {
        InteractionResult result = Resolve_Interaction();

        Update_Target(result);

        if (input.interact)
        {
            Try_Interact(result);
            input.interact = false;
        }

        if (draw_Debug)
            Draw_Debug(result);
    }

    private InteractionResult Resolve_Interaction()
    {
        Ray ray = aim_Camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        InteractionResult result = new InteractionResult
        {
            ray = ray,
            didHit = false,
            point = ray.GetPoint(interact_Distance),
            target = null
        };

        bool didHit = Cast_Interaction(ray, out RaycastHit hit);

        if (didHit)
        {
            result.didHit = true;
            result.hit = hit;
            result.point = hit.point;
            result.target = hit.collider.GetComponentInParent<Simple_Interactable>();
        }

        return result;
    }

    private bool Cast_Interaction(Ray ray, out RaycastHit hit)
    {
        if (interact_Radius > 0f)
            return Physics.SphereCast(ray.origin, interact_Radius, ray.direction, out hit, interact_Distance, interact_Layer, QueryTriggerInteraction.Ignore);

        return Physics.Raycast(ray, out hit, interact_Distance, interact_Layer, QueryTriggerInteraction.Ignore);
    }

    private void Update_Target(InteractionResult result)
    {
        if (!result.didHit || result.target == null)
        {
            current_Target = null;
            has_Entered_Target = false;
            return;
        }

        if (current_Target != result.target)
        {
            current_Target = result.target;
            has_Entered_Target = false;
        }

        if (!has_Entered_Target)
        {
            Debug.Log(current_Target.Get_Message());
            has_Entered_Target = true;
        }
    }

    private void Try_Interact(InteractionResult result)
    {
        if (!result.didHit || result.target == null)
            return;

        result.target.Interact();
    }

    private void Draw_Debug(InteractionResult result)
    {
        float debugDistance = result.didHit ? result.hit.distance : interact_Distance;
        Color color = result.target != null ? Color.green : Color.yellow;

        Debug.DrawRay(result.ray.origin, result.ray.direction * debugDistance, color);
    }

    private void OnDrawGizmosSelected()
    {
        if (aim_Camera == null || interact_Radius <= 0f)
            return;

        Ray ray = aim_Camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        Vector3 endPoint = ray.GetPoint(interact_Distance);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(endPoint, interact_Radius);
    }
}