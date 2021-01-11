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
    private int smallerNeuronsNumber = 5;
    private int smallerNeuronsSamplingStep;

    public enum tiles
    {
        DOWNLEFT,
        UPLEFT,
        DOWNRIGHT,
        UPRIGHT
    }

    public TerrainSampler TerrainSampler { get => terrainSampler; set => terrainSampler = value; }
    public int SmallerNeurons { get => smallerNeuronsNumber; set => smallerNeuronsNumber = value; }

    private void Start()
    {
        TerrainSampler = gameObject.GetComponent<TerrainSampler>();

        smallerNeuronsSamplingStep = TerrainSampler.SomDimensionInTiles * TerrainSampler.TileDimension / TerrainSampler.MatrixSideLength / smallerNeuronsNumber;
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

    public void ThickenGridOnPlainNodes()
    {
        bool downLeft = false;
        bool upLeft = false;
        bool downRight = false;

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
                if ( centralNeuron.terrainType == Neuron.TerrainType.PLAIN && leftNeuron.terrainType != Neuron.TerrainType.PLAIN
                    && downNeuron.terrainType != Neuron.TerrainType.PLAIN )
                {
                    #region tiles

                    // down-left tile
                    Vector3[,] downLeftTile = new Vector3[ smallerNeuronsNumber , smallerNeuronsNumber ];

                    downLeftTile = ManageNetTile( downLeftTile , tiles.DOWNLEFT , i , j );
                    centralNeuron.DownLeftTile = downLeftTile;

                    // up-left tile
                    Vector3[,] upLeftTile = new Vector3[ smallerNeuronsNumber , smallerNeuronsNumber ];

                    upLeftTile = ManageNetTile( upLeftTile , tiles.UPLEFT , i , j );
                    centralNeuron.UpLeftTile = upLeftTile;

                    // down-right tile
                    Vector3[,] downRightTile = new Vector3[ smallerNeuronsNumber , smallerNeuronsNumber ];

                    downRightTile = ManageNetTile( downRightTile , tiles.DOWNRIGHT , i , j );
                    centralNeuron.DownRightTile = downRightTile;

                    #endregion
                }

                // thicken only up-left (in addition to the up-right)
                else if ( centralNeuron.terrainType == Neuron.TerrainType.PLAIN && leftNeuron.terrainType != Neuron.TerrainType.PLAIN
                    && downNeuron.terrainType == Neuron.TerrainType.PLAIN )
                {
                    #region tiles

                    // up-left tile
                    Vector3[,] upLeftTile = new Vector3[ smallerNeuronsNumber , smallerNeuronsNumber ];

                    upLeftTile = ManageNetTile( upLeftTile , tiles.UPLEFT , i , j );
                    centralNeuron.UpLeftTile = upLeftTile;

                    #endregion
                }

                // thicken only down-right (in addition to the up-right)
                else if ( centralNeuron.terrainType == Neuron.TerrainType.PLAIN && leftNeuron.terrainType == Neuron.TerrainType.PLAIN
                    && downNeuron.terrainType != Neuron.TerrainType.PLAIN )
                {
                    #region tiles

                    // down-right tile
                    Vector3[,] downRightTile = new Vector3[ smallerNeuronsNumber , smallerNeuronsNumber ];

                    downRightTile = ManageNetTile( downRightTile , tiles.DOWNRIGHT , i , j );
                    centralNeuron.DownRightTile = downRightTile;

                    #endregion
                }

                // up-right tile
                if ( centralNeuron.terrainType == Neuron.TerrainType.PLAIN )
                {
                    #region tiles

                    Vector3[,] upRightTile = new Vector3[ smallerNeuronsNumber , smallerNeuronsNumber ];

                    upRightTile = ManageNetTile( upRightTile , tiles.UPRIGHT , i , j );
                    centralNeuron.UpRightTile = upRightTile;

                    #endregion
                }
            }
        }

        // Building the nets
        for ( int i = 1; i < terrainSampler.MatrixSideLength - 1; i++ )
        {
            for ( int j = 1; j < terrainSampler.MatrixSideLength - 1; j++ )
            {
                Neuron centralNeuron = terrainSampler.SoMap.GetNeuron( i , j ) as Neuron;
                Neuron leftNeuron = terrainSampler.SoMap.GetNeuron( i - 1 , j ) as Neuron;
                Neuron downNeuron = terrainSampler.SoMap.GetNeuron( i , j - 1 ) as Neuron;

                if ( centralNeuron.terrainType == Neuron.TerrainType.PLAIN && leftNeuron.terrainType != Neuron.TerrainType.PLAIN
                    && downNeuron.terrainType != Neuron.TerrainType.PLAIN )
                {
                    BuildSingleTileNet( centralNeuron.DownLeftTile , tiles.DOWNLEFT , centralNeuron );
                    BuildSingleTileNet( centralNeuron.UpLeftTile , tiles.UPLEFT , centralNeuron );
                    BuildSingleTileNet( centralNeuron.DownRightTile , tiles.DOWNRIGHT , centralNeuron );
                }

                // thicken only up-left (in addition to the up-right)
                else if ( centralNeuron.terrainType == Neuron.TerrainType.PLAIN && leftNeuron.terrainType != Neuron.TerrainType.PLAIN
                    && downNeuron.terrainType == Neuron.TerrainType.PLAIN )
                {
                    BuildSingleTileNet( centralNeuron.UpLeftTile , tiles.UPLEFT , centralNeuron );
                }

                // thicken only down-right (in addition to the up-right)
                else if ( centralNeuron.terrainType == Neuron.TerrainType.PLAIN && leftNeuron.terrainType == Neuron.TerrainType.PLAIN
                    && downNeuron.terrainType != Neuron.TerrainType.PLAIN )
                {
                    BuildSingleTileNet( centralNeuron.DownRightTile , tiles.DOWNRIGHT , centralNeuron );
                }

                // up-right tile
                if ( centralNeuron.terrainType == Neuron.TerrainType.PLAIN )
                {
                    BuildSingleTileNet( centralNeuron.UpRightTile , tiles.UPRIGHT , centralNeuron );
                }
            }
        }
    }

    public Vector3[,] ManageNetTile( Vector3[,] tile , tiles whichTile , int i , int j )
    {


        switch ( whichTile )
        {
            case tiles.DOWNLEFT:

                if ( i > 0 && j > 0 )
                {
                    Neuron downLeftNeuron = terrainSampler.SoMap.GetNeuron( i - 1 , j - 1 ) as Neuron;
                    Vector3 downLeftNeuronWorldPosition = downLeftNeuron.GetworldPosition();
                    for ( int x = 0; x < smallerNeuronsNumber; x++ )
                    {
                        for ( int z = 0; z < smallerNeuronsNumber; z++ )
                        {
                            Vector3 pointToSample = downLeftNeuronWorldPosition + new Vector3( x * smallerNeuronsSamplingStep , 0 , z * smallerNeuronsSamplingStep );
                            pointToSample.y = TerrainSampler.Terrains[ terrainSampler.GetTerrainsIndexByPoint( pointToSample ) ].SampleHeight( pointToSample );

                            tile[ x , z ] = pointToSample;
                        }
                    }

                    // Changing the net (of the tile) in the Unity scene
                    terrainSampler.HorizontalNeuronLinksMatrix[ i - 1 , j - 1 ].GetComponentInChildren<MeshRenderer>().enabled = false;
                    terrainSampler.VerticalNeuronLinksMatrix[ i - 1 , j - 1 ].GetComponentInChildren<MeshRenderer>().enabled = false;
                }

                break;

            case tiles.UPLEFT:

                if ( i > 0 && j > 0 )
                {
                    Neuron leftNeuron = terrainSampler.SoMap.GetNeuron( i - 1 , j ) as Neuron;
                    Vector3 leftNeuronWorldPosition = leftNeuron.GetworldPosition();
                    for ( int x = 0; x < smallerNeuronsNumber; x++ )
                    {
                        for ( int z = 0; z < smallerNeuronsNumber; z++ )
                        {
                            Vector3 pointToSample = leftNeuronWorldPosition + new Vector3( x * smallerNeuronsSamplingStep , 0 , z * smallerNeuronsSamplingStep );
                            pointToSample.y = TerrainSampler.Terrains[ terrainSampler.GetTerrainsIndexByPoint( pointToSample ) ].SampleHeight( pointToSample );

                            tile[ x , z ] = pointToSample;
                        }
                    }

                    // Changing the net (of the tile) in the Unity scene
                    terrainSampler.HorizontalNeuronLinksMatrix[ i - 1 , j ].GetComponentInChildren<MeshRenderer>().enabled = false;
                    terrainSampler.VerticalNeuronLinksMatrix[ i - 1 , j ].GetComponentInChildren<MeshRenderer>().enabled = false;
                }

                break;

            case tiles.DOWNRIGHT:

                if ( i > 0 && j > 0 )
                {
                    Neuron downNeuron = terrainSampler.SoMap.GetNeuron( i , j - 1 ) as Neuron;
                    Vector3 downNeuronWorldPosition = downNeuron.GetworldPosition();
                    for ( int x = 0; x < smallerNeuronsNumber; x++ )
                    {
                        for ( int z = 0; z < smallerNeuronsNumber; z++ )
                        {
                            Vector3 pointToSample = downNeuronWorldPosition + new Vector3( x * smallerNeuronsSamplingStep , 0 , z * smallerNeuronsSamplingStep );
                            pointToSample.y = TerrainSampler.Terrains[ terrainSampler.GetTerrainsIndexByPoint( pointToSample ) ].SampleHeight( pointToSample );

                            tile[ x , z ] = pointToSample;
                        }
                    }

                    // Changing the net (of the tile) in the Unity scene
                    terrainSampler.HorizontalNeuronLinksMatrix[ i , j - 1 ].GetComponentInChildren<MeshRenderer>().enabled = false;
                    terrainSampler.VerticalNeuronLinksMatrix[ i , j - 1 ].GetComponentInChildren<MeshRenderer>().enabled = false;
                }

                break;

            case tiles.UPRIGHT:

                if ( i > 0 && j > 0 )
                {
                    Neuron centralNeuron = terrainSampler.SoMap.GetNeuron( i , j ) as Neuron;
                    Vector3 centralNeuronWorldPosition = centralNeuron.GetworldPosition();
                    for ( int x = 0; x < smallerNeuronsNumber; x++ )
                    {
                        for ( int z = 0; z < smallerNeuronsNumber; z++ )
                        {
                            Vector3 pointToSample = centralNeuronWorldPosition + new Vector3( x * smallerNeuronsSamplingStep , 0 , z * smallerNeuronsSamplingStep );
                            pointToSample.y = TerrainSampler.Terrains[ terrainSampler.GetTerrainsIndexByPoint( pointToSample ) ].SampleHeight( pointToSample );

                            tile[ x , z ] = pointToSample;
                        }
                    }

                    // Changing the net (of the tile) in the Unity scene
                    terrainSampler.HorizontalNeuronLinksMatrix[ i , j ].GetComponentInChildren<MeshRenderer>().enabled = false;
                    terrainSampler.VerticalNeuronLinksMatrix[ i , j ].GetComponentInChildren<MeshRenderer>().enabled = false;
                }

                break;
        }

        return tile;
    }

    // Build the visual net for the single tile and classify the nodes
    public void BuildSingleTileNet( Vector3[,] tile , tiles tileType , Neuron neuron )
    {
        GameObject[,] neuronPointsTile = new GameObject[ smallerNeuronsNumber , smallerNeuronsNumber ];
        GameObject[,] horizontalLinksTile = new GameObject[ smallerNeuronsNumber , smallerNeuronsNumber ];
        GameObject[,] verticalLinksTile = new GameObject[ smallerNeuronsNumber , smallerNeuronsNumber ];

        GameObject neuronPoint = ( GameObject ) Resources.Load( "NeuronPoint" );
        GameObject neuronLink = ( GameObject ) Resources.Load( "NeuronLinkFather" );

        Vector3 horizontalLinkTileDirection = new Vector3();
        Vector3 verticalLinkTileDirection = new Vector3();

        // Smaller than the basic neurons
        int neuronPointScale = 10;
        Neuron rightNeuron;
        Neuron upNeuron;

        for ( int j = 0; j < smallerNeuronsNumber; j++ )
        {
            for ( int i = 0; i < smallerNeuronsNumber; i++ )
            {
                neuronPointsTile[ i , j ] = Instantiate( neuronPoint , new Vector3( 0 , 0 , 0 ) , Quaternion.identity );
                neuronPointsTile[ i , j ].transform.localScale = new Vector3( neuronPointScale , neuronPointScale , neuronPointScale );
                neuronPointsTile[ i , j ].transform.position = tile[ i , j ];

                horizontalLinksTile[ i , j ] = Instantiate( neuronLink , new Vector3( 0 , 0 , 0 ) , Quaternion.identity );
                verticalLinksTile[ i , j ] = Instantiate( neuronLink , new Vector3( 0 , 0 , 0 ) , Quaternion.identity );

                // Need better way
                if ( i < smallerNeuronsNumber - 1 && j < smallerNeuronsNumber - 1 )
                {
                    // Links Position
                    horizontalLinksTile[ i , j ].transform.position = (tile[ i + 1 , j ] + tile[ i , j ]) / 2;
                    verticalLinksTile[ i , j ].transform.position = (tile[ i , j + 1 ] + tile[ i , j ]) / 2;

                    // Links Direction
                    horizontalLinkTileDirection = (tile[ i + 1 , j ] - tile[ i , j ]) / 2;
                    verticalLinkTileDirection = (tile[ i , j + 1 ] - tile[ i , j ]) / 2;
                }

                #region Horizontal connection between tiles

                else if ( i == smallerNeuronsNumber - 1 && j < smallerNeuronsNumber - 1 )
                {
                    switch ( tileType )
                    {
                        case tiles.UPLEFT:

                            // Links Position
                            if ( neuron.UpRightTile != null )
                                horizontalLinksTile[ i , j ].transform.position = (neuron.UpRightTile[ 0 , j ] + tile[ i , j ]) / 2;
                            verticalLinksTile[ i , j ].transform.position = (tile[ i , j + 1 ] + tile[ i , j ]) / 2;

                            // Links Direction
                            if ( neuron.UpRightTile != null )
                                horizontalLinkTileDirection = (neuron.UpRightTile[ 0 , j ] - tile[ i , j ]) / 2;
                            verticalLinkTileDirection = (tile[ i , j + 1 ] - tile[ i , j ]) / 2;

                            // Links Scale
                            if ( neuron.UpRightTile != null )
                                horizontalLinksTile[ i , j ].transform.localScale = new Vector3( 5 , 5 , ((neuron.UpRightTile[ 0 , j ] - tile[ i , j ]) / 2).magnitude );
                            verticalLinksTile[ i , j ].transform.localScale = new Vector3( 5 , 5 , ((tile[ i , j + 1 ] - tile[ i , j ]) / 2).magnitude );

                            break;

                        case tiles.DOWNLEFT:

                            // Links Position
                            if ( neuron.DownRightTile != null )
                                horizontalLinksTile[ i , j ].transform.position = (neuron.DownRightTile[ 0 , j ] + tile[ i , j ]) / 2;
                            verticalLinksTile[ i , j ].transform.position = (tile[ i , j + 1 ] + tile[ i , j ]) / 2;

                            // Links Direction
                            if ( neuron.DownRightTile != null )
                                horizontalLinkTileDirection = (neuron.DownRightTile[ 0 , j ] - tile[ i , j ]) / 2;
                            verticalLinkTileDirection = (tile[ i , j + 1 ] - tile[ i , j ]) / 2;

                            // Links Scale
                            if ( neuron.DownRightTile != null )
                                horizontalLinksTile[ i , j ].transform.localScale = new Vector3( 5 , 5 , ((neuron.DownRightTile[ 0 , j ] - tile[ i , j ]) / 2).magnitude );
                            verticalLinksTile[ i , j ].transform.localScale = new Vector3( 5 , 5 , ((tile[ i , j + 1 ] - tile[ i , j ]) / 2).magnitude );

                            break;

                        case tiles.UPRIGHT:

                            // Check if we are at the border of the SOM matrix
                            if ( neuron.X < TerrainSampler.MatrixSideLength - 1 )
                            {
                                rightNeuron = terrainSampler.SoMap.GetNeuron( neuron.X + 1 , neuron.Y ) as Neuron;

                                // Links Position
                                if ( rightNeuron.UpRightTile != null )
                                    horizontalLinksTile[ i , j ].transform.position = (rightNeuron.UpRightTile[ 0 , j ] + tile[ i , j ]) / 2;
                                verticalLinksTile[ i , j ].transform.position = (tile[ i , j + 1 ] + tile[ i , j ]) / 2;

                                // Links Direction
                                if ( rightNeuron.UpRightTile != null )
                                    horizontalLinkTileDirection = (rightNeuron.UpRightTile[ 0 , j ] - tile[ i , j ]) / 2;
                                verticalLinkTileDirection = (tile[ i , j + 1 ] - tile[ i , j ]) / 2;

                                // Links Scale
                                if ( rightNeuron.UpRightTile != null )
                                    horizontalLinksTile[ i , j ].transform.localScale = new Vector3( 5 , 5 , ((rightNeuron.UpRightTile[ 0 , j ] - tile[ i , j ]) / 2).magnitude );
                                verticalLinksTile[ i , j ].transform.localScale = new Vector3( 5 , 5 , ((tile[ i , j + 1 ] - tile[ i , j ]) / 2).magnitude );
                            }

                            break;

                        case tiles.DOWNRIGHT:

                            // Check if we are at the border of the SOM matrix
                            if ( neuron.X < TerrainSampler.MatrixSideLength - 1 )
                            {
                                rightNeuron = terrainSampler.SoMap.GetNeuron( neuron.X + 1 , neuron.Y ) as Neuron;

                                // Links Position
                                if ( rightNeuron.DownRightTile != null )
                                    horizontalLinksTile[ i , j ].transform.position = (neuron.DownRightTile[ 0 , j ] + tile[ i , j ]) / 2;
                                verticalLinksTile[ i , j ].transform.position = (tile[ i , j + 1 ] + tile[ i , j ]) / 2;

                                // Links Direction
                                if ( rightNeuron.DownRightTile != null )
                                    horizontalLinkTileDirection = (rightNeuron.DownRightTile[ 0 , j ] - tile[ i , j ]) / 2;
                                verticalLinkTileDirection = (tile[ i , j + 1 ] - tile[ i , j ]) / 2;

                                // Links Scale
                                if ( rightNeuron.DownRightTile != null )
                                    horizontalLinksTile[ i , j ].transform.localScale = new Vector3( 5 , 5 , ((neuron.DownRightTile[ 0 , j ] - tile[ i , j ]) / 2).magnitude );
                                verticalLinksTile[ i , j ].transform.localScale = new Vector3( 5 , 5 , ((tile[ i , j + 1 ] - tile[ i , j ]) / 2).magnitude );
                            }

                            break;
                    }
                }
                #endregion

                #region Vertical connection between tiles

                else if ( i < smallerNeuronsNumber - 1 && j == smallerNeuronsNumber - 1 )
                {
                    switch ( tileType )
                    {
                        case tiles.UPLEFT:

                            // Check if we are at the border of the SOM matrix
                            if ( neuron.Y < TerrainSampler.MatrixSideLength - 1 )
                            {
                                upNeuron = terrainSampler.SoMap.GetNeuron( neuron.X , neuron.Y + 1 ) as Neuron;

                                // Links Position
                                if ( upNeuron.UpLeftTile != null )
                                    verticalLinksTile[ i , j ].transform.position = (upNeuron.UpLeftTile[ i , 0 ] + tile[ i , j ]) / 2;
                                horizontalLinksTile[ i , j ].transform.position = (tile[ i + 1 , j ] + tile[ i , j ]) / 2;

                                // Links Direction
                                if ( upNeuron.UpLeftTile != null )
                                    verticalLinkTileDirection = (upNeuron.UpLeftTile[ i , 0 ] - tile[ i , j ]) / 2;
                                horizontalLinkTileDirection = (tile[ i + 1 , j ] - tile[ i , j ]) / 2;

                                // Links Scale
                                if ( upNeuron.UpLeftTile != null )
                                    verticalLinksTile[ i , j ].transform.localScale = new Vector3( 5 , 5 , ((upNeuron.UpLeftTile[ i , 0 ] - tile[ i , j ]) / 2).magnitude );
                                horizontalLinksTile[ i , j ].transform.localScale = new Vector3( 5 , 5 , ((tile[ i + 1 , j ] - tile[ i , j ]) / 2).magnitude );
                            }

                            break;

                        case tiles.DOWNLEFT:

                            // Links Position
                            if ( neuron.UpLeftTile != null )
                                verticalLinksTile[ i , j ].transform.position = (neuron.UpLeftTile[ i , 0 ] + tile[ i , j ]) / 2;
                            horizontalLinksTile[ i , j ].transform.position = (tile[ i + 1 , j ] + tile[ i , j ]) / 2;

                            // Links Direction
                            if ( neuron.UpLeftTile != null )
                                verticalLinkTileDirection = (neuron.UpLeftTile[ i , 0 ] - tile[ i , j ]) / 2;
                            horizontalLinkTileDirection = (tile[ i + 1 , j ] - tile[ i , j ]) / 2;

                            // Links Scale
                            if ( neuron.UpLeftTile != null )
                                verticalLinksTile[ i , j ].transform.localScale = new Vector3( 5 , 5 , ((neuron.UpLeftTile[ i , 0 ] - tile[ i , j ]) / 2).magnitude );
                            horizontalLinksTile[ i , j ].transform.localScale = new Vector3( 5 , 5 , ((tile[ i + 1 , j ] - tile[ i , j ]) / 2).magnitude );

                            break;

                        case tiles.UPRIGHT:

                            // Check if we are at the border of the SOM matrix
                            if ( neuron.Y < TerrainSampler.MatrixSideLength - 1 )
                            {
                                upNeuron = terrainSampler.SoMap.GetNeuron( neuron.X , neuron.Y + 1 ) as Neuron;

                                // Links Position
                                if ( upNeuron.UpRightTile != null )
                                    verticalLinksTile[ i , j ].transform.position = (upNeuron.UpRightTile[ i , 0 ] + tile[ i , j ]) / 2;
                                horizontalLinksTile[ i , j ].transform.position = (tile[ i + 1 , j ] + tile[ i , j ]) / 2;

                                // Links Direction
                                if ( upNeuron.UpRightTile != null )
                                    verticalLinkTileDirection = (upNeuron.UpRightTile[ i , 0 ] - tile[ i , j ]) / 2;
                                horizontalLinkTileDirection = (tile[ i + 1 , j ] - tile[ i , j ]) / 2;

                                // Links Scale
                                if ( upNeuron.UpRightTile != null )
                                    verticalLinksTile[ i , j ].transform.localScale = new Vector3( 5 , 5 , ((upNeuron.UpRightTile[ i , 0 ] - tile[ i , j ]) / 2).magnitude );
                                horizontalLinksTile[ i , j ].transform.localScale = new Vector3( 5 , 5 , ((tile[ i + 1 , j ] - tile[ i , j ]) / 2).magnitude );
                            }

                            break;

                        case tiles.DOWNRIGHT:

                            // Links Position
                            if ( neuron.UpRightTile != null )
                                verticalLinksTile[ i , j ].transform.position = (neuron.UpRightTile[ i , 0 ] + tile[ i , j ]) / 2;
                            horizontalLinksTile[ i , j ].transform.position = (tile[ i + 1 , j ] + tile[ i , j ]) / 2;

                            // Links Direction
                            if ( neuron.UpRightTile != null )
                                verticalLinkTileDirection = (neuron.UpRightTile[ i , 0 ] - tile[ i , j ]) / 2;
                            horizontalLinkTileDirection = (tile[ i + 1 , j ] - tile[ i , j ]) / 2;

                            // Links Scale
                            if ( neuron.UpRightTile != null )
                                verticalLinksTile[ i , j ].transform.localScale = new Vector3( 5 , 5 , ((neuron.UpRightTile[ i , 0 ] - tile[ i , j ]) / 2).magnitude );
                            horizontalLinksTile[ i , j ].transform.localScale = new Vector3( 5 , 5 , ((tile[ i + 1 , j ] - tile[ i , j ]) / 2).magnitude );

                            break;
                    }
                }
                #endregion

                #region Upper right corner connection between tiles

                #region Vertical connection

                else if ( i == smallerNeuronsNumber - 1 && j == smallerNeuronsNumber - 1 )
                {
                    switch ( tileType )
                    {
                        case tiles.UPLEFT:

                            // Check if we are at the border of the SOM matrix
                            if ( neuron.Y < TerrainSampler.MatrixSideLength - 1 )
                            {
                                upNeuron = terrainSampler.SoMap.GetNeuron( neuron.X , neuron.Y + 1 ) as Neuron;

                                // Links Position
                                if ( upNeuron.UpLeftTile != null )
                                    verticalLinksTile[ i , j ].transform.position = (upNeuron.UpLeftTile[ i , 0 ] + tile[ i , j ]) / 2;
                                horizontalLinksTile[ i , j ].transform.position = (neuron.UpRightTile[ 0 , j ] + tile[ i , j ]) / 2;

                                // Links Direction
                                if ( upNeuron.UpLeftTile != null )
                                    verticalLinkTileDirection = (upNeuron.UpLeftTile[ i , 0 ] - tile[ i , j ]) / 2;
                                horizontalLinkTileDirection = (neuron.UpRightTile[ 0 , j ] - tile[ i , j ]) / 2;

                                // Links Scale
                                if ( upNeuron.UpLeftTile != null )
                                    verticalLinksTile[ i , j ].transform.localScale = new Vector3( 5 , 5 , ((upNeuron.UpLeftTile[ i , 0 ] - tile[ i , j ]) / 2).magnitude );
                                horizontalLinksTile[ i , j ].transform.localScale = new Vector3( 5 , 5 , ((neuron.UpRightTile[ 0 , j ] - tile[ i , j ]) / 2).magnitude );
                            }

                            break;

                        case tiles.DOWNLEFT:

                            // Links Position
                            if ( neuron.UpLeftTile != null )
                                verticalLinksTile[ i , j ].transform.position = (neuron.UpLeftTile[ i , 0 ] + tile[ i , j ]) / 2;
                            horizontalLinksTile[ i , j ].transform.position = (neuron.DownRightTile[ 0 , j ] + tile[ i , j ]) / 2;

                            // Links Direction
                            if ( neuron.UpLeftTile != null )
                                verticalLinkTileDirection = (neuron.UpLeftTile[ i , 0 ] - tile[ i , j ]) / 2;
                            horizontalLinkTileDirection = (neuron.DownRightTile[ 0 , j ] - tile[ i , j ]) / 2;

                            // Links Scale
                            if ( neuron.UpLeftTile != null )
                                verticalLinksTile[ i , j ].transform.localScale = new Vector3( 5 , 5 , ((neuron.UpLeftTile[ i , 0 ] - tile[ i , j ]) / 2).magnitude );
                            horizontalLinksTile[ i , j ].transform.localScale = new Vector3( 5 , 5 , ((neuron.DownRightTile[ 0 , j ] - tile[ i , j ]) / 2).magnitude );

                            break;

                        case tiles.UPRIGHT:

                            // Check if we are at the border of the SOM matrix
                            if ( neuron.Y < TerrainSampler.MatrixSideLength - 1 && neuron.X < TerrainSampler.MatrixSideLength - 1 )
                            {
                                upNeuron = terrainSampler.SoMap.GetNeuron( neuron.X , neuron.Y + 1 ) as Neuron;
                                rightNeuron = terrainSampler.SoMap.GetNeuron( neuron.X + 1 , neuron.Y ) as Neuron;


                                // Links Position
                                if ( upNeuron.UpRightTile != null && rightNeuron.UpRightTile != null )
                                {
                                    verticalLinksTile[ i , j ].transform.position = (upNeuron.UpRightTile[ i , 0 ] + tile[ i , j ]) / 2;
                                    horizontalLinksTile[ i , j ].transform.position = (rightNeuron.UpRightTile[ 0 , j ] + tile[ i , j ]) / 2;
                                }

                                // Links Direction
                                if ( upNeuron.UpRightTile != null && rightNeuron.UpRightTile != null )
                                {
                                    verticalLinkTileDirection = (upNeuron.UpRightTile[ i , 0 ] - tile[ i , j ]) / 2;
                                    horizontalLinkTileDirection = (rightNeuron.UpRightTile[ 0 , j ] - tile[ i , j ]) / 2;
                                }

                                // Links Scale
                                if ( upNeuron.UpRightTile != null && rightNeuron.UpRightTile != null )
                                {
                                    verticalLinksTile[ i , j ].transform.localScale = new Vector3( 5 , 5 , ((upNeuron.UpRightTile[ i , 0 ] - tile[ i , j ]) / 2).magnitude );
                                    horizontalLinksTile[ i , j ].transform.localScale = new Vector3( 5 , 5 , ((rightNeuron.UpRightTile[ 0 , j ] - tile[ i , j ]) / 2).magnitude );
                                }
                            }

                            break;

                        case tiles.DOWNRIGHT:

                            // Check if we are at the border of the SOM matrix
                            if ( neuron.X < TerrainSampler.MatrixSideLength - 1 )
                            {
                                rightNeuron = terrainSampler.SoMap.GetNeuron( neuron.X + 1 , neuron.Y ) as Neuron;

                                // Links Position
                                if ( neuron.UpRightTile != null && rightNeuron.DownRightTile != null )
                                {
                                    verticalLinksTile[ i , j ].transform.position = (neuron.UpRightTile[ i , 0 ] + tile[ i , j ]) / 2;
                                    horizontalLinksTile[ i , j ].transform.position = (rightNeuron.DownRightTile[ 0 , j ] + tile[ i , j ]) / 2;
                                }

                                // Links Direction
                                if ( neuron.UpRightTile != null && rightNeuron.DownRightTile != null )
                                {
                                    verticalLinkTileDirection = (neuron.UpRightTile[ i , 0 ] - tile[ i , j ]) / 2;
                                    horizontalLinkTileDirection = (rightNeuron.DownRightTile[ 0 , j ] - tile[ i , j ]) / 2;
                                }

                                // Links Scale
                                if ( neuron.UpRightTile != null && rightNeuron.DownRightTile != null )
                                {
                                    verticalLinksTile[ i , j ].transform.localScale = new Vector3( 5 , 5 , ((neuron.UpRightTile[ i , 0 ] - tile[ i , j ]) / 2).magnitude );
                                    horizontalLinksTile[ i , j ].transform.localScale = new Vector3( 5 , 5 , ((rightNeuron.DownRightTile[ 0 , j ] - tile[ i , j ]) / 2).magnitude );
                                }
                            }

                            break;
                    }
                }
                #endregion

                #region Horizontal connection

                else if ( i == smallerNeuronsNumber - 1 && j == smallerNeuronsNumber - 1 )
                {
                    switch ( tileType )
                    {
                        case tiles.UPLEFT:

                            // Check if we are at the border of the SOM matrix
                            if ( neuron.Y < TerrainSampler.MatrixSideLength - 1 )
                            {
                                upNeuron = terrainSampler.SoMap.GetNeuron( neuron.X , neuron.Y + 1 ) as Neuron;

                                // Links Position
                                if ( neuron.UpRightTile != null && upNeuron.UpLeftTile != null )
                                {
                                    horizontalLinksTile[ i , j ].transform.position = (neuron.UpRightTile[ 0 , j ] + tile[ i , j ]) / 2;
                                    verticalLinksTile[ i , j ].transform.position = (upNeuron.UpLeftTile[ i , 0 ] + tile[ i , j ]) / 2;
                                }

                                // Links Direction
                                if ( neuron.UpRightTile != null && upNeuron.UpLeftTile != null )
                                {
                                    horizontalLinkTileDirection = (neuron.UpRightTile[ 0 , j ] - tile[ i , j ]) / 2;
                                    verticalLinkTileDirection = (upNeuron.UpLeftTile[ i , 0 ] - tile[ i , j ]) / 2;
                                }

                                // Links Scale
                                if ( neuron.UpRightTile != null && upNeuron.UpLeftTile != null )
                                {
                                    horizontalLinksTile[ i , j ].transform.localScale = new Vector3( 5 , 5 , ((neuron.UpRightTile[ 0 , j ] - tile[ i , j ]) / 2).magnitude );
                                    verticalLinksTile[ i , j ].transform.localScale = new Vector3( 5 , 5 , ((upNeuron.UpLeftTile[ i , 0 ] - tile[ i , j ]) / 2).magnitude );
                                }
                            }

                            break;

                        case tiles.DOWNLEFT:

                            // Links Position
                            if ( neuron.DownRightTile != null )
                                horizontalLinksTile[ i , j ].transform.position = (neuron.DownRightTile[ 0 , j ] + tile[ i , j ]) / 2;
                            verticalLinksTile[ i , j ].transform.position = (neuron.UpLeftTile[ i , 0 ] + tile[ i , j ]) / 2;

                            // Links Direction
                            if ( neuron.DownRightTile != null )
                                horizontalLinkTileDirection = (neuron.DownRightTile[ 0 , j ] - tile[ i , j ]) / 2;
                            verticalLinkTileDirection = (neuron.UpLeftTile[ i , 0 ] - tile[ i , j ]) / 2;

                            // Links Scale
                            if ( neuron.DownRightTile != null )
                                horizontalLinksTile[ i , j ].transform.localScale = new Vector3( 5 , 5 , ((neuron.DownRightTile[ 0 , j ] - tile[ i , j ]) / 2).magnitude );
                            verticalLinksTile[ i , j ].transform.localScale = new Vector3( 5 , 5 , ((neuron.UpLeftTile[ i , 0 ] - tile[ i , j ]) / 2).magnitude );

                            break;

                        case tiles.UPRIGHT:

                            // Check if we are at the border of the SOM matrix
                            if ( neuron.X < TerrainSampler.MatrixSideLength - 1 && neuron.Y < TerrainSampler.MatrixSideLength - 1 )
                            {
                                rightNeuron = terrainSampler.SoMap.GetNeuron( neuron.X + 1 , neuron.Y ) as Neuron;
                                upNeuron = terrainSampler.SoMap.GetNeuron( neuron.X , neuron.Y + 1 ) as Neuron;

                                // Links Position
                                if ( rightNeuron.UpRightTile != null && upNeuron.UpRightTile != null )
                                {
                                    horizontalLinksTile[ i , j ].transform.position = (rightNeuron.UpRightTile[ 0 , j ] + tile[ i , j ]) / 2;
                                    verticalLinksTile[ i , j ].transform.position = (upNeuron.UpRightTile[ i , 0 ] + tile[ i , j ]) / 2;
                                }

                                // Links Direction
                                if ( rightNeuron.UpRightTile != null && upNeuron.UpRightTile != null )
                                {
                                    horizontalLinkTileDirection = (rightNeuron.UpRightTile[ 0 , j ] - tile[ i , j ]) / 2;
                                    verticalLinkTileDirection = (upNeuron.UpRightTile[ i , 0 ] - tile[ i , j ]) / 2;
                                }

                                // Links Scale
                                if ( rightNeuron.UpRightTile != null && upNeuron.UpRightTile != null )
                                {
                                    horizontalLinksTile[ i , j ].transform.localScale = new Vector3( 5 , 5 , ((rightNeuron.UpRightTile[ 0 , j ] - tile[ i , j ]) / 2).magnitude );
                                    verticalLinksTile[ i , j ].transform.localScale = new Vector3( 5 , 5 , ((upNeuron.UpRightTile[ i , 0 ] - tile[ i , j ]) / 2).magnitude );
                                }
                            }

                            break;

                        case tiles.DOWNRIGHT:

                            // Check if we are at the border of the SOM matrix
                            if ( neuron.X < TerrainSampler.MatrixSideLength - 1 )
                            {
                                rightNeuron = terrainSampler.SoMap.GetNeuron( neuron.X + 1 , neuron.Y ) as Neuron;

                                // Links Position
                                if ( rightNeuron.DownRightTile != null )
                                    horizontalLinksTile[ i , j ].transform.position = (neuron.DownRightTile[ 0 , j ] + tile[ i , j ]) / 2;
                                verticalLinksTile[ i , j ].transform.position = (neuron.UpRightTile[ i , 0 ] + tile[ i , j ]) / 2;

                                // Links Direction
                                if ( rightNeuron.DownRightTile != null )
                                    horizontalLinkTileDirection = (rightNeuron.DownRightTile[ 0 , j ] - tile[ i , j ]) / 2;
                                verticalLinkTileDirection = (neuron.UpRightTile[ i , 0 ] - tile[ i , j ]) / 2;

                                // Links Scale
                                if ( rightNeuron.DownRightTile != null )
                                    horizontalLinksTile[ i , j ].transform.localScale = new Vector3( 5 , 5 , ((neuron.DownRightTile[ 0 , j ] - tile[ i , j ]) / 2).magnitude );
                                verticalLinksTile[ i , j ].transform.localScale = new Vector3( 5 , 5 , ((neuron.UpRightTile[ i , 0 ] - tile[ i , j ]) / 2).magnitude );
                            }

                            break;
                    }
                }
                #endregion

                #endregion

                // TODO Color the smaller neuron point with the same method used for big neurons

                // Links Rotation
                horizontalLinksTile[ i , j ].transform.rotation = Quaternion.LookRotation( horizontalLinkTileDirection ,
                    horizontalLinksTile[ i , j ].transform.forward );
                verticalLinksTile[ i , j ].transform.rotation = Quaternion.LookRotation( verticalLinkTileDirection ,
                    verticalLinksTile[ i , j ].transform.forward );

                // Need better way
                if ( i < smallerNeuronsNumber - 1 && j < smallerNeuronsNumber - 1 )
                {
                    // Links Scale
                    horizontalLinksTile[ i , j ].transform.localScale = new Vector3( 5 , 5 , ((tile[ i + 1 , j ] - tile[ i , j ]) / 2).magnitude );
                    verticalLinksTile[ i , j ].transform.localScale = new Vector3( 5 , 5 , ((tile[ i , j + 1 ] - tile[ i , j ]) / 2).magnitude );
                }

                // Raising everything
                Vector3 raisedPosition = neuronPointsTile[ i , j ].transform.position;
                raisedPosition.y += TerrainSampler.Som3DNetHeight;
                neuronPointsTile[ i , j ].transform.position = raisedPosition;

                raisedPosition = horizontalLinksTile[ i , j ].transform.position;
                raisedPosition.y += TerrainSampler.Som3DNetHeight;
                horizontalLinksTile[ i , j ].transform.position = raisedPosition;

                raisedPosition = verticalLinksTile[ i , j ].transform.position;
                raisedPosition.y += TerrainSampler.Som3DNetHeight;
                verticalLinksTile[ i , j ].transform.position = raisedPosition;
            }
        }
    }
}
