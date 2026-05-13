using UnityEngine;
using StarterAssets;

public class Player_Combat_Controller : MonoBehaviour
{
    [Header("Combat")]
    [SerializeField] private Player_Melee_Attack meleeAttack;
    [SerializeField] private Player_Ranged_Attack rangedAttack;

    private StarterAssetsInputs input;

    private void Awake()
    {
        input = GetComponent<StarterAssetsInputs>();

        if (meleeAttack == null)
            meleeAttack = GetComponent<Player_Melee_Attack>();

        if (rangedAttack == null)
            rangedAttack = GetComponent<Player_Ranged_Attack>();
    }

    private void Update()
    {
        if (input.attack)
        {
            if (meleeAttack != null)
                meleeAttack.Attack();

            input.attack = false;
        }

        if (input.rangedAttack)
        {
            if (rangedAttack != null)
                rangedAttack.Attack();

            input.rangedAttack = false;
        }
    }
}