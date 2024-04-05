using System.Runtime.CompilerServices;
using System.Numerics;


namespace Scratch;

/// <summary>
/// Performs tonemapping via Reinhard's method acting on luminance only, allows defining a white point
/// </summary>
/// <param name="max_white">The maximum white point of the input or output color to tonemap against</param>
public struct ReinhardLuminanceTonemapper(float max_white = 1f) : IToneMapper
{
    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Vector3 PerformTonemap(in Vector3 color, in float exposure)
    {
        
        float oldLum = (color * MathF.Exp(exposure)).Luminance();
        float num = oldLum * (1f + (oldLum / (max_white * max_white)));
        float newLum = num  / (1f + oldLum);

        return color.ChangeLuminance(newLum);
    }



    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Vector3 PerformInverse(in Vector3 color, in float exposure)
	{
        float oldLum = color.Luminance();
        float newLum = -(oldLum / Math.Min((oldLum / (max_white * max_white)) - 1.0f, -0.1f));
		return color.ChangeLuminance(newLum);
	} 
}
