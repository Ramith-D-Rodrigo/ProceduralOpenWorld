using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowState : NPCState
{
    public void Enter(NPC npc)
    {

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
}
