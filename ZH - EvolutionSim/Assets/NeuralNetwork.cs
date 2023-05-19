using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using Random = UnityEngine.Random;

public class NeuralNetwork : MonoBehaviour
{
    //Does not include input layer
    [NonSerialized]
    public Layer[] layers;
    
    //Must start with the input layer
    public int[] networkShape;

    public class Layer
    {
        //2d array of weights
        public float[,] weightsArray;
        
        //1d array of biases
        public float[] biasArray;
        
        //1d array of nodes outputs
        public float[] nodesArray;

        private int numNodes;
        private int numInputs;
        
        
        public Layer(int numInputs, int numNodes)
        {
            this.numNodes = numNodes;
            this.numInputs = numInputs;
            
            weightsArray = new float[numNodes, numInputs];
            biasArray = new float[numNodes];
            nodesArray = new float[numNodes];
            
        }

        public void ForwardThruNetwork(float[] previousInputs)
        {
            nodesArray = new float[numNodes];
            
            //For each node in the layer
            //i = node
            for (int i = 0; i < numNodes; i++)
            {
                //Get sum of weights on node times the inputs for thr node
                //j = input
                for (int j = 0; j < numInputs; j++)
                {
                    nodesArray[i] += weightsArray[i, j] * previousInputs[j];
                }
                
                //Add bias
                nodesArray[i] += biasArray[i];
            }
        }

        public void Activation()
        {
            //Uses ReLU activation function
            for (int i = 0; i < numNodes; i++)
            {
                if(nodesArray[i] < 0)
                    nodesArray[i] = 0;
            }
        }
        
        public void MutateLayer(float mutationChance, float mutationAmount)
        {
            for (int i = 0; i < numNodes; i++)
            {
                for (int j = 0; j < numInputs; j++)
                {
                    if(Random.value < mutationChance)
                        weightsArray[i, j] += Random.Range(-1f, 1f) * mutationAmount;
                }
                
                if(Random.value < mutationChance)
                    biasArray[i] += Random.Range(-1f, 1f) * mutationAmount;
            }
        }
        
    }
    
    public void Awake()
    {
        //For each layer minus input layer
        layers = new Layer[networkShape.Length - 1];
        for (int i = 0; i < layers.Length; i++)
        {
            layers[i] = new Layer(networkShape[i], networkShape[i + 1]);
        }
    }
    
    public float[] Brain(float[] inputs)
    {
        for (int i = 0; i < layers.Length; i++)
        {
            //First layer directly takes in inputs
            if (i == 0)
            {
                layers[i].ForwardThruNetwork(inputs);
                layers[i].Activation();
            }
            //Output layer has no activation finction
            else if (i == layers.Length - 1)
            {
                layers[i].ForwardThruNetwork(layers[i - 1].nodesArray);
            }
            //All other cases
            else
            {
                layers[i].ForwardThruNetwork(layers[i - 1].nodesArray);
                layers[i].Activation();
            }
        }
        
        return layers[^1].nodesArray;
    }

    public Layer[] CopyLayers()
    {
        Layer[] tempLayers = new Layer[networkShape.Length - 1];
        for (int i = 0; i < layers.Length; i++)
        {
            tempLayers[i] = new Layer(networkShape[i], networkShape[i + 1]);
            Array.Copy(layers[i].weightsArray, tempLayers[i].weightsArray, layers[i].weightsArray.GetLength(0) * layers[i].weightsArray.GetLength(1));
            Array.Copy(layers[i].biasArray, tempLayers[i].biasArray, layers[i].biasArray.GetLength(0));
        }

        return tempLayers;
    }

    public void MutateNetwork(float mutationChance, float mutationAmount)
    {
        foreach (var layer in layers)
        {
            layer.MutateLayer(mutationChance, mutationAmount);
        }
    }
}
