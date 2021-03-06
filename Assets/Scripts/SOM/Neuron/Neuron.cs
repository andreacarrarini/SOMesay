﻿/*
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
        public int X { get; set; }
        public int Y { get; set; }
        public IVector Weights { get; }
        public TerrainType terrainType { get => _terrainType; set => _terrainType = value; }
        public GameObject NeuronPointGameObject { get => neuronPoint; set => neuronPoint = value; }
        public Vector3[,] UpRightTile { get => upRightTile; set => upRightTile = value; }
        public Vector3[,] UpLeftTile { get => upLeftTile; set => upLeftTile = value; }
        public Vector3[,] DownRightTile { get => downRightTile; set => downRightTile = value; }
        public Vector3[,] DownLeftTile { get => downLeftTile; set => downLeftTile = value; }

        private double mapSize = 5000;                                                                  // Remember to change if # of tiles or dimensions change
        private Vector3[,] upRightTile;
        private Vector3[,] upLeftTile;
        private Vector3[,] downRightTile;
        private Vector3[,] downLeftTile;

        /*
         * I put PLAIN first cause terrainType seems to be initialized as the first value in the enum
         * so if I put SEA or SHORE the TerrainAnalyzer will not change the terrainType value of the nodes
         */
        public enum TerrainType
        {
            PLAIN,
            SEA,
            SHORE,
            HILL,
            UNSUITABLE,
            NOCONNECTION
        }
        private TerrainType _terrainType;
        private Vector3 worldPosition;
        private GameObject neuronPoint;

        public Vector3 GetworldPosition()
        {
            return worldPosition;
        }

        public void SetworldPosition( Vector3 value )
        {
            worldPosition = value;
        }

        /*
         * It's the sin(alfa) where alfa is the angle between the side (inputPoint (sample) and Neuron.worldPosition (with y = 0)) and 
         * the side (inputPoint (sample) and Neuron.worldPosition (with y = 0))
         */
        private float contributeByDistance;

        public Neuron( int numOfWeights, GameObject neuronPointGameObject )
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

            NeuronPointGameObject = neuronPointGameObject;
        }

        public double Distance( INeuron neuron )
        {
            return Math.Pow( (X - neuron.X) , 2 ) + Math.Pow( (Y - neuron.Y) , 2 );
            //return Math.Pow( zeroHeightDistance , 2 ) + Math.Pow( worldPosition.y - neuron.worldPosition.y , 2 );
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
                Vector3 samplePoint = new Vector3( ( float ) input[ 0 ] , ( float ) input[ 1 ] , ( float ) input[ 2 ] );
                Vector3 worldPositionZeroHeight = new Vector3( GetworldPosition().x , 0 , GetworldPosition().z );

                contributeByDistance = samplePoint.y / (samplePoint - worldPositionZeroHeight).magnitude;
                //Weights[ i ] += distanceDecay * learningRate * (input[ i ] - Weights[ i ]);

                //Weights[ i ] += distanceDecay * learningRate * (input[ i ] / mapSize - Weights[ i ] * input[ i ] / mapSize);

                Weights[ i ] += distanceDecay * learningRate * (input[ i ] - GetworldPosition().y);
                //Weights[ i ] += distanceDecay * learningRate * contributeByDistance * (input[ i ] - worldPosition.y);
                //Weights[ i ] += learningRate * contributeByDistance * (input[ i ] - worldPosition.y);

                //Weights[ i ] = ( double ) Mathf.Clamp01( ( float ) Weights[ i ] );
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

            double newX = GetworldPosition().x;

            //double newY = worldPosition.y + distanceDecay * learningRate * (Weights[ 1 ] * input[ 1 ] - worldPosition.y * Weights[ 1 ]);

            // STUPID TEST
            //double newY = distanceDecay * learningRate * input[ 1 ];

            //double newY = worldPosition.y + distanceDecay * learningRate * (input[ 1 ] - worldPosition.y);
            //double newY = worldPosition.y + distanceDecay * learningRate * contributeByDistance * (input[ 1 ] - worldPosition.y);
            double newY = GetworldPosition().y + learningRate * contributeByDistance * (input[ 1 ] - GetworldPosition().y);

            //double newZ = ( double ) Mathf.Clamp( ( float ) (worldPosition.z + distanceDecay * learningRate * (Weights[ 2 ] * input[ 2 ] - worldPosition.z * Weights[ 2 ])) ,
            //    worldPosition.z - 1000 , worldPosition.z + 1000 );
            //newZ = ( double ) Mathf.Clamp( ( float ) newZ , -4000 , -4000 + 5000 );

            double newZ = GetworldPosition().z;

            //double newX = worldPosition.x;
            //double newZ = worldPosition.z;

            Vector3 newWorldPosition = new Vector3( ( float ) newX , ( float ) newY , ( float ) newZ );

            SetworldPosition( newWorldPosition );
        }
    }
}