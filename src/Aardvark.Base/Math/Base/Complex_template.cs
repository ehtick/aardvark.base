﻿using System;
using System.Linq;
using System.Globalization;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;

namespace Aardvark.Base
{
    //# var signedtypes = Meta.SignedTypes;
    //# var numtypes = Meta.StandardNumericTypes;
    //# var dreptypes = Meta.DoubleRepresentableTypes;
    //# foreach (var isDouble in new[] { false, true }) {
    //#   var ftype = isDouble ? Meta.DoubleType : Meta.FloatType;
    //#   var ftype2 = isDouble ? Meta.FloatType : Meta.DoubleType;
    //#   var ft = ftype.Name;
    //#   var ft2 = ftype2.Name;
    //#   var ct = isDouble ? "ComplexD" : "ComplexF";
    //#   var ct2 = isDouble ? "ComplexF" : "ComplexD";
    //#   var constant = isDouble ? "Constant" : "ConstantF";
    //#   var half = isDouble ? "0.5" : "0.5f";
    [DataContract]
    [StructLayout(LayoutKind.Sequential)]
    public struct __ct__ : IEquatable<__ct__>
    {
        [DataMember]
        public __ft__ Real;
        [DataMember]
        public __ft__ Imag;

        #region Constructors

        /// <summary>
        /// Constructs a <see cref="__ct__"/> from a real scalar.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public __ct__(__ft__ real)
        {
            Real = real;
            Imag = 0;
        }

        /// <summary>
        /// Constructs a <see cref="__ct__"/> from a real and an imaginary part.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public __ct__(__ft__ real, __ft__ imag)
        {
            Real = real;
            Imag = imag;
        }

        /// <summary>
        /// Constructs a <see cref="__ct__"/> from a <see cref="__ct2__"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public __ct__(__ct2__ complex)
        {
            Real = (__ft__)complex.Real;
            Imag = (__ft__)complex.Imag;
        }

        #endregion

        #region Constants

        /// <summary>
        /// Returns 0 + 0i.
        /// </summary>
        public static __ct__ Zero
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new __ct__(0, 0);
        }

