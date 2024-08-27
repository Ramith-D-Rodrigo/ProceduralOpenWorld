using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowState : NPCState
{
    public void Enter(NPC npc, NPCStateId previousState)
    {
        if (npc as KidNPC != null)
        {
            KidNPC kidNPC = npc as KidNPC;
            npc.animator.SetBool("isFollowing", true);
            kidNPC.dialogSystem.ClearConversation(npc.type);
            kidNPC.dialogSystem.SetupKidFollowConversation();
            kidNPC.gameObject.GetComponent<SphereCollider>().enabled = true; // enable the collider
        }
    }

    public void Exit(NPC npc)
    {

    }

    public NPCStateId GetId()
    {
        return NPCStateId.Follow;
    }

    public void Update(NPC npc)
    {
        KidNPC kidNPC = npc as KidNPC;
        if(kidNPC != null)
        {
            kidNPC.timer -= Time.deltaTime;


            if (kidNPC.agent.isOnNavMesh)
            {
                if (kidNPC.timer < 0.0f)
                {
                    float sqrDistance = (kidNPC.player.CurrentTransform.position - kidNPC.agent.destination).sqrMagnitude;
                    if (sqrDistance > kidNPC.maxDistance * kidNPC.maxDistance)
                    {
                        kidNPC.agent.SetDestination(kidNPC.player.CurrentTransform.position);
                    }
                    kidNPC.timer = kidNPC.maxTime;

                }
                kidNPC.animator.SetFloat("Speed", kidNPC.agent.velocity.magnitude);
            }
        }

        if (npc.IsWithinTalkRange && Input.GetKeyDown(NPCDialogSystem.interactKey) && npc.dialogSystem.CurrentConversation == null)
        {
            npc.player.StopAllMovements();
            npc.stateMachine.ChangeState(NPCStateId.Talk);
        }
    }
}
