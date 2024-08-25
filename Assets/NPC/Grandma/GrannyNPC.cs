using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrannyNPC : NPC
{
    // Start is called before the first frame update
    void Start()
    {
        type = NPCType.Granny;
        stateMachine = new NPCStateMachine(this);
        RegisterStates();
        stateMachine.ChangeState(initialState);
    }

    new protected void RegisterStates()
    {
        stateMachine.RegisterState(new IdleState());
        stateMachine.RegisterState(new TalkState());
        stateMachine.RegisterState(new SearchState());
    }
}
