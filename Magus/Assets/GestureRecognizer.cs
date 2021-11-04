using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using PDollarGestureRecognizer;
using System.IO; 

public class GestureRecognizer : MonoBehaviour
{
    public XRNode inputSource;
    public InputHelpers.Button inputButton;
    public float inputThreshold = 0.1f;
    public Transform HandSource;

    public float maxGestureNodeDistanceThreshold = 0.03f;
    public GameObject linePrefab;
    public bool creationMode = true;
    public string newGestureName;

    private List<Gesture> trainingSet = new List<Gesture>();
    private bool isCreatingNewGesture = false;
    private List<Vector3> gestureNodePositionList = new List<Vector3>();
    private GameObject instantiatedLinePrefab = null; 
    // Start is called before the first frame update
    void Start()
    {
        string[] gestureFiles = Directory.GetFiles(Application.persistentDataPath, "*.xml");
        foreach(string gestureFile_ in gestureFiles)
        {
            trainingSet.Add(GestureIO.ReadGestureFromFile(gestureFile_));
        }
    }

    // Update is called once per frame
    void Update()
    {
        InputHelpers.IsPressed(InputDevices.GetDeviceAtXRNode(inputSource), inputButton, out bool isPressed, inputThreshold);

        //Player has started drawing new Gesture/Pattern 
        if(!isCreatingNewGesture && isPressed)
        {
            BeginGesture(); 
        }

        //Player has stopped drawing new Gesture/Pattern 
        else if(isCreatingNewGesture && !isPressed)
        {
            EndGesture(); 
        }

        //Player is still drawing new Gesture/Pattern (UPDATING GESTURE) 
        else if(isCreatingNewGesture && isPressed)
        {
            UpdateGesture(); 
        }
    }

    void BeginGesture()
    {
        Debug.Log("Player has begun drawing a new gesture!");
        isCreatingNewGesture = true;
        gestureNodePositionList.Clear();
        gestureNodePositionList.Add(HandSource.position);

        instantiatedLinePrefab = Instantiate(linePrefab, transform.position, Quaternion.identity);
        instantiatedLinePrefab.GetComponent<LineRenderer>().positionCount += 1;
 
        Vector3 lastNodePosition = gestureNodePositionList[gestureNodePositionList.Count - 1];
        instantiatedLinePrefab.GetComponent<LineRenderer>().SetPosition(gestureNodePositionList.Count - 1, lastNodePosition);
        //Destroy(Instantiate(cubePrefab, HandSource.position, Quaternion.identity), 2);
    }

    void EndGesture()
    {
        Debug.Log("Player has stopped drawing a new gesture!");
        isCreatingNewGesture = false;

        if (instantiatedLinePrefab)
        {
            Destroy(instantiatedLinePrefab, 3.0f);
        }

        Point[] pointArray = new Point[gestureNodePositionList.Count];
        for (int i = 0; i < gestureNodePositionList.Count; i++)
        {
            Vector2 screenPoint = Camera.main.WorldToScreenPoint(gestureNodePositionList[i]);
            pointArray[i] = new Point(screenPoint.x, screenPoint.y, 0);
        }

        Gesture newGesture = new Gesture(pointArray);
        if(creationMode)
        {
            newGesture.Name = newGestureName;
            trainingSet.Add(newGesture);

            string fileName = Application.persistentDataPath + "/" + newGestureName + ".xml";
            GestureIO.WriteGesture(pointArray, newGestureName, fileName);
        }

        else
        {
            Result gestureResult = PointCloudRecognizer.Classify(newGesture, trainingSet.ToArray());
            Debug.Log(gestureResult.GestureClass + ", " + gestureResult.Score);
        }
    }

    void UpdateGesture()
    {
        Debug.Log("Updating Player's new gesture!");

        Vector3 lastNodePosition = gestureNodePositionList[gestureNodePositionList.Count - 1];
        if(Vector3.Distance(HandSource.position, lastNodePosition) > maxGestureNodeDistanceThreshold)
        {
            gestureNodePositionList.Add(HandSource.position);
            if(instantiatedLinePrefab)
            {
                instantiatedLinePrefab.GetComponent<LineRenderer>().positionCount += 1;
                instantiatedLinePrefab.GetComponent<LineRenderer>().SetPosition(gestureNodePositionList.Count - 1, lastNodePosition);
                //Destroy(Instantiate(cubePrefab, HandSource.position, Quaternion.identity), 2);
            }
        }
    }
}
