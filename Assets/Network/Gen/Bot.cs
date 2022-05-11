using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;


public class Bot : MonoBehaviour
{
    public float speed;
    public float rotation;
    public LayerMask raycastMask;

    private float[] input = new float[5];
    public NeuralNetwork network;

    public int distance;
    
    public float rewardPoints = 1;
    public bool failed;
    public bool winner;
    public string collisionTag;
    public string checkpointTag;

    private List<GameObject> checkedCheckPoints = new();

    
    void FixedUpdate()
    {
        if (failed) return;
        for (var i = 0; i < 5; i++)
        {
            var newVector = Quaternion.AngleAxis(i * 45 - 90, new Vector3(0, 1, 0)) * transform.right;
            var ray = new Ray(transform.position, newVector);
            if (Physics.Raycast(ray, out var hit,distance, raycastMask))
            {
                input[i] = (distance - hit.distance) / distance;
            }
            else
            {
                input[i] = 0;
            }
        }

        var output = network.FeedForward(input);
        
        transform.Rotate(0, output[0] * rotation, 0, Space.World);
        transform.position += transform.forward * output[1] * speed;
    }


    private void OnTriggerEnter(Collider other)
    {
        var collisionWithObject = other.gameObject;
      
        if(collisionWithObject.CompareTag(checkpointTag))
        {
            var checkPoints = GameObject.FindGameObjectsWithTag(checkpointTag);
            for (var i=0; i < checkPoints.Length; i++)
            {
                if (collisionWithObject != checkPoints[i] || checkedCheckPoints.Contains(collisionWithObject)) continue;
                checkedCheckPoints.Add(collisionWithObject);
                rewardPoints+=2;
                break;
            }
        }
        else if(collisionWithObject.CompareTag(collisionTag))
        {
            rewardPoints -=1;
            failed = true;
        }else if(collisionWithObject.CompareTag("finish"))
        {
            rewardPoints += 100;
            winner = true;
        }
    }


    public void UpdateFitness()
    {
        rewardPoints+=Mathf.Sqrt(Vector3.Distance(GameObject.FindGameObjectWithTag("Start").transform.position, transform.position));
        network.fitness = rewardPoints;
    }

    public void Save()
    {
        network.Save("Assets/Save.txt");
    }
}
