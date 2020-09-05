using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainSampler : MonoBehaviour
{

    private int somDimensionInTiles = 5;                                                            // The dimension of the terrain, in tiles, that the SOM has to analyze
    private int tileDimension = 1000;                                                               // The tile dimension in world's unit
    private int tileSubdivision = 10;                                                               // How many input vectors a tile contains
    private int inputVectorDimension = 50;                                                          // The dimension and the number of each input vector for the SOM
    private List<List<Vector3>> inputMatrix;                                                        // The SOM input matrix (list of list of vectors)
    private Vector3 pointToSample;                                                                  // The position used to sample the height of each point required

    // TODO:    set the starting point fo sampling as the bottom left angle of the map

    void Start()
    {
        inputMatrix = new List<List<Vector3>>();
        pointToSample = new Vector3();

        for ( int i = 0; i < somDimensionInTiles; i++ )
        {
            for ( int j = 0; j < somDimensionInTiles; j++ )
            {
                for ( int iTile = 0; iTile < tileSubdivision; iTile++ )
                {
                    for ( int jTile = 0; jTile < tileSubdivision; jTile++ )
                    {
                        // SAMPLING
                        pointToSample.Set( i * tileDimension + iTile * (tileDimension / tileSubdivision), 0f, j * tileDimension + jTile * (tileDimension / tileSubdivision) );

                        pointToSample.y = Terrain.activeTerrain.SampleHeight( pointToSample );

                        inputMatrix[ i * tileSubdivision + iTile ][ j * tileSubdivision + jTile ] = pointToSample;
                    }
                }
            }
        }
    }
}
