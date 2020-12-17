using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Resource", menuName = "farmResource/Make New FarmResource", order = 0)]  //Scriptable object!
public class Resource : MonoBehaviour
{
    //Animation override
    [SerializeField] public AnimatorOverrideController weaponAnimatorOverride = null; //Things that will cahnge based upon having a different weapon
    private const string weaponName = "ResourceName";



}
