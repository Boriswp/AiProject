using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class Generator : MonoBehaviour
{
    
    [Range(1, 250)]
    public int TrackLength;

    [Range(0,1.0f)]
    public float TurnRate = 0.1f;
    

    [Range(0, 50)]
    public int CheckpointEveryNSegments = 1;
    
    public GameObject[] StraightTemplateSegments;
    public GameObject[] RightCurveTemplateSegments;
    public GameObject[] LeftCurveTemplateSegments;
    public GameObject CheckpointTemplate;
    public GameObject FinaleTemplate;

    public List<GameObject> SavedCheckpoints = new();
    public List<GameObject> SavedObjects = new();
    private int turnDeviation = 0;

    public bool autoStart = true;


    private void Awake()
    {
        if (autoStart)
        {
            GenerateTrack();
        }
    }

    private GameObject[] UseRightTurn()
    {
        turnDeviation++; 
        return RightCurveTemplateSegments;
    }


    private GameObject[] UseLeftTurn()
    {
        turnDeviation--; 
        return LeftCurveTemplateSegments;
    }

    [ContextMenu("Generate")]
    public void GenerateTrack()
    {
        Debug.Log("Generating");

        foreach (var checkpoint in SavedCheckpoints)
        {
            DestroyImmediate(checkpoint);
        }
        SavedCheckpoints.Clear();
        
        foreach (var content in SavedObjects)
        {
            DestroyImmediate(content);
        }
        SavedObjects.Clear();

        foreach (Transform child in transform)
        {
            DestroyImmediate(child.gameObject);
        }
            
        var currentNextPoint = transform;
        var middleTransform = transform;
        currentNextPoint.position = new Vector3(0f,0f,2f);
        for (int i = 0, tillCheckpointCounter = 0; i < TrackLength; i++, tillCheckpointCounter++)
        {
            if (IsCheckpointPlace(i, tillCheckpointCounter))
            {
                tillCheckpointCounter = CreateCheckpoint(currentNextPoint);
                //CreateCheckpoint(middleTransform);
            }

            var randomRoll = Random.Range(0.0f, 1.0f + TurnRate);

            var templateSetToUse = CreateRandomSegmentTemplate(randomRoll);

            var selectedSegment = templateSetToUse[Random.Range(0, templateSetToUse.Length)];

            var createdSegment = Instantiate(selectedSegment, transform);
          
            var inputTransform = createdSegment.transform.Find("InputPoint");
            //middleTransform = createdSegment.transform.Find("MiddlePoint");
            var outputTransform = createdSegment.transform.Find("OutputPoint");

            var rotationAngle = SetCorrectOrientation(currentNextPoint, createdSegment, inputTransform);

            SetNextAttachmentPoint(currentNextPoint, createdSegment, inputTransform, rotationAngle);
            currentNextPoint = outputTransform;
            SavedObjects.Add(createdSegment);
        }
        CreateFinish(currentNextPoint);
        var cameraMan = FindObjectOfType<CameraTracking>();
        if (cameraMan != null)
        {
            cameraMan.Init();
        }
    }

    private GameObject[] CreateRandomSegmentTemplate(float randomRoll)
    {
        GameObject[] templateSetToUse;
        if (randomRoll <= TurnRate)
        {
            templateSetToUse = turnDeviation switch
            {
                0 => Random.Range(0.0f, 1.0f) < 0.5f ? UseRightTurn() : UseLeftTurn(),
                < 0 => UseRightTurn(),
                _ => UseLeftTurn()
            };
        }
        else
        {
            templateSetToUse = StraightTemplateSegments;
        }

        return templateSetToUse;
    }

    private bool IsCheckpointPlace(int i, int tillCheckpointCounter)
    {
        return tillCheckpointCounter == CheckpointEveryNSegments && i != TrackLength;
    }

    private int CreateCheckpoint(Transform currentPoint)
    {
        var checkpoint = Instantiate(CheckpointTemplate, transform);
        checkpoint.transform.position = currentPoint.position;
        checkpoint.transform.rotation = currentPoint.rotation;
        SavedCheckpoints.Add(checkpoint);
        return 0;
    }

    private void CreateFinish(Transform currentPoint)
    {
        var checkpoint = Instantiate(FinaleTemplate, transform);
        SavedCheckpoints.Add(checkpoint);
        checkpoint.transform.position = currentPoint.position;
        checkpoint.transform.rotation = currentPoint.rotation;
    }

    private static float SetCorrectOrientation(Transform currentPoint, GameObject createdSegment, Transform inputTransform)
    {
        var forward = currentPoint.transform.forward;
        var rotationAngle = Vector3.SignedAngle(createdSegment.transform.forward, forward, Vector3.up);
        var rotationOffset = inputTransform.InverseTransformDirection(forward);
        createdSegment.transform.rotation = Quaternion.LookRotation(rotationOffset, Vector3.up);
        return rotationAngle;
    }

    private static void SetNextAttachmentPoint(Transform currentNextPoint, GameObject createdSegment, Transform inputTransform, float rotationAngle)
    {
        var inputOffset = inputTransform.InverseTransformPoint(createdSegment.transform.position);
        inputOffset = Quaternion.AngleAxis(rotationAngle, Vector3.up) * inputOffset;
        createdSegment.transform.position = currentNextPoint.position + inputOffset;
    }
}
