using UnityEngine;

public class Simple_Interactable : MonoBehaviour
{
    [Header("Message")]
    [SerializeField] private string interact_Message = "[E] 상호작용";
    [SerializeField] private string interact_Result_Message = "상호작용 완료";

    public string Get_Message()
    {
        return interact_Message;
    }

    public void Interact()
    {
        Debug.Log(interact_Result_Message);
    }
}