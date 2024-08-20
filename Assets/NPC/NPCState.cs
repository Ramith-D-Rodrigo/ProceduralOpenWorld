using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum NPCStateId
{
    Idle,
    Follow,
    Scared,
    Happy,
    Talk
}

public interface NPCState
{
    NPCStateId GetId();

    void Enter(NPC npc);
    void Exit(NPC npc);
    void Update(NPC npc);
}
