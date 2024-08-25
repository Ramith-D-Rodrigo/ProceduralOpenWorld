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
        convo1.Add(("Player", "That doesn't look like a fishing rod."));
        convo1.Add(("Fishing Guy", "It's a special one. Leave me alone!"));

        List<(string, string)> convo2 = new List<(string, string)>();
        convo2.Add(("Player" ,"Hel-"));
        convo2.Add(("Fishing Guy", "Shhh! Be quiet. You'll scare the fish away!"));

        List<(string, string)>[] allConvos = {convo1, convo2};
        npcDialogs.Add(NPCType.FishingGuy, allConvos);
    }

    public void SetupGrannySearchConversation()
    {
        List<(string, string)> convo = new List<(string, string)>();
        convo.Add(("Player", "Hello!"));
        convo.Add(("Granny", "Oh hello dear..."));
        convo.Add(("Player", "Are you looking for something?"));
        convo.Add(("Granny", "Actually, yes. I came here with my grandson, but he ran off somewhere and now I can't find him."));
        convo.Add(("Player", "Mind if I go and look for him?"));
        convo.Add(("Granny", "Oh, that would be wonderful! Thank you so much!"));
        convo.Add(("Granny", "He's a small boy, wearing a cap. Also he likes to play in the woods."));
        convo.Add(("Player", "I'll go and look for him. Don't worry!"));

        List<(string, string)>[] allConvos = {convo};
        npcDialogs.Add(NPCType.Granny, allConvos);
    }

    public void SetupGrannySearchProgressConversation()
    {
        List<(string, string)> convo = new List<(string, string)>();
        convo.Add(("Granny", "Have you found my grandson yet?"));
        convo.Add(("Player", "Not yet, but I'm still looking."));
        convo.Add(("Granny", "Please hurry!"));
        convo.Add(("Granny", "He's a small boy, wearing a cap. Also he likes to play in the woods."));
        convo.Add(("Player", "Got it!"));

        List<(string, string)>[] allConvos = {convo};
        npcDialogs.Add(NPCType.Granny, allConvos);
    }

    public void SetupGrannyFoundKidConversation()
    {
        List<(string, string)> convo = new List<(string, string)>();
        convo.Add(("Player", "I found your grandson!"));
        convo.Add(("Granny", "Oh thank you so much!"));
        convo.Add(("Granny", "I was so worried about him!"));
        convo.Add(("Player", "No problem!"));
        convo.Add(("Granny", "Here, take this as a token of my gratitude."));
        convo.Add(("Player", "Thank you!"));

        List<(string, string)>[] allConvos = {convo};
        npcDialogs.Add(NPCType.Granny, allConvos);
    }

    public void SetupKidScaredConversation()
    {
        List<(string, string)> convo = new List<(string, string)>();
        convo.Add(("Player", "Hey! Are you okay?"));
        convo.Add(("Kid", "I'm scared!"));
        convo.Add(("Player", "What happened?"));
        convo.Add(("Kid", "I got lost and I don't know how to get back to my grandma."));
        convo.Add(("Player", "Ohh so you're Granny's grandson?"));
        convo.Add(("Kid", "Yes!"));
        convo.Add(("Player", "Don't worry! I'll take you back to her!"));

        List<(string, string)>[] allConvos = { convo };
        npcDialogs.Add(NPCType.Kid, allConvos);
    }

    public void SetupKidFollowConversation()
    {
        List<(string, string)> convo1 = new List<(string, string)>();
        convo1.Add(("Player", "Follow me."));
        convo1.Add(("Kid", "Okay!"));

        List<(string, string)> convo2 = new List<(string, string)>();
        convo2.Add(("Kid", "Are we there yet?"));
        convo2.Add(("Player", "Almost there!"));

        List<(string, string)>[] allConvos = { convo1, convo2 };
        npcDialogs.Add(NPCType.Kid, allConvos);
    }

    public void ClearConversation(NPCType npcType)
    {
        npcDialogs.Remove(npcType);
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
