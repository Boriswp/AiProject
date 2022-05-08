using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager : MonoBehaviour
{

    public float timeframe;
    public int populationSize;
    public GameObject learnPrefab;
    public GameObject gamePrefab;
    private GameObject prefab;
    public GameObject InstantiatePoint;
    public bool learning = false;
    public int[] layers = { 5, 3, 2 };
    public string[] layerActivation = {"tanh","tanh","tanh"};

    [Range(0.0001f, 1f)] public float MutationChance = 0.01f;

    [Range(0f, 1f)] public float MutationStrength = 0.5f;

    [Range(0.1f, 100f)] public float Gamespeed = 1f;

    private List<NeuralNetwork> networks;
    private List<Bot> bots;

    void Start()
    {
        prefab = learning ? learnPrefab : gamePrefab;
        InitNetworks();
        InvokeRepeating(nameof(CreateBots), 0.1f, timeframe);
    }

    private void InitNetworks()
    {
        networks = new List<NeuralNetwork>();
        for (int i = 0; i < populationSize; i++)
        {
            NeuralNetwork net = new NeuralNetwork(layers,layerActivation);
            if(!learning){
                net.Load("Assets/Network/Gen/Pre-trained");
            }
            networks.Add(net);
        }
    }

    public void CreateBots()
    {
        Time.timeScale = Gamespeed;
        if (bots != null)
        {
            if(learning)
                SortNetworks();
            for (int i = 0; i < bots.Count; i++)
            {
                Destroy(bots[i].gameObject);//if there are Prefabs in the scene this will get rid of them
            }
        }

        bots = new List<Bot>();
        for (int i = 0; i < populationSize; i++)
        {
            var car = (Instantiate(prefab, InstantiatePoint.transform.position, new Quaternion(0, 0, 0, 0))).GetComponent<Bot>();//create bots
            car.network = networks[i];//deploys network to each learner
            bots.Add(car);
        }
    }

    private void SortNetworks()
    {
        for (int i = 0; i < populationSize; i++)
        {
            bots[i].UpdateFitness();
        }
        networks.Sort();
        networks[populationSize - 1].Save("Assets/Save.txt");
        for (int i = 0; i < populationSize / 2; i++)
        {
            networks[i] = networks[i + populationSize / 2].Copy(new NeuralNetwork(layers,layerActivation));
            networks[i].Mutate((int)(1/MutationChance), MutationStrength);
        }
    }
}
