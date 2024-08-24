using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishingGuyNPC : NPC
{
    bool isWithinRange = false;

    public GameObject player;
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
        if(isWithinRange)
        {
            if (Input.GetKeyDown(NPCDialogSystem.interactKey) && stateMachine.GetCurrentState() != NPCStateId.Talk)
            {
                player.GetComponent<ThirdPersonController>().enabled = false;
                player.GetComponent<Animator>().SetFloat("Speed", 0.0f);
                //change the state to talk
                stateMachine.ChangeState(NPCStateId.Talk);
            }
            else if (Input.GetKeyDown(NPCDialogSystem.interactKey) && stateMachine.GetCurrentState() == NPCStateId.Talk)
            {
                bool canContinue = dialogSystem.DisplayNextDialog();
                if (!canContinue)
                {
                    stateMachine.ChangeState(NPCStateId.Idle);
                    player.GetComponent<ThirdPersonController>().enabled = true;
                }
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
            isWithinRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            isWithinRange = false;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Player")
        {

        }
    }
}
