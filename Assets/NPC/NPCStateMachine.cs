using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCStateMachine
{
    public NPCState[] states;
    public NPC npc;

    NPCStateId currentState;

    public NPCStateMachine(NPC npc)
    {
        this.npc = npc;
        int numStates = System.Enum.GetNames(typeof(NPCStateId)).Length;
        states = new NPCState[numStates];
    }

    //preallocate all states
    public void RegisterState(NPCState state)
    {
        int index = (int)state.GetId();
        states[index] = state;
    }

    public NPCState GetState(NPCStateId id)
    {
        return states[(int)id];
    }

    public void Update()
    {
        GetState(currentState).Update(npc);
    }

    public void ChangeState(NPCStateId newState)
    {
        GetState(currentState).Exit(npc);
        currentState = newState;
        GetState(currentState).Enter(npc);
    }

    public NPCStateId GetCurrentState()
    {
        return currentState;
    }
}