        /// <summary>
        /// Returns 1 + 0i.
        /// </summary>
        public static __ct__ One
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new __ct__(1, 0);
        }

        /// <summary>
        /// Returns 0 + 1i.
        /// </summary>
        public static __ct__ I
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new __ct__(0, 1);
        }

        /// <summary>
        /// Returns ∞ + 0i.
        /// </summary>
        public static __ct__ PositiveInfinity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new __ct__(__ft__.PositiveInfinity);
        }

        /// <summary>
        /// Returns -∞ + 0i.
        /// </summary>
        public static __ct__ NegativeInfinity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new __ct__(__ft__.NegativeInfinity);
        }

        /// <summary>
        /// Returns 0 + ∞i.
        /// </summary>
        public static __ct__ PositiveInfinityI
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new __ct__(0, __ft__.PositiveInfinity);
        }

        /// <summary>
        /// Returns 0 - ∞i.
        /// </summary>
        public static __ct__ NegativeInfinityI
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new __ct__(0, __ft__.NegativeInfinity);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Returns the conjugated of the complex number.
        /// </summary>
        public readonly __ct__ Conjugated
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return new __ct__(Real, -Imag); }
        }

        /// <summary>
        /// Returns the reciprocal of the complex number.
        /// </summary>
        public readonly __ct__ Reciprocal
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                __ft__ t = 1 / NormSquared;
                return new __ct__(Real * t, -Imag * t);
            }
        }

        /// <summary>
        /// Returns the squared Gaussian Norm (modulus) of the complex number.
        /// </summary>
        public readonly __ft__ NormSquared
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return Real * Real + Imag * Imag; }
        }

        /// <summary>
        /// Returns the Gaussian Norm (modulus) of the complex number.
        /// </summary>
        [XmlIgnore]
        public __ft__ Norm
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get { return Fun.Sqrt(Real * Real + Imag * Imag); }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                __ft__ r = Norm;
                Real = value * Real / r;
                Imag = value * Imag / r;
            }
        }

        /// <summary>
        /// Retruns the argument of the complex number.
        /// </summary>
        [XmlIgnore]
        public __ft__ Argument
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get { return Fun.Atan2(Imag, Real); }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                __ft__ r = Norm;

                Real = r * Fun.Cos(value);
                Imag = r * Fun.Sin(value);
            }
        }

        /// <summary>
        /// Returns whether the complex number has no imaginary part.
        /// </summary>
        public readonly bool IsReal
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return Imag.IsTiny(); }
        }

        /// <summary>
        /// Returns whether the complex number has no real part.
        /// </summary>
        public readonly bool IsImaginary
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return Real.IsTiny(); }
        }

        /// <summary>
        /// Returns whether the complex number is 1 + 0i.
        /// </summary>
        public readonly bool IsOne
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return Real.ApproximateEquals(1) && Imag.IsTiny(); }
        }

        /// <summary>
        /// Returns whether the complex number is zero.
        /// </summary>
        public readonly bool IsZero
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return Real.IsTiny() && Imag.IsTiny(); }
        }

        /// <summary>
        /// Returns whether the complex number is 0 + 1i.
        /// </summary>
        public readonly bool IsI
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return Real.IsTiny(Real) && Imag.ApproximateEquals(1); }
        }

        /// <summary>
        /// Returns whether the complex number has a part that is NaN.
        /// </summary>
        public readonly bool IsNaN
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return (__ft__.IsNaN(Real) || __ft__.IsNaN(Imag)); }
        }

        /// <summary>
        /// Returns whether the complex number has a part that is infinite (positive or negative).
        /// </summary>
        public readonly bool IsInfinity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return (__ft__.IsInfinity(Real) || __ft__.IsInfinity(Imag)); }
        }

        /// <summary>
        /// Returns whether the complex number has a part that is infinite and positive.
        /// </summary>
        public readonly bool IsPositiveInfinity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return (__ft__.IsPositiveInfinity(Real) || __ft__.IsPositiveInfinity(Imag)); }
        }

        /// <summary>
        /// Returns whether the complex number has a part that is infinite and negative.
        /// </summary>
        public readonly bool IsNegativeInfinity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return (__ft__.IsNegativeInfinity(Real) || __ft__.IsNegativeInfinity(Imag)); }
        }

        /// <summary>
        /// Returns whether the complex number is finite (i.e. not NaN and not infinity).
        /// </summary>
        public readonly bool IsFinite
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return !(IsNaN || IsInfinity); }
        }

        #endregion

        #region Static factories

        /// <summary>
        /// Creates a Radial Complex
        /// </summary>
        /// <param name="r">Norm of the complex number</param>
        /// <param name="phi">Argument of the complex number</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static __ct__ CreateRadial(__ft__ r, __ft__ phi)
            => new __ct__(r * Fun.Cos(phi), r * Fun.Sin(phi));

        /// <summary>
        /// Creates a Orthogonal Complex
        /// </summary>
        /// <param name="real">Real-Part</param>
        /// <param name="imag">Imaginary-Part</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static __ct__ CreateOrthogonal(__ft__ real, __ft__ imag)
            => new __ct__(real, imag);

        #endregion

        #region Static methods for F# core and Aardvark library support

        [EditorBrowsable(EditorBrowsableState.Never)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static __ct__ Acos(__ct__ x)
            => x.Acos();

        [EditorBrowsable(EditorBrowsableState.Never)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static __ct__ Acoshb(__ct__ x)
            => x.Acosh();

        [EditorBrowsable(EditorBrowsableState.Never)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static __ct__ Cos(__ct__ x)
            => x.Cos();

        [EditorBrowsable(EditorBrowsableState.Never)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static __ct__ Cosh(__ct__ x)
            => x.Cosh();

        [EditorBrowsable(EditorBrowsableState.Never)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static __ct__ Asin(__ct__ x)
            => x.Asin();

        [EditorBrowsable(EditorBrowsableState.Never)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static __ct__ Asinhb(__ct__ x)
            => x.Asinh();

        [EditorBrowsable(EditorBrowsableState.Never)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static __ct__ Sin(__ct__ x)
            => x.Sin();

        [EditorBrowsable(EditorBrowsableState.Never)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static __ct__ Sinh(__ct__ x)
            => x.Sinh();

        [EditorBrowsable(EditorBrowsableState.Never)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static __ct__ Atan(__ct__ x)
            => x.Atan();

        [EditorBrowsable(EditorBrowsableState.Never)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static __ct__ Atanhb(__ct__ x)
            => x.Atanh();

        [EditorBrowsable(EditorBrowsableState.Never)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static __ct__ Tan(__ct__ x)
            => x.Tan();

        [EditorBrowsable(EditorBrowsableState.Never)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static __ct__ Tanh(__ct__ x)
            => x.Tanh();

        [EditorBrowsable(EditorBrowsableState.Never)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static __ct__ Sqrt(__ct__ x)
            => x.Sqrt();

        [EditorBrowsable(EditorBrowsableState.Never)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static __ct__ CubeRoot(__ct__ x)
            => x.Cbrt();

        [EditorBrowsable(EditorBrowsableState.Never)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static __ct__ Exp(__ct__ x)
            => x.Exp();

        [EditorBrowsable(EditorBrowsableState.Never)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static __ct__ Log(__ct__ x)
            => x.Log();

        [EditorBrowsable(EditorBrowsableState.Never)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static __ct__ LogBinary(__ct__ x)
            => x.Log2();

        [EditorBrowsable(EditorBrowsableState.Never)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static __ct__ Log10(__ct__ x)
            => x.Log10();

        [EditorBrowsable(EditorBrowsableState.Never)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static __ct__ DivideByInt(__ct__ x, int y)
            => x / y;

        #endregion

        #region Operators

        /// <summary>
        /// Conversion from a <see cref="__ct__"/> to a <see cref="__ct2__"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator __ct2__(__ct__ c)
            => new __ct2__(c);

        /// <summary>
        /// Implicit conversion from a <see cref="__ft__"/> to a <see cref="__ct__"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator __ct__(__ft__ a)
            => new __ct__(a);

        /// <summary>
        /// Adds two complex numbers.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static __ct__ operator +(__ct__ a, __ct__ b)
            => new __ct__(a.Real + b.Real, a.Imag + b.Imag);

        /// <summary>
        /// Adds a complex number and a real number.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static __ct__ operator +(__ct__ a, __ft__ b)
            => new __ct__(a.Real + b, a.Imag);

        /// <summary>
        /// Adds a real number and a complex number.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static __ct__ operator +(__ft__ a, __ct__ b)
            => new __ct__(a + b.Real, b.Imag);

        /// <summary>
        /// Subtracts two complex numbers.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static __ct__ operator -(__ct__ a, __ct__ b)
            => new __ct__(a.Real - b.Real, a.Imag - b.Imag);

        /// <summary>
        /// Subtracts a real number from a complex number.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static __ct__ operator -(__ct__ a, __ft__ b)
            => new __ct__(a.Real - b, a.Imag);

        /// <summary>
        /// Subtracts a complex number from a real number.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static __ct__ operator -(__ft__ a, __ct__ b)
            => new __ct__(a - b.Real, -b.Imag);

        /// <summary>
        /// Multiplies two complex numbers.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static __ct__ operator *(__ct__ a, __ct__ b)
            => new __ct__(
                a.Real * b.Real - a.Imag * b.Imag,
                a.Real * b.Imag + a.Imag * b.Real);

        /// <summary>
        /// Multiplies a complex number and a real number.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static __ct__ operator *(__ct__ a, __ft__ b)
            => new __ct__(a.Real * b, a.Imag * b);

        /// <summary>
        /// Multiplies a real number and a complex number.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static __ct__ operator *(__ft__ a, __ct__ b)
            => new __ct__(a * b.Real, a * b.Imag);

        /// <summary>
        /// Divides two complex numbers.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static __ct__ operator /(__ct__ a, __ct__ b)
        {
            __ft__ t = 1 / b.NormSquared;
            return new __ct__(
                t * (a.Real * b.Real + a.Imag * b.Imag),
                t * (a.Imag * b.Real - a.Real * b.Imag));
        }

        /// <summary>
        /// Divides a complex number by a real number.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static __ct__ operator /(__ct__ a, __ft__ b)
            => new __ct__(a.Real / b, a.Imag / b);

        /// <summary>
        /// Divides a real number by a complex number.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static __ct__ operator /(__ft__ a, __ct__ b)
        {
            __ft__ t = 1 / b.NormSquared;
            return new __ct__(
                t * (a * b.Real),
                t * (-a * b.Imag));
        }

        /// <summary>
        /// Negates a complex number.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static __ct__ operator -(__ct__ a)
            => new __ct__(-a.Real, -a.Imag);

        /// <summary>
        /// Returns the conjugate of a complex number.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static __ct__ operator !(__ct__ a)
            => a.Conjugated;

        #endregion

        #region Comparison Operators

        /// <summary>
        /// Returns whether two <see cref="__ct__"/> are equal.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(__ct__ a, __ct__ b)
            => a.Real == b.Real && a.Imag == b.Imag;

        /// <summary>
        /// Returns whether two <see cref="__ct__"/> are not equal.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(__ct__ a, __ct__ b)
            => !(a == b);

        #endregion

        #region Overrides

        public override readonly int GetHashCode()
            => HashCode.GetCombined(Real, Imag);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(__ct__ other)
            => Real.Equals(other.Real) && Imag.Equals(other.Imag);

        public override readonly bool Equals(object other)
        {
            if (other is __ct__ obj)
                return Equals(obj);
            else
                return false;
        }

        public override readonly string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "[{0}, {1}]", Real, Imag);
        }

        public static __ct__ Parse(string s)
        {
            var x = s.NestedBracketSplitLevelOne().ToArray(2);
            return new __ct__(
                __ft__.Parse(x[0], CultureInfo.InvariantCulture),
                __ft__.Parse(x[1], CultureInfo.InvariantCulture)
            );
        }

        #endregion
    }

    public static partial class Complex
    {
        #region Conjugate

        /// <summary>
        /// Returns the conjugate of a complex number.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static __ct__ Conjugated(__ct__ c)
            => c.Conjugated;

        #endregion

        #region Norm

        /// <summary>
        /// Returns the squared Gaussian Norm (modulus) of the complex number.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static __ft__ NormSquared(__ct__ c)
            => c.NormSquared;

        /// <summary>
        /// Returns the Gaussian Norm (modulus) of the complex number.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static __ft__ Norm(__ct__ c)
            => c.Norm;

        #endregion

        #region Argument

        /// <summary>
        /// Retruns the argument of the complex number.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static __ft__ Argument(__ct__ c)
            => c.Argument;

        #endregion
    }

    public static partial class Fun
    {
        #region Power

        /// <summary>
        /// Returns the complex number <paramref name="number"/> raised to the power of <paramref name="exponent"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static __ct__ Pow(this __ct__ number, __ct__ exponent)
        {
            if (number.IsZero)
                return __ct__.Zero;
            else if (exponent.IsZero)
                return __ct__.One;
            else
            {
                __ft__ r = number.Norm;
                __ft__ phi = number.Argument;

                __ft__ a = exponent.Real;
                __ft__ b = exponent.Imag;

                return __ct__.CreateRadial(Exp(Log(r) * a - b * phi), a * phi + b * Log(r));
            }
        }

        /// <summary>
        /// Returns the complex number <paramref name="number"/> raised to the power of <paramref name="exponent"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static __ct__ Pow(this __ct__ number, __ft__ exponent)
        {
            if (number.IsZero)
                return __ct__.Zero;
            else
            {
                __ft__ r = number.Norm;
                __ft__ phi = number.Argument;
                return __ct__.CreateRadial(Pow(r, exponent), exponent * phi);
            }
        }

        /// <summary>
        /// Returns <paramref name="number"/> raised to the power of <paramref name="exponent"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static __ct__ Pow(this __ft__ number, __ct__ exponent)
        {
            if (number == 0)
                return __ct__.Zero;
            else
            {
                __ft__ a = exponent.Real;
                __ft__ b = exponent.Imag;

                if (number < 0)
                {
                    var phi = __constant__.Pi;
                    return __ct__.CreateRadial(Exp(Log(-number) * a - b * phi), a * phi + b * Log(-number));
                }
                else
                    return __ct__.CreateRadial(Pow(number, a), b * Log(number));
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static __ct__ Power(this __ct__ number, __ct__ exponent)
            => Pow(number, exponent);

        [EditorBrowsable(EditorBrowsableState.Never)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static __ct__ Power(this __ct__ number, __ft__ exponent)
            => Pow(number, exponent);

        [EditorBrowsable(EditorBrowsableState.Never)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static __ct__ Power(this __ft__ number, __ct__ exponent)
            => Pow(number, exponent);

        #endregion

        #region Trigonometry

        /// <summary>
        /// Returns the angle that is the arc cosine of the complex number <paramref name="x"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static __ct__ Acos(this __ct__ x)
        {
            var t = Log(new __ct__(-x.Imag, x.Real) + Sqrt(1 - x * x));
            return new __ct__(-t.Imag + __constant__.PiHalf, t.Real);
        }

        /// <summary>
        /// Returns the angle that is the hyperbolic arc cosine of the complex number <paramref name="x"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static __ct__ Acosh(this __ct__ x)
            => Log(x + Sqrt(x * x - 1));

        /// <summary>
        /// Returns the cosine of the complex number <paramref name="x"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static __ct__ Cos(this __ct__ x)
            => (
                Exp(new __ct__(-x.Imag, x.Real)) +
                Exp(new __ct__(x.Imag, -x.Real))
            ) * __half__;

        /// <summary>
        /// Returns the hyperbolic cosine of the complex number <paramref name="x"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static __ct__ Cosh(this __ct__ x)
            => Cos(new __ct__(-x.Imag, x.Real));

        /// <summary>
        /// Returns the angle that is the arc sine of the complex number <paramref name="x"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static __ct__ Asin(this __ct__ x)
        {
            var t = Log(new __ct__(-x.Imag, x.Real) + Sqrt(1 - x * x));
            return new __ct__(t.Imag, -t.Real);
        }

        /// <summary>
        /// Returns the angle that is the hyperbolic arc sine of the complex number <paramref name="x"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static __ct__ Asinh(this __ct__ x)
            => Log(x + Sqrt(1 + x * x));

        /// <summary>
        /// Returns the sine of the complex number <paramref name="x"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static __ct__ Sin(this __ct__ x)
        {
            var a = Exp(new __ct__(-x.Imag, x.Real)) - Exp(new __ct__(x.Imag, -x.Real));
            return new __ct__(a.Imag, -a.Real) * __half__;
        }

        /// <summary>
        /// Returns the hyperbolic sine of the complex number <paramref name="x"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static __ct__ Sinh(this __ct__ x)
        {
            var sin = Sin(new __ct__(-x.Imag, x.Real));
            return new __ct__(sin.Imag, -sin.Real);
        }

        /// <summary>
        /// Returns the angle that is the arc tangent of the complex number <paramref name="x"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static __ct__ Atan(this __ct__ x)
        {
            if (x == __ct__.I)
                return __ct__.PositiveInfinityI;
            else if (x == -__ct__.I)
                return __ct__.NegativeInfinityI;
            else if (x == __ct__.PositiveInfinity)
                return new __ct__(__constant__.PiHalf);
            else if (x == __ct__.NegativeInfinity)
                return new __ct__(-__constant__.PiHalf);
            else
                return new __ct__(0, __half__) * Log((__ct__.I + x) / (__ct__.I - x));
        }

        /// <summary>
        /// Returns the angle that is the hyperbolic arc tangent of the complex number <paramref name="x"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static __ct__ Atanh(this __ct__ x)
        {
            if (x == __ct__.Zero)
                return __ct__.Zero;
            else if (x == __ct__.One)
                return __ct__.PositiveInfinity;
            else if (x == __ct__.PositiveInfinity)
                return new __ct__(0, -__constant__.PiHalf);
            else if (x == __ct__.I)
                return new __ct__(0, __constant__.PiQuarter);
            else
                return __half__ * (Log(1 + x) - Log(1 - x));
        }

        /// <summary>
        /// Returns the tangent of the complex number <paramref name="x"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static __ct__ Tan(this __ct__ x)
        {
            if (x == __ct__.PositiveInfinityI)
                return __ct__.I;
            else if (x == __ct__.NegativeInfinityI)
                return -__ct__.I;
            else
                return Sin(x) / Cos(x);
        }

        /// <summary>
        /// Returns the hyperbolic tangent of the complex number <paramref name="x"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static __ct__ Tanh(this __ct__ x)
        {
            var tan = Tan(new __ct__(-x.Imag, x.Real));
            return new __ct__(tan.Imag, -tan.Real);
        }

        #endregion

        #region Exp, Log

        /// <summary>
        /// Returns e raised to the power of the complex number <paramref name="x"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static __ct__ Exp(this __ct__ x)
            => new __ct__(Cos(x.Imag), Sin(x.Imag)) * Exp(x.Real);

        /// <summary>
        /// Returns the natural logarithm of the complex number <paramref name="x"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static __ct__ Log(this __ct__ x)
            => new __ct__(Log(x.Norm), x.Argument);

        /// <summary>
        /// Returns the logarithm of the complex number <paramref name="x"/> in the given basis.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static __ct__ Log(this __ct__ x, __ft__ basis)
            => x.Log() / basis.Log();

        /// <summary>
        /// Returns the base-10 logarithm of the complex number <paramref name="x"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static __ct__ Log10(this __ct__ x)
            => Log(x, 10);

        /// <summary>
        /// Returns the base-2 logarithm of the complex number <paramref name="x"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static __ct__ Log2(this __ct__ x)
            => x.Log() * __constant__.Ln2Inv;

        #endregion

        #region Roots

        /// <summary>
        /// Returns the principal square root of the complex number <paramref name="x"/>.
        /// </summary>
        // https://math.stackexchange.com/a/44500
        // TODO: Check if this is actually better than the naive implementation
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static __ct__ Sqrt(this __ct__ x)
        {
            if (x.Imag == 0)
            {
                if (x.Real < 0)
                    return new __ct__(0, Sqrt(-x.Real));
                else
                    return new __ct__(Sqrt(x.Real), 0);
            }
            else
            {
                var a = x.Norm;
                var b = x + a;
                return a.Sqrt() * (b / b.Norm);
            }
        }

        /// <summary>
        /// Returns the principal cubic root of the complex number <paramref name="x"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static __ct__ Cbrt(this __ct__ x)
            => __ct__.CreateRadial(Cbrt(x.Norm), x.Argument / 3);

        //# signedtypes.ForEach(t => { if (t != Meta.DecimalType) {
        //# if (ftype == Meta.DoubleType ^ t == Meta.FloatType) {
        /// <summary>
        /// Returns the square root of the given real number and returns a complex number.
        //# if (!dreptypes.Contains(t)) {
        /// Note: This function uses a double representation internally, but not all __t.Name__ values can be represented exactly as double.
        //# }
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static __ct__ Csqrt(this __t.Name__ number)
        {
            if (number >= 0)
            {
                return new __ct__(Sqrt(number), 0);
            }
            else
            {
                return new __ct__(0, Sqrt(-number));
            }
        }
        //# }}});

        /// <summary>
        /// Calculates both square roots of a complex number.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static __ct__[] Csqrt(this __ct__ number)
        {
            __ct__ res0 = __ct__.CreateRadial(Sqrt(number.Norm), number.Argument / 2);
            __ct__ res1 = __ct__.CreateRadial(Sqrt(number.Norm), number.Argument / 2 + __constant__.Pi);

            return new __ct__[2] { res0, res1 };
        }

        /// <summary>
        /// Calculates the n-th root of a complex number and returns n solutions.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static __ct__[] Root(this __ct__ number, int n)
        {
            __ct__[] values = new __ct__[n];

            __ft__ invN = 1 / (__ft__)n;
            __ft__ phi = number.Argument / n;
            __ft__ dphi = __constant__.PiTimesTwo * invN;
            __ft__ r = Pow(number.Norm, invN);

            for (int i = 0; i < n; i++)
            {
                values[i] = __ct__.CreateRadial(r, phi + dphi * i);
            }

            return values;
        }

        #endregion

        #region ApproximateEquals

        /// <summary>
        /// Returns whether the given complex numbers are equal within the given tolerance.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ApproximateEquals(this __ct__ a, __ct__ b, __ft__ tolerance)
        {
            return ApproximateEquals(a.Real, b.Real, tolerance) && ApproximateEquals(a.Imag, b.Imag, tolerance);
        }

        /// <summary>
        /// Returns whether the given complex numbers are equal within
        /// Constant&lt;__ft__&gt;.PositiveTinyValue.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ApproximateEquals(this __ct__ a, __ct__ b)
        {
            return ApproximateEquals(a, b, Constant<__ft__>.PositiveTinyValue);
        }

        #endregion

        #region Special Floating Point Value Checks

        /// <summary>
        /// Returns whether the given <see cref="__ct__"/> is NaN.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNaN(__ct__ v)
            => v.IsNaN;

        /// <summary>
        /// Returns whether the given <see cref="__ct__"/> is infinity (positive or negative).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsInfinity(__ct__ v)
            => v.IsInfinity;

        /// <summary>
        /// Returns whether the given <see cref="__ct__"/> is positive infinity.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPositiveInfinity(__ct__ v)
            => v.IsPositiveInfinity;

        /// <summary>
        /// Returns whether the given <see cref="__ct__"/> is negative infinity.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNegativeInfinity(__ct__ v)
            => v.IsNegativeInfinity;

        /// <summary>
        /// Returns whether the given <see cref="__ct__"/> is finite (i.e. not NaN and not infinity).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsFinite(__ct__ v)
            => v.IsFinite;

        #endregion
    }

    //# } // isDouble
}
