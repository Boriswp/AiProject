using System;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using VehicleBehaviour;

public class CarRacerAgent : Agent
{
    // This should discourage idle agent existence
    private const float LowSpeedPerFrameThreshold = 0.03f;
    private const float CheckpointReachedReward = 1.25f;
    private const float FinishReachedReward = 8f;
    private const float SpeedPenaltyMultiplier = 0.06f;
    private const float OutOfRoadPenalty = -2.75f;
    private const float TickPenalty = -0.00001f;

    // Every n successful route completions agent will receive new route
    private const int _stepsToRouteRegeneration = 5;

    public Generator Generator;

    private float _startTime;
    private float _maxEpisodeTime = 300f;
    private float _steering = 0.0f;
    private int _tillRouteGenerationCounter = 5;
    private Vector3 positionLastUpdate;
    private Rigidbody _body;
    private WheelVehicle _vehicle;


    [SerializeField]
    private int maxCollisionCount;
    public int MaxCollisionCount
    {
        get => maxCollisionCount;
        set => maxCollisionCount = value;
    }

    private int collisionCount;
    
    public override void Initialize()
    {
        _body = GetComponent<Rigidbody>();
        _vehicle = GetComponent<WheelVehicle>();
    }


    public override void OnEpisodeBegin()
    {
        collisionCount = 0;
        _vehicle?.ResetPos();
        _startTime = Time.time;
        positionLastUpdate = transform.position;

        if (_tillRouteGenerationCounter == 0)
        {
            _tillRouteGenerationCounter = _stepsToRouteRegeneration;
            Generator.TrackLength = (int) Academy.Instance.EnvironmentParameters.GetWithDefault("track_length", 10);
            Generator.TurnRate = Academy.Instance.EnvironmentParameters.GetWithDefault("turn_rate", 0.07f);
            Generator.GenerateTrack();
        }

        foreach (var item in Generator.SavedCheckpoints)
        {
            item.GetComponent<BoxCollider>().enabled = true;
        } 
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        base.CollectObservations(sensor);
        sensor.AddObservation(_body.velocity);
        sensor.AddObservation(_steering);
    }

    private void SetOutOfRoadPenalty()
    {
        if (!IsOnRoad() || collisionCount > MaxCollisionCount)
        {
            AddReward(OutOfRoadPenalty);
            EndEpisode();
            collisionCount = 0;
        }
    }

    private void SetBrakingPenalty(ActionBuffers actions)
    {
        if (actions.ContinuousActions[0] < 0 && _body.velocity.magnitude < 10)
        {
            AddReward(actions.ContinuousActions[0] * 0.008f);
        }
    }

    /// <summary>
    /// This gets reduced, or even becomes reward if sufficient speed is maintained
    /// </summary>
    private float SetLowSpeedPenalty()
    {
        float distanceThisFrame = Vector3.Distance(positionLastUpdate, transform.position);

        float movementScore = 0.0f;
        movementScore += Mathf.Pow(Mathf.Clamp(distanceThisFrame - LowSpeedPerFrameThreshold, -0.05f, 0.015f), 3f) * SpeedPenaltyMultiplier;
        positionLastUpdate = transform.position;

        return movementScore;
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        if (_vehicle != null)
        {
            _vehicle.Throttle = actions.ContinuousActions[0];
            UpdateSteeringValue(actions.ContinuousActions[1]);
        }

        if (Time.time - _startTime > _maxEpisodeTime)
        {
            Debug.Log("Max time exceeded");
            EndEpisode();
        }

        SetOutOfRoadPenalty();
        SetBrakingPenalty(actions);
        float movementScore = SetLowSpeedPenalty();

        movementScore += TickPenalty;

        AddReward(Mathf.Clamp(movementScore, -0.01f, 0.005f));
    }

    private void UpdateSteeringValue(float steeringAction)
    {
        if (steeringAction > _steering + 0.1f)
        {
            _steering += 0.1f;
        }
        else if (steeringAction < _steering - 0.1f)
        {
            _steering -= 0.1f;
        }
        else
        {
            _steering = steeringAction;
        }
        Mathf.Clamp(_steering, -1f, 1f);
        _vehicle.Steering = _steering;
    }

    private bool IsOnRoad()
    {
        //RaycastHit hit;
        //Debug.DrawRay(new Vector3(transform.position.x, transform.position.y - 0.15f, transform.position.z), -transform.up);
        return Physics.Raycast(new Vector3(transform.position.x, transform.position.y - 0.15f, transform.position.z + 0.5f), -transform.up, 0.5f)
            && Physics.Raycast(new Vector3(transform.position.x, transform.position.y - 0.15f, transform.position.z - 1.0f), -transform.up, 0.5f)
            && Physics.Raycast(new Vector3(transform.position.x + 0.5f, transform.position.y - 0.15f, transform.position.z), -transform.up, 0.5f)
            && Physics.Raycast(new Vector3(transform.position.x - 0.5f, transform.position.y - 0.15f, transform.position.z), -transform.up, 0.5f);
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.gameObject.layer == LayerMask.NameToLayer("BOT")) return;
        AddReward(-0.5f);
        collisionCount++;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("checkpoint"))
        {
            AddReward(CheckpointReachedReward);
        }
        else if (other.gameObject.CompareTag("finish"))
        {
            _tillRouteGenerationCounter--;
            AddReward(FinishReachedReward);
            EndEpisode();
        }
    }
}
