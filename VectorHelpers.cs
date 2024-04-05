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
    /// <param name="target"></param>
    /// <param name="other"></param>
    /// <param name="mask"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 Mask(in this Vector3 target, in Vector3 other, Vector128<float> mask)
    {
        return Vector128.ConditionalSelect(mask, target.AsVector128(), other.AsVector128()).AsVector3();
    }



    /// <summary>
    /// Does a component-wise greater or less than
    /// </summary>
    /// <param name="left">Left side to compare</param>
    /// <param name="right">Right side to compare</param>
    /// <returns>Vector representing the component-wise results as zero or one.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<float> LessThanOrEqual(in this Vector3 left, in Vector3 right)
    {
        return Vector128.LessThanOrEqual(left.AsVector128(), right.AsVector128());
    }



    /// <summary>
    /// Raises each component of the vector to a power
    /// </summary>
    /// <param name="baseValue">Base</param>
    /// <param name="power">Power to raise to</param>
    /// <returns>Raised Vector3</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 Pow(in this Vector3 baseValue, in float power)
    {
        return new(
            MathF.Pow(baseValue.X, power),
            MathF.Pow(baseValue.Y, power),
            MathF.Pow(baseValue.Z, power));
    }



    /// <summary>
    /// Raises each component of the vector to a power, component-wise
    /// </summary>
    /// <param name="baseValue">Base</param>
    /// <param name="power">Component-wise power to raise to</param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 Pow(in this Vector3 baseValue, in Vector3 power)
    {
        return new(
            MathF.Pow(baseValue.X, power.X),
            MathF.Pow(baseValue.Y, power.Y),
            MathF.Pow(baseValue.Z, power.Z));
    }
}