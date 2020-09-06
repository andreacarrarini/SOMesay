using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
This class is in charge of modeling one of the neurons from the output layer of the Self-Organizing Map.
The objects of this class will represent one of the elements in the matrix.
*/

using SOM.VectorNamespace;

namespace SOM.NeuronNamespace
{

    public interface INeuron
    {
        int X { get; set; }                                                                         // These properties determine the position of the neuron in the matrix
        int Y { get; set; }
        IVector Weights { get; }                                                                    // This object represents all the weighted connections that are attached to this neuron

        double Distance( INeuron neuron );                                                          // Calculates the distance from this neuron to another neuron in the matrix
        void SetWeight( int index, double value );
        double GetWeight( int index );
        void UpdateWeights( IVector input, double distanceDecay, double learningRate );             // Updates weights of a neuron based on the input, learning rate and the distance decay
    }
}