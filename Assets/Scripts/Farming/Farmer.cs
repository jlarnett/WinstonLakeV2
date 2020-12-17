using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;
using Winston.Core;

namespace Winston.Farming
{
    public class Farmer : MonoBehaviour, IAction
    {
        [SerializeField] private FarmResource target;
        [SerializeField] private float resourceMoveSpeedFraction = 1;
        [SerializeField] private float farmingRange = 2f;

        [SerializeField] private Animator animator = null;
        [SerializeField] private Energy energy = null;

        [SerializeField] private int turnAnimationSpeed = 100;

        private ActionScheduler actionScheduler;

        void Start()
        {
            actionScheduler = GetComponent<ActionScheduler>();
        }

        void Update()
        {
            if (target != null)
            {
                var q = Quaternion.LookRotation(target.transform.position - transform.position);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, q, turnAnimationSpeed * Time.deltaTime);
            }
        
        }
        
        public bool TryHarvest(FarmResource farmResource)
        {
            //Done this way since its called in Update Every frame when target is not null.
            if ((farmResource.transform.position - transform.position).sqrMagnitude > farmingRange * farmingRange) 
            {    
                //Really Efficient way to check distance between unit and target
                return false;
            }
            else
            {
                return true;
            }
        }

        public void Harvest(FarmResource farmResource)
        {
            actionScheduler.StartAction(this);

            //Set farmResource Target
            target = farmResource;

            //Fire grab animation
            animator.SetBool("GrabResource", true);
        }

        public void StopHarvest()
        {
            target = null;
            animator.SetBool("GrabResource", false);
        }

        public void Cancel()
        {
            target = null;
            animator.SetBool("GrabResource", false);
        }
    }
}

