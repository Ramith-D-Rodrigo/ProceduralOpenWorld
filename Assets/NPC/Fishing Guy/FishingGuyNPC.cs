using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishingGuyNPC : NPC
{
    bool isWithinRange = false;

    // Start is called before the first frame update
    void Start()
    {
        type = NPCType.FishingGuy;
        stateMachine = new NPCStateMachine(this);
        RegisterStates();
        stateMachine.ChangeState(initialState);
    }

    // Update is called once per frame
    void Update()
    {
        base.Update();
    }

    new protected void RegisterStates()
    {
        stateMachine.RegisterState(new IdleState());
        stateMachine.RegisterState(new TalkState());
    }
}
