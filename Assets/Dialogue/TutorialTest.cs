using UnityEngine;

public class TutorialTest : MonoBehaviour
{
    public DialogueSet[] tutorialDialogue;

    // void Start()
    // {
    //     DialogueManager.Instance.StartDialogue(tutorialDialogue);
    // }

    public void TutorialDialogue(int index)
    {
        DialogueManager.Instance.StartDialogue(tutorialDialogue[index]);
    }
}
