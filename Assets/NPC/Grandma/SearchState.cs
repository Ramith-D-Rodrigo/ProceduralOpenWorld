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
                npc.animator.SetBool("hasAskedForHelp", true);
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
        GrannyNPC grannyNPC = npc as GrannyNPC;
        if(progressIncrement == 0 && grannyNPC)
        {
            grannyNPC.grandSon.SetActive(true);
            progressIncrement = 1;
        }
    }

    public NPCStateId GetId()
    {
        return NPCStateId.Search;
    }

    public void Update(NPC npc)
    {
        if (npc.IsWithinTalkRange && Input.GetKeyDown(NPCDialogSystem.interactKey) && npc.dialogSystem.CurrentConversation == null)
        {
            npc.player.StopAllMovements();
            npc.stateMachine.ChangeState(NPCStateId.Talk);
        }
    } 
}
