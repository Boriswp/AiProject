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

    public int position;//Checkpoint number on the course
    public bool collided;//To tell if the car has crashed

    public string collisionTag;
    public string checkpointTag;

    void FixedUpdate()//FixedUpdate is called at a constant interval
    {
        if (collided) return;
        for (var i = 0; i < 5; i++)//draws five debug rays as inputs
        {
            var newVector = Quaternion.AngleAxis(i * 45 - 90, new Vector3(0, 1, 0)) * transform.right;//calculating angle of raycast
            var ray = new Ray(transform.position, newVector);

            if (Physics.Raycast(ray, out var hit, 10, raycastMask))
            {
                input[i] = (10 - hit.distance) / 10;//return distance, 1 being close
            }
            else
            {
                input[i] = 0;//if nothing is detected, will return 0 to network
            }
        }

        var output = network.FeedForward(input);//Call to network to feedforward
        
        transform.Rotate(0, output[0] * rotation, 0, Space.World);//controls the cars movement
        transform.position += transform.right * output[1] * speed;//controls the cars turning
    }


    void OnCollisionEnter(Collision collision)
    {
        var collisionWithObject = collision.collider.gameObject;
        Debug.Log(collisionWithObject);
        if(collisionWithObject.CompareTag(checkpointTag))//check if the car passes a gate
        {
            var checkPoints = GameObject.FindGameObjectsWithTag(checkpointTag);
            if (checkPoints.Where((t, i) => collisionWithObject == t && i == (position + 1 + checkPoints.Length) % checkPoints.Length).Any())
            {
                position++;//if the gate is one ahead of it, it increments the position, which is used for the fitness/performance of the network
            }
        }
        else if(collisionWithObject.CompareTag(collisionTag))
        {
            collided = true;//stop operation if bot has collided
        }
    }


    public void UpdateFitness()
    {
        network.fitness = position;//updates fitness of network for sorting
    }
}
