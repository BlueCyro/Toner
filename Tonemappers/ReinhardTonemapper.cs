using System.Numerics;
using System.Runtime.CompilerServices;


namespace Scratch;

/// <summary>
/// Performs tonemapping via Reinhard's method
/// </summary>
public struct ReinhardTonemapper : IToneMapper
{
    /// <summary>
    /// Tonemaps HDR RGB values into SDR
    /// </summary>
    /// <param name="color">HDR RGB values</param>
    /// <param name="exposure">Exposure to perform tonemapping at</param>
    /// <returns>SDR RGB values</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector3 PerformTonemap(in Vector3 color, in float exposure)
    {
        return color * MathF.Exp(exposure) / (Vector3.One + color);
    }



    /// <summary>
    /// Inversely maps SDR RGB values into HDR assuming Reinhard
    /// </summary>
    /// <param name="color">SDR RGB values</param>
    /// <param name="exposure">Exposure to perform inverse tonemapping at</param>
    /// <returns>Approximated HDR RGB values</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector3 PerformInverse(in Vector3 color, in float exposure)
	{
		return -(color / Vector3.Min(color - Vector3.One, new(-0.1f)));
	}
}
