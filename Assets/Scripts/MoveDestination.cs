using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MoveDestination : MonoBehaviour {
    public Transform goal;
    public bool follow;
    private NavMeshAgent agent;

    void Start()
    {
        follow = true;
        Move(goal.position);
        //StartCoroutine(Move(goal.position));
        
    }

    void Move (Vector3 position)
    {
        agent = GetComponent<NavMeshAgent>();
        agent.destination = position;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name == "Goal")
        {
            print("You made it!");
            follow = false;
            agent.destination = this.transform.position;
        }
    }

}


// TODO:
// onCollisionEnter, agent.stop or similar method.