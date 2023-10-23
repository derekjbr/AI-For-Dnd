using System.Collections;
using System.Collections.Generic;
using System.Xml.Schema;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(CameraMovement))]
public class SelectionController : MonoBehaviour
{
    public GameObject SelectedIcon;

    private CameraMovement CameraMovementControl;
    private PlayableCharacterController SelectedController;
    
    void Start()
    {
        CameraMovementControl = GetComponent<CameraMovement>();
        SelectedController = null;
    }

    // Update is called once per frame
    void Update()
    {  
        if(Input.GetMouseButtonDown((int)MouseButton.Left))
        {
            RaycastHit hit;
            Ray cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(cameraRay, out hit))
            {
                GameObject hitObject = hit.transform.gameObject;
                if (hitObject != null)
                {
                    PlayableCharacterController hitController = hitObject.GetComponent<PlayableCharacterController>();
                    if (hitController != null)
                    {
                        if(SelectedController != hitController)
                        {
                            // Update Old and New Selected Controller
                            if(SelectedController != null)
                                SelectedController.IsSelected = false;
                            SelectedController = hitController;
                            SelectedController.IsSelected = true;

                            // Set the camera and selected icon's selected controller 
                            CameraMovementControl.Selected = hitObject;
                            SelectedIcon.transform.parent = hitObject.transform;
                            SelectedIcon.transform.localPosition = new Vector3(0,0.01f,0);
                        } 
                    } 
                    else if(SelectedController != null)
                    {
                        SelectedController.SetDestination(hit.point);
                    }
                }
            }
        }

        if (Input.GetMouseButtonDown((int)MouseButton.Right))
        {
            RaycastHit hit;
            Ray cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(cameraRay, out hit))
            {
                if (SelectedController != null)
                {
                    SelectedController.SetTestDestination(hit.point);
                }
            }
        }
    }
}
