using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;

public class NeuralNetwork : IComparable<NeuralNetwork>
{
    //fundamental 
    private int[] layers;//layers
    private float[][] neurons;//neurons
    private float[][] biases;//biasses
    private float[][][] weights;//weights
    private int[] activations;//layers

    //genetic
    public float fitness = 0;//fitness
    

    private float[][] deltaBiases;//biasses
    private float[][][] deltaWeights;//weights
    private int deltaCount;

    public NeuralNetwork(int[] layers, string[] layerActivations)
    {
        this.layers = new int[layers.Length];
        for (int i = 0; i < layers.Length; i++)
        {
            this.layers[i] = layers[i];
        }
        activations = new int[layers.Length - 1];
        for (int i = 0; i < layers.Length - 1; i++)
        {
            string action = layerActivations[i];
            activations[i] = action switch
            {
                "sigmoid" => 0,
                "tanh" => 1,
                "relu" => 2,
                "leakyrelu" => 3,
                _ => 2
            };
        }
        InitNeurons();
        InitBiases();
        InitWeights();
    }


    private void InitNeurons()//create empty storage array for the neurons in the network.
    {
        neurons = layers.Select(layer => new float[layer]).ToArray();
    }

    private void InitBiases()//initializes random array for the biases being held within the network.
    {


        var biasList = new List<float[]>();
        for (var i = 1; i < layers.Length; i++)
        {
            var bias = new float[layers[i]];
            for (var j = 0; j < layers[i]; j++)
            {
                bias[j] = UnityEngine.Random.Range(-0.5f, 0.5f);
            }
            biasList.Add(bias);
        }
        biases = biasList.ToArray();
    }

    private void InitWeights()//initializes random array for the weights being held in the network.
    {
        var weightsList = new List<float[][]>();
        for (int i = 1; i < layers.Length; i++)
        {
            var layerWeightsList = new List<float[]>();
            var neuronsInPreviousLayer = layers[i - 1];
            for (int j = 0; j < layers[i]; j++)
            {
                var neuronWeights = new float[neuronsInPreviousLayer];
                for (int k = 0; k < neuronsInPreviousLayer; k++)
                {
                    neuronWeights[k] = UnityEngine.Random.Range(-0.5f, 0.5f);
                }
                layerWeightsList.Add(neuronWeights);
            }
            weightsList.Add(layerWeightsList.ToArray());
        }
        weights = weightsList.ToArray();
    }

    public float[] FeedForward(float[] inputs)//feed forward, inputs >==> outputs.
    {
        for (int i = 0; i < inputs.Length; i++)
        {
            neurons[0][i] = inputs[i];
        }
        for (int i = 1; i < layers.Length; i++)
        {
            int layer = i - 1;
            for (int j = 0; j < layers[i]; j++)
            {
                float value = 0f;
                for (int k = 0; k < layers[i - 1]; k++)
                {
                    value += weights[i - 1][j][k] * neurons[i - 1][k];
                }
                neurons[i][j] = Activate(value + biases[i-1][j], layer);
            }
        }
        return neurons[layers.Length-1];
    }
    //Backpropagation implemtation down until mutation.
    public float Activate(float value, int layer)//all activation functions
    {
        switch (activations[layer])
        {
            case 0:
                return sigmoid(value);
            case 1:
                return tanh(value);
            case 2:
                return relu(value);
            case 3:  
                return leakyrelu(value);
            default:
                return relu(value);
        }
    }


    public float sigmoid(float x)//activation functions and their corrosponding derivatives
    {
        float k = (float)Math.Exp(x);
        return k / (1.0f + k);
    }
    public float tanh(float x)
    {
        return (float)Math.Tanh(x);
    }
    public float relu(float x)
    {
        return (0 >= x) ? 0 : x;
    }
    public float leakyrelu(float x)
    {
        return (0 >= x) ? 0.01f * x : x;
    }


    //Genetic implementations down onwards until save.

    public void Mutate(int high, float val)//used as a simple mutation function for any genetic implementations.
    {
        for (int i = 0; i < biases.Length; i++)
        {
            for (int j = 0; j < biases[i].Length; j++)
            {
                biases[i][j] = (UnityEngine.Random.Range(0f, high) <= 2) ? biases[i][j] += UnityEngine.Random.Range(-val, val) : biases[i][j];
            }
        }

        for (int i = 0; i < weights.Length; i++)
        {
            for (int j = 0; j < weights[i].Length; j++)
            {
                for (int k = 0; k < weights[i][j].Length; k++)
                {
                    weights[i][j][k] = (UnityEngine.Random.Range(0f, high) <= 2) ? weights[i][j][k] += UnityEngine.Random.Range(-val, val) : weights[i][j][k];
                }
            }
        }
    }

    public int CompareTo(NeuralNetwork other) //Comparing For Genetic implementations. Used for sorting based on the fitness of the network
    {
        if (other == null) return 1;
        if (fitness > other.fitness)
            return 1;
        if (fitness < other.fitness)
            return -1;
        return 0;
    }

    public NeuralNetwork Copy(NeuralNetwork nn) //For creatinga deep copy, to ensure arrays are serialzed.
    {
        for (int i = 0; i < biases.Length; i++)
        {
            for (int j = 0; j < biases[i].Length; j++)
            {
                nn.biases[i][j] = biases[i][j];
            }
        }
        for (int i = 0; i < weights.Length; i++)
        {
            for (int j = 0; j < weights[i].Length; j++)
            {
                for (int k = 0; k < weights[i][j].Length; k++)
                {
                    nn.weights[i][j][k] = weights[i][j][k];
                }
            }
        }
        return nn;
    }

    //save and load functions
    public void Load(string path)//this loads the biases and weights from within a file into the neural network.
    {
        TextReader tr = new StreamReader(path);
        int NumberOfLines = (int)new FileInfo(path).Length;
        string[] ListLines = new string[NumberOfLines];
        int index = 1;
        for (int i = 1; i < NumberOfLines; i++)
        {
            ListLines[i] = tr.ReadLine();
        }
        tr.Close();
        if (new FileInfo(path).Length > 0)
        {
            for (int i = 0; i < biases.Length; i++)
            {
                for (int j = 0; j < biases[i].Length; j++)
                {
                    biases[i][j] = float.Parse(ListLines[index]);
                    index++;
                }
            }

            for (int i = 0; i < weights.Length; i++)
            {
                for (int j = 0; j < weights[i].Length; j++)
                {
                    for (int k = 0; k < weights[i][j].Length; k++)
                    {
                        weights[i][j][k] = float.Parse(ListLines[index]); ;
                        index++;
                    }
                }
            }
        }
    }
    public void Save(string path)//this is used for saving the biases and weights within the network to a file.
    {
        File.Create(path).Close();
        StreamWriter writer = new StreamWriter(path, true);

        for (int i = 0; i < biases.Length; i++)
        {
            for (int j = 0; j < biases[i].Length; j++)
            {
                writer.WriteLine(biases[i][j]);
            }
        }

        for (int i = 0; i < weights.Length; i++)
        {
            for (int j = 0; j < weights[i].Length; j++)
            {
                for (int k = 0; k < weights[i][j].Length; k++)
                {
                    writer.WriteLine(weights[i][j][k]);
                }
            }
        }
        writer.Close();
    }
}
