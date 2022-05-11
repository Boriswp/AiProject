using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Generator : MonoBehaviour
{
    
    [Range(1, 250)]
    public int TrackLength;

    [Range(0,1.0f)]
    public float TurnRate = 0.1f;
    

    [Range(0, 50)]
    public int CheckpointEveryNSegments = 4;
    
    public GameObject[] StraightTemplateSegments;
    public GameObject[] RightCurveTemplateSegments;
    public GameObject[] LeftCurveTemplateSegments;
    public GameObject CheckpointTemplate;
    public GameObject FinaleTemplate;

    public List<GameObject> SavedCheckpoints = new();
    public List<GameObject> SavedObjects = new();
    private int turnDeviation = 0;



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
            if (ShouldNowBeCheckpoint(i, tillCheckpointCounter))
            {
                tillCheckpointCounter = CreateCheckpointGate(currentNextPoint);
                CreateCheckpointGate(middleTransform);
            }

            var randomRoll = Random.Range(0.0f, 1.0f + TurnRate);

            var templateSetToUse = DrawRandomPathSegmentTemplate(randomRoll);

            var selectedSegment = templateSetToUse[Random.Range(0, templateSetToUse.Length)];

            var createdSegment = Instantiate(selectedSegment, transform);
          
            var inputTransform = createdSegment.transform.Find("InputPoint");
            middleTransform = createdSegment.transform.Find("MiddlePoint");
            var outputTransform = createdSegment.transform.Find("OutputPoint");

            var rotationAngle = GetCorrectOrientation(currentNextPoint, createdSegment, inputTransform);

            // Get next attachment point
            GetNextAttachmentPoint(currentNextPoint, createdSegment, inputTransform, rotationAngle);
            currentNextPoint = outputTransform;
            SavedObjects.Add(createdSegment);
        }
        CreateFinishGate(currentNextPoint);
    }

    private GameObject[] DrawRandomPathSegmentTemplate(float randomRoll)
    {
        GameObject[] templateSetToUse;
        if (randomRoll <= TurnRate)
        {
            if (turnDeviation == 0)
            {
                if (Random.Range(0.0f, 1.0f) < 0.5f)
                {
                    templateSetToUse = UseRightTurn();
                }
                else
                {
                    templateSetToUse = UseLeftTurn();
                }
            }
            else if (turnDeviation < 0)
            {
                templateSetToUse = UseRightTurn();
            }
            else
            {
                templateSetToUse = UseLeftTurn();
            }
        }
        else
        {
            templateSetToUse = StraightTemplateSegments;
        }

        return templateSetToUse;
    }

    private bool ShouldNowBeCheckpoint(int i, int tillCheckpointCounter)
    {
        return tillCheckpointCounter == CheckpointEveryNSegments && i != TrackLength;
    }

    private int CreateCheckpointGate(Transform currentNextPoint)
    {
        var checkpoint = Instantiate(CheckpointTemplate, transform);
        checkpoint.transform.position = currentNextPoint.position;
        checkpoint.transform.rotation = currentNextPoint.rotation;
        SavedCheckpoints.Add(checkpoint);
        return 0;
    }

    private void CreateFinishGate(Transform currentNextPoint)
    {
        var checkpoint = Instantiate(FinaleTemplate, transform);
        SavedCheckpoints.Add(checkpoint);
        checkpoint.transform.position = currentNextPoint.position;
        checkpoint.transform.rotation = currentNextPoint.rotation;
    }

    private static float GetCorrectOrientation(Transform currentNextPoint, GameObject createdSegment, Transform inputTransform)
    {
        var forward = currentNextPoint.transform.forward;
        var rotationAngle = Vector3.SignedAngle(createdSegment.transform.forward, forward, Vector3.up);
        var rotationOffset = inputTransform.InverseTransformDirection(forward);
        createdSegment.transform.rotation = Quaternion.LookRotation(rotationOffset, Vector3.up);
        return rotationAngle;
    }

    private static void GetNextAttachmentPoint(Transform currentNextPoint, GameObject createdSegment, Transform inputTransform, float rotationAngle)
    {
        var inputOffset = inputTransform.InverseTransformPoint(createdSegment.transform.position);
        inputOffset = Quaternion.AngleAxis(rotationAngle, Vector3.up) * inputOffset;
        createdSegment.transform.position = currentNextPoint.position + inputOffset;
    }
}
