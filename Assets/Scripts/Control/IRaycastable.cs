using UnityEngine;
using UnityStandardAssets.Characters.ThirdPerson;

namespace Winston.Control
{
    public interface IRaycastable
    {
        bool HandleRaycast(ThirdPersonUserControl playerController);
    }
}