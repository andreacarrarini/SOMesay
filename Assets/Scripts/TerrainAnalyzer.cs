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

        foreach ( Neuron neuron in soMap._matrix )
        {
            Vector3 neuronPosition = neuron.worldPosition;
            float realTerrainHeight = terrainSampler.terrains[ terrainSampler.GetTerrainsIndexByPoint( neuronPosition ) ].SampleHeight( neuronPosition );

            if ( realTerrainHeight > neuronPosition.y )
            {
                neuronPosition.y += Math.Abs( realTerrainHeight - neuronPosition.y );
                neuron.worldPosition = neuronPosition;
            }
            else if ( neuronPosition.y > realTerrainHeight )
            {
                neuronPosition.y -= Math.Abs( neuronPosition.y - realTerrainHeight );
                neuron.worldPosition = neuronPosition;
            }

            realTerrainHeight = terrainSampler.terrains[ terrainSampler.GetTerrainsIndexByPoint( neuronPosition ) ].SampleHeight( neuronPosition );

            if ( realTerrainHeight < 100 )
            {
                neuron.terrainType = Neuron.TerrainType.SEA;
            }
            else if ( realTerrainHeight >= 100 && realTerrainHeight < 120 )
            {
                neuron.terrainType = Neuron.TerrainType.SHORE;
            }

            terrainSampler.UpdateRealSom3DNet();

            terrainSampler.BuildRealSom3DNet();
        }
    }

    public void ClassifyZoneNodes( SOMap soMap )
    {
        for ( int i = 0; i < soMap._height; i++ )
        {
            for ( int j = 0; j < soMap._width; j++ )
            {
                if ( Math.Abs( soMap._matrix[ i - 1 , j ].worldPosition.y - soMap._matrix[ i , j ].worldPosition.y ) < 50 ||
                    Math.Abs( soMap._matrix[ i , j - 1 ].worldPosition.y - soMap._matrix[ i , j ].worldPosition.y ) < 50 )
                {
                    Neuron neuron = ( Neuron ) soMap._matrix[ i , j ];
                    neuron.terrainType = Neuron.TerrainType.PLAIN;

                    // Useless???
                    soMap._matrix[ i , j ] = neuron;
                }
                // Might need to add also > 50 (but let's see)
                else if ( Math.Abs( soMap._matrix[ i - 1 , j ].worldPosition.y - soMap._matrix[ i , j ].worldPosition.y ) < 100 || 
                    Math.Abs( soMap._matrix[ i , j - 1 ].worldPosition.y - soMap._matrix[ i , j ].worldPosition.y ) < 100 )
                {
                    Neuron neuron = ( Neuron ) soMap._matrix[ i , j ];
                    neuron.terrainType = Neuron.TerrainType.HILL;

                    // Useless???
                    soMap._matrix[ i , j ] = neuron;
                }
                else
                {
                    Neuron neuron = ( Neuron ) soMap._matrix[ i , j ];
                    neuron.terrainType = Neuron.TerrainType.UNSUITABLE;

                    // Useless???
                    soMap._matrix[ i , j ] = neuron;
                }
            }
        }
    }

    public void ChangeNeuronsTexture(SOMap soMap)
    {
    }
}
