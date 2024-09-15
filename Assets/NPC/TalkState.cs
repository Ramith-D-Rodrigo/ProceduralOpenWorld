using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TalkState : NPCState
{
    NPCStateId previousState;
    public void Enter(NPC npc, NPCStateId previousState)
    {
        this.previousState = previousState;
        int convoIndex = Random.Range(0, npc.dialogSystem.GetConversationCount(npc.type));
        npc.dialogSystem.StartConverstation(npc.type, convoIndex);
    }

    public void Exit(NPC npc)
    {
        npc.dialogSystem.EndConversation();
    }

    public NPCStateId GetId()
    {
        return NPCStateId.Talk;
    }

    public void Update(NPC npc)
    {
        if(Input.GetKeyDown(NPCDialogSystem.interactKey))
        {
            bool canContinue = npc.dialogSystem.DisplayNextDialog();
            if (!canContinue)
            {
                npc.stateMachine.ChangeState(previousState);
                npc.player.EnableAllMovements();
            }
        }
    }
}
