using Elements.Core;


namespace Scratch;

public struct ReinhardTonemapper : IToneMapper
{
    public float3 PerformTonemap(float3 color, float exposure)
    {
        color *= MathX.Exp(exposure);
        return color / (1.0f + color);
    }

    public float3 PerformInverse(float3 color, float exposure)
	{
		return -(color / MathX.Min(color - 1.0f, -0.1f));
	}
}
