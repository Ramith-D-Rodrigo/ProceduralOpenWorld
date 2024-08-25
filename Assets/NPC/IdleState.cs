using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleState : NPCState
{
    public void Enter(NPC npc, NPCStateId previousState)
    {

    }

    public void Exit(NPC npc)
    {

    }

    public NPCStateId GetId()
    {
        return NPCStateId.Idle;
    }

    public void Update(NPC npc)
    {
        if(npc.IsWithinTalkRange && Input.GetKeyDown(NPCDialogSystem.interactKey))
        {
            npc.player.StopAllMovements();
            npc.stateMachine.ChangeState(NPCStateId.Talk);
        }
    }
}
