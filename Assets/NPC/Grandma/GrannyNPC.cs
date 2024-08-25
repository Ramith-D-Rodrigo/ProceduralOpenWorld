using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrannyNPC : NPC
{
    // Start is called before the first frame update

    public bool hasPlayerFoundGrandSon = false;
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
        stateMachine.RegisterState(new HappyState());
    }

    new protected void Update()
    {
        base.Update();
        if (hasPlayerFoundGrandSon)
        {

        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.GetComponent<KidNPC>())
        {
            stateMachine.ChangeState(NPCStateId.Happy);
        }
    }

    new private void OnTriggerExit(Collider other)
    {
        base.OnTriggerExit(other);
        if (other.GetComponent<KidNPC>())
        {
            stateMachine.ChangeState(NPCStateId.Search);
        }
    }
}
