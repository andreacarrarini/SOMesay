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
using System;
using SOM.NeuronNamespace;
using SOM.VectorNamespace;

namespace SOM
{
    public class SOMap
    {
        private INeuron[,] matrix;
        private int height;
        private int width;
        internal double _matrixRadius;
        internal double _numberOfIterations;
        internal double _timeConstant;
        internal double _learningRate;
        private TerrainSampler terrainSampler;
        private TerrainAnalyzer terrainAnalyzer;

        public INeuron[,] Matrix { get => matrix; set => matrix = value; }
        public int Height { get => height; set => height = value; }
        public int Width { get => width; set => width = value; }
        public TerrainSampler TerrainSampler { get => terrainSampler; set => terrainSampler = value; }
        public TerrainAnalyzer TerrainAnalyzer { get => terrainAnalyzer; set => terrainAnalyzer = value; }

        //private IEnumerator coroutine;

        public SOMap( int width , int height , int inputDimension , int numberOfIterations , double learningRate , TerrainSampler ts , TerrainAnalyzer ta )
        {
            Width = width;
            Height = height;
            Matrix = new INeuron[ Width , Height ];
            _numberOfIterations = numberOfIterations;
            _learningRate = learningRate;

            _matrixRadius = Math.Max( Width , Height ) / 2;
            _timeConstant = _numberOfIterations / Math.Log( _matrixRadius );

            TerrainSampler = ts;
            InitializeConnections( inputDimension );
            TerrainAnalyzer = ta;
        }

        /*
         * This method is one big loop that runs for a defined number of iterations.
         * In each iteration, the first thing that is done is a calculation of neighborhood radius.
         * This is done in CalculateNeighborhoodRadius() method.
         * This is driven by the number of iterations that had been run so far as well as the total number of iterations and dimensions of the output matrix.
         * After this number is calculated, the BMU is determined for each input vector. For this purpose, CalculateBMU() method is used.
         * Afterward, the indexes of the neurons that are within radius are calculated.
         * For each neuron within this radius, the distance from the BMU is calculated, which is used for calculating the new value for the distance decay.
         * Finally, this value,  along with the value of the learning rate is used to update the weights on each connection of the neuron.
         * At the end of the Train method, the learning rate is updated after each iteration.
        */

        public IEnumerator TrainCoroutine( Vector[] input )
        {
            int iteration = 0;
            var learningRate = _learningRate;

            while ( iteration < _numberOfIterations )
            {
                var currentRadius = CalculateNeighborhoodRadius( iteration );

                // DEBUG
                //terrainSampler.PrintSomWeights();

                yield return null;

                for ( int i = 0; i < input.Length; i++ )
                {
                    var currentInput = input[ i ];
                    var bmu = CalculateBMU( currentInput );

                    (int xStart, int xEnd, int yStart, int yEnd) = GetRadiusIndexes( bmu , currentRadius );

                    for ( int x = xStart; x < xEnd; x++ )
                    {
                        for ( int y = yStart; y < yEnd; y++ )
                        {
                            Neuron processingNeuron = GetNeuron( x , y ) as Neuron;
                            var distance = bmu.Distance( processingNeuron );

                            if ( distance <= Math.Pow( currentRadius , 2.0 ) )
                            {
                                var distanceDrop = GetDistanceDrop( distance , currentRadius );

                                // I inverted th order of learningRate and distanceDrop, because in the definition of UpdateWeights it is that way
                                processingNeuron.UpdateWeights( currentInput , distanceDrop , learningRate );

                                processingNeuron.UpdateNeuronsWorldPositions( currentInput , distanceDrop , learningRate );
                            }
                        }
                    }
                    TerrainSampler.number++;
                }
                iteration++;
            }
            MonoBehaviour.print( TerrainSampler.number );

            TerrainSampler.BuildRealSom3DNet();

            TerrainAnalyzer.AnalyzeTerrainUnderNodes( this );

            TerrainAnalyzer.ClassifyZoneNodes( this );

            TerrainAnalyzer.ChangeNeuronsMaterial( TerrainSampler.NeuronPointsMatrix );

            TerrainAnalyzer.ChangeLinksMaterial();
        }

