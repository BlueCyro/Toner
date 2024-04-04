using System.Numerics;


namespace Scratch;

/// <summary>
/// <para>Performs tonemapping via standard defined in Report ITU-R BT.2446-1: Page 12 of <see href="https://www.itu.int/dms_pub/itu-r/opb/rep/R-REP-BT.2446-1-2021-PDF-E.pdf"/></para>
/// <para>Keeps color relationships intact, more suitable for preserving the look of SDR content on HDR displays, re-tonemapping is lackluster</para>
/// </summary>
/// <param name="sdrNits">Input SDR nits</param>
/// <param name="targetNits">Output HDR nits</param>
/// <param name="fast">Whether to use the 'fast' path. NOTE: Fast path is currently broken</param>
public struct BT2556ATonemapper(float sdrNits = 100f, float targetNits = 1000f, bool fast = false) : IToneMapper
{
    static readonly Vector3 k_bt2020 = new(0.262698338956556f, 0.678008765772817f, 0.0592928952706273f);
    const float k_bt2020_r_helper = 1.47460332208689f; // 2 - 2 * 0.262698338956556
    const float k_bt2020_b_helper = 1.88141420945875f; // 2 - 2 * 0.0592928952706273
    const float inverse_gamma = 2.4f;
    const float gamma = 1f / inverse_gamma;
    const float a1 = 1.8712e-5f;
    const float b1 = -2.7334e-3f;
    const float c1 = 1.3141f;
    const float a2 = 2.8305e-6f;
    const float b2 = -7.4622e-4f;
    const float c2 = 1.2328f;


    /// <summary>
    /// Performs inverse tonemap assuming BT.2446 standard NOTE: NOT implemented
    /// </summary>
    /// <param name="col"></param>
    /// <param name="exposure"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public Vector3 PerformTonemap(in Vector3 col, in float exposure) => throw new NotImplementedException("Not implemented yet!!");


    // Credit goes to EndlesslyFlowering on github for this function
    // Rep. ITU-R BT.2446-1 Table 2-4 (inversed)
    // BT.2446 Method A inverse tone mapping (itm)
    /// <summary>
    /// Performs inverse tonmapping assuming BT.2446
    /// </summary>
    /// <param name="color"></param>
    /// <param name="exposure"></param>
    /// <returns></returns>
    public Vector3 PerformInverse(
        in Vector3 color,
        in float exposure)
    {   
        // RGB->R'G'B' gamma compression
        Vector3 col = color.Pow(gamma);


        // Rec. ITU-R BT.2020-2 Table 4
        // Y'tmo
        float yTmo = Vector3.Dot(col, k_bt2020);
        // C'b,tmo
        float cBTmo = (col.Z - yTmo) / k_bt2020_b_helper;
        // C'r,tmo
        float cRTmo = (col.X - yTmo) / k_bt2020_r_helper;


        // fast path as per Rep. ITU-R BT.2446-1 Table 4
        // matches the output of the inversed version for the given input
        if (fast && sdrNits > 99.0f && sdrNits < 101.0f && targetNits > 999.0f && targetNits < 1001.0f)
        {
            // Console.WriteLine("Fast path");
            sdrNits = 100.0f;
            targetNits = 1000.0f;

            float yy_ = 255.0f * yTmo;
            float t = 70.0f;
            float yy_Pow = MathF.Pow(yy_, 2f);


            float e = yy_ <= t ?
                a1 * yy_Pow + b1 * yy_ + c1 :
                a2 * yy_Pow + b2 * yy_ + c2;

            float yHdr = MathF.Pow(yy_, e);

            float sC = yTmo > 0.0f ?
                1.075f * (yHdr / yTmo) :
                1.0f;

            float cBHdr = cBTmo * sC;
            float cRHdr = cRTmo * sC;

            col = new(
                Math.Clamp(yHdr + k_bt2020_r_helper * cRHdr, 0.0f, 1000.0f),
                Math.Clamp(yHdr - 0.16455312684366f * cBHdr - 0.57135312684366f * cRHdr, 0.0f, 1000.0f),
                Math.Clamp(yHdr + k_bt2020_b_helper * cBHdr, 0.0f, 1000.0f));

            col /= 1000f;
        }
        else
        {
            // Console.WriteLine("Slow path");
            // adjusted luma component (inverse)
            // get Y'sdr
            float ySdr = yTmo + Math.Max(0.1f * cRTmo, 0.0f);

            // Tone mapping step 3 (inverse)
            // get Y'c
            float pSdr = 1 + 32 * MathF.Pow(sdrNits / 10000.0f, gamma);
            float yC = MathF.Log((ySdr * (pSdr - 1f)) + 1f) / MathF.Log(pSdr); // log = ln

            // Tone mapping step 2 (inverse)
            // get Y'p
            float yP = 0.0f;
            float yP0 = yC / 1.0770f;
            float yP2 = (yC - 0.5000f) / 0.5000f;
            float first = -2.7811f;
            float sqrt = MathF.Sqrt(4.83307641f - 4.604f * yC);
            float div = -2.302f;
            float yP1 = (first + sqrt) / div;


            if (yP0 <= 0.7399f)
                yP = yP0;
            else if (yP1 > 0.7399f && yP1 < 0.9909f)
                yP = yP1;
            else if (yP2 >= 0.9909f)
                yP = yP2;
            else //y_p_1 sometimes (about 0.12% out of the full RGB range)
                 //is less than 0.7399f or more than 0.9909f because of float inaccuracies
            {
                // error is small enough (less than 0.001) for this to be OK
                // ideally you would choose between y_p_0 and y_p_1 if y_p_1 < 0.7399f depending on which is closer to 0.7399f
                // or between y_p_1 and y_p_2 if y_p_1 > 0.9909f depending on which is closer to 0.9909f
                yP = yP1;


                // this clamps it to 2 float steps above 0.7399f or 2 float steps below 0.9909f
                // if (y_p_1 < 0.7399f)
                //     y_p = 0.7399001f;
                // else
                //     y_p = 0.99089986f;
            }


            // Tone mapping step 1 (inverse)
            // get Y'
            float pHdr = 1f + 32f * MathF.Pow(targetNits / 10000f, gamma);
            float y_ = MathF.Pow(pHdr, yP) - 1f / (pHdr - 1);


            // Colour scaling function
            float colScale = 0f;
            if (y_ > 0f) // avoid divison by zero
                colScale = ySdr / (1.1f * y_);

            // Colour difference signals (inverse) and Luma (inverse)
            // get R'G'B'
            // clamp for safety
            col = new(
                Math.Clamp(cBTmo * k_bt2020_b_helper / colScale + y_, 0f, 1f),
                Math.Clamp(cRTmo * k_bt2020_r_helper / colScale + y_, 0f, 1f),
                Math.Clamp((y_ - (k_bt2020.X * col.X + k_bt2020.Z * col.Z)) / k_bt2020.Y, 0f, 1f));
        }

        // R'G'B' gamma expansion
        col = col.Pow(inverse_gamma);

        // map target luminance into 10000 nits
        col *= targetNits;

        return col;
    }
}
