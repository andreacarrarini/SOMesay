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

        private double mapSize = 5000;                                                                  // Remember to change if # of tiles or dimensions change

        public Neuron( int numOfWeights )
        {
            var random = new System.Random();
            Weights = new Vector();                                                                     // Weights of the connections are initialized to random values

            for ( int i = 0; i < numOfWeights; i++ )
            {
                //Weights.Add( random.NextDouble() );

                Weights.Add( 0.5 );

                double pippo = Weights[ i ];

                //Weights.Add( 1 );
            }
        }

        public double Distance( INeuron neuron )
        {
            return Math.Pow( (X - neuron.X) , 2 ) + Math.Pow( (Y - neuron.Y) , 2 );
        }

        public void SetWeight( int index , double value )
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

        public void UpdateWeights( IVector input , double distanceDecay , double learningRate )
        {
            if ( input.Count != Weights.Count )
                throw new ArgumentException( "Wrong input!" );

            for ( int i = 0; i < Weights.Count; i++ )
            {
                //Weights[ i ] += distanceDecay * learningRate * (input[ i ] - Weights[ i ]);

                Weights[ i ] += distanceDecay * learningRate * (input[ i ] / mapSize - Weights[ i ] * input[ i ] / mapSize);
                Weights[ i ] = ( double ) Mathf.Clamp01( ( float ) Weights[ i ] );
            }
        }

        public void UpdateNeuronsWorldPositions( IVector input , double distanceDecay , double learningRate )
        {
            if ( input.Count != Weights.Count )
                throw new ArgumentException( "Wrong input!" );

            // First clamp in the previous range and then clamp in the area of the map
            //double newX = ( double ) Mathf.Clamp( ( float ) (worldPosition.x + distanceDecay * learningRate * (input[ 0 ] - Weights[ 0 ] * input[ 0 ])) ,
            //    worldPosition.x - 1000 , worldPosition.x + 1000 );
            //newX = ( double ) Mathf.Clamp( ( float ) newX , -3000 , -3000 + 5000 );

            //double newY = worldPosition.y + distanceDecay * learningRate * (input[ 1 ] - Weights[ 1 ] * input[ 1 ]);

            //double newZ = ( double ) Mathf.Clamp( ( float ) (worldPosition.z + distanceDecay * learningRate * (input[ 2 ] - Weights[ 2 ] * input[ 2 ])) ,
            //    worldPosition.z - 1000 , worldPosition.z + 1000 );
            //newZ = ( double ) Mathf.Clamp( ( float ) newZ , -4000 , -4000 + 5000 );

            //double newX = ( double ) Mathf.Clamp( ( float ) (worldPosition.x + distanceDecay * learningRate * (worldPosition.x - Weights[ 0 ] * input[ 0 ])) ,
            //    worldPosition.x - 1000 , worldPosition.x + 1000 );
            //newX = ( double ) Mathf.Clamp( ( float ) newX , -3000 , -3000 + 5000 );

            //double newY = worldPosition.y + distanceDecay * learningRate * (worldPosition.y - Weights[ 1 ] * input[ 1 ]);

            //double newZ = ( double ) Mathf.Clamp( ( float ) (worldPosition.z + distanceDecay * learningRate * (worldPosition.z - Weights[ 2 ] * input[ 2 ])) ,
            //    worldPosition.z - 1000 , worldPosition.z + 1000 );
            //newZ = ( double ) Mathf.Clamp( ( float ) newZ , -4000 , -4000 + 5000 );

            //double newX = ( double ) Mathf.Clamp( ( float ) (worldPosition.x + distanceDecay * learningRate * (Weights[ 0 ] * input[ 0 ] - worldPosition.x * Weights[ 0 ])) ,
            //    worldPosition.x - 1000 , worldPosition.x + 1000 );
            //newX = ( double ) Mathf.Clamp( ( float ) newX , -3000 , -3000 + 5000 );

            double newX = worldPosition.x;

            //double newY = worldPosition.y + distanceDecay * learningRate * (Weights[ 1 ] * input[ 1 ] - worldPosition.y * Weights[ 1 ]);

            double newY = worldPosition.y + distanceDecay * learningRate * (input[ 1 ] - worldPosition.y);

            //double newZ = ( double ) Mathf.Clamp( ( float ) (worldPosition.z + distanceDecay * learningRate * (Weights[ 2 ] * input[ 2 ] - worldPosition.z * Weights[ 2 ])) ,
            //    worldPosition.z - 1000 , worldPosition.z + 1000 );
            //newZ = ( double ) Mathf.Clamp( ( float ) newZ , -4000 , -4000 + 5000 );

            double newZ = worldPosition.z;

            //double newX = worldPosition.x;
            //double newZ = worldPosition.z;

            Vector3 newWorldPosition = new Vector3( ( float ) newX , ( float ) newY , ( float ) newZ );

            worldPosition = newWorldPosition;
        }
    }
}