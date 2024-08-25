using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaredState : NPCState
{
    public void Enter(NPC npc, NPCStateId previousState)
    {
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
        KidNPC kidNPC = npc as KidNPC;
    }
}
