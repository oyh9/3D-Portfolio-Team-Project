// DialogueSet.cs
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewDialogueSet", menuName = "Dialogue/Set")]
public class DialogueSet : ScriptableObject {
    public List<DialogueLine> lines;
}