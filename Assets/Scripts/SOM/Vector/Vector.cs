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

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace SOM.VectorNamespace
{
    [Serializable]
    public class Vector : List<double>, IVector
    {
        public double EuclidianDistance( IVector vector )
        {
            if ( vector.Count != Count )
                throw new ArgumentException( "Not the same size" );

            return this.Select( x => System.Math.Pow( x - vector[ this.IndexOf( x ) ], 2 ) ).Sum();
        }

        /*
         * Method used to clone a Vector avoiding to pass it by reference
         */
        public static object DeepClone( object obj )
        {
            object objResult = null;
            using ( MemoryStream ms = new MemoryStream() )
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize( ms, obj );

                ms.Position = 0;
                objResult = bf.Deserialize( ms );
            }
            return objResult;
        }
    }
}
