using SystemicOverload.Phase1;
using UnityEngine;

namespace SystemicOverload.Combat
{
    /// <summary>
    /// 기본 원거리(레이캐스트) 공격과 발사 간격을 처리합니다. Animator가 있으면 Attack 트리거를 전달합니다.
    /// </summary>
    [RequireComponent(typeof(InputProvider))]
    public sealed class CombatComponent : MonoBehaviour
    {
        private const string AttackTriggerParameterName = "AttackTrig";

        [Header("Weapon")]
        [SerializeField] private float damage = 12.0f;
        [SerializeField] private float shotsPerSecond = 4.0f;
        [SerializeField] private float maxRange = 40.0f;
        [SerializeField] private float rayOriginHeight = 1.0f;
        [SerializeField] private float rayStartForwardOffset = 0.35f;
        [SerializeField] private LayerMask hitLayerMask = ~0;

        [Header("References")]
        [SerializeField] private MovementComponent movementComponent;
        [SerializeField] private Animator animator;

        private InputProvider inputProvider;
        private float nextAllowedShotTime;

        private static readonly int AttackTriggerHash = Animator.StringToHash(AttackTriggerParameterName);

        private void Awake()
        {
            inputProvider = GetComponent<InputProvider>();
            movementComponent ??= GetComponent<MovementComponent>();
        }

        private void OnValidate()
        {
            damage = Mathf.Max(0.0f, damage);
            shotsPerSecond = Mathf.Max(0.01f, shotsPerSecond);
            maxRange = Mathf.Max(0.1f, maxRange);
        }

        private void Update()
        {
            if (!inputProvider.WasAttackPressedThisFrame)
            {
                return;
            }

            if (Time.time < nextAllowedShotTime)
            {
                return;
            }

            float interval = 1.0f / shotsPerSecond;
            nextAllowedShotTime = Time.time + interval;

            TryFireHitScan();
            TrySetAttackTrigger();
        }

        /// <summary>
        /// 히트 스캔 한 발을 수행하고, 맞은 대상에 <see cref="IDamageable"/> 데미지를 적용합니다.
        /// </summary>
        private void TryFireHitScan()
        {
            Vector3 origin = transform.position + Vector3.up * rayOriginHeight + transform.forward * rayStartForwardOffset;
            Vector3 direction = ResolveFireDirection();
            if (direction.sqrMagnitude < 0.0001f)
            {
                direction = transform.forward;
            }
            else
            {
                direction.Normalize();
            }

            if (!Physics.Raycast(origin, direction, out RaycastHit hitInfo, maxRange, hitLayerMask, QueryTriggerInteraction.Ignore))
            {
                return;
            }

            if (hitInfo.collider != null && hitInfo.collider.transform.IsChildOf(transform))
            {
                return;
            }

            IDamageable damageable = hitInfo.collider.GetComponentInParent<IDamageable>();
            if (damageable == null || !damageable.IsAlive)
            {
                return;
            }

            DamagePayload payload = new DamagePayload
            {
                Amount = damage,
                Attacker = transform
            };
            damageable.ApplyDamage(in payload);
        }

        private Vector3 ResolveFireDirection()
        {
            if (movementComponent != null)
            {
                Vector3 toAim = movementComponent.LastAimPoint - transform.position;
                toAim.y = 0.0f;
                if (toAim.sqrMagnitude > 0.0001f)
                {
                    return toAim.normalized;
                }
            }

            return transform.forward;
        }

        private void TrySetAttackTrigger()
        {
            if (animator == null)
            {
                return;
            }

            foreach (AnimatorControllerParameter parameter in animator.parameters)
            {
                if (parameter.name == AttackTriggerParameterName && parameter.type == AnimatorControllerParameterType.Trigger)
                {
                    animator.SetTrigger(AttackTriggerHash);
                    return;
                }
            }
        }
    }
}
