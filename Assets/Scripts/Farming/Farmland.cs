using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Winston.Control;

public class Farmland : MonoBehaviour, IRaycastable
{

    [SerializeField] private GameObject highlight = null;
    [SerializeField] private float farmingRange = 5f;

    private bool lightup = true;
    private ThirdPersonUserControl playerController;


    // Start is called before the first frame update
    void Start()
    {
        playerController = GameObject.FindGameObjectWithTag("Player").GetComponent<ThirdPersonUserControl>();
    }

    // Update is called once per frame
    void Update()
    {
         highlight.SetActive(CheckMousePosition(playerController));
    }

    public bool HandleRaycast(ThirdPersonUserControl playerController)
    {

        if((transform.position - playerController.transform.position).sqrMagnitude < farmingRange * farmingRange)
        {
            highlight.SetActive(true);
            return true;
        }
        else
        {
            EnableHighlight(false);
            return false;
        }
    }

    private bool CheckMousePosition(ThirdPersonUserControl playerController)
    {
        RaycastHit[] hits = playerController.RaycastAllSorted();

        foreach (var hit in hits)
        {
            if (hit.collider == gameObject.GetComponent<Collider>())
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        return false;
    }


    private void EnableHighlight(bool enablevalue)
    {
        highlight.SetActive(enablevalue);
    }
}
