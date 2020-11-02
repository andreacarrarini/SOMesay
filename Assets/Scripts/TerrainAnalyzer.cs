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
    private TerrainSampler terrainSampler;

    public TerrainSampler TerrainSampler { get => terrainSampler; set => terrainSampler = value; }

    private void Start()
    {
        TerrainSampler = gameObject.GetComponent<TerrainSampler>();
    }

    public void AnalyzeTerrainUnderNodes( SOMap soMap )
    {

        foreach ( Neuron neuron in soMap.Matrix )
        {
            Vector3 neuronPosition = neuron.GetworldPosition();
            float realTerrainHeight = TerrainSampler.Terrains[ TerrainSampler.GetTerrainsIndexByPoint( neuronPosition ) ].SampleHeight( neuronPosition );

            // TO COMMENT IF YOU WANT NO CORRECTION ON SOM
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

            if ( realTerrainHeight < 100 )
            {
                neuron.terrainType = Neuron.TerrainType.SEA;
            }
            else if ( realTerrainHeight >= 100 && realTerrainHeight < 120 )
            {
                neuron.terrainType = Neuron.TerrainType.SHORE;
            }
        }

        TerrainSampler.UpdateRealSom3DNet();
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
                else if ( Math.Abs( soMap.Matrix[ i - 1 , j ].GetworldPosition().y - soMap.Matrix[ i , j ].GetworldPosition().y ) < 50 &&
                    Math.Abs( soMap.Matrix[ i , j - 1 ].GetworldPosition().y - soMap.Matrix[ i , j ].GetworldPosition().y ) < 50 )
                {
                    Neuron neuron = ( Neuron ) soMap.Matrix[ i , j ];
                    if ( !(neuron.terrainType == Neuron.TerrainType.SEA || neuron.terrainType == Neuron.TerrainType.SHORE) )
                    {
                        neuron.terrainType = Neuron.TerrainType.PLAIN;

                        // Useless???
                        soMap.Matrix[ i , j ] = neuron;
                    }
                }
                // Might need to add also > 50 (but let's see)
                else if ( Math.Abs( soMap.Matrix[ i - 1 , j ].GetworldPosition().y - soMap.Matrix[ i , j ].GetworldPosition().y ) < 100 &&
                    Math.Abs( soMap.Matrix[ i , j - 1 ].GetworldPosition().y - soMap.Matrix[ i , j ].GetworldPosition().y ) < 100 )
                {
                    Neuron neuron = ( Neuron ) soMap.Matrix[ i , j ];
                    if ( !(neuron.terrainType == Neuron.TerrainType.SEA || neuron.terrainType == Neuron.TerrainType.SHORE) )
                    {
                        neuron.terrainType = Neuron.TerrainType.HILL;

                        // Useless???
                        soMap.Matrix[ i , j ] = neuron;
                    }
                }
                else
                {
                    Neuron neuron = ( Neuron ) soMap.Matrix[ i , j ];
                    if ( !(neuron.terrainType == Neuron.TerrainType.SEA || neuron.terrainType == Neuron.TerrainType.SHORE) )
                    {
                        neuron.terrainType = Neuron.TerrainType.UNSUITABLE;

                        // Useless???
                        soMap.Matrix[ i , j ] = neuron;
                    }
                }
            }
        }
    }

    public void ChangeNeuronsMaterial( GameObject[,] neuronPointsMatrix )
    {

        foreach ( Neuron neuron in TerrainSampler.SoMap.Matrix )
        {
            ChangeMaterialOnTerrainType( neuron.NeuronPointGameObject , neuron.terrainType );
        }
    }

    public void ChangeLinksMaterial()
    {
        // -1 to avoid index out of range
        for ( int i = 0; i < terrainSampler.MatrixSideLength - 1; i++ )
        {
            for ( int j = 0; j < terrainSampler.MatrixSideLength - 1; j++ )
            {
                // Horizontal
                Neuron leftNeuron = terrainSampler.SoMap.GetNeuron( i , j ) as Neuron;
                Neuron rightNeuron = terrainSampler.SoMap.GetNeuron( i + 1 , j ) as Neuron;
                if ( leftNeuron.terrainType == rightNeuron.terrainType )
                {
                    ChangeMaterialOnTerrainType( terrainSampler.HorizontalNeuronLinksMatrix[ i , j ] , leftNeuron.terrainType );
                }
                else
                {
                    ChangeMaterialOnTerrainType( terrainSampler.HorizontalNeuronLinksMatrix[ i , j ] , Neuron.TerrainType.NOCONNECTION );
                }

                // Vertical
                Neuron downNeuron = terrainSampler.SoMap.GetNeuron( i , j ) as Neuron;
                Neuron upNeuron = terrainSampler.SoMap.GetNeuron( i , j + 1 ) as Neuron;
                if ( downNeuron.terrainType == upNeuron.terrainType )
                {
                    ChangeMaterialOnTerrainType( terrainSampler.VerticalNeuronLinksMatrix[ i , j ] , downNeuron.terrainType );
                }
                else
                {
                    ChangeMaterialOnTerrainType( terrainSampler.VerticalNeuronLinksMatrix[ i , j ] , Neuron.TerrainType.NOCONNECTION );
                }
            }
        }
    }

    public void ChangeMaterialOnTerrainType( GameObject go , Neuron.TerrainType terrainType )
    {
        switch ( terrainType )
        {
            case Neuron.TerrainType.SEA:

                go.GetComponentInChildren<MeshRenderer>().material = Resources.Load( "Materials/Sea" ) as Material;
                break;

            case Neuron.TerrainType.SHORE:

                go.GetComponentInChildren<MeshRenderer>().material = Resources.Load( "Materials/Shore" ) as Material;
                break;

            case Neuron.TerrainType.PLAIN:

                go.GetComponentInChildren<MeshRenderer>().material = Resources.Load( "Materials/Plain" ) as Material;
                break;

            case Neuron.TerrainType.HILL:

                go.GetComponentInChildren<MeshRenderer>().material = Resources.Load( "Materials/Hill" ) as Material;
                break;

            case Neuron.TerrainType.UNSUITABLE:

                go.GetComponentInChildren<MeshRenderer>().material = Resources.Load( "Materials/Unsuitable" ) as Material;
                break;

            case Neuron.TerrainType.NOCONNECTION:

                go.GetComponentInChildren<MeshRenderer>().material = Resources.Load( "Materials/NoConnection" ) as Material;
                break;
        }
    }

    public void PlaceHouses()
    {
        var random = new System.Random();

        // starts form 1 and end at -1 to avoid index out of range
        for ( int i = 1; i < terrainSampler.MatrixSideLength - 1; i++ )
        {
            for ( int j = 1; j < terrainSampler.MatrixSideLength - 1; j++ )
            {
                Neuron centralNeuron = terrainSampler.SoMap.GetNeuron( i , j ) as Neuron;
                Neuron leftNeuron = terrainSampler.SoMap.GetNeuron( i - 1 , j ) as Neuron;
                Neuron rightNeuron = terrainSampler.SoMap.GetNeuron( i + 1 , j ) as Neuron;
                Neuron downNeuron = terrainSampler.SoMap.GetNeuron( i , j - 1 ) as Neuron;
                Neuron upNeuron = terrainSampler.SoMap.GetNeuron( i , j + 1 ) as Neuron;

                if ( centralNeuron.terrainType == Neuron.TerrainType.PLAIN )
                {
                    if ( centralNeuron.terrainType == leftNeuron.terrainType || centralNeuron.terrainType == rightNeuron.terrainType ||
                        centralNeuron.terrainType == downNeuron.terrainType || centralNeuron.terrainType == upNeuron.terrainType )
                    {
                        string housePath = "Houses/House" + Math.Ceiling( random.NextDouble() * 10 );

                        // For now only a house per neuron
                        GameObject house = Instantiate( Resources.Load( housePath ) , centralNeuron.GetworldPosition() , Quaternion.identity ) as GameObject;

                        Vector3 raisedPosition = house.transform.position;
                        raisedPosition.y += 30;
                        house.transform.position = raisedPosition;
                    }
                }
            }
        }

        //// For now only a house per neuron
        //foreach ( Neuron neuron in terrainSampler.SoMap.Matrix )
        //{
        //    if ( neuron.terrainType == Neuron.TerrainType.PLAIN )
        //    {
        //        string housePath = "Houses/House" + Math.Ceiling( random.NextDouble() * 10 );
        //        Instantiate( Resources.Load( housePath ) , neuron.GetworldPosition() , Quaternion.identity );
        //    }
        //}
    }

    public void ThickenGridOnPlaneNodes()
    {
        // starts form 1 and end at -1 to avoid index out of range
        for ( int i = 1; i < terrainSampler.MatrixSideLength - 1; i++ )
        {
            for ( int j = 1; j < terrainSampler.MatrixSideLength - 1; j++ )
            {
                Neuron centralNeuron = terrainSampler.SoMap.GetNeuron( i , j ) as Neuron;
                Neuron leftNeuron = terrainSampler.SoMap.GetNeuron( i - 1 , j ) as Neuron;
                //Neuron rightNeuron = terrainSampler.SoMap.GetNeuron( i + 1 , j ) as Neuron;
                Neuron downNeuron = terrainSampler.SoMap.GetNeuron( i , j - 1 ) as Neuron;
                //Neuron upNeuron = terrainSampler.SoMap.GetNeuron( i , j + 1 ) as Neuron;

                // thicken the grid on up-left, down-left and down-right tiles (in addition to the up-right)
                if ( leftNeuron.terrainType != Neuron.TerrainType.PLAIN && downNeuron.terrainType != Neuron.TerrainType.PLAIN )
                {
                    // down-left tile
                    Vector3[,] downLeftTile = new Vector3[ 9 , 9 ];

                    if ( i > 0 && j > 0 )
                    {
                        Neuron downLeftNeuron = terrainSampler.SoMap.GetNeuron( i - 1 , j - 1 ) as Neuron;
                        Vector3 downLeftNeuronWorldPosition = downLeftNeuron.GetworldPosition();
                        for ( int x = 1; x < 10; x++ )
                        {
                            for ( int z = 1; z < 10; z++ )
                            {
                                Vector3 pointToSample = downLeftNeuronWorldPosition + new Vector3( x * 10 , 0 , z * 10 );
                                pointToSample.y = TerrainSampler.Terrains[ terrainSampler.GetTerrainsIndexByPoint( pointToSample ) ].SampleHeight( pointToSample );

                                downLeftTile[ x - 1 , z - 1 ] = pointToSample;
                            }
                        }
                    }

                    // Changing the net (of the tile) in the Unity scene
                    terrainSampler.HorizontalNeuronLinksMatrix[ i - 1 , j - 1 ].GetComponentInChildren<MeshRenderer>().enabled = false;
                }

                // thicken only up-left (in addition to the up-right)
                else if ( leftNeuron.terrainType != Neuron.TerrainType.PLAIN && downNeuron.terrainType == Neuron.TerrainType.PLAIN )
                {

                }

                // thicken only down-right (in addition to the up-right)
                else if ( leftNeuron.terrainType == Neuron.TerrainType.PLAIN && downNeuron.terrainType != Neuron.TerrainType.PLAIN )
                {

                }

                // thicken the up-right
                //  TODO

            }
        }
    }
}
