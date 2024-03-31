using System.Runtime.CompilerServices;
using Elements.Core;


namespace Scratch;

public struct ACESTonemapper(bool quick = false) : IToneMapper
{
    const float A = 0.0245786f;
	const float B = 0.000090537f;
	const float C = 0.983729f;
	const float D = 0.4329510f;
	const float E = 0.238081f;

    public static readonly float3x3 ACESInputMat = new
	(
		0.59719f, 0.35458f, 0.04823f,
		0.07600f, 0.90834f, 0.01566f,
		0.02840f, 0.13383f, 0.83777f
	);

	// ODT_SAT => XYZ => D60_2_D65 => sRGB
	public static readonly float3x3 ACESOutputMat = new(
		1.60475f, -0.53108f, -0.07367f,
		-0.10208f,  1.10813f, -0.00605f,
		-0.00327f, -0.07276f,  1.07602f
	);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float3 RRTAndODTFit(in float3 v)
    {
        float3 a = v * (v + A) - B;
        float3 b = v * (C * v + D) + E;
        return a / b;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float3 PerformTonemap(float3 color, float exposure)
	{
        color *= MathX.Exp(exposure);

        if (quick)
        {
            // Apply RRT and ODT
            color = RRTAndODTFit(color);
        }
        else
        {
            color = ACESInputMat * color;

            // Apply RRT and ODT
            color = RRTAndODTFit(color);

            color = ACESOutputMat * color;
        }

		// Clamp to [0, 1]
		color = MathX.Clamp(color, 0f, 1f);

		return color;
	}


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float3 PerformInverse(float3 color, float exposure)
    {
        return MathX.Abs(
			((A - D * color) -
			MathX.Sqrt(
				MathX.Pow(MathX.Abs(D * color - A), 2f) -
				4f * (C * color - 1f) * (B + E * color))) /
			(2f * (C * color - 1f)));
    }
}
