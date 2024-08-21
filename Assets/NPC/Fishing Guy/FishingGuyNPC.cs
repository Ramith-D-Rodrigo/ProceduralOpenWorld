using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishingGuyNPC : NPC
{
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
        
    }

    new protected void RegisterStates()
    {
        stateMachine.RegisterState(new IdleState());
        stateMachine.RegisterState(new TalkState());
    }

    private void OnTriggerEnter(Collider other)
    {
/*        if (other.tag == "Player")
        {
            stateMachine.ChangeState(NPCStateId.Follow);
        }*/
    }

    private void OnTriggerExit(Collider other)
    {
/*        if (other.tag == "Player")
        {
            stateMachine.ChangeState(NPCStateId.Scared);
        }*/
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Player")
        {
            if (Input.GetKeyDown(NPCDialogSystem.interactKey) && stateMachine.GetCurrentState() != NPCStateId.Talk)
            {
                other.gameObject.GetComponent<ThirdPersonController>().enabled = false;
                //change the state to talk
                stateMachine.ChangeState(NPCStateId.Talk);
            }
            else if(Input.GetKeyDown(NPCDialogSystem.interactKey) && stateMachine.GetCurrentState() == NPCStateId.Talk)
            {
                bool canContinue = dialogSystem.DisplayNextDialog();
                if(!canContinue)
                {
                    stateMachine.ChangeState(NPCStateId.Idle);
                    other.gameObject.GetComponent<ThirdPersonController>().enabled = true;
                }
            }
        }
    }
}
