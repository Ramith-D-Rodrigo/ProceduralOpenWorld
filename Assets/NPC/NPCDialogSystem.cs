using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCDialogSystem : MonoBehaviour
{
    private Dictionary<NPCType, List<string>[]> npcDialogs = new Dictionary<NPCType, List<string>[]>();
    //List<string> SwimmingGirlConversation = new List<string>();

    // Start is called before the first frame update
    void Start()
    {
        SetupConversations();
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
        List<string> convo1 = new List<string>();
        convo1.Add("What are you doing?");
        convo1.Add("I am fishing. What does it look like?");
        convo1.Add("That doesn't look like a fishing rod");
        convo1.Add("It's a special one. Leave me alone!");

        List<string> convo2 = new List<string>();
        convo2.Add("Hel-");
        convo2.Add("Shhh! Be quiet. You'll scare the fish away!");

        List<string>[] allConvos = {convo1, convo2};

        npcDialogs.Add(NPCType.FishingGuy, allConvos);
    }

    public List<string> GetConversation(NPCType npcType, int conversationIndex)
    {
        return npcDialogs[npcType][conversationIndex];
    }

    void SetupKidConversations()
    {

    }


}
