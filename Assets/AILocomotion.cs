using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AILocomotion : MonoBehaviour
{
    public Transform playerTransform;
    public float maxTime = 1.0f;
    public float maxDistance = 2.0f;

    NavMeshAgent agent;
    Animator animator;
    float timer = 0.0f;
    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        timer -= Time.deltaTime;


        if(agent.isOnNavMesh)
        {
            if (timer < 0.0f)
            {
                float sqrDistance = (playerTransform.position - agent.destination).sqrMagnitude;
                if (sqrDistance > maxDistance * maxDistance)
                {
                    agent.SetDestination(playerTransform.position);
                }
                timer = maxTime;
            }
            animator.SetFloat("Speed", agent.velocity.magnitude);
        }
    }
}
