﻿using System;
using System.Collections;
using System.Collections.Generic;
using Network.Gen;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

public class Manager : MonoBehaviour
{

    public float timeframe;
    public int populationSize;
    public GameObject prefab;
    public GameObject InstantiatePoint;
    public bool learning = false;
    public Net net;
   
    [Range(0.0001f, 1f)] public float MutationChance = 0.01f;

    [Range(0f, 1f)] public float MutationStrength = 0.5f;

    [Range(0.1f, 100f)] public float Gamespeed = 1f;

    public NeuralNetwork[] networks;
    private List<Bot> bots;
    private int epochCount = 0;

    void Start()
    {
        if (!learning) populationSize = 1;
        InitNetworks();
        InvokeRepeating(nameof(CreateBots), 0.1f, timeframe);
    }

    private void InitNetworks()
    {
        networks = new NeuralNetwork[populationSize];
        for (int i = 0; i < populationSize; i++)
        {
            NeuralNetwork network = new NeuralNetwork( net.layers, net.layerActivation);
            if(!learning){
                network.Load("Assets/Save.txt");
            }
            networks[i]=network;
        }
    }

    public void CreateBots()
    {
        Time.timeScale = Gamespeed;
        if (bots != null)
        {
            if (learning)
            {
                SortNetworks();
                epochCount++;
                Debug.Log("Learning Epochs: "+epochCount);
            }
            for (int i = 0; i < bots.Count; i++)
            {
                Destroy(bots[i].gameObject);
            }
        }

        bots = new List<Bot>();
        for (int i = 0; i < populationSize; i++)
        {
            var car = (Instantiate(prefab, InstantiatePoint.transform.position, new Quaternion(0, 0, 0, 0))).GetComponent<Bot>();//create bots
            car.network = networks[i];
            bots.Add(car);
        }
    }
    

    private void SortNetworks()
    {
        for (int i = 0; i < populationSize; i++)
        {
            bots[i].UpdateFitness();
            if (bots[i].winner)
            {
                bots[i].Save();
                Debug.Log("Learning End");
                EditorApplication.ExitPlaymode();
            }
        }
        Array.Sort(networks);
        for (int i = 0; i < populationSize / 2; i++)
        {
            networks[i] = networks[i + populationSize / 2].Copy(new NeuralNetwork( net.layers, net.layerActivation));
            networks[i].Mutate((int)(1/MutationChance), MutationStrength);
        }
    }
}
