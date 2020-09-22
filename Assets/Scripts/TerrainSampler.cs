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
using SOM;
using SOM.VectorNamespace;
using System;

public class TerrainSampler : MonoBehaviour
{

    private int somDimensionInTiles = 5;                                                            // The dimension of the terrain, in tiles, that the SOM has to analyze
    private int tileDimension = 1000;                                                               // The tile dimension in world's unit
    private int tileSubdivision = 10;                                                               // How many input vectors a tile contains
    private int samplingVectorDimension = 50;                                                       // The dimension and the number of each sampling vector
    private int numberOfIterations = 10;
    private double leariningRate = 0.5;
    private int matrixSideLength = 20;                                                              // The length of the side (square) of the SOM (how many neurons in heigth and length)
    public Vector[] inputMatrix;                                                                    // The SOM input matrix (list of vectors)
    private Vector3 pointToSample;                                                                  // The position used to sample the height of each point required
    private Vector3 samplingStartingPoint;
    private Vector3[,] visualSomMatrix;                                                             // The matrix of points sampled (used to spawn visual objects)
    private GameObject[,] neuronPointsMatrix;
    private GameObject[,] horizontalNeuronLinksMatrix;
    private GameObject[,] verticalNeuronLinksMatrix;
    private float linkLength;                                                                       // The length of the link between 2 neurons
    private float neuronPointScale;                                                                 // The scale of the sphere in Unity
    private float som3DNetHeight = 400;                                                             // The height offset of the 3D net
    public SOMap soMap;                                                                             // The actual SOM
    private IEnumerator coroutine;
    public int number = 0;                                                                          // Just for debug
    float mapMagicGraphHeight = 751f;

    void Start()
    {
        PrepareInputMatrix();

        GetBottomLeftCorner();

        PrepareRealSomNet3D();
        //PrepareSomNet3D();

        Sampling();

        SomTrainLauncher();

        //BuildRealSom3DNet();
        //BuildSom3DNet();
    }

    public void PrepareInputMatrix()                                                                // Prepares the Input Matrix
    {
        inputMatrix = new Vector[ samplingVectorDimension * samplingVectorDimension ];
        Vector inputVector = new Vector();

        for ( int i = 0; i < 3; i++ )                                                               // 3 is the dimension of a Vector3
        {
            inputVector.Add( i );                                                                   // Initializing the base inputVector
        }

        for ( int i = 0; i < samplingVectorDimension * samplingVectorDimension; i++ )
        {
            Vector inputVectorToInsert = ( Vector ) Vector.DeepClone( inputVector );
            inputMatrix[ i ] = inputVectorToInsert;
        }
    }

    public Terrain[] SortActiveTerrains( Terrain[] terrains , float xOffset , float zOffset )
    {
        for ( int i = 0; i < terrains.Length; i++ )
        {
            Terrain currentTerrain = terrains[ i ];

            int position = 0;

            //for ( int j = 0; j < terrains.Length; j++ )
            //{
            //    Terrain comparisonTerrain = terrains[ j ];

            //    if ( currentTerrain.transform.position.x > comparisonTerrain.transform.position.x )
            //        position++;

            //    if ( currentTerrain.transform.position.z > comparisonTerrain.transform.position.z )
            //        position++;
            //}

            foreach ( Terrain terrain in terrains )
            {
                position = ( int ) (Math.Floor( (terrain.transform.position.z + Math.Abs( zOffset )) / tileDimension ) * somDimensionInTiles +
                    Math.Floor( (terrain.transform.position.x + Math.Abs( xOffset )) / tileDimension ));

                int swapPosition = ( int ) (Math.Floor( (terrains[ position ].transform.position.z + Math.Abs( zOffset )) / tileDimension ) * somDimensionInTiles +
                    Math.Floor( (terrains[ position ].transform.position.x + Math.Abs( xOffset )) / tileDimension ));

                // Swap
                Terrain swapTerrain = terrains[ position ];
                terrains[ position ] = terrain;
                terrains[ swapPosition ] = swapTerrain;
            }
        }
        return terrains;
    }

