using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaredState : NPCState
{
    public void Enter(NPC npc, NPCStateId previousState)
    {
        KidNPC kidNPC = npc as KidNPC;
        if (kidNPC != null)
        {
            npc.dialogSystem.ClearConversation(npc.type);
            npc.dialogSystem.SetupKidScaredConversation();
        }
    }

    public void Exit(NPC npc)
    {
    }

    public NPCStateId GetId()
    {
        return NPCStateId.Scared;
    }

    public void Update(NPC npc)
    {
        if (npc.IsWithinTalkRange && Input.GetKeyDown(NPCDialogSystem.interactKey) && npc.dialogSystem.CurrentConversation == null)
        {
            if (npc as KidNPC != null)
            {
                KidNPC kidNPC = npc as KidNPC;
                kidNPC.hasPlayerFound = true;
            }
            npc.player.StopAllMovements();
            npc.stateMachine.ChangeState(NPCStateId.Talk);
        }
        else
        {
            if (npc as KidNPC != null && (npc as KidNPC).hasPlayerFound)
            {
                npc.stateMachine.ChangeState(NPCStateId.Follow);
            }
        }
    }
}
