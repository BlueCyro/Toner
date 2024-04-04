using System.Runtime.CompilerServices;
using System.Numerics;


namespace Scratch;

/// <summary>
/// Helper class for a few specific functions that intrinsic vectors lack
/// </summary>
public static class VectorHelpers
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 Mask(in this Vector3 from, Span<bool> flags, in Vector3 other)
    {
        return new(
            flags[0] ? from[0] : other[0],
            flags[1] ? from[1] : other[1],
            flags[2] ? from[2] : other[2]);
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Span<bool> LessThanOrEqual(in this Vector3 A, in Vector3 B)
    {
        bool[] flags = [A.X <= B.X, A.Y <= B.Y, A.Z <= B.Z];
        return flags;
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 Pow(in this Vector3 A, in float B)
    {
        return new(
            MathF.Pow(A.X, B),
            MathF.Pow(A.Y, B),
            MathF.Pow(A.Z, B));
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 Pow(in this Vector3 A, in Vector3 B)
    {
        return new(
            MathF.Pow(A.X, B.X),
            MathF.Pow(A.Y, B.Y),
            MathF.Pow(A.Z, B.Z));
    }
}