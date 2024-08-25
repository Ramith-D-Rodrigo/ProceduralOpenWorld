using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum NPCStateId
{
    Idle,
    Follow,
    Scared,
    Happy,
    Talk,
    Search,
    SearchProgress
}

public interface NPCState
{
    NPCStateId GetId();

    void Enter(NPC npc, NPCStateId previousState);
    void Exit(NPC npc);
    void Update(NPC npc);
}
