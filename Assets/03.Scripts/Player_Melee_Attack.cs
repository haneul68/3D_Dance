using UnityEngine;

public class Player_Melee_Attack : MonoBehaviour
{
    [Header("Attack")]
    [SerializeField] private Transform attack_Point;
    [SerializeField] private float attack_Radius = 1.7f;
    [SerializeField] private int damage = 15;
    [SerializeField] private LayerMask enemy_Layer;

    public void Attack()
    {
        if (attack_Point == null) return;

        Collider[] hits = Physics.OverlapSphere(attack_Point.position, attack_Radius, enemy_Layer, QueryTriggerInteraction.Ignore);

        Debug.Log($"근접 공격 실행 / 감지된 적 수 : {hits.Length}");

        foreach (Collider hit in hits)
        {
            IDamageable damageable = hit.GetComponentInParent<IDamageable>();

            if (damageable != null)
                damageable.Take_Damage(damage);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (attack_Point == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attack_Point.position, attack_Radius);
    }
}