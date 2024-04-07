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