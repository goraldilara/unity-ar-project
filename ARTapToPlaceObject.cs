using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.Experimental.XR;
using System;
using UnityEngine.XR.ARSubsystems;

public class ARTapToPlaceObject : MonoBehaviour
{
    //variables for set on unity
    public GameObject placementIndicator;
    public GameObject objectToPlace;

    private GameObject spawnedObject;
    private ARRaycastManager aRRaycastManager;
    private Pose placementPose;
    private bool placementPoseIsValid = false;

    private bool isPlaced = false;
    List<ARRaycastHit> m_Hits = new List<ARRaycastHit>();
    private bool isTouched = false;
    Camera arCam;

    //scale
    Vector2 firstStartPosition;
    Vector2 secondStartPosition;
    Vector2 firstEndPosition;
    Vector2 secondEndPosition;

    float endDistance;
    float currentDistance;

    Vector3 scaleBigger = new Vector3(0.005f, 0.005f, 0.005f);
    Vector3 scaleSmaller = new Vector3(-0.005f, -0.005f, -0.005f);

    //rotation
    float rotationSpeed = 20;

    // Start is called before the first frame update
    void Start()
    {
        //initializing of variables
        aRRaycastManager = FindObjectOfType<ARRaycastManager>();
        arCam = GameObject.Find("AR Camera").GetComponent<Camera>();
        spawnedObject = null;
    }

    // Update is called once per frame
    void Update()
    {
        //functions to set location of placement indicator momentarily
        UpdatePlacementPose();
        UpdatePlacementIndicator();

        //spawn object on first touch on placement indicator
        if(isPlaced == false && placementPoseIsValid && Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            PlaceObject();
        }

        //touch operations after object is spawned
        if(Input.touchCount > 0 && isPlaced == true)
        {
            //drag, move and rotate operations with one touch on the screen
            if(Input.touchCount == 1)
            {
                //variables for ray casting on spawned object
                RaycastHit hit;
                Ray ray = arCam.ScreenPointToRay(Input.GetTouch(0).position);

                if (aRRaycastManager.Raycast(Input.GetTouch(0).position, m_Hits))
                {
                    if (Input.GetTouch(0).phase == TouchPhase.Began)
                    {
                        if (Physics.Raycast(ray, out hit))
                        {
                            //control for if ray hit an object on the screen
                            if (hit.collider.gameObject)
                            {
                                //i have no idea what this thing is doing lol
                                if (spawnedObject == null)
                                {
                                    spawnedObject = hit.collider.gameObject;
                                }
                                isTouched = true;
                            }
                        }
                    }
                    //move and drag operation
                    else if (Input.GetTouch(0).phase == TouchPhase.Moved && spawnedObject != null && isTouched == true)
                    {
                        spawnedObject.transform.position = m_Hits[0].pose.position;
                    }
                    //rotate operation
                    else if (Input.GetTouch(0).phase == TouchPhase.Moved && spawnedObject != null && isTouched == false)
                    {
                        spawnedObject.transform.Rotate(0f, Input.GetTouch(0).deltaPosition.x, Input.GetTouch(0).deltaPosition.y);
                         //spawnedObject.transform.Rotate(0f, Input.GetTouch(0).deltaPosition.y, 0f);
                    }
                    //cut the connection on object when touch is ended
                    if (Input.GetTouch(0).phase == TouchPhase.Ended)
                    {
                        isTouched = false;
                    }
                }
            }
            //pinch and scale operation with two touch on the screen
            else if (Input.touchCount == 2)
            {

            //    startDistance = 0;
            //    currentDistance = 0;

                Touch firstTouch = Input.GetTouch(0);
                Touch secondTouch = Input.GetTouch(1);

                if (firstTouch.phase == TouchPhase.Began && secondTouch.phase == TouchPhase.Began)
                {
                    currentDistance = Vector3.Distance(firstTouch.position, secondTouch.position);

                    //firstStartPosition = firstTouch.position;
                    //secondStartPosition = secondTouch.position;
                }
                else if (firstTouch.phase == TouchPhase.Moved && secondTouch.phase == TouchPhase.Moved)
                {

                    endDistance = Vector3.Distance(firstTouch.position, secondTouch.position);

                    //firstEndPosition = firstTouch.position;
                    //secondEndPosition = secondTouch.position;

                    //float startDistance = firstStartPosition.magnitude - secondStartPosition.magnitude;
                    //float endDistance = firstEndPosition.magnitude - secondEndPosition.magnitude;

                    if (currentDistance > endDistance)
                    {
                        scale(scaleSmaller);
                    }
                    else
                    {
                        scale(scaleBigger);
                    }
                    currentDistance = endDistance;
                }
            }
        }
    }

    private void PlaceObject()
    {
        spawnedObject = Instantiate(objectToPlace, placementPose.position, placementPose.rotation);
        isPlaced = true;
    }

    private void UpdatePlacementIndicator()
    {
        if (isPlaced == false && placementPoseIsValid)
        {
            placementIndicator.SetActive(true);
            placementIndicator.transform.SetPositionAndRotation(placementPose.position, placementPose.rotation);
        }
        else
        {
            placementIndicator.SetActive(false);
        }
    }

    private void UpdatePlacementPose()
    {
        var screenCenter = Camera.current.ViewportToScreenPoint(new Vector3(0.5f, 0.5f));
        var hits = new List<ARRaycastHit>();
        aRRaycastManager.Raycast(screenCenter, hits, TrackableType.Planes);

        placementPoseIsValid = hits.Count > 0;
        if (placementPoseIsValid)
        {
            placementPose = hits[0].pose;
        }
    }

    void scale(Vector3 scaleChange)
    {
        spawnedObject.transform.localScale += scaleChange;
    }

}