    public void Sampling()
    {
        pointToSample = new Vector3();
        float xOffset = samplingStartingPoint.x;                                                    // To make it independent from the tile positioning
        float zOffset = samplingStartingPoint.z;
        Terrain[] terrains = Terrain.activeTerrains;                                                // Array of all active terrains
        terrains = SortActiveTerrains( terrains , xOffset , zOffset );

        for ( int j = 0; j < somDimensionInTiles; j++ )
        {
            for ( int i = 0; i < somDimensionInTiles; i++ )
            {
                for ( int jTile = 0; jTile < tileSubdivision; jTile++ )
                {
                    for ( int iTile = 0; iTile < tileSubdivision; iTile++ )
                    {
                        int terrainIndex = 0;

                        // SAMPLING
                        pointToSample.Set( xOffset + i * tileDimension + iTile * (tileDimension / tileSubdivision) , 0f ,
                            zOffset + j * tileDimension + jTile * (tileDimension / tileSubdivision) );

                        //while ( terrains[ terrainIndex ].transform.position.x > pointToSample.x || pointToSample.x - terrains[ terrainIndex ].transform.position.x >= tileDimension
                        //    || terrains[ terrainIndex ].transform.position.z > pointToSample.z || pointToSample.z - terrains[ terrainIndex ].transform.position.z >= tileDimension )
                        //{
                        //    print( terrains[ terrainIndex ].transform.position );                   // Debug
                        //    terrainIndex++;                                                         // Looking for the correct terrain to sample
                        //}

                        terrainIndex = ( int ) (Math.Floor( (pointToSample.z + Math.Abs( zOffset )) / tileDimension ) * somDimensionInTiles +
                            Math.Floor( (pointToSample.x + Math.Abs( xOffset )) / tileDimension ));

                        Terrain activeTerrain = terrains[ terrainIndex ];
                        if ( !activeTerrain.isActiveAndEnabled )
                            return;

                        pointToSample.y = terrains[ terrainIndex ].SampleHeight( pointToSample );

                        //visualSomMatrix[ i * tileSubdivision + iTile , j * tileSubdivision + jTile ] = pointToSample;

                        // The index in the Input Matrix in which the sample will be memorized
                        int positionInInputMatrix = ( int ) (Math.Floor( (pointToSample.z + Math.Abs( zOffset )) / (tileDimension / tileSubdivision) ) * samplingVectorDimension
                            + Math.Floor( (pointToSample.x + Math.Abs( xOffset )) / (tileDimension / tileSubdivision) ));

                        inputMatrix[ positionInInputMatrix ][ 0 ] = pointToSample.x;
                        inputMatrix[ positionInInputMatrix ][ 1 ] = pointToSample.y;
                        inputMatrix[ positionInInputMatrix ][ 2 ] = pointToSample.z;
                    }
                }
            }
        }
    }

    public void GetBottomLeftCorner()                                                               // Find the bottom left corner of the "tilemap"
    {
        GameObject mapMagicFather = GameObject.FindGameObjectWithTag( "MapMagic" );                 // The MapMagic object father of all tiles
        Vector3 bottomLeftCorner = new Vector3( 0f , 0f , 0f );

        foreach ( Transform tileTransform in mapMagicFather.GetComponentInChildren<Transform>() )
        {
            if ( tileTransform.position.x <= bottomLeftCorner.x && tileTransform.position.z <= bottomLeftCorner.z )
                bottomLeftCorner = tileTransform.position;
        }

        samplingStartingPoint = bottomLeftCorner;
    }

    public void SomTrainLauncher()                                                                  // Launches the SOM training
    {
        soMap = new SOMap( matrixSideLength , matrixSideLength , 3 , numberOfIterations , leariningRate , this );
        soMap.SetNeuronsInitialWorldPositions( matrixSideLength , somDimensionInTiles , tileDimension , samplingStartingPoint.x ,
            samplingStartingPoint.z , som3DNetHeight , mapMagicGraphHeight );

        coroutine = soMap.TrainCoroutine( inputMatrix );
        StartCoroutine( coroutine );
        //soMap.Train( inputMatrix );
    }

    public void PrintSomWeights()                                                                   // DEBUG
    {
        for ( int _i = 0; _i < matrixSideLength; _i++ )
        {
            for ( int _j = 0; _j < matrixSideLength; _j++ )
            {
                print( soMap._matrix[ _i , _j ].Weights[ 0 ] + " " + soMap._matrix[ _i , _j ].Weights[ 1 ] + " " + soMap._matrix[ _i , _j ].Weights[ 2 ] );
            }
        }
    }

