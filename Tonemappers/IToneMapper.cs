using System.Numerics;

namespace Scratch;

/// <summary>
/// Interface for a tonemapper.
/// </summary>
public interface IToneMapper
{
    /// <summary>
    /// Tonemaps HDR RGB values into SDR
    /// </summary>
    /// <param name="col">HDR RGB values</param>
    /// <param name="exposure">Exposure to tonemap at</param>
    /// <returns>SDR RGB values</returns>
    public Vector3 PerformTonemap(in Vector3 col, in float exposure);



    /// <summary>
    /// Extrapolates SDR RGB values into HDR
    /// </summary>
    /// <param name="col">SDR RGB values</param>
    /// <param name="exposure">Exposure to inversely tonemap at</param>
    /// <returns>HDR RGB Values</returns>
    public Vector3 PerformInverse(in Vector3 col, in float exposure);
}
