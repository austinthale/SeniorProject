using UnityEngine;
using UnityEngine.AI;
using UnityStandardAssets.Characters.ThirdPerson;

public class PlayerController : MonoBehaviour
{

    //public Camera cam = Camera.main;

    public NavMeshAgent agent;

    public ThirdPersonCharacter character;

    public bool IsSimMode = false;

    void Start()
    {
        //character.GetComponent<ThirdPersonCharacter>();
        agent.updateRotation = false;
        IsSimMode = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (IsSimMode)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit))
                {
                    agent.SetDestination(hit.point);
                }
            }

            if (agent.remainingDistance > agent.stoppingDistance)
            {
                character.Move(agent.desiredVelocity, false, false);
            }
            else
            {
                character.Move(Vector3.zero, false, false);
            }
        }
        else
        {
            character.Move(Vector3.zero, false, false);
            agent.SetDestination(transform.position);
        }


    }

    public void stopMoving()
    {
        agent.SetDestination(gameObject.transform.position);
    }
}