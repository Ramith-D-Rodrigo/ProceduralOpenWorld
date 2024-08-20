using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC : MonoBehaviour
{
    public NPCStateMachine stateMachine;
    public NPCStateId initialState;

    public NPCType type;

    public NPCDialogSystem dialogSystem;
    // Start is called before the first frame update
    protected void Start()
    {
        stateMachine = new NPCStateMachine(this);
        dialogSystem = GetComponent<NPCDialogSystem>();
        RegisterStates();
        stateMachine.ChangeState(initialState);
    }

    // Update is called once per frame
    void Update()
    {
        stateMachine.Update();
    }

    protected void RegisterStates()  //This method will be overridden in the child class
    {
        //must have the idle state
        stateMachine.RegisterState(new IdleState());
        stateMachine.RegisterState(new TalkState());
    }
}
