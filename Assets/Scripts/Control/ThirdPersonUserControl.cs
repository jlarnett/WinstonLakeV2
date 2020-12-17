using System;
using UnityEngine;
using UnityStandardAssets.Characters.ThirdPerson;
using UnityStandardAssets.CrossPlatformInput;

namespace Winston.Control
{
    [RequireComponent(typeof (ThirdPersonCharacter))]
    public class ThirdPersonUserControl : MonoBehaviour
    {

        [SerializeField] private Health health = null;
        [SerializeField] private float raycastRadius = 1f;                          //raycast size

        //Custom SERIALIZABLE FIELDS

        private ThirdPersonCharacter m_Character; // A reference to the ThirdPersonCharacter on the object
        private Transform m_Cam;                  // A reference to the main camera in the scenes transform
        private Vector3 m_CamForward;             // The current forward direction of the camera

        //Mmove is the destination to move tool at end up script.
        private Vector3 m_Move;
        private bool m_Jump;                      // the world-relative desired move direction, calculated from the camForward and user input.


        
        private void Start()
        {
            //Get health component
            health = GetComponent<Health>();
            m_Character = GetComponent<ThirdPersonCharacter>();

            // get the transform of the main camera
            if (Camera.main != null)
            {
                m_Cam = Camera.main.transform;
            }
            else
            {
                Debug.LogWarning(
                    "Warning: no main camera found. Third person character needs a Camera tagged \"MainCamera\", for camera-relative controls.", gameObject);
                // we use self-relative controls in this case, which probably isn't what the user wants, but hey, we warned them!
            }
        }

        private void Update()
        {

            //Interact with component is how we implement IRaycastable
            if (InteractWithComponent()) return;            //Interact with IRAYCASTABLE components in the world

            if (!m_Jump)
            {
                m_Jump = CrossPlatformInputManager.GetButtonDown("Jump");
            }

            if (transform.position.y < -10)
            {
                health.TakeDamage(1);
            }
        }

        private bool InteractWithComponent()
	    {
	        //Interact Order 1
	        //Extremely important. Interacts with all raycastable items that can be clicked. Sorted in order of closeness of position.
	        RaycastHit[] hits = RaycastAllSorted();                                 // Get all hits from mouse raycast

	        foreach (RaycastHit hit in hits)                                        //We go through all hits in the raycast
	        {
	            IRaycastable[] raycastables = hit.transform.GetComponents<IRaycastable>();      //Check all hits to sett if it has Iraycastable component. and then store them in array.

	            foreach (IRaycastable raycastable in raycastables)                              //We cycle through all the raycastable components we collected in our hits
	            {
	                if (raycastable.HandleRaycast(this))
	                {
	                    return true;
	                }
	            }
	        }

	        return false;                                                   //If we make it here on the update cycle that means there were no raycastable components
	    }

	    RaycastHit[] RaycastAllSorted()
	    {
	        //Sorts the order of raycast relative to distance of camera
	        //Get all hits
	        RaycastHit[] hits = Physics.SphereCastAll(GetMouseRay(), raycastRadius);

	        //Sort by distance
	        float[] distances = new float[hits.Length];     //Create an aray same size as hits 

	        //build array distances
	        for (int i = 0; i < hits.Length; i++)
	        {
	            distances[i] = hits[i].distance;
	        }

	        //sort the hits
	        Array.Sort(distances, hits);

	        //Return sortged array
	        return hits;
	    }

	    private static Ray GetMouseRay()
	    {
	        //Sends a ray where player clicks within camera & Detects colisions for player movement
	        return Camera.main.ScreenPointToRay(Input.mousePosition);
	    }


        // Fixed update is called in sync with physics
        private void FixedUpdate()
        {
            // read inputs
            float h = CrossPlatformInputManager.GetAxis("Horizontal");
            float v = CrossPlatformInputManager.GetAxis("Vertical");
            bool crouch = Input.GetKey(KeyCode.C);

            // calculate move direction to pass to character
            if (m_Cam != null)
            {
                // calculate camera relative direction to move:
                m_CamForward = Vector3.Scale(m_Cam.forward, new Vector3(1, 0, 1)).normalized;
                m_Move = v*m_CamForward + h*m_Cam.right;
            }
            else
            {
                // we use world-relative directions in the case of no main camera
                m_Move = v*Vector3.forward + h*Vector3.right;
            }
#if !MOBILE_INPUT
			// walk speed multiplier
	        if (Input.GetKey(KeyCode.LeftShift)) m_Move *= 0.5f;
#endif

            // pass all parameters to the character control script
            m_Character.Move(m_Move, crouch, m_Jump);
            m_Jump = false;
        }
    }
}
