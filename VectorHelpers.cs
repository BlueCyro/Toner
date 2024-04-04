using System.Runtime.CompilerServices;
using System.Numerics;
using System.Runtime.Intrinsics;


namespace Scratch;

/// <summary>
/// Helper class for a few specific functions that intrinsic vectors lack
/// </summary>
public static class VectorHelpers
{
    /// <summary>
    /// Performs a component mask based on input flags. Essentially: "flags ? from : to" on a per-component basis
    /// </summary>
    /// <param name="from"></param>
    /// <param name="other"></param>
    /// <param name="flags"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 Mask(in this Vector3 from, in Vector3 other, Vector128<float> flags)
    {
        return Vector128.ConditionalSelect(flags, from.AsVector128(), other.AsVector128()).AsVector3();
    }



    /// <summary>
    /// Does a component-wise greater or less than
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<float> LessThanOrEqual(in this Vector3 left, in Vector3 right)
    {
        return Vector128.LessThanOrEqual(left.AsVector128(), right.AsVector128());
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