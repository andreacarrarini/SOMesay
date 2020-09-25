/*
MIT License
Copyright (c) 2020 Andrea Carrarini
Author: Andrea Carrarini
Contributors: 
Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), 
to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, 
and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions: 
The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. 
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SOM.VectorNamespace;

/*
This class is in charge of modeling one of the neurons from the output layer of the Self-Organizing Map.
The objects of this class will represent one of the elements in the matrix.
*/

namespace SOM.NeuronNamespace
{

    public interface INeuron
    {
        Vector3 GetworldPosition();
        void SetworldPosition( Vector3 value );

        int X { get; set; }                                                                         // These properties determine the position of the neuron in the matrix
        int Y { get; set; }
        IVector Weights { get; }                                                                    // This object represents all the weighted connections that are attached to this neuron

        double Distance( INeuron neuron );                                                          // Calculates the distance from this neuron to another neuron in the matrix
        void SetWeight( int index , double value );
        double GetWeight( int index );
        void UpdateWeights( IVector input , double distanceDecay , double learningRate );           // Updates weights of a neuron based on the input, learning rate and the distance decay
    }
}