﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Aardvark.Base
{
    /// <summary>
    /// A 2^Exponent sized cube positioned at (X,Y,Z) * 2^Exponent.
    /// </summary>
    [DataContract]
    public partial struct Cell : IEquatable<Cell>
    {
        /// <summary>
        /// Unit cell (0, 0, 0, 0) -> Box3d[(0.0, 0.0, 0.0), (1.0, 1.0, 1.0)]
        /// </summary>
        public static Cell Unit => new Cell(0L, 0L, 0L, 0);

        /// <summary>
        /// Special value denoting "invalid cell".
        /// </summary>
        public static Cell Invalid => new Cell(long.MinValue, long.MinValue, long.MinValue, int.MinValue);

        /// <summary>
        /// </summary>
        [DataMember]
        public readonly long X;

        /// <summary>
        /// </summary>
        [DataMember]
        public readonly long Y;

        /// <summary>
        /// </summary>
        [DataMember]
        public readonly long Z;

        /// <summary>
        /// </summary>
        [DataMember(Name = "E")]
        public readonly int Exponent;

        #region constructors

        /// <summary>
        /// Creates a 2^exponent sized cube positioned at (x,y,z) * 2^exponent.
        /// </summary>
        public Cell(long x, long y, long z, int exponent)
        {
            X = x; Y = y; Z = z; Exponent = exponent;
        }

        /// <summary>
        /// Creates copy of cell.
        /// </summary>
        public Cell(Cell cell)
        {
            X = cell.X; Y = cell.Y; Z = cell.Z; Exponent = cell.Exponent;
        }

        /// <summary>
        /// Cell with min corner at index*2^exponent and dimension 2^exponent.
        /// </summary>
        public Cell(V3l index, int exponent) : this(index.X, index.Y, index.Z, exponent) { }

        /// <summary>
        /// Special cell, which is centered at origin, with dimension 2^exponent.
        /// </summary>
        public Cell(int exponent)
        {
            if (exponent == int.MinValue)
            {
                X = long.MinValue; Y = long.MinValue; Z = long.MinValue; Exponent = exponent;
            }
            else
            {
                X = long.MaxValue; Y = long.MaxValue; Z = long.MaxValue; Exponent = exponent;
            }
        }

        /// <summary>
        /// Smallest cell that contains given box.
        /// </summary>
        public Cell(Box3f box) : this(new Box3d(box)) { }

        /// <summary>
        /// Smallest cell that contains given points.
        /// </summary>
        public Cell(V3f[] ps) : this(new Box3f(ps)) { }

        /// <summary>
        /// Smallest cell that contains given points.
        /// </summary>
        public Cell(V3d[] ps) : this(new Box3d(ps)) { }

        /// <summary>
        /// Smallest cell that contains given points.
        /// </summary>
        public Cell(IEnumerable<V3f> ps) : this(new Box3f(ps)) { }

        /// <summary>
        /// Smallest cell that contains given points.
        /// </summary>
        public Cell(IEnumerable<V3d> ps) : this(new Box3d(ps)) { }

        /// <summary>
        /// Smallest cell that contains given point.
        /// </summary>
        public Cell(V3d p) : this(new Box3d(p)) { }

        /// <summary>
        /// Smallest cell that contains given box, 
        /// where box.Min and box.Max are both interpreted as inclusive
        /// (whereas a cell's maximum is defined as exclusive).
        /// </summary>
        public Cell(Box3d box)
        {
            if (!box.IsValid) throw new InvalidOperationException();

            // case 1: contains origin
            if (box.Min.X < 0.0 && box.Max.X >= 0.0 ||
                box.Min.Y < 0.0 && box.Max.Y >= 0.0 ||
                box.Min.Z < 0.0 && box.Max.Z >= 0.0)
            {
                X = Y = Z = long.MaxValue;
                Exponent = Math.Max(box.Min.NormMax, box.Max.NormMax).Log2CeilingInt() + 1;
            }
            else // case 2: doesn't contain origin
            {
                Exponent = (box.Min == box.Max)
                        ? (box.Min.NormMax / (long.MaxValue >> 1)).Log2CeilingInt()
                        : box.Size.NormMax.Log2CeilingInt()
                        ;
                var s = Math.Pow(2.0, Exponent);
                var a = (box.Min * (1.0 / s)).Floor();

                X = (long)a.X;
                Y = (long)a.Y;
                Z = (long)a.Z;

                while (box.Max.X >= X * s + s || box.Max.Y >= Y * s + s || box.Max.Z >= Z * s + s)
                {
                    X >>= 1; Y >>= 1; Z >>= 1; ++Exponent;
                    s *= 2.0;
                }
            }
        }

        /// <summary>
        /// Common root of a and b.
        /// </summary>
        public Cell(Cell a, Cell b) : this(GetCommonRoot(a, b)) { }

        /// <summary>
        /// Common root of cells.
        /// </summary>
        public Cell(params Cell[] cells) : this(GetCommonRoot(cells)) { }

        #endregion

        /// <summary>
        /// </summary>
        [JsonIgnore]
        public V3l XYZ => new V3l(X, Y, Y);

        /// <summary>
        /// Gets whether this cell is a special cell centered at origin.
        /// </summary>
        [JsonIgnore]
        public bool IsCenteredAtOrigin => X == long.MaxValue && Y == long.MaxValue && Z == long.MaxValue;

        /// <summary>
        /// Returns true if this is the special invalid cell.
        /// </summary>
        [JsonIgnore]
        public bool IsInvalid => X == long.MinValue && Y == long.MinValue && Z == long.MinValue && Exponent == int.MinValue;

        /// <summary>
        /// Returns true if this is NOT the special invalid cell.
        /// </summary>
        [JsonIgnore]
        public bool IsValid => X != long.MinValue || Y != long.MinValue || Z != long.MinValue || Exponent != int.MinValue;

        /// <summary>
        /// Returns true if the cell completely contains the other cell.
        /// A cell contains itself.
        /// </summary>
        public bool Contains(Cell other)
        {
            if (other.Exponent > Exponent) return false;
            if (other.Exponent == Exponent) return other.X == X && other.Y == Y && other.Z == Z;

            // always true from here on:
            //   Exponent > other.Exponent
            //   other.Exponent < Exponent

            if (IsCenteredAtOrigin)
            {
                if (other.IsCenteredAtOrigin)
                {
                    return true;
                }
                else
                {
                    // bring other into scale of this
                    var d = (Exponent - 1) - other.Exponent;

                    // right-shift with long argument only uses low-order 6 bits
                    // https://docs.microsoft.com/en-us/dotnet/articles/csharp/language-reference/operators/right-shift-operator
                    if (d > 63) d = 63;

                    var x = other.X >> d;
                    var y = other.Y >> d;
                    var z = other.Z >> d;
                    return x >= -1 && x <= 0 && y >= -1 && y <= 0 && z >= -1 && z <= 0;
                }
            }
            else
            {
                if (other.IsCenteredAtOrigin)
                {
                    return false;
                }
                else
                {
                    // bring other into scale of this
                    var d = Exponent - other.Exponent;

                    // right-shift with long argument only uses low-order 6 bits
                    // https://docs.microsoft.com/en-us/dotnet/articles/csharp/language-reference/operators/right-shift-operator
                    if (d > 63) d = 63;

                    var x = other.X >> d;
                    var y = other.Y >> d;
                    var z = other.Z >> d;
                    return x == X && y == Y && z == Z;
                }
            }
        }

        /// <summary>
        /// Returns true if two cells intersect each other (or one contains the other).
        /// Cells DO NOT intersect if only touching from the outside.
        /// A cell intersects itself.
        /// </summary>
        public bool Intersects(Cell other)
        {
            if (X == other.X && Y == other.Y && Z == other.Z && Exponent == other.Exponent) return true;
            return BoundingBox.Intersects(other.BoundingBox);
        }

        /// <summary>
        /// Gets indices of the 8 subcells.
        /// </summary>
        [JsonIgnore]
        public Cell[] Children
        {
            get
            {
                var x0 = X << 1; var y0 = Y << 1; var z0 = Z << 1;
                if (IsCenteredAtOrigin) { x0++; y0++; z0++; }
                var x1 = x0 + 1; var y1 = y0 + 1; var z1 = z0 + 1;
                var e = Exponent - 1;
                return new[]
                {
                    new Cell(x0, y0, z0, e),
                    new Cell(x1, y0, z0, e),
                    new Cell(x0, y1, z0, e),
                    new Cell(x1, y1, z0, e),
                    new Cell(x0, y0, z1, e),
                    new Cell(x1, y0, z1, e),
                    new Cell(x0, y1, z1, e),
                    new Cell(x1, y1, z1, e),
                };
            }
        }

        /// <summary>
        /// Gets parent cell.
        /// </summary>
        [JsonIgnore]
        public Cell Parent => IsCenteredAtOrigin ? new Cell(Exponent + 1) : new Cell(X >> 1, Y >> 1, Z >> 1, Exponent + 1);

        /// <summary>
        /// True if one corner of this cell touches the origin.
        /// Centered cells DO NOT touch the origin.
        /// </summary>
        [JsonIgnore]
        public bool TouchesOrigin => !IsCenteredAtOrigin && (X == -1 || X == 0) && (Y == -1 || Y == 0) && (Z == -1 || Z == 0);

        /// <summary>
        /// Gets cell's bounds.
        /// </summary>
        [JsonIgnore]
        public Box3d BoundingBox => ComputeBoundingBox(X, Y, Z, Exponent);

        private static Box3d ComputeBoundingBox(long x, long y, long z, int e)
        {
            var d = Math.Pow(2.0, e);
            var isCenteredAtOrigin = x == long.MaxValue && y == long.MaxValue && z == long.MaxValue;
            var min = isCenteredAtOrigin ? new V3d(-0.5 * d) : new V3d(x * d, y * d, z * d);
            return Box3d.FromMinAndSize(min, new V3d(d, d, d));
        }

        /// <summary>
        /// Computes cell's center.
        /// </summary>
        public V3d GetCenter() => BoundingBox.Center;

        /// <summary>
        /// Octant 0-7.
        /// 0th, 1st and 2nd bit encodes x-, y-, z-axis, respectively.
        /// E.g. 0 is octant at origin, 7 is octant oposite from origin.
        /// </summary>
        public Cell GetOctant(int i)
        {
            if (i < 0 || i > 7) throw new IndexOutOfRangeException();

            if (IsCenteredAtOrigin)
            {
                return new Cell(
                    (i & 1) == 0 ? -1 : 0,
                    (i & 2) == 0 ? -1 : 0,
                    (i & 4) == 0 ? -1 : 0,
                    Exponent - 1
                    );
            }
            else
            {
                return new Cell(
                    (X << 1) + ((i & 1) == 0 ? 0 : 1),
                    (Y << 1) + ((i & 2) == 0 ? 0 : 1),
                    (Z << 1) + ((i & 4) == 0 ? 0 : 1),
                    Exponent - 1
                    );
            }
        }

        /// <summary>
        /// Returns the octant of this cell, in which the other cell is contained.
        /// If the other cell is not contained in any octant, then no value is returned.
        /// </summary>
        public int? GetOctant(Cell other)
        {
            // if other cell is bigger or same size, 
            // then it cannot be contained in any octant
            if (other.Exponent >= Exponent) return null;

            if (IsCenteredAtOrigin)
            {
                if (other.IsCenteredAtOrigin) return null;

                // scale up other to scale of this cell's octants
                // where coord values 0 or 1 mean left or right,
                // and all other values mean that the other cell is outside
                other <<= Exponent - other.Exponent - 1;
                var o = 0;
                if (other.X == 0) o  = 1; else if (other.X != -1) return null;
                if (other.Y == 0) o |= 2; else if (other.Y != -1) return null;
                if (other.Z == 0) o |= 4; else if (other.Z != -1) return null;
                return o;
            }
            else
            {
                if (other.IsCenteredAtOrigin) return null;

                // scale up other to scale of this cell's octants,
                // where coord values -1 or 0 mean left or right,
                // and all other values mean that the other cell is outside
                other <<= Exponent - other.Exponent - 1;
                var x = X << 1;
                var y = Y << 1;
                var z = Z << 1;
                var o = 0;
                if (other.X == x + 1) o  = 1; else if (other.X != x) return null;
                if (other.Y == y + 1) o |= 2; else if (other.Y != y) return null;
                if (other.Z == z + 1) o |= 4; else if (other.Z != z) return null;
                return o;
            }
        }

        /// <summary>
        /// Get the global octant this cell is located in, or no value if this is a centered cell.
        /// </summary>
        public int? Octant
        {
            get
            {
                if (IsCenteredAtOrigin) return null;

                var o = 0;
                if (X >= 0) o  = 1;
                if (Y >= 0) o |= 2;
                if (Z >= 0) o |= 4;
                return o;
            }
        }

        /// <summary>
        /// Returns smallest common root of a and b.
        /// </summary>
        public static Cell GetCommonRoot(Cell a, Cell b)
        {
            if (a == b) return a;

            if (a.IsCenteredAtOrigin)
            {
                if (b.IsCenteredAtOrigin)
                {
                    // if both are centered, then the larger cell is the result
                    return a.Exponent > b.Exponent ? a : b;
                }
                else
                {
                    // if A is centered and B is not centered,
                    // then enlarge A until it contains B
                    while (!a.Contains(b)) a = a.Parent;
                    return a;
                }
            }
            else
            {
                if (b.IsCenteredAtOrigin)
                {
                    // if A is not centered and B is centered,
                    // then enlarge B until it contains A
                    while (!b.Contains(a)) b = b.Parent;
                    return b;
                }
                else
                {
                    // if no cell is centered, ...
                    if (a.Octant == b.Octant)
                    {
                        // and both cells are located in the same global octant,
                        // then recursively grow smaller cell (take parent)
                        // until a and b meet at their common root
                        return a.Exponent > b.Exponent ? GetCommonRoot(a, b.Parent) : GetCommonRoot(a.Parent, b);
                    }
                    else
                    {
                        // and each cell is located in a different global octant,
                        // then take a centered cell and enlarge it until it contains both A and B
                        var c = new Cell(Math.Max(a.Exponent, b.Exponent) + 1); // centered cell needs to be at least twice as large as the bigger one of A and B
                        while (!c.Contains(a)) c = c.Parent;
                        while (!c.Contains(b)) c = c.Parent;
                        return c;
                    }
                }
            }
        }

        /// <summary>
        /// Returns smallest common root of cells.
        /// </summary>
        public static Cell GetCommonRoot(params Cell[] cells)
        {
            if (cells == null || cells.Length == 0) throw new ArgumentException("No cells.");
            var r = cells[0];
            for (var i = 1; i < cells.Length; i++) r = new Cell(r, cells[i]);
            return r;
        }

        #region operators

        /// <summary>
        /// Scales down cell by 'd' powers of two.
        /// BoundingBox.Min stays the same, except for cells centered at origin, which stay centered.
        /// </summary>
        public static Cell operator >>(Cell a, int d)
        {
            if (d < 1) return a;

            if (a.IsCenteredAtOrigin) return new Cell(a.Exponent - d);

            // right-shift with long argument only uses low-order 6 bits
            // https://docs.microsoft.com/en-us/dotnet/articles/csharp/language-reference/operators/right-shift-operator
            if (d > 63) d = 63;

            return new Cell(a.X << d, a.Y << d, a.Z << d, a.Exponent - d);
        }

        /// <summary>
        /// Scales up cell by 'd' powers of two.
        /// BoundingBox.Min will snap to the next smaller or equal position allowed by the new cell size, except for cells centered at origin, which stay centered.
        /// </summary>
        public static Cell operator <<(Cell a, int d)
        {
            if (d < 1) return a;

            if (a.IsCenteredAtOrigin) return new Cell(a.Exponent + d);

            // right-shift with long argument only uses low-order 6 bits
            // https://docs.microsoft.com/en-us/dotnet/articles/csharp/language-reference/operators/right-shift-operator
            if (d > 63) d = 63;

            return new Cell(a.X >> d, a.Y >> d, a.Z >> d, a.Exponent + d);
        }

        #endregion

        #region equality and hashing

        /// <summary>
        /// </summary>
        public bool Equals(Cell other) => this == other;

        /// <summary>
        /// </summary>
        public static bool operator ==(Cell a, Cell b) => a.X == b.X && a.Y == b.Y && a.Z == b.Z && a.Exponent == b.Exponent;

        /// <summary>
        /// </summary>
        public static bool operator !=(Cell a, Cell b) => !(a == b);

        /// <summary>
        /// </summary>
        public override bool Equals(object obj) => obj is Cell cell && this == cell;

        /// <summary>
        /// </summary>
        public override int GetHashCode() => HashCode.GetCombined(X, Y, Z, Exponent);

        #endregion

        #region string serialization

        /// <summary></summary>
        public override string ToString()
        {
            if (IsCenteredAtOrigin || IsInvalid)
            {
                return $"[{Exponent}]";
            }
            else
            {
                return $"[{X}, {Y}, {Z}, {Exponent}]";
            }
        }

        public static Cell Parse(string s)
        {
            var xs = s.Substring(1, s.Length - 2).Split(',');
            return xs.Length switch
            {
                1 => new Cell(exponent: int.Parse(xs[0])),
                4 => new Cell(x: long.Parse(xs[0]), y: long.Parse(xs[1]), z: long.Parse(xs[2]), exponent: int.Parse(xs[3])),
                _ => throw new FormatException(
                    $"Expected [<x>,<y>,<z>,<exponent>], or [<exponent>], but found {s}. " +
                    $"Error 973f176d-6780-4f77-8736-0ed341adf136."
                    )
            };
        }

        #endregion

        #region binary serialization

        /// <summary>
        /// </summary>
        public byte[] ToByteArray()
        {
            var buffer = new byte[28];
            using (var ms = new MemoryStream(buffer))
            using (var bw = new BinaryWriter(ms))
            {
                bw.Write(X); bw.Write(Y); bw.Write(Z); bw.Write(Exponent);
            }
            return buffer;
        }

        /// <summary>
        /// </summary>
        public static Cell Parse(byte[] buffer)
        {
            using var ms = new MemoryStream(buffer, 0, 28);
            using var br = new BinaryReader(ms);
            var x = br.ReadInt64(); var y = br.ReadInt64(); var z = br.ReadInt64(); var e = br.ReadInt32();
            return new Cell(x, y, z, e);
        }

        #endregion
    }
}
