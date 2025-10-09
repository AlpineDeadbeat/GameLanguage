using UnityEngine;

public class DialogueCloseButton : MonoBehaviour
{
    public void Close()
    {
        if (DialogueController.Instance != null)
            DialogueController.Instance.Close();
    }
}
