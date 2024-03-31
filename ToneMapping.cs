using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Elements.Core;
using ImageMagick;


namespace Scratch;



public static class ToneMapping
{
    public static float3 PerformTonemap(float3 col, float exposure, bool roundTrip, IToneMapper from, IToneMapper to)
    {
        col /= 65535f;
        col = SrgbToLinear(col);
        col = from.PerformInverse(col, 0f);

        if (roundTrip)
        {
            col = to.PerformTonemap(col, exposure);
            col = MathX.Clamp(col, 0f, 1f);
            col = LinearToSrgb(col);
        }
        else
        {
            col *= MathX.Exp(exposure);
        }

        col *= 65535f;

        return col;
    }


    public static void PerformImageTonemap(this IPixelCollection<float> pixels, float exposure, IToneMapper from, IToneMapper to, bool roundTrip = true, bool alpha = false)
    {
        float[]? values = pixels.GetValues();
        Span<float> newValues = values.AsSpan();
        if (alpha)
        {
            Span<float4> colors = MemoryMarshal.Cast<float, float4>(newValues); // Interpret the values as a float3 span - GO VERY FAST
            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = new(PerformTonemap(colors[i].xyz, exposure, roundTrip, from, to), colors[i].W); // Tonemap that guy
            }
            newValues = MemoryMarshal.Cast<float4, float>(colors); // Disguise it as my own cooking (interpret back to floats)
        }
        else
        {
            Span<float3> colors = MemoryMarshal.Cast<float, float3>(values.AsSpan());
            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = PerformTonemap(colors[i], exposure, roundTrip, from, to);
            }
            newValues = MemoryMarshal.Cast<float3, float>(colors);
        }
        pixels.SetPixels(newValues); // Write the new pixels to the 
    }


    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float3 SrgbToLinear(in float3 color)
    {
        bool3 isLo = color <= 0.04045f;

        float3 loPart = color / 12.92f;
        float3 hiPart = MathX.Pow((color + 0.055f) / 1.055f, 12.0f / 5.0f);
        return loPart.Mask(isLo, hiPart);
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float3 LinearToSrgb(in float3 color)
    {
        bool3 isLo = color <= 0.0031308f;

        float3 loPart = color * 12.92f;
        float3 hiPart = MathX.Pow(color, 5.0f / 12.0f) * 1.055f - 0.055f;
        return loPart.Mask(isLo, hiPart);
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float3 ChangeLuminance(this float3 color, float outLum)
    {
        return color * (outLum / color.Luminance());
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Luminance(this float3 color)
    {
        return MathX.Dot(color, new(0.2126f, 0.7152f, 0.0722f));
    }
}