    public void PrepareRealSomNet3D()                                                               // Prepares the Som matrix 3D net in scene
    {
        GameObject neuronPoint = ( GameObject ) Resources.Load( "NeuronPoint" );
        GameObject neuronLink = ( GameObject ) Resources.Load( "NeuronLinkFather" );
        visualSomMatrix = new Vector3[ matrixSideLength , matrixSideLength ];
        neuronPointsMatrix = new GameObject[ matrixSideLength , matrixSideLength ];
        horizontalNeuronLinksMatrix = new GameObject[ matrixSideLength , matrixSideLength ];
        verticalNeuronLinksMatrix = new GameObject[ matrixSideLength , matrixSideLength ];

        for ( int i = 0; i < matrixSideLength; i++ )
        {
            for ( int j = 0; j < matrixSideLength; j++ )
            {
                neuronPointsMatrix[ i , j ] = Instantiate( neuronPoint , new Vector3( 0 , 0 , 0 ) , Quaternion.identity );
                horizontalNeuronLinksMatrix[ i , j ] = Instantiate( neuronLink , new Vector3( 0 , 0 , 0 ) , Quaternion.identity );
                verticalNeuronLinksMatrix[ i , j ] = Instantiate( neuronLink , new Vector3( 0 , 0 , 0 ) , Quaternion.identity );
            }
        }
    }

    public void PrepareSomNet3D()
    {
        GameObject neuronPoint = ( GameObject ) Resources.Load( "NeuronPoint" );
        GameObject neuronLink = ( GameObject ) Resources.Load( "NeuronLinkFather" );
        visualSomMatrix = new Vector3[ samplingVectorDimension , samplingVectorDimension ];
        neuronPointsMatrix = new GameObject[ samplingVectorDimension , samplingVectorDimension ];
        horizontalNeuronLinksMatrix = new GameObject[ samplingVectorDimension , samplingVectorDimension ];
        verticalNeuronLinksMatrix = new GameObject[ samplingVectorDimension , samplingVectorDimension ];

        for ( int i = 0; i < samplingVectorDimension; i++ )
        {
            for ( int j = 0; j < samplingVectorDimension; j++ )
            {
                neuronPointsMatrix[ i , j ] = Instantiate( neuronPoint , new Vector3( 0 , 0 , 0 ) , Quaternion.identity );
                horizontalNeuronLinksMatrix[ i , j ] = Instantiate( neuronLink , new Vector3( 0 , 0 , 0 ) , Quaternion.identity );
                verticalNeuronLinksMatrix[ i , j ] = Instantiate( neuronLink , new Vector3( 0 , 0 , 0 ) , Quaternion.identity );
            }
        }
    }

