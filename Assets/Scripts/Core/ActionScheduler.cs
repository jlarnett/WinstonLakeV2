using UnityEngine;

namespace Winston.Core
{

    public class ActionScheduler : MonoBehaviour
    {
        public IAction currentAction;

        public void StartAction(IAction action)
        {
            //If current action is = to action nothing is needed to be done return
            if(currentAction == action) return;
            
            //If current action is not null cancel current action
            if (currentAction != null)
            {
                currentAction.Cancel();
            }
            //Assign new action to current action
            currentAction = action;
        }

        public void CancelCurrentAction()
        {
            //call StartAction() above with null paramater
            StartAction(null);
        }
    }
}
