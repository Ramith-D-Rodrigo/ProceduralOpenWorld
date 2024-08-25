using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SearchState : NPCState
{
    int progressIncrement = 0;
    public void Enter(NPC npc, NPCStateId previousState)
    {
        GrannyNPC grannyNPC = npc as GrannyNPC;
        if (grannyNPC != null)
        {
            if (progressIncrement == 0)
            {
                npc.dialogSystem.SetupGrannySearchConversation();
            }
            else if (progressIncrement == 1)
            {
                npc.dialogSystem.ClearConversation(npc.type);
                npc.dialogSystem.SetupGrannySearchProgressConversation();
            }
            else
            {
            }
        }
    }

    public void Exit(NPC npc)
    {
        progressIncrement = 1;
    }

    public NPCStateId GetId()
    {
        return NPCStateId.Search;
    }

    public void Update(NPC npc)
    {
        if (npc.IsWithinTalkRange && Input.GetKeyDown(NPCDialogSystem.interactKey))
        {
            npc.player.StopAllMovements();
            npc.stateMachine.ChangeState(NPCStateId.Talk);
        }
    } 
}
