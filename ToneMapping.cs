using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Numerics;
using ImageMagick;


namespace Scratch;



/// <summary>
/// All things tonemapping-related
/// </summary>
public static class ToneMapping
{
    static readonly Vector3 sRGBHelperConstant = new(0.055f);
    static readonly Vector3 sRGBToLinearAddend = sRGBHelperConstant / 1.055f;
    static readonly Vector3 sRGBLowThreshold = new(0.04045f);
    static readonly Vector3 LinearLowThreshold = new(0.0031308f);
    const float sRGBToLinearPower = 12f / 5f;
    const float LinearTosRGBPower = 5f / 12f;



    /// <summary>
    /// Performs a round-trip tonemap on a single RGB color
    /// </summary>
    /// <param name="color">The color to round-trip tonemap</param>
    /// <param name="exposure">The exposure to tonemap at</param>
    /// <param name="roundTrip">True if the color should be re-tonemapped at all (produces approximated HDR output)</param>
    /// <param name="from">What tonemap to assume as the inverse</param>
    /// <param name="to">What tonemap to use for re-mapping the values back to SDR</param>
    /// <returns>Re-tonemapped or approximated HDR RGB color</returns>
    public static Vector3 PerformTonemap(in Vector3 color, in float exposure, in bool roundTrip, in IToneMapper from, in IToneMapper to)
    {
        Vector3 col = from.PerformInverse(SrgbToLinear(color / 65535f), 1f);


        if (roundTrip)
            col = LinearToSrgb(Vector3.Clamp(to.PerformTonemap(col, exposure), Vector3.Zero, Vector3.One));
        else
            col *= (float)Math.Exp(exposure);


        col *= 65535f;

        return col;
    }



    /// <summary>
    /// Performs an image-wide tonemap
    /// </summary>
    /// <param name="pixels">Collection of pixels to tonemap</param>
    /// <param name="exposure">Exposure to tonemap at</param>
    /// <param name="from">What tonemap to assume as the inverse</param>
    /// <param name="to">What tonemap to use for re-mapping the values back to SDR</param>
    /// <param name="roundTrip">True if the the pixels should be re-tonemapped at all (produces approximated HDR output)</param>
    /// <param name="alpha">True if the input contains alpha</param>
    public static void PerformImageTonemap(this IPixelCollection<float> pixels, float exposure, IToneMapper from, IToneMapper to, bool roundTrip = true, bool alpha = false)
    {
        float[]? values = pixels.GetValues();
        Span<float> newValues = values.AsSpan();
        if (alpha) // Vector4 if alpha
        {
            Span<Vector4> colors = MemoryMarshal.Cast<float, Vector4>(newValues); // Interpret the values as a Vector3 span - GO VERY FAST


            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = new Vector4(
                    PerformTonemap(new Vector3(colors[i].X, colors[i].Y, colors[i].Z), exposure, roundTrip, from, to),
                    colors[i].W); // Tonemap that guy
            }


            newValues = MemoryMarshal.Cast<Vector4, float>(colors); // Disguise it as my own cooking (interpret back to floats)
        }
        else // The same, but with just Vector3
        {
            Span<Vector3> colors = MemoryMarshal.Cast<float, Vector3>(values.AsSpan());


            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = PerformTonemap(colors[i], exposure, roundTrip, from, to);
            }


            newValues = MemoryMarshal.Cast<Vector3, float>(colors);
        }
        pixels.SetPixels(newValues); // Write the new pixels to the 
    }



    /// <summary>
    /// Converts sRGB values into linear RGB values
    /// </summary>
    /// <param name="color">sRGB values</param>
    /// <returns>Linear RGB values</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 SrgbToLinear(in Vector3 color)
    {
        Span<bool> isLo = color.LessThanOrEqual(sRGBLowThreshold);

        Vector3 loPart = color / 12.92f;
        Vector3 hiPart = (color + sRGBToLinearAddend).Pow(sRGBToLinearPower);
        return loPart.Mask(isLo, hiPart);
    }



    /// <summary>
    /// Converts linear RGB values into sRGB values
    /// </summary>
    /// <param name="color">Linear RGB values</param>
    /// <returns>sRGB values</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 LinearToSrgb(in Vector3 color)
    {
        Span<bool> isLo = color.LessThanOrEqual(LinearLowThreshold);

        Vector3 loPart = color * 12.92f;
        Vector3 hiPart = color.Pow(LinearTosRGBPower) * 1.055f - sRGBHelperConstant;
        return loPart.Mask(isLo, hiPart);
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
        return Vector3.Dot(color, new(0.2126f, 0.7152f, 0.0722f));
    }
}