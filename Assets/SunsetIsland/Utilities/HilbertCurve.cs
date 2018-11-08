using System;
using UnityEngine;

namespace Assets.SunsetIsland.Utilities
{
	/// <summary>
	///     Convert between Hilbert index and N-dimensional points.
	///     The Hilbert index is expressed as an array of transposed bits.
	///     Example: 5 bits for each of n=3 coordinates.
	///     15-bit Hilbert integer = A B C D E F G H I J K L M N O is stored
	///     as its Transpose                        ^
	///     X[0] = A D G J M                    X[1]|  7
	///     X[1] = B E H K N        <------->       | /X[2]
	///     X[2] = C F I L O                   axes |/
	///     high low                                0------> X[0]
	///     NOTE: This algorithm is derived from work done by John Skilling and published in "Programming the Hilbert curve".
	///     (c) 2004 American Institute of Physics.
	/// </summary>
	public static class HilbertCurve
    {
        private static Vector3Uint BuildTranspose(uint index, int bits)
        {
            var transpose = new Vector3Uint();
            var mask = (2U << (bits - 1)) - 1;
            uint row = 0;
            for (var j = 0; j < bits; ++j)
            {
                var source = j * 3 + 2;
                row |= ((index >> source) & 1) << j;
            }

            transpose.x = row & mask;
            row = 0;
            for (var j = 0; j < bits; ++j)
            {
                var source = j * 3 + 1;
                row |= ((index >> source) & 1) << j;
            }

            transpose.y = row & mask;
            row = 0;
            for (var j = 0; j < bits; ++j)
            {
                var source = j * 3;
                row |= ((index >> source) & 1) << j;
            }

            transpose.z = row & mask;

            return transpose;
        }

        public static Vector3Int HilbertAxes(uint index, int bits = 5)
        {
            const int dimensions = 3;
            var vector = BuildTranspose(index, 5);
            uint q;
            // Gray decode by H ^ (H/2)
            var grayCode = vector.z >> 1;
            vector.z ^= vector.y;
            vector.y ^= vector.x;
            vector.x ^= grayCode;

            // Undo excess work
            var n = 2U << (bits - 1);
            for (q = 2; q != n; q <<= 1)
            {
                var p = q - 1;
                int dimension;
                for (dimension = dimensions - 1; dimension >= 0; dimension--)
                    if ((vector[dimension] & q) != 0U)
                    {
                        vector.x ^= p; // invert
                    }
                    else
                    {
                        grayCode = (vector.x ^ vector[dimension]) & p;
                        vector.x ^= grayCode;
                        vector[dimension] ^= grayCode;
                    }
            } // exchange

            return vector;
        }

        private struct Vector3Uint
        {
            public uint x;
            public uint y;
            public uint z;

            public uint this[int i]
            {
                get
                {
                    switch (i)
                    {
                        case 0: return x;
                        case 1: return y;
                        case 2: return z;
                        default: throw new IndexOutOfRangeException();
                    }
                }
                set
                {
                    switch (i)
                    {
                        case 0:
                            x = value;
                            break;
                        case 1:
                            y = value;
                            break;
                        case 2:
                            z = value;
                            break;
                        default: throw new IndexOutOfRangeException();
                    }
                }
            }

            public static implicit operator Vector3Int(Vector3Uint source)
            {
                return new Vector3Int((int) source.x, (int) source.y, (int) source.z);
            }
        }
    }
}