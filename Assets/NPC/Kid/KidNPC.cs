using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class KidNPC : NPC
{
    public float maxTime = 0.5f;
    public float maxDistance = 2.0f;

    public NavMeshAgent agent;
    public float timer = 0.0f;

    public SphereCollider SphereCollider;

    public bool hasPlayerFound = false;

    new protected void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        SphereCollider = GetComponent<SphereCollider>();

        type = NPCType.Kid;

        stateMachine = new NPCStateMachine(this);
        RegisterStates();
        stateMachine.ChangeState(initialState);
    }

    new protected void RegisterStates()
    {
        stateMachine.RegisterState(new IdleState());
        stateMachine.RegisterState(new ScaredState());
        stateMachine.RegisterState(new TalkState());
        stateMachine.RegisterState(new FollowState());
        stateMachine.RegisterState(new HappyState());
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.GetComponent<GrannyNPC>())
        {
            stateMachine.ChangeState(NPCStateId.Happy);
        }
    }

    new private void OnTriggerExit(Collider other)
    {
        base.OnTriggerExit(other);
        if (other.GetComponent<GrannyNPC>())
        {
            stateMachine.ChangeState(NPCStateId.Follow);
        }
    }
}
