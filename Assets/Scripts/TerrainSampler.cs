using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainSampler : MonoBehaviour
{

    private int somDimensionInTiles = 5;                                                            // The dimension of the terrain, in tiles, that the SOM has to analyze
    private int tileDimension = 1000;                                                               // The tile dimension in world's unit
    private int tileSubdivision = 10;                                                               // How many input vectors a tile contains
    private int inputVectorDimension = 50;                                                          // The dimension and the number of each input vector for the SOM
    private Vector3[,] inputMatrix;                                                                 // The SOM input matrix (list of list of vectors)
    private Vector3 pointToSample;                                                                  // The position used to sample the height of each point required
    private Vector3 samplingStartingPoint;

    void Start()
    {
        GetBottomLeftCorner();

        Sampling();

        foreach ( Vector3 item in inputMatrix )
        {
            print( item );
        }
    }

    public void Sampling()
    {
        inputMatrix = new Vector3[ inputVectorDimension, inputVectorDimension ];
        pointToSample = new Vector3();
        float xOffset = samplingStartingPoint.x;                                                    // To make it independent from the tile positioning
        float zOffset = samplingStartingPoint.z;

        for ( int i = 0; i < somDimensionInTiles; i++ )
        {
            for ( int j = 0; j < somDimensionInTiles; j++ )
            {
                for ( int iTile = 0; iTile < tileSubdivision; iTile++ )
                {
                    for ( int jTile = 0; jTile < tileSubdivision; jTile++ )
                    {
                        // SAMPLING
                        pointToSample.Set( xOffset + i * tileDimension + iTile * (tileDimension / tileSubdivision), 0f,
                            zOffset + j * tileDimension + jTile * (tileDimension / tileSubdivision) );

                        pointToSample.y = Terrain.activeTerrain.SampleHeight( pointToSample );

                        inputMatrix[ i * tileSubdivision + iTile, j * tileSubdivision + jTile ] = pointToSample;
                    }
                }
            }
        }
    }

    public void GetBottomLeftCorner()                                                               // Find the bottom left corner of the "tilemap"
    {
        GameObject mapMagicFather = GameObject.FindGameObjectWithTag( "MapMagic" );                 // The MapMagic object father of all tiles
        Vector3 bottomLeftCorner = new Vector3( 0f, 0f, 0f );

        foreach ( Transform tileTransform in mapMagicFather.GetComponentInChildren<Transform>() )
        {
            if ( tileTransform.position.x <= bottomLeftCorner.x && tileTransform.position.z <= bottomLeftCorner.z )
                bottomLeftCorner = tileTransform.position;
        }
        samplingStartingPoint = bottomLeftCorner;
        return;
    }
}
