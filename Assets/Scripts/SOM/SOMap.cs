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
        internal INeuron[,] _matrix;
        internal int _height;
        internal int _width;
        internal double _matrixRadius;
        internal double _numberOfIterations;
        internal double _timeConstant;
        internal double _learningRate;

        public SOMap( int width, int height, int inputDimension, int numberOfIterations, double learningRate )
        {
            _width = width;
            _height = height;
            _matrix = new INeuron[ _width, _height ];
            _numberOfIterations = numberOfIterations;
            _learningRate = learningRate;

            _matrixRadius = Math.Max( _width, _height ) / 2;
            _timeConstant = _numberOfIterations / Math.Log( _matrixRadius );

            InitializeConnections( inputDimension );
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
        public void Train( Vector[] input )
        {
            int iteration = 0;
            var learningRate = _learningRate;

            while ( iteration < _numberOfIterations )
            {
                var currentRadius = CalculateNeighborhoodRadius( iteration );

                for ( int i = 0; i < input.Length; i++ )
                {
                    var currentInput = input[ i ];
                    var bmu = CalculateBMU( currentInput );

                    (int xStart, int xEnd, int yStart, int yEnd) = GetRadiusIndexes( bmu, currentRadius );

                    for ( int x = xStart; x < xEnd; x++ )
                    {
                        for ( int y = yStart; y < yEnd; y++ )
                        {
                            var processingNeuron = GetNeuron( x, y );
                            var distance = bmu.Distance( processingNeuron );
                            if ( distance <= Math.Pow( currentRadius, 2.0 ) )
                            {
                                var distanceDrop = GetDistanceDrop( distance, currentRadius );
                                processingNeuron.UpdateWeights( currentInput, learningRate, distanceDrop );
                            }
                        }
                    }
                }
                iteration++;
                learningRate = _learningRate * Math.Exp( -( double ) iteration / _numberOfIterations );
            }
        }

        internal (int xStart, int xEnd, int yStart, int yEnd) GetRadiusIndexes( INeuron bmu, double currentRadius )
        {
            var xStart = ( int ) (bmu.X - currentRadius - 1);
            xStart = (xStart < 0) ? 0 : xStart;

            var xEnd = ( int ) (xStart + (currentRadius * 2) + 1);
            if ( xEnd > _width ) xEnd = _width;

            var yStart = ( int ) (bmu.Y - currentRadius - 1);
            yStart = (yStart < 0) ? 0 : yStart;

            var yEnd = ( int ) (yStart + (currentRadius * 2) + 1);
            if ( yEnd > _height ) yEnd = _height;

            return (xStart, xEnd, yStart, yEnd);
        }

        internal INeuron GetNeuron( int indexX, int indexY )
        {
            if ( indexX > _width || indexY > _height )
                throw new ArgumentException( "Wrong index!" );

            return _matrix[ indexX, indexY ];
        }

        internal double CalculateNeighborhoodRadius( double iteration )
        {
            return _matrixRadius * Math.Exp( -iteration / _timeConstant );
        }

        internal double GetDistanceDrop( double distance, double radius )
        {
            return Math.Exp( -(Math.Pow( distance, 2.0 ) / Math.Pow( radius, 2.0 )) );
        }

        internal INeuron CalculateBMU( IVector input )
        {
            INeuron bmu = _matrix[ 0, 0 ];
            double bestDist = input.EuclidianDistance( bmu.Weights );

            for ( int i = 0; i < _width; i++ )
            {
                for ( int j = 0; j < _height; j++ )
                {
                    var distance = input.EuclidianDistance( _matrix[ i, j ].Weights );
                    if ( distance < bestDist )
                    {
                        bmu = _matrix[ i, j ];
                        bestDist = distance;
                    }
                }
            }

            return bmu;
        }

        private void InitializeConnections( int inputDimension )
        {
            for ( int i = 0; i < _width; i++ )
            {
                for ( int j = 0; j < _height; j++ )
                {
                    _matrix[ i, j ] = new Neuron( inputDimension ) { X = i, Y = j };
                }
            }
        }
    }
}
