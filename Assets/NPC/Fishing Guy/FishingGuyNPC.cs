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
        if(isWithinRange)
        {
            if (Input.GetKeyDown(NPCDialogSystem.interactKey) && stateMachine.GetCurrentState() != NPCStateId.Talk)
            {
                player.StopAllMovements();
                //change the state to talk
                stateMachine.ChangeState(NPCStateId.Talk);
            }
        }
    }

    new protected void RegisterStates()
    {
        stateMachine.RegisterState(new IdleState());
        stateMachine.RegisterState(new TalkState());
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            IsWithinTalkRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            IsWithinTalkRange = true;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Player")
        {

        }
    }
}
