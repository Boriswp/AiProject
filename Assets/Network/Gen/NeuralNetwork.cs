using System.Collections.Generic;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.PlayerLoop;

[Serializable]
public class NeuralNetwork : IComparable<NeuralNetwork>
{
    //fundamental 
    private int[] layers;
    private float[][] neurons;
    private float[][] biases;
    private float[][][] weights;
    private int[] activations;

    //genetic
    public float fitness = 0;//fitness
    

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
            var action = layerActivations[i];
            activations[i] = action switch
            {
                "sigmoid" => 0,
                "tanh" => 1,
                "relu" => 2,
                "leakyrelu" => 3,
                _ => 2
            };
        }

        Initialization();
    }

    private void Initialization()
    {
        neurons = layers.Select(layer => new float[layer]).ToArray();
        
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
            
            var weightsList = new List<float[][]>();
            for (var i = 1; i < layers.Length; i++)
            {
                var layerWeightsList = new List<float[]>();
                var neuronsInPreviousLayer = layers[i - 1];
                for (var j = 0; j < layers[i]; j++)
                {
                    var neuronWeights = new float[neuronsInPreviousLayer];
                    for (var k = 0; k < neuronsInPreviousLayer; k++)
                    {
                        neuronWeights[k] = UnityEngine.Random.Range(-0.5f, 0.5f);
                    }

                    layerWeightsList.Add(neuronWeights);
                }

                weightsList.Add(layerWeightsList.ToArray());
            }
            weights = weightsList.ToArray();
    }

    public float[] FeedForward(float[] inputs)
    {
        for (var i = 0; i < inputs.Length; i++)
        {
            neurons[0][i] = inputs[i];
        }
        for (var i = 1; i < layers.Length; i++)
        {
            var layer = i - 1;
            for (var j = 0; j < layers[i]; j++)
            {
                var value = 0f;
                for (var k = 0; k < layers[i - 1]; k++)
                {
                    value += weights[i - 1][j][k] * neurons[i - 1][k];
                }
                neurons[i][j] = Activate(value + biases[i-1][j], layer);
            }
        }
        return neurons[layers.Length-1];
    }
 
    public float Activate(float value, int layer)
    {
        return activations[layer] switch
        {
            0 => Sigmoid(value),
            1 => Tanh(value),
            2 => Relu(value),
            3 => LeakyRelu(value),
            _ => Relu(value)
        };
    }


    public float Sigmoid(float x)
    {
        float k = (float)Math.Exp(x);
        return k / (1.0f + k);
    }
    public float Tanh(float x)
    {
        return (float)Math.Tanh(x);
    }
    public float Relu(float x)
    {
        return (0 >= x) ? 0 : x;
    }
    public float LeakyRelu(float x)
    {
        return (0 >= x) ? 0.01f * x : x;
    }

    

    public void Mutate(int probabilityHigh, float strengthVal)
    {
        for (int i = 0; i < biases.Length; i++)
        {
            for (int j = 0; j < biases[i].Length; j++)
            {
                biases[i][j] = (UnityEngine.Random.Range(0f, probabilityHigh) <= 2) ? biases[i][j] += UnityEngine.Random.Range(-strengthVal, strengthVal) : biases[i][j];
            }
        }

        for (int i = 0; i < weights.Length; i++)
        {
            for (int j = 0; j < weights[i].Length; j++)
            {
                for (int k = 0; k < weights[i][j].Length; k++)
                {
                    weights[i][j][k] = (UnityEngine.Random.Range(0f, probabilityHigh) <= 2) ? weights[i][j][k] += UnityEngine.Random.Range(-strengthVal, strengthVal) : weights[i][j][k];
                }
            }
        }
    }

    public int CompareTo(NeuralNetwork other)
    {
        if (other == null) return 1;
        if (fitness > other.fitness)
            return 1;
        if (fitness < other.fitness)
            return -1;
        return 0;
    }

    public NeuralNetwork Copy(NeuralNetwork nn) 
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


    public void Load(string path)
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
        if (new FileInfo(path).Length <= 0) return;
        {
            for (int i = 0; i < biases.Length; i++)
            {
                for (int j = 0; j < biases[i].Length; j++)
                {
                    Debug.Log(float.Parse(ListLines[index]));
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
                        Debug.Log(float.Parse(ListLines[index]));
                        weights[i][j][k] = float.Parse(ListLines[index]); ;
                        index++;
                    }
                }
            }
        }
    }
    public void Save(string path)
    {
        File.Create(path).Close();
        StreamWriter writer = new StreamWriter(path, true);

        for (var i = 0; i < biases.Length; i++)
        {
            for (var j = 0; j < biases[i].Length; j++)
            {
                writer.WriteLine(biases[i][j]);
            }
        }

        for (var i = 0; i < weights.Length; i++)
        {
            for (var j = 0; j < weights[i].Length; j++)
            {
                for (var k = 0; k < weights[i][j].Length; k++)
                {
                    writer.WriteLine(weights[i][j][k]);
                }
            }
        }
        writer.Close();
    }
}
