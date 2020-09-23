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

    void Start()
    {
        
    }

    public void AnalyzeTerrainUnderNodes(SOMap soMap)
    {
        foreach ( Neuron neuron in soMap._matrix )
        {
            Vector3 neuronPosition = neuron.worldPosition;
            float realTerrainHeight = terrainSampler.terrains[ terrainSampler.GetTerrainsIndexByPoint( neuronPosition ) ].SampleHeight( neuronPosition );


            // TODO: Fix the height mistake and then change the material of neurons according to a logic
            if ( true )
                return;
        }
    }
}
