using System.Runtime.CompilerServices;
using System.Numerics;


namespace Scratch;

/// <summary>
/// Performs BakingLab ACES tonemapping: <see href="https://github.com/TheRealMJP/BakingLab/blob/master/BakingLab/ACES.hlsl"/>
/// </summary>
/// <param name="quick">If true, skips the matrix multiplication when <see cref="PerformTonemap"/> is called. Saturates brighter colors.</param>
public struct ACESTonemapper(bool quick = false) : IToneMapper
{
    static Vector3 A = new(0.0245786f);
	static Vector3 B = new(0.000090537f);
	static Vector3 C = new(0.983729f);
	static Vector3 D = new(0.4329510f);
	static Vector3 E = new(0.238081f);

    /// <summary>
    /// Input color matrix for fitting
    /// </summary>
    public static readonly Matrix4x4 ACESInputMat = Matrix4x4.Transpose(new(
		0.59719f, 0.35458f, 0.04823f, 0f,
		0.07600f, 0.90834f, 0.01566f, 0f,
		0.02840f, 0.13383f, 0.83777f, 0f,
        0f, 0f, 0f, 1f
	));

	// ODT_SAT => XYZ => D60_2_D65 => sRGB
    /// <summary>
    /// Output color matrix for fitting
    /// </summary>
	public static readonly Matrix4x4 ACESOutputMat = Matrix4x4.Transpose(new(
		1.60475f, -0.53108f, -0.07367f, 0f,
		-0.10208f,  1.10813f, -0.00605f, 0f,
		-0.00327f, -0.07276f,  1.07602f, 0f,
        0f, 0f, 0f, 1f
	));



    /// <summary>
    /// Fits an HDR input color to the ACES curve
    /// </summary>
    /// <param name="v">HDR RGB values to be fitted</param>
    /// <returns>Fitted SDR RGB values</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 RRTAndODTFit(in Vector3 v)
    {
        Vector3 a = v * (v + A) - B;
        Vector3 b = v * (C * v + D) + E;
        return a / b;
    }



    /// <summary>
    /// Tonemaps an HDR color into SDR using an ACES tonemap
    /// </summary>
    /// <param name="col">HDR RGB values to be tonemapped</param>
    /// <param name="exposure">Exposure to tonemap at</param>
    /// <returns>Tonemapped SDR RGB values</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Vector3 PerformTonemap(in Vector3 col, in float exposure)
	{
        Vector3 color = col * MathF.Exp(exposure);

        if (quick)
        {
            // Apply RRT and ODT
            color = RRTAndODTFit(col);
        }
        else
        {
            color = Vector3.Transform(col, ACESInputMat);

            // Apply RRT and ODT
            color = RRTAndODTFit(col);

            color = Vector3.Transform(col, ACESOutputMat);
        }

		// Clamp to [0, 1]
		color = Vector3.Clamp(col, Vector3.Zero, Vector3.One);

		return color;
	}



    /// <summary>
    /// Inversely maps SDR RGB values into HDR RGB values assuming ACES
    /// </summary>
    /// <param name="color">SDR RGB values to be up-mapped</param>
    /// <param name="exposure">Exposure to up-map at</param>
    /// <returns>Approximated HDR RGB values</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Vector3 PerformInverse(in Vector3 color, in float exposure)
    {
        return Vector3.Abs(
			((A - D * color) -
			Vector3.SquareRoot(
				Vector3.Abs(D * color - A).Pow(2f) -
				4f * (C * color - Vector3.One) * (B + E * color))) /
			(2f * (C * color - Vector3.One)));
    }
}
