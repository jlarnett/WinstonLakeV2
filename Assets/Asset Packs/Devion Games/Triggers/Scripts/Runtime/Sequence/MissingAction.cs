using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DevionGames
{
    [UnityEngine.Scripting.APIUpdating.MovedFromAttribute(false,null,null,"Log1")]
    [ExcludeFromCreation]
    public class MissingAction : Action
    {
  
        public override ActionStatus OnUpdate()
        {
            Debug.LogWarning("Missing referenced script in "+gameObject+".");
            return ActionStatus.Success;
        }
    }
}