    public void BuildRealSom3DNet()
    {
        //linkLength = tileDimension / tileSubdivision;                                               // Roughly
        linkLength = 10;
        neuronPointScale = 20;

        for ( int i = 0; i < matrixSideLength; i++ )
        {
            for ( int j = 0; j < matrixSideLength; j++ )
            {
                // NEURONS
                neuronPointsMatrix[ i , j ].transform.position = soMap._matrix[ i , j ].worldPosition;
                neuronPointsMatrix[ i , j ].transform.localScale = new Vector3( neuronPointScale , neuronPointScale , neuronPointScale );

                // LINKS
                Vector3 horizontalLinkNewPosition;                                                  // The position ehre the horizontal link should be (between two neuron points)                       
                Vector3 verticalLinkNewPosition;                                                    // Same but vertical link
                Vector3 horizontalLinkDirection = new Vector3();                                    // The direction of the vector from a neuron point to his right neighbour
                Vector3 verticalLinkDirection = new Vector3();                                      // The direction of the vector from a neuron point to his front neighbour

                horizontalLinkNewPosition = soMap._matrix[ i , j ].worldPosition;
                verticalLinkNewPosition = soMap._matrix[ i , j ].worldPosition;

                if ( i + 1 < matrixSideLength && j + 1 < matrixSideLength )
                {
                    horizontalLinkNewPosition = (soMap._matrix[ i , j ].worldPosition + soMap._matrix[ i + 1 , j ].worldPosition) / 2;
                    verticalLinkNewPosition = (soMap._matrix[ i , j ].worldPosition + soMap._matrix[ i , j + 1 ].worldPosition) / 2;

                    horizontalLinkDirection = soMap._matrix[ i + 1 , j ].worldPosition - soMap._matrix[ i , j ].worldPosition;
                    verticalLinkDirection = soMap._matrix[ i , j + 1 ].worldPosition - soMap._matrix[ i , j ].worldPosition;
                }

                float horizontalNeuronLinkLength;
                float verticalNeuronLinkLength;

                if ( i + 1 >= matrixSideLength || j + 1 >= matrixSideLength )
                {
                    horizontalNeuronLinkLength = 50;
                    verticalNeuronLinkLength = 50;
                }
                else
                {
                    horizontalNeuronLinkLength = Math.Abs( (soMap._matrix[ i + 1 , j ].worldPosition - soMap._matrix[ i , j ].worldPosition).magnitude ) / 2;
                    verticalNeuronLinkLength = Math.Abs( (soMap._matrix[ i , j + 1 ].worldPosition - soMap._matrix[ i , j ].worldPosition).magnitude ) / 2;
                }

                // Changing their position
                horizontalNeuronLinksMatrix[ i , j ].transform.position = horizontalLinkNewPosition;
                verticalNeuronLinksMatrix[ i , j ].transform.position = verticalLinkNewPosition;

                // Changing their rotation
                horizontalNeuronLinksMatrix[ i , j ].transform.rotation = Quaternion.LookRotation( horizontalLinkDirection , horizontalNeuronLinksMatrix[ i , j ].transform.forward );
                verticalNeuronLinksMatrix[ i , j ].transform.rotation = Quaternion.LookRotation( verticalLinkDirection , horizontalNeuronLinksMatrix[ i , j ].transform.forward );

                // Changinge their scale
                // TODO: make this values parametric in order to fit with every change the SOM has (only the length must change)
                horizontalNeuronLinksMatrix[ i , j ].transform.localScale = new Vector3( 5 , 5 , horizontalNeuronLinkLength );
                verticalNeuronLinksMatrix[ i , j ].transform.localScale = new Vector3( 5 , 5 , verticalNeuronLinkLength );

                // Raising everything at the end
                Vector3 raisedPosition = neuronPointsMatrix[ i , j ].transform.position;
                raisedPosition.y += som3DNetHeight;
                neuronPointsMatrix[ i , j ].transform.position = raisedPosition;

                raisedPosition = horizontalNeuronLinksMatrix[ i , j ].transform.position;
                raisedPosition.y += som3DNetHeight;
                horizontalNeuronLinksMatrix[ i , j ].transform.position = raisedPosition;

                raisedPosition = verticalNeuronLinksMatrix[ i , j ].transform.position;
                raisedPosition.y += som3DNetHeight;
                verticalNeuronLinksMatrix[ i , j ].transform.position = raisedPosition;
            }
        }
    }

    public void UpdateRealSom3DNet()                                                                // Updates the positions of neuronPoints and Links during the iteration
    {
        for ( int i = 0; i < matrixSideLength; i++ )
        {
            for ( int j = 0; j < matrixSideLength; j++ )
            {
                // NEURONS
                neuronPointsMatrix[ i , j ].transform.position = soMap._matrix[ i , j ].worldPosition;

                // LINKS
                Vector3 horizontalLinkNewPosition;
                Vector3 verticalLinkNewPosition;
                Vector3 horizontalLinkDirection = new Vector3();
                Vector3 verticalLinkDirection = new Vector3();

                horizontalLinkNewPosition = soMap._matrix[ i , j ].worldPosition;
                verticalLinkNewPosition = soMap._matrix[ i , j ].worldPosition;

                if ( i + 1 < matrixSideLength && j + 1 < matrixSideLength )
                {
                    horizontalLinkNewPosition = (soMap._matrix[ i , j ].worldPosition + soMap._matrix[ i + 1 , j ].worldPosition) / 2;
                    verticalLinkNewPosition = (soMap._matrix[ i , j ].worldPosition + soMap._matrix[ i , j + 1 ].worldPosition) / 2;

                    horizontalLinkDirection = soMap._matrix[ i + 1 , j ].worldPosition - soMap._matrix[ i , j ].worldPosition;
                    verticalLinkDirection = soMap._matrix[ i , j + 1 ].worldPosition - soMap._matrix[ i , j ].worldPosition;
                }

                // Changing their position
                horizontalNeuronLinksMatrix[ i , j ].transform.position = horizontalLinkNewPosition;
                verticalNeuronLinksMatrix[ i , j ].transform.position = verticalLinkNewPosition;

                // Changing their rotation
                horizontalNeuronLinksMatrix[ i , j ].transform.rotation = Quaternion.LookRotation( horizontalLinkDirection , horizontalNeuronLinksMatrix[ i , j ].transform.forward );
                verticalNeuronLinksMatrix[ i , j ].transform.rotation = Quaternion.LookRotation( verticalLinkDirection , horizontalNeuronLinksMatrix[ i , j ].transform.forward );
            }
        }
    }

