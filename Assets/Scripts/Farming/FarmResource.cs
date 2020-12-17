using UnityEngine;
using Winston.Control;
using Winston.Farming;

public class FarmResource : MonoBehaviour, IRaycastable
{

    private bool isHarvested = false;

    //Animals
    //We need to know which resource this is
    //We need to know which animation to use
    //We need to know what to reward farmer & how much
    //A way to tell if it has been harvested for the day
    //
    public bool HandleRaycast(ThirdPersonUserControl playerController)
    {
        
            if (!enabled) return false;
            //if(!fighter.enabled) return false;

            if (!playerController.GetComponent<Farmer>().TryHarvest(this))  //We check if the calling player can attack the gameobject enemy attached.
            {
                return false;                                           //if player cant attack we return false
            }

            if (Input.GetMouseButtonDown(0))        //If Mouse button is held down  and all things are above qualifiers work we attack it
            {
                playerController.GetComponent<Farmer>().Harvest(this);       //We attack the gameobject enemey attached
            }

            return true;
    }
}
