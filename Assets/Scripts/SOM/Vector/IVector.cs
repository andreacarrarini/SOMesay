using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
This class models all the vectors in the system. It covers the input layer, weighted connections and the input itself.
It is modeled as a list of double values, with one extra functionality – EuclidianDistance.
*/

namespace SOM.VectorNamespace
{
    public interface IVector : IList<double>
    {
        double EuclidianDistance( IVector vector );                                                 // This function is calculating the Euclidean distance between two vectors
    }
}