    public void BuildSom3DNet()
    {
        //linkLength = tileDimension / tileSubdivision;                                               // Roughly
        linkLength = 10;
        neuronPointScale = 20;

        for ( int i = 0; i < samplingVectorDimension; i++ )
        {
            for ( int j = 0; j < samplingVectorDimension; j++ )
            {
                // NEURONS
                neuronPointsMatrix[ i , j ].transform.position = visualSomMatrix[ i , j ];
                neuronPointsMatrix[ i , j ].transform.localScale = new Vector3( neuronPointScale , neuronPointScale , neuronPointScale );

                // LINKS
                Vector3 horizontalLinkNewPosition;                                                  // The position ehre the horizontal link should be (between two neuron points)                       
                Vector3 verticalLinkNewPosition;                                                    // Same but vertical link
                Vector3 horizontalLinkDirection = new Vector3();                                    // The direction of the vector from a neuron point to his right neighbour
                Vector3 verticalLinkDirection = new Vector3();                                      // The direction of the vector from a neuron point to his front neighbour

                horizontalLinkNewPosition = visualSomMatrix[ i , j ];                               // To get the y
                verticalLinkNewPosition = visualSomMatrix[ i , j ];

                if ( i + 1 < samplingVectorDimension && j + 1 < samplingVectorDimension )
                {
                    horizontalLinkNewPosition = (visualSomMatrix[ i , j ] + visualSomMatrix[ i + 1 , j ]) / 2;
                    verticalLinkNewPosition = (visualSomMatrix[ i , j ] + visualSomMatrix[ i , j + 1 ]) / 2;

                    horizontalLinkDirection = visualSomMatrix[ i + 1 , j ] - visualSomMatrix[ i , j ];
                    verticalLinkDirection = visualSomMatrix[ i , j + 1 ] - visualSomMatrix[ i , j ];
                }

                // Changing their position
                horizontalNeuronLinksMatrix[ i , j ].transform.position = horizontalLinkNewPosition;
                verticalNeuronLinksMatrix[ i , j ].transform.position = verticalLinkNewPosition;

                // Changing their rotation
                horizontalNeuronLinksMatrix[ i , j ].transform.rotation = Quaternion.LookRotation( horizontalLinkDirection , horizontalNeuronLinksMatrix[ i , j ].transform.forward );
                verticalNeuronLinksMatrix[ i , j ].transform.rotation = Quaternion.LookRotation( verticalLinkDirection , horizontalNeuronLinksMatrix[ i , j ].transform.forward );

                // Changinge their scale
                horizontalNeuronLinksMatrix[ i , j ].transform.localScale = new Vector3( 5 , 5 , 50 );
                verticalNeuronLinksMatrix[ i , j ].transform.localScale = new Vector3( 5 , 5 , 50 );

                // Raising everything at the end
                Vector3 raisedPosition = neuronPointsMatrix[ i , j ].transform.position;
                raisedPosition.y += som3DNetHeight;
                neuronPointsMatrix[ i , j ].transform.position = raisedPosition;

                raisedPosition = horizontalNeuronLinksMatrix[ i , j ].transform.position;
                raisedPosition.y += som3DNetHeight;
                horizontalNeuronLinksMatrix[ i , j ].transform.position = raisedPosition;

                raisedPosition = verticalNeuronLinksMatrix[ i , j ].transform.position;
                raisedPosition.y += som3DNetHeight;
                verticalNeuronLinksMatrix[ i , j ].transform.position = raisedPosition;
            }
        }
    }
}