        internal (int xStart, int xEnd, int yStart, int yEnd) GetRadiusIndexes( INeuron bmu , double currentRadius )
        {
            var xStart = ( int ) (bmu.X - currentRadius - 1);
            xStart = (xStart < 0) ? 0 : xStart;

            var xEnd = ( int ) (xStart + (currentRadius * 2) + 1);
            if ( xEnd > Width ) xEnd = Width;

            var yStart = ( int ) (bmu.Y - currentRadius - 1);
            yStart = (yStart < 0) ? 0 : yStart;

            var yEnd = ( int ) (yStart + (currentRadius * 2) + 1);
            if ( yEnd > Height ) yEnd = Height;

            return (xStart, xEnd, yStart, yEnd);
        }

        internal INeuron GetNeuron( int indexX , int indexY )
        {
            if ( indexX > Width || indexY > Height )
                throw new ArgumentException( "Wrong index!" );

            return Matrix[ indexX , indexY ];
        }

        /*
         * This method calculates the radius of the area in which neurons inside will have their weights modified.
         * It drops with the iterations.
         */
        internal double CalculateNeighborhoodRadius( double iteration )
        {
            return _matrixRadius * Math.Exp( -iteration / _timeConstant );
        }

        internal double GetDistanceDrop( double distance , double radius )
        {
            return Math.Exp( -(Math.Pow( distance , 2.0 ) / Math.Pow( radius , 2.0 )) );
        }

        internal INeuron CalculateBMU( IVector input )
        {
            INeuron bmu = Matrix[ 0 , 0 ];
            double bestDist = Double.PositiveInfinity;

            for ( int i = 0; i < Width; i++ )
            {
                for ( int j = 0; j < Height; j++ )
                {
                    /*
                     * Attempt to make the distance a real distance from points in space and not from random weights
                     */
                    Vector vectorToComputeDistanceWithWorldPosition = new Vector();

                    for ( int g = 0; g < 3; g++ )
                    {
                        vectorToComputeDistanceWithWorldPosition.Add( 0.5 );
                    }

                    vectorToComputeDistanceWithWorldPosition[ 0 ] = Matrix[ i , j ].GetworldPosition().x;
                    vectorToComputeDistanceWithWorldPosition[ 1 ] = Matrix[ i , j ].GetworldPosition().y;
                    vectorToComputeDistanceWithWorldPosition[ 2 ] = Matrix[ i , j ].GetworldPosition().z;

                    var distance = input.EuclidianDistance( vectorToComputeDistanceWithWorldPosition );

                    if ( distance < bestDist )
                    {
                        bmu = Matrix[ i , j ];
                        bestDist = distance;
                    }
                }
            }

            return bmu;
        }

        private void InitializeConnections( int inputDimension )
        {
            for ( int i = 0; i < Width; i++ )
            {
                for ( int j = 0; j < Height; j++ )
                {
                    Matrix[ i , j ] = new Neuron( inputDimension , TerrainSampler.NeuronPointsMatrix[ i , j ] ) { X = i , Y = j };
                }
            }
        }

        public void SetNeuronsInitialWorldPositions( int matrixSideLength , int somDimensionInTiles , float tileDimension ,
            float xOffset , float zOffset , float som3DNetHeight , float mapMagicGraphHeight )
        {
            for ( int j = 0; j < matrixSideLength; j++ )
            {
                for ( int i = 0; i < matrixSideLength; i++ )
                {
                    //Vector3 neuronWorldPosition = new Vector3( xOffset + i * (somDimensionInTiles * tileDimension / matrixSideLength) ,
                    //    som3DNetHeight , zOffset + j * (somDimensionInTiles * tileDimension / matrixSideLength) );
                    //_matrix[ i , j ].worldPosition = neuronWorldPosition;

                    Vector3 neuronWorldPosition = new Vector3( xOffset + i * (somDimensionInTiles * tileDimension / matrixSideLength) ,
                        0 , zOffset + j * (somDimensionInTiles * tileDimension / matrixSideLength) );
                    Matrix[ i , j ].SetworldPosition( neuronWorldPosition );
                }
            }
        }
    }
}
