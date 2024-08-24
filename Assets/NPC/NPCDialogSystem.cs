using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NPCDialogSystem : MonoBehaviour
{
    public static KeyCode interactKey = KeyCode.E;

    public TextMeshProUGUI characterNameBox;
    public TextMeshProUGUI dialogBox;
    public TextMeshProUGUI nextButtonText;

    private Dictionary<NPCType, List<(string, string)>[]> npcDialogs = new Dictionary<NPCType, List<(string, string)>[]>();
    //List<string> SwimmingGirlConversation = new List<string>();

    private List<(string, string)> currentConversation = null;
    private int currentDialogIndex = 0;

    // Start is called before the first frame update
    void Start()
    {
        nextButtonText.text += " " + interactKey.ToString();
        SetupConversations();
        gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
    }

    void SetupConversations()
    {
        SetupFishingGuyConversations();
    }

    void SetupFishingGuyConversations()
    {

        List<(string, string)> convo1 = new List<(string, string)>();
        convo1.Add(("Player", "What are you doing?"));
        convo1.Add(("Fishing Guy", "I am fishing. What does it look like?"));
        convo1.Add(("Player", "That doesn't look like a fishing rod"));
        convo1.Add(("Fishing Guy", "It's a special one. Leave me alone!"));

        List<(string, string)> convo2 = new List<(string, string)>();
        convo2.Add(("Player" ,"Hel-"));
        convo2.Add(("Fishing Guy", "Shhh! Be quiet. You'll scare the fish away!"));

        List<(string, string)>[] allConvos = {convo1, convo2};
        npcDialogs.Add(NPCType.FishingGuy, allConvos);
    }

    private List<(string, string)> GetConversation(NPCType npcType, int conversationIndex)
    {
        return npcDialogs[npcType][conversationIndex];
    }

    public int GetConversationCount(NPCType npcType)
    {
        return npcDialogs[npcType].Length;
    }

    void SetupKidConversations()
    {

    }

    public void StartConverstation(NPCType npcType, int convoIndex)
    {
        currentConversation = GetConversation(npcType, convoIndex);
        currentDialogIndex = 0;
        characterNameBox.text = currentConversation[currentDialogIndex].Item1;
        dialogBox.text = currentConversation[currentDialogIndex].Item2;
        this.gameObject.SetActive(true);
        currentDialogIndex++;
    }

    public bool DisplayNextDialog()
    {
        if(currentDialogIndex < currentConversation.Count)
        {
            characterNameBox.text = currentConversation[currentDialogIndex].Item1;
            dialogBox.text = currentConversation[currentDialogIndex].Item2;
            currentDialogIndex++;
            return true;
        }
        else
        {
            return false;
        }
    }

    public void EndConversation()
    {
        currentConversation = null;
        currentDialogIndex = 0;
        this.gameObject.SetActive(false);
    }
}
