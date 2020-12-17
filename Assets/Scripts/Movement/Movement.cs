using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.AI;

namespace Winston.Movement
{

    public class Movement : MonoBehaviour
    {
        [SerializeField] private Transform target = null;
        [SerializeField] private NavMeshAgent agent = null;
        [SerializeField] private float maxSpeed = 3;
        [SerializeField] private float movementBuffer = 1;
        
        //Class References
        //private ActionScheduler actionScheduler;

        private Health health;
        private Animator animator;

        private void Awake()
        {
            //Assigns NavmeshAgent to Whatever Gameobject Assigned (Character) Component<NavMeshAgent>
            health = GetComponent<Health>();
            animator = GetComponent<Animator>();

            //actionScheduler = GetComponent<ActionScheduler>();
        }

        void Start()
        {
            if (target == null) 
            {
                target = transform;
            }
        }

        void Update()
        {
        }

        public bool CanMoveTo(Vector3 destination)
        {
            //If we have a complete path return true. Otherwise false
            NavMeshPath path = new NavMeshPath();
            bool hasPath = NavMesh.CalculatePath(transform.position, destination, NavMesh.AllAreas, path);          //We get the path our player is trying to take

            if (!hasPath) return false;                                             //If we dont have path return false
            if (path.status != NavMeshPathStatus.PathComplete) return false;        //Keep from using incomplete paths... may change later for teleporting over water

            return true;
        }

        public void StartMoveAction(Vector3 destination, float speedFraction)
        {
            MoveTo(destination, speedFraction);
        }

        public void MoveTo(Vector3 destination,float speedFraction)
        {

            if (agent.isOnNavMesh)
            {
                if (agent.isStopped)
                {
                    agent.isStopped = false;
                }
            }

            //Sets NavMeshAgents destination to Vector3 Destination && makes sure agents.isStopped = false -> Moves character
            agent.enabled = true;
            agent.destination = destination;

            agent.speed = maxSpeed * Mathf.Clamp01(speedFraction);
            agent.isStopped = false;
        }

        private void UpdateAnimator()
        {
            //Tells animator your moving foward at X units istead of global works better for animators.
            Vector3 velocity = agent.velocity;
            Vector3 localVelocity = transform.InverseTransformDirection(velocity);  // Takes the global Vector3 position & turns into local
            float speed = localVelocity.z;

            animator.SetFloat("fowardSpeed", speed); //Passes speed value into Animator blend or fowardspeed value
        }

        public void Cancel()
        {
            //Sets Agent.isStopped = true -> Stop character
            agent.isStopped = true;
        }
    }
}




