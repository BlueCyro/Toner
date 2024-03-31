using Elements.Core;


namespace Scratch;

public interface IToneMapper
{
    public float3 PerformTonemap(float3 col, float exposure);
    public float3 PerformInverse(float3 col, float exposure);
}
