using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SearchProgressState : NPCState
{
    public void Enter(NPC npc, NPCStateId previousState)
    {
        GrannyNPC grannyNPC = npc as GrannyNPC;
        if (grannyNPC != null)
        {
            npc.dialogSystem.ClearConversation(npc.type);
            npc.dialogSystem.SetupGrannySearchProgressConversation();
        }
    }

    public void Exit(NPC npc)
    {

    }

    public NPCStateId GetId()
    {
        return NPCStateId.SearchProgress;
    }

    public void Update(NPC npc)
    {

    }
}
