using UnityEngine;

public class Enemy_Dummy_Health : MonoBehaviour, IDamageable
{
    [SerializeField] private int max_Hp = 30;

    private int current_Hp;

    private void Awake()
    {
        current_Hp = max_Hp;
    }

    public void Take_Damage(int damage)
    {
        current_Hp -= damage;

        Debug.Log($"{gameObject.name} ÇÇ°Ý / Damage : {damage} / HP : {current_Hp}");

        if (current_Hp <= 0)
        {
            Debug.Log($"{gameObject.name} »ç¸Á");
            Destroy(gameObject);
        }
    }
}