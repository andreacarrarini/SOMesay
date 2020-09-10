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

public class TerrainSampler : MonoBehaviour
{

    private int somDimensionInTiles = 5;                                                            // The dimension of the terrain, in tiles, that the SOM has to analyze
    private int tileDimension = 1000;                                                               // The tile dimension in world's unit
    private int tileSubdivision = 10;                                                               // How many input vectors a tile contains
    private int inputVectorDimension = 50;                                                          // The dimension and the number of each input vector for the SOM
    private int numberOfIterations = 100;
    private double leariningRate = 0.5;
    private int matrixSideLength = 25;                                                              // The length of the side (square) of the SOM (how many neurons in heigth and length)
    private Vector[] inputMatrix;                                                                   // The SOM input matrix (list of list of vectors)
    private Vector3 pointToSample;                                                                  // The position used to sample the height of each point required
    private Vector3 samplingStartingPoint;
    private Vector3[,] visualSomMatrix;                                                             // The matrix of points sampled (used to spawn visual objects)
    private GameObject[,] neuronPointsMatrix;
    private GameObject[,] horizontalNeuronLinksMatrix;
    private GameObject[,] verticalNeuronLinksMatrix;
    private float linkLength;                                                                       // The length of the link between 2 neurons
    private float neuronPointScale;                                                                 // The scale of the sphere in Unity
    private float som3DNetHeight = 300;                                                             // The height offset of the 3D net

    void Start()
    {
        PrepareInputMatrix();

        GetBottomLeftCorner();

        PrepareSomNet3D();

        Sampling();

        BuildSom3DNet();
    }

    public void PrepareInputMatrix()                                                                // Prepares the Input Matrix
    {
        inputMatrix = new Vector[ inputVectorDimension ];
        Vector inputVector = new Vector();

        for ( int i = 0; i < inputVectorDimension; i++ )
        {
            inputVector.Add( i );                                                                   // Initializing the base inputVector
        }

        for ( int i = 0; i < inputVectorDimension; i++ )
        {
            Vector inputVectorToInsert = ( Vector ) Vector.DeepClone( inputVector );
            inputMatrix[ i ] = inputVectorToInsert;
        }
    }

    public void Sampling()
    {
        pointToSample = new Vector3();
        float xOffset = samplingStartingPoint.x;                                                    // To make it independent from the tile positioning
        float zOffset = samplingStartingPoint.z;
        Terrain[] terrains = Terrain.activeTerrains;                                                // Array of all active terrains

        for ( int i = 0; i < somDimensionInTiles; i++ )
        {
            for ( int j = 0; j < somDimensionInTiles; j++ )
            {
                for ( int iTile = 0; iTile < tileSubdivision; iTile++ )
                {
                    for ( int jTile = 0; jTile < tileSubdivision; jTile++ )
                    {
                        int terrainIndex = 0;

                        // SAMPLING
                        pointToSample.Set( xOffset + i * tileDimension + iTile * (tileDimension / tileSubdivision) , 0f ,
                            zOffset + j * tileDimension + jTile * (tileDimension / tileSubdivision) );

                        while ( terrains[ terrainIndex ].transform.position.x > pointToSample.x || pointToSample.x - terrains[ terrainIndex ].transform.position.x >= tileDimension
                            || terrains[ terrainIndex ].transform.position.z > pointToSample.z || pointToSample.z - terrains[ terrainIndex ].transform.position.z >= tileDimension )
                        {
                            terrainIndex++;                                                         // Looking for the correct terrain to sample
                        }

                        pointToSample.y = terrains[ terrainIndex ].SampleHeight( pointToSample );

                        visualSomMatrix[ i * tileSubdivision + iTile , j * tileSubdivision + jTile ] = pointToSample;

                        inputMatrix[ i * tileSubdivision + iTile ][ j * tileSubdivision + jTile ] = pointToSample.y;
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
        var som = new SOMap( matrixSideLength , matrixSideLength , inputVectorDimension , numberOfIterations , leariningRate );
        som.Train( inputMatrix );
    }

    public void PrepareSomNet3D()
    {
        GameObject neuronPoint = ( GameObject ) Resources.Load( "NeuronPoint" );
        GameObject neuronLink = ( GameObject ) Resources.Load( "NeuronLinkFather" );
        visualSomMatrix = new Vector3[ inputVectorDimension , inputVectorDimension ];
        neuronPointsMatrix = new GameObject[ inputVectorDimension , inputVectorDimension ];
        horizontalNeuronLinksMatrix = new GameObject[ inputVectorDimension , inputVectorDimension ];
        verticalNeuronLinksMatrix = new GameObject[ inputVectorDimension , inputVectorDimension ];

        for ( int i = 0; i < inputVectorDimension; i++ )
        {
            for ( int j = 0; j < inputVectorDimension; j++ )
            {
                neuronPointsMatrix[ i , j ] = Instantiate( neuronPoint , new Vector3( 0 , 0 , 0 ) , Quaternion.identity );
                horizontalNeuronLinksMatrix[ i , j ] = Instantiate( neuronLink , new Vector3( 0 , 0 , 0 ) , Quaternion.identity );
                verticalNeuronLinksMatrix[ i , j ] = Instantiate( neuronLink , new Vector3( 0 , 0 , 0 ) , Quaternion.identity );
            }
        }
    }

    public void BuildSom3DNet()
    {
        //linkLength = tileDimension / tileSubdivision;                                               // Roughly
        linkLength = 10;
        neuronPointScale = 20;

        for ( int i = 0; i < inputVectorDimension; i++ )
        {
            for ( int j = 0; j < inputVectorDimension; j++ )
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

                if ( i + 1 < inputVectorDimension && j + 1 < inputVectorDimension )
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
