using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class TrackImage : MonoBehaviour
{
    //Reference to AR tracked image manager component
    private ARTrackedImageManager trackedImagesManager;

    //Array of prefabs to instantiate later
    public GameObject[] prefabsToPlace;

    //Dictionary of instantiated prefabs to control the instantiated prefabs with the same name from the prefabsToPlace array
    private readonly Dictionary<string, GameObject> instantiatedPrefabs = new Dictionary<string, GameObject>();
    
    void Awake()
    {
        //Cache a reference to the Tracked Image Manager component
        trackedImagesManager = GetComponent<ARTrackedImageManager>();
    }
    private void OnEnable()
    {
        //When the script is enabled and when the tracked images change, calls the OnTrackedImagesChanged function
        trackedImagesManager.trackedImagesChanged += OnTrackedImagesChanged;
    }
    private void OnDisable()
    {
        //Do not call the OnTrackedImagesChanged function when the script is disabled
        trackedImagesManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    //Function that is called to track which image has been added, updated, or removed inside the event data
    private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        //For loop that loops through all the new tracked images that is detected and added in the event data
        foreach (var trackedImage in eventArgs.added)
        {
            //Get the name of the reference image
            var imageName = trackedImage.referenceImage.name;
            //For every new image found, loop through the array of prefabs
            foreach (var curPrefab in prefabsToPlace)
            {
                //Compares if there is any prefab in the array that matches the name of the tracked image that is detected in the scene while ignoring the string case,
                //and if the prefab has not been instantiated yet
                if(string.Compare(curPrefab.name, imageName, StringComparison.OrdinalIgnoreCase) == 0 && !instantiatedPrefabs.ContainsKey(imageName))
                {
                    //Instantiate the new prefab, while parenting it to the tracked image to make the prefab's transform change in relation
                    //to its parent
                    var newPrefab = Instantiate(curPrefab, trackedImage.transform);
                    //Adding the newly created prefab into the dictionary
                    instantiatedPrefabs[imageName] = newPrefab;
                }
            }
        }

        //Instead, if any of the tracked images in the scene have been updated, set the corresponding prefab to active or inactive,
        //depending on if the tracked image is being tracked in the scene or not
        foreach (var trackedImage in eventArgs.updated)
        {
            instantiatedPrefabs[trackedImage.referenceImage.name].SetActive(trackedImage.trackingState == TrackingState.Tracking);
        }

        //Instead, if any of the tracked images have been removed from the event data
        foreach (var trackedImage in eventArgs.removed)
        {
            // Destroy the corresponding prefab
            Destroy(instantiatedPrefabs[trackedImage.referenceImage.name]);
            //Remove the prefab from the array
            instantiatedPrefabs.Remove(trackedImage.referenceImage.name);
        }
    }
}