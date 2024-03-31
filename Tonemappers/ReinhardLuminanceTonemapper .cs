using System.Runtime.CompilerServices;
using Elements.Core;


namespace Scratch;

public struct ReinhardLuminanceTonemapper(float max_white) : IToneMapper
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float3 PerformTonemap(float3 color, float exposure)
    {
        color *= MathX.Exp(exposure);
        return color / (1.0f + color);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float3 PerformInverse(float3 color, float exposure)
	{
        float oldLum = color.Luminance();
        float newLum = -(oldLum / MathX.Min((oldLum / (max_white * max_white)) - 1.0f, -0.1f));
		return color.ChangeLuminance(newLum);
	} 
}
