# 🖨️✒️ Toner: Easy peasy round-trip tonemapping!

Toner is a simple round-trip tonemapper that can be used to extrapolate HDR 'exposures' from a single SDR image.

This is accomplished by first inverse-tonemapping the image with a Luminance-based Reinhard tonemapper, then *re*-tonemapping the image with [BakingLab ACES](https://github.com/TheRealMJP/BakingLab/blob/master/BakingLab/ACES.hlsl).

This of course doesn't necessarily give you perfect reproduction, but a Luminance-based Reinhard appears to be a pretty good all-rounder when it comes to peak detection to retain those sparkly highlights.


# Usage

```
Toner [<inputFile>] [options]

<inputFile>  The file to operate on [default: ./input.png]

Options:
  -e, --startExposure <startExposure>    The exposure level to start at [default: -2]

  -s, --stepSize <stepSize>              The size of each exposure step to take [default: 0.2]

  -c, --steps <steps>                    The amount of exposure steps generate starting from the initial exposure (can be negative) [default: 20]

  -q, --quickFit                         Skips the matrix multiplication for ACES (results in slightly over-saturated bright colors) [default: False]

  -m, --maxConcurrency <maxConcurrency>  Defines the maximum amount of threads to use (e.g. how many pictures are generated at once) when defined - otherwise auto []

  -w, --whitePoint <whitePoint>          Defines the "maximum" white point of the input image - values higher than one will make highlights more saturated [default: 1]

  -ct, --contrast <contrast>             Defines the contrast to apply post-tonemapping [default: 1]
  ```

  For example, if you wanted to tonemap an image starting at -2 exposure, and extrapolate 20 steps up by 0.2 increments, you'd do:

  * `Toner -e -2 -s 0.2 -c 20`

# Examples

Here is an example from a picture in a game known as [Resonite](https://www.resonite.com). Resonite notably does not have any tonemapping or color grading, and thus has no curve applied to the RGB output color of the game. This makes it prime for use with this tool, as there are are no competing transforms on the image. The following was generated using the defaults, but with a contrast of 0.88 to loosen the deep blacks up a bit.


Original:

<img src="Examples/Porch%20Original.jpg" width="600" />

Extrapolated -2 exposure:

<img src="Examples/Porch%20-2.png" width="600" />

Extrapolated -0.6 exposure:

<img src="Examples/Porch%20-0.6.png" width="600" />

Extrapolated 0.8 exposure:

<img src="Examples/Porch%200.8.png" width="600" />

# What's going on here?

You may note that the color transform of the image has changed, and the overall look as taken on a more filmic appearance. This is because to achieve a more realistic representation of exposure, HDR data is extrapolated assuming a luminance-based inverse Reinhard function (this just means that the SDR image is blown up into HDR with a math curve), after which, it is premultiplied according to the current exposure and the SDR data is restored using the BakingLab ACES tonemapping function. (this means squishing the HDR data back down to what your monitor can display)

The ACES tonemapper is what gives the image this filmic appearance, and is what makes the color take on more realistic properties. In sum, it tries to map HDR values into SDR in a pleasing way that is more reminiscent of how your eye sees dynamic scenes in real life. For example: being able to observe a bright sunny beach, but also see under an umbrella at the same time even though the sun is making the beach orders of magnitude brighter than under the umbrella.


<p align="right">
    <sub>
        💙💚 Feel free to support me via <a href="https://paypal.me/BlueCyro">PayPal</a>
    </sub>
</div>
