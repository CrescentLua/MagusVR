using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class HandVisuals : MonoBehaviour
{
    public InputDeviceCharacteristics controllerCharacteristics;
    public GameObject handPrefab; 
    private InputDevice targetDevice;

    private GameObject spawnedHandPrefab;
    private Animator handAnimator;

    void InitializeHandVisuals()
    {
        List<InputDevice> inputDevices = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(controllerCharacteristics, inputDevices);

        foreach (InputDevice inputDevice in inputDevices)
        {
            Debug.Log(inputDevice.name + inputDevice.characteristics);
        }

        if (inputDevices.Count > 0)
        {
            targetDevice = inputDevices[0];
            spawnedHandPrefab = Instantiate(handPrefab, transform);
            handAnimator = spawnedHandPrefab.GetComponent<Animator>();
        }
    }

    void UpdateHandAnimations()
    {
        if (targetDevice.TryGetFeatureValue(CommonUsages.trigger, out float triggerVal))
        {
            handAnimator.SetFloat("Trigger", triggerVal);
        }

        else
        {
            handAnimator.SetFloat("Trigger", 0);
        }

        if (targetDevice.TryGetFeatureValue(CommonUsages.grip, out float gripVal))
        {
            handAnimator.SetFloat("Grip", gripVal);
        }

        else
        {
            handAnimator.SetFloat("Grip", 0);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        InitializeHandVisuals(); 
    }

    // Update is called once per frame
    void Update()
    {
        if (!targetDevice.isValid)
        {
            InitializeHandVisuals();
        }

        else
        {
            UpdateHandAnimations();
        }
    }
}
