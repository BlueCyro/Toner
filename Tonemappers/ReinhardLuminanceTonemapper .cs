using System.Runtime.CompilerServices;
using System.Numerics;


namespace Scratch;

/// <summary>
/// Performs tonemapping via Reinhard acting on luminance only, allows defining a white point
/// </summary>
/// <param name="max_white">The maximum white point of the input or output color to tonemap against</param>
public struct ReinhardLuminanceTonemapper(float max_white = 1f) : IToneMapper
{
    /// <summary>
    /// Tonemaps HDR RGB values into SDR RGB values via Reinhard
    /// </summary>
    /// <param name="color">HDR RGB values</param>
    /// <param name="exposure">Exposure to tonemap at</param>
    /// <returns>SDR RGB values</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Vector3 PerformTonemap(in Vector3 color, in float exposure)
    {
        
        float oldLum = (color * MathF.Exp(exposure)).Luminance();
        float num = oldLum * (1f + (oldLum / (max_white * max_white)));
        float newLum = num  / (1f + oldLum);

        return color.ChangeLuminance(newLum);
    }



    /// <summary>
    /// Inversely tonemaps SDR RGB values into HDR RGB values assuming luminance-only Reinhard
    /// </summary>
    /// <param name="color">SDR RGB values</param>
    /// <param name="exposure">Exposure to inversely tonemap at</param>
    /// <returns>Approximated HDR RGB values</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Vector3 PerformInverse(in Vector3 color, in float exposure)
	{
        float oldLum = color.Luminance();
        float newLum = -(oldLum / Math.Min((oldLum / (max_white * max_white)) - 1.0f, -0.1f));
		return color.ChangeLuminance(newLum);
	} 
}
