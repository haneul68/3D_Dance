using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

[RequireComponent(typeof(PlayerInput))]
public class CenterRaycastShooter : MonoBehaviour
{
    [Header("Raycast")]
    [SerializeField] private Camera m_cam;
    [SerializeField] private LayerMask m_hittableMask;
    [SerializeField] private float m_maxDistance = 100f;

    private PlayerInput _pi;
    private InputAction _fire;

    private void Awake()
    {
        _pi = GetComponent<PlayerInput>();

        _fire = _pi.actions.FindAction("Fire", throwIfNotFound: true);

        if (m_cam == null) m_cam = Camera.main;
    }

    private void OnEnable()
    {
        _fire.performed += OnRayFire;
    }

    private void OnDisable()
    {
        _fire.performed -= OnRayFire;
    }

    private void OnRayFire(InputAction.CallbackContext _)
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

        Vector2 screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);

        Ray ray = m_cam.ScreenPointToRay(screenCenter);

        if (Physics.Raycast(ray, out var hit, m_maxDistance, m_hittableMask, QueryTriggerInteraction.Ignore))
        {
            Debug.Log($"[CenterRaycastShooter_PI] Hit {hit.collider.name} @ {hit.point}");

            var rend = hit.collider.GetComponent<Renderer>();

            if (rend != null) rend.material.color = Color.red;

            Debug.DrawLine(ray.origin, hit.point, Color.green, 1f);
        }
        else
        {
            Debug.DrawRay(ray.origin, ray.direction * m_maxDistance, Color.yellow, 0.5f);
        }
    }
}