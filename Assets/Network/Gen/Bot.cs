using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class Bot : MonoBehaviour
{
    public float speed;//Speed Multiplier
    public float rotation;//Rotation multiplier
    public LayerMask raycastMask;//Mask for the sensors

    private float[] input = new float[5];//input to the neural network
    public NeuralNetwork network;

    public int distance;

    public float position = 1;//Checkpoint number on the course
    public bool collided;//To tell if the car has crashed
    public bool winner;
    public string collisionTag;
    public string checkpointTag;

    private List<GameObject> checkedCheckPoints = new();

    
    void FixedUpdate()//FixedUpdate is called at a constant interval
    {
        if (collided) return;
        for (var i = 0; i < 5; i++)//draws five debug rays as inputs
        {
            var newVector = Quaternion.AngleAxis(i * 45 - 90, new Vector3(0, 1, 0)) * transform.right;//calculating angle of raycast
            var ray = new Ray(transform.position, newVector);
            if (Physics.Raycast(ray, out var hit,distance, raycastMask))
            {
                input[i] = (distance - hit.distance) / distance;//return distance, 1 being close
            }
            else
            {
                input[i] = 0;//if nothing is detected, will return 0 to network
            }
        }

        var output = network.FeedForward(input);//Call to network to feedforward
        
        transform.Rotate(0, output[0] * rotation, 0, Space.World);//controls the cars movement
        transform.position += transform.forward * output[1] * speed;//controls the cars turning
    }


    private void OnTriggerEnter(Collider other)
    {
        var collisionWithObject = other.gameObject;
      
        if(collisionWithObject.CompareTag(checkpointTag))//check if the car passes a gate
        {
            var checkPoints = GameObject.FindGameObjectsWithTag(checkpointTag);
            for (int i=0; i < checkPoints.Length; i++)
            {
                if (collisionWithObject != checkPoints[i] || checkedCheckPoints.Contains(collisionWithObject)) continue;
                checkedCheckPoints.Add(collisionWithObject);
                position+=2;
                break;
            }
        }
        else if(collisionWithObject.CompareTag(collisionTag))
        {
            position -=1;
            collided = true;//stop operation if bot has collided
        }else if(collisionWithObject.CompareTag("finish"))
        {
            position += 100;
            winner = true;
        }
    }


    public void UpdateFitness()
    {
        position+=Mathf.Sqrt(Vector3.Distance(GameObject.FindGameObjectWithTag("Start").transform.position, transform.position));
        network.fitness = position;//updates fitness of network for sorting
    }

    public void Save()
    {
        network.Save("Assets/Save.txt");
    }
}
