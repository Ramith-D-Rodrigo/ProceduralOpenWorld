using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC : MonoBehaviour
{
    public Player player;

    public NPCStateMachine stateMachine;
    public NPCStateId initialState;

    public NPCType type;

    public NPCDialogSystem dialogSystem;

    bool isWithinTalkRange = false;
    public bool IsWithinTalkRange
    {
        get
        {
            return isWithinTalkRange;
        }
        set
        {
            isWithinTalkRange = value;
        }
    }

    // Start is called before the first frame update
    protected void Start()
    {
        //The following lines must be in child class's Start method in that order
        //we have to override the Start method because RegisterStates can be different for each NPC
        // and start method can vary for each NPC

        //stateMachine = new NPCStateMachine(this);
        //RegisterStates();
        //stateMachine.ChangeState(initialState);
    }

    // Update is called once per frame
    protected void Update()
    {
        stateMachine.Update();
    }

    protected void RegisterStates()  //This method will be overridden in the child class
    {
        //must have the idle state
        stateMachine.RegisterState(new IdleState());
        stateMachine.RegisterState(new TalkState());
    }

    protected void OnTriggerStay(Collider other)
    {
        if (other.tag == "Player")
        {
           IsWithinTalkRange = true;
        }

    }

    protected void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            IsWithinTalkRange = false;
        }
    }
}
