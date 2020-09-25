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
using SOM.NeuronNamespace;
using System;

public class TerrainAnalyzer : MonoBehaviour
{
    TerrainSampler terrainSampler;

    private void Start()
    {
        terrainSampler = gameObject.GetComponent<TerrainSampler>();
    }

    public void AnalyzeTerrainUnderNodes( SOMap soMap )
    {

        foreach ( Neuron neuron in soMap.Matrix )
        {
            Vector3 neuronPosition = neuron.GetworldPosition();
            //Camera.main.transform.position = neuronPosition;
            float realTerrainHeight = terrainSampler.Terrains[ terrainSampler.GetTerrainsIndexByPoint( neuronPosition ) ].SampleHeight( neuronPosition );

            // DEBUG
            //print( terrainSampler.GetTerrainsIndexByPoint( neuronPosition ) );

            if ( realTerrainHeight > neuronPosition.y )
            {
                neuronPosition.y += Math.Abs( realTerrainHeight - neuronPosition.y );
                neuron.SetworldPosition( neuronPosition );
            }
            else if ( neuronPosition.y > realTerrainHeight )
            {
                neuronPosition.y -= Math.Abs( neuronPosition.y - realTerrainHeight );
                neuron.SetworldPosition( neuronPosition );
            }

            realTerrainHeight = terrainSampler.Terrains[ terrainSampler.GetTerrainsIndexByPoint( neuronPosition ) ].SampleHeight( neuronPosition );

            if ( realTerrainHeight < 100 )
            {
                neuron.terrainType = Neuron.TerrainType.SEA;
            }
            else if ( realTerrainHeight >= 100 && realTerrainHeight < 120 )
            {
                neuron.terrainType = Neuron.TerrainType.SHORE;
            }
        }

        terrainSampler.UpdateRealSom3DNet();

        //print( terrainSampler.Terrains[ terrainSampler.GetTerrainsIndexByPoint( new Vector3( -3250 , 0 , -2750 ) ) ].SampleHeight( new Vector3( -4000 , 0 , -250 ) ) );
        //print( terrainSampler.GetTerrainsIndexByPoint( new Vector3( -3250 , 0 , -2750 ) ) );
        //print( terrainSampler.Terrains[ terrainSampler.GetTerrainsIndexByPoint( new Vector3( -4000 , 0 , -250 ) ) ].SampleHeight( new Vector3( -4000 , 0 , -250 ) ) );
        //print( terrainSampler.GetTerrainsIndexByPoint( new Vector3( -4000 , 0 , -250 ) ) );

        //terrainSampler.BuildRealSom3DNet();
    }

    public void ClassifyZoneNodes( SOMap soMap )
    {
        for ( int i = 0; i < soMap.Height; i++ )
        {
            for ( int j = 0; j < soMap.Width; j++ )
            {
                // TODO: fix this stuff, for now it only skips
                if ( i == 0 || j == 0 )
                {
                    Neuron neuron = ( Neuron ) soMap.Matrix[ i , j ];
                    neuron.terrainType = Neuron.TerrainType.SEA;
                }
                else if ( Math.Abs( soMap.Matrix[ i - 1 , j ].GetworldPosition().y - soMap.Matrix[ i , j ].GetworldPosition().y ) < 50 ||
                    Math.Abs( soMap.Matrix[ i , j - 1 ].GetworldPosition().y - soMap.Matrix[ i , j ].GetworldPosition().y ) < 50 )
                {
                    Neuron neuron = ( Neuron ) soMap.Matrix[ i , j ];
                    if ( neuron.terrainType != Neuron.TerrainType.SEA || neuron.terrainType != Neuron.TerrainType.SHORE )
                    {
                        neuron.terrainType = Neuron.TerrainType.PLAIN;

                        // Useless???
                        soMap.Matrix[ i , j ] = neuron;
                    }
                }
                // Might need to add also > 50 (but let's see)
                else if ( Math.Abs( soMap.Matrix[ i - 1 , j ].GetworldPosition().y - soMap.Matrix[ i , j ].GetworldPosition().y ) < 100 ||
                    Math.Abs( soMap.Matrix[ i , j - 1 ].GetworldPosition().y - soMap.Matrix[ i , j ].GetworldPosition().y ) < 100 )
                {
                    Neuron neuron = ( Neuron ) soMap.Matrix[ i , j ];
                    if ( neuron.terrainType != Neuron.TerrainType.SEA || neuron.terrainType != Neuron.TerrainType.SHORE )
                    {
                        neuron.terrainType = Neuron.TerrainType.HILL;

                        // Useless???
                        soMap.Matrix[ i , j ] = neuron;
                    }
                }
                else
                {
                    Neuron neuron = ( Neuron ) soMap.Matrix[ i , j ];
                    if ( neuron.terrainType != Neuron.TerrainType.SEA || neuron.terrainType != Neuron.TerrainType.SHORE )
                    {
                        neuron.terrainType = Neuron.TerrainType.UNSUITABLE;

                        // Useless???
                        soMap.Matrix[ i , j ] = neuron;
                    }
                }
            }
        }
    }

    public void ChangeNeuronsTexture( GameObject[,] neuronPointsMatrix )
    {
        foreach ( GameObject neuronPoint in neuronPointsMatrix )
        {
            neuronPoint.GetComponent<MeshRenderer>().material = Resources.Load( "Materials/Sea" ) as Material;
        }
    }
}
