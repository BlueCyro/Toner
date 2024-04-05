using System.Runtime.CompilerServices;
using System.Numerics;
using System.Runtime.Intrinsics;


namespace Scratch;



/// <summary>
/// All things tonemapping-related
/// </summary>
public static class ToneMapping
{
    static readonly Vector3 LuminanceWeight = new(0.2126f, 0.7152f, 0.0722f);
    static readonly Vector3 sRGBHelperConstant = new(0.055f);
    static readonly Vector3 sRGBToLinearAddend = sRGBHelperConstant / 1.055f;
    static readonly Vector3 sRGBLowThreshold = new(0.04045f);
    static readonly Vector3 LinearLowThreshold = new(0.0031308f);
    const float sRGBToLinearPower = 12f / 5f;
    const float LinearTosRGBPower = 5f / 12f;



    /// <summary>
    /// Performs an image-wide inverse tonemap
    /// </summary>
    /// <param name="pixels">Span of pixels to tonemap</param>
    /// <param name="exposure">Exposure to inversely tonemap at. NOTE: Currently non-functional</param>
    /// <param name="toneMapper">The tonemap to assume as the inverse</param>
    // /// <param name="to">What tonemap to use for re-mapping the values back to SDR</param>
    // /// <param name="roundTrip">True if the the pixels should be re-tonemapped at all (produces approximated HDR output)</param>
    // /// <param name="alpha">True if the input contains alpha</param>
    public static void PerformInverseImageTonemap(this ref Span<Vector128<float>> pixels, in float exposure, in IToneMapper toneMapper)
    {
        for (int i = 0; i < pixels.Length; i++)
        {
            // We can actually avoid a whole mess of branches here with the interpretation to Vector3
            pixels[i] = new Vector4(toneMapper.PerformInverse(SrgbToLinear(pixels[i].AsVector3()), exposure), pixels[i][3]).AsVector128(); // Extrapolate that guy
        }
    }



    /// <summary>
    /// Performs an image-wide tonemap
    /// </summary>
    /// <param name="pixels">Collection of pixels to tonemap</param>
    /// <param name="exposure">Exposure to tonemap at</param>
    /// <param name="toneMapper">The tonemap to use</param>
    public static void PerformImageTonemap(this ref Span<Vector128<float>> pixels, in float exposure, in IToneMapper toneMapper)
    {
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = new Vector4(LinearToSrgb(toneMapper.PerformTonemap(pixels[i].AsVector3(), exposure)), pixels[i][3]).AsVector128(); // Tonemap that guy
        }
    }



    /// <summary>
    /// Converts sRGB values into linear RGB values
    /// </summary>
    /// <param name="color">sRGB values</param>
    /// <returns>Linear RGB values</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 SrgbToLinear(in Vector3 color)
    {
        Vector128<float> isLo = color.LessThanOrEqual(sRGBLowThreshold);

        Vector3 loPart = color / 12.92f;
        Vector3 hiPart = (color + sRGBToLinearAddend).Pow(sRGBToLinearPower);
        return loPart.Mask(hiPart, isLo);
    }



    /// <summary>
    /// Converts linear RGB values into sRGB values
    /// </summary>
    /// <param name="color">Linear RGB values</param>
    /// <returns>sRGB values</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 LinearToSrgb(in Vector3 color)
    {
        Vector128<float> isLo = color.LessThanOrEqual(LinearLowThreshold);

        Vector3 loPart = color * 12.92f;
        Vector3 hiPart = color.Pow(LinearTosRGBPower) * 1.055f - sRGBHelperConstant;
        return loPart.Mask(hiPart, isLo);
    }



    /// <summary>
    /// Changes the luminance of RGB values
    /// </summary>
    /// <param name="color">RGB values to change the luminance of</param>
    /// <param name="newLum">What the luminance should be changed to</param>
    /// <returns>Luminance-adjusted RGB values</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 ChangeLuminance(in this Vector3 color, in float newLum)
    {
        return color * (newLum / color.Luminance());
    }



    /// <summary>
    /// Gets the perceived luminance of an RGB color
    /// </summary>
    /// <param name="color">The RGB values to get the luminance of</param>
    /// <returns>The perceived luminance of the input</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Luminance(in this Vector3 color)
    {
        return Vector3.Dot(color, LuminanceWeight);
    }
}