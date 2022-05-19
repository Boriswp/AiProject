using System;
using System.Collections.Generic;
using Network.Gen;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

public class GeneticController : MonoBehaviour
{

    public float timeframe;
    public int populationSize;
    public GameObject prefab;
    public GameObject InstantiatePoint;
    public bool isLearning = false;
    public Net net;
   
    [Range(0.0001f, 1f)] public float MutationChance = 0.01f;

    [Range(0f, 1f)] public float MutationStrength = 0.5f;

    [Range(0.1f, 100f)] public float Gamespeed = 1f;

    public NeuralNetwork[] networks;
    private List<Bot> bots;
    private int epochCount = 0;

    void Start()
    {
        if (!isLearning) populationSize = 1;
        InitNetworks();
        InvokeRepeating(nameof(CreateBots), 0.1f, timeframe);
    }

    private void InitNetworks()
    {
        networks = new NeuralNetwork[populationSize];
        for (int i = 0; i < populationSize; i++)
        {
            NeuralNetwork network = new NeuralNetwork( net.layers, net.layerActivation);
            if(!isLearning){
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
            if (isLearning)
            {
                SortNetworks();
                epochCount++;
            }
            for (int i = 0; i < bots.Count; i++)
            {
                Destroy(bots[i].gameObject);
            }
        }
        Debug.Log("Learning Epochs: "+epochCount);
        bots = new List<Bot>();
        for (int i = 0; i < populationSize; i++)
        {
            var car = (Instantiate(prefab, InstantiatePoint.transform.position, new Quaternion(0, 0, 0, 0))).GetComponent<Bot>();
            car.network = networks[i];
            bots.Add(car);
        }
    }
    

    private void SortNetworks()
    {
        for (int i = 0; i < populationSize; i++)
        {
            bots[i].UpdateFitness();
            if (!bots[i].winner) continue;
            bots[i].Save();
            Debug.Log("Learning End");
#if UNITY_EDITOR
            EditorApplication.ExitPlaymode(); 
#endif
        }
        Array.Sort(networks);
        for (int i = 0; i < populationSize / 2; i++)
        {
            networks[i] = networks[i + populationSize / 2].Copy(new NeuralNetwork( net.layers, net.layerActivation));
            networks[i].Mutate((int)(1/MutationChance), MutationStrength);
        }
    }
}
