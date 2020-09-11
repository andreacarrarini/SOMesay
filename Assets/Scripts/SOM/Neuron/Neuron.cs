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

using System;
using SOM.VectorNamespace;
using UnityEngine;
using System.Runtime.CompilerServices;

namespace SOM.NeuronNamespace
{

    public class Neuron : INeuron
    {
        public Vector3 worldPosition { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public IVector Weights { get; }

        public Neuron( int numOfWeights )
        {
            var random = new System.Random();
            Weights = new Vector();                                                                     // Weights of the connections are initialized to random values

            for ( int i = 0; i < numOfWeights; i++ )
            {
                Weights.Add( random.NextDouble() );
            }
        }

        public double Distance( INeuron neuron )
        {
            return Math.Pow( (X - neuron.X), 2 ) + Math.Pow( (Y - neuron.Y), 2 );
        }

        public void SetWeight( int index, double value )
        {
            if ( index >= Weights.Count )
                throw new ArgumentException( "Wrong index!" );

            Weights[ index ] = value;
        }

        public double GetWeight( int index )
        {
            if ( index >= Weights.Count )
                throw new ArgumentException( "Wrong index!" );

            return Weights[ index ];
        }

        public void UpdateWeights( IVector input, double distanceDecay, double learningRate )
        {
            if ( input.Count != Weights.Count )
                throw new ArgumentException( "Wrong input!" );

            for ( int i = 0; i < Weights.Count; i++ )
            {
                Weights[ i ] += distanceDecay * learningRate * (input[ i ] - Weights[ i ]);
            }
        }
    }
}