using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HappyState : NPCState
{
    public bool hasConversed = false;
    public void Enter(NPC npc, NPCStateId previousState)
    {
        if (npc as GrannyNPC != null)
        {
            npc.dialogSystem.ClearConversation(npc.type);
            npc.dialogSystem.SetupGrannyFoundKidConversation();

        }
        else if (npc as KidNPC != null)
        {
            KidNPC kidNPC = npc as KidNPC;
            kidNPC.agent.isStopped = true;
            kidNPC.animator.SetFloat("Speed", 0.0f);
            kidNPC.animator.SetBool("isFollowing", false);
            kidNPC.gameObject.GetComponent<SphereCollider>().enabled = false; // disable the collider
            kidNPC.IsWithinTalkRange = false;
        }
    }

    public void Exit(NPC npc)
    {

    }

    public NPCStateId GetId()
    {
        return NPCStateId.Happy;
    }

    public void Update(NPC npc)
    {
        if (npc.IsWithinTalkRange && Input.GetKeyDown(NPCDialogSystem.interactKey) && npc.dialogSystem.CurrentConversation == null 
            && !hasConversed)
        {
            if(npc as GrannyNPC != null)
            {
                npc.animator.SetBool("hasFoundGrandSon", true);
            }

            npc.player.StopAllMovements();
            npc.stateMachine.ChangeState(NPCStateId.Talk);
            hasConversed = true;
        }
    }
}
