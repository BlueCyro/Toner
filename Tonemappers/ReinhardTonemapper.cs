using System.Numerics;
using System.Runtime.CompilerServices;


namespace Scratch;

/// <summary>
/// Performs tonemapping via Reinhard's method
/// </summary>
public struct ReinhardTonemapper : IToneMapper
{
    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Vector3 PerformTonemap(in Vector3 color, in float exposure)
    {
        return color * MathF.Exp(exposure) / (Vector3.One + color);
    }



    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Vector3 PerformInverse(in Vector3 color, in float exposure)
	{
		return -(color / Vector3.Min(color - Vector3.One, new(-0.1f)));
	}
}
