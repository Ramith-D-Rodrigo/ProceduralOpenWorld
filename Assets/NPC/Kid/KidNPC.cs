using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class KidNPC : NPC
{
    public Transform playerTransform;
    public float maxTime = 1.0f;
    public float maxDistance = 2.0f;

    public NavMeshAgent agent;
    public Animator animator;
    public float timer = 0.0f;

    public SphereCollider SphereCollider;

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
        stateMachine.RegisterState(new FollowState());
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            stateMachine.ChangeState(NPCStateId.Follow);
        }
    }

    private void OnTriggerExit(Collider other)
    {
/*        Debug.Log("Trigger exit");
        if(other == SphereCollider)
        {
            Debug.Log("Trigger exit sphere");
            stateMachine.ChangeState(NPCStateId.Scared);
        }*/
    }
}
