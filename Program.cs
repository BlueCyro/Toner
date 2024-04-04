using ImageMagick;
using System.Collections.Concurrent;
using System.CommandLine;
using System.Numerics;
using System.Runtime.Intrinsics;


namespace Scratch;

/// <summary>
/// Main program
/// </summary>
public class Program
{
    const string OUTPUT_DIR = "./output";
    const string DEFAULT_INPUT_FILE = "./input.png";

    /// <summary>
    /// Main entry point for the program
    /// </summary>
    /// <param name="args">Optional arguments</param>
    /// <returns></returns>
    public static int Main(string[] args)
    {
        RootCommand root = new("Takes an SDR image and applies round-trip tonemapping. (Upscales SDR to HDR with inverse tonemapping, re-tonemaps with BakingLab ACES)");

        Argument<string> inputFile = new(
            "inputFile",
            () => DEFAULT_INPUT_FILE,
            "The file to operate on");


        Option<float> startExposure = new(
            "--startExposure",
            () => -6f,
            "The exposure level to start at");
        
        startExposure.AddAlias("-e");


        Option<float> stepSize = new(
            "--stepSize",
            () => 0.2f,
            "The size of each exposure step to take");

        stepSize.AddAlias("-s");


        Option<int> steps = new(
            "--steps",
            () => 6,
            "The amount of exposure steps generate starting from the initial exposure (can be negative)");

        steps.AddAlias("-c");


        Option<bool> quickFit = new(
            "--quickFit",
            () => false,
            "Skips the matrix multiplication for ACES (results in slightly over-saturated bright colors)");

        quickFit.AddAlias("-q");


        Option<bool> exportEXR = new(
            "--exportEXR",
            () => false,
            "Exports an HDR (EXR) image at the specified start exposure");

        exportEXR.AddAlias("-exr");

        
        Option<int?> maxConcurrency = new(
            "--maxConcurrency",
            () => null,
            "Defines the maximum amount of threads to use (e.g. how many pictures are generated at once) when defined - otherwise auto");

        maxConcurrency.AddAlias("-m");


        Option<float> whitePoint = new(
            "--whitePoint",
            () => 1f,
            "Defines the \"maximum\" white point of the image - values higher than one will make highlights more saturated");

        whitePoint.AddAlias("-w");


        root.AddArgument(inputFile);
        root.AddOption(startExposure);
        root.AddOption(stepSize);
        root.AddOption(steps);
        root.AddOption(quickFit);
        root.AddOption(exportEXR);
        root.AddOption(maxConcurrency);
        root.AddOption(whitePoint);


        root.SetHandler(Execute, startExposure, stepSize, steps, exportEXR, quickFit, inputFile, maxConcurrency, whitePoint);

        return root.Invoke(args);
    }



    /// <summary>
    /// Executes the main functionality of the program
    /// </summary>
    /// <param name="startExposure">Exposure to start approximating at</param>
    /// <param name="stepSize">The size of the exposure steps to take</param>
    /// <param name="steps">The amount of exposure steps to take</param>
    /// <param name="exportEXR">Whether to export accompanying EXR files for each exposure</param>
    /// <param name="quickFit">Whether to perform a quick fit, if available</param>
    /// <param name="inputFile">The input image to round-trip tonemap</param>
    /// <param name="maxConcurrency">The maximum amount of concurrency (image jobs) that can run at once, limited by CPU core count</param>
    /// <param name="white_point">The maximum white point</param>
    public static void Execute(
        float startExposure,
        float stepSize,
        int steps,
        bool exportEXR,
        bool quickFit,
        string inputFile,
        int? maxConcurrency,
        float white_point)
    {
        if (!Directory.Exists(OUTPUT_DIR))
            Directory.CreateDirectory(OUTPUT_DIR);


        if (!File.Exists(inputFile))
        {
            Console.WriteLine($"{inputFile} not found! Please make sure to provide an input image");
            return;
        }


        var settings = new MagickReadSettings()
        {
            ColorSpace = ColorSpace.sRGB,
        };


        var range = Enumerable.Range(0, steps);
        
        
        IToneMapper from = new BT2446ATonemapper(100f, 1000f);
        IToneMapper to = new ACESTonemapper(quickFit);
        

        ParallelQuery<int> query = range.AsParallel();

        if (maxConcurrency is int val)
        {
            query = query.WithDegreeOfParallelism(val);
            Console.WriteLine($"Setting max parallelism to {val}");
        }

        ConcurrentBag<(MagickImage image, FileStream stream)> streams = [];

        query.ForAll(n =>
        {
            var fromFile = new MagickImage(inputFile, settings);
            var pixels = fromFile.GetPixels();
            float exposure = startExposure + (n * stepSize);


            Console.WriteLine($"Performing tonemapping for Image #{n}");
            pixels.PerformImageTonemap(exposure, from, to, alpha: fromFile.HasAlpha);
            Console.WriteLine($"Finished Image #{n}");


            float roundedExp = MathF.Round(exposure, 3);
            // fromFile.Settings.SetDefine(MagickFormat.Png, "compression-level", 5);
            // fromFile.Settings.SetDefine(MagickFormat.Png, "compression-strategy", 2);
            streams.Add((fromFile, File.OpenWrite($"./{OUTPUT_DIR}/#{n} (exposure {roundedExp}).bmp")));
            // fromFile.Write(newImage);

            if (exportEXR)
            {
                var fromEXR = new MagickImage(inputFile, settings);
                var exrPixels = fromEXR.GetPixels();
                exrPixels.PerformImageTonemap(exposure, from, to, false);


                fromEXR.Format = MagickFormat.Exr;
                //fromEXR.Settings.SetDefine(MagickFormat.Exr, "color-type", "RGBA");
                fromEXR.SetCompression(CompressionMethod.Zip);
                streams.Add((fromEXR, File.OpenWrite($"./{OUTPUT_DIR}/#{n} (exposure {roundedExp}).exr")));
                
                
                // fromEXR.Write(newEXR);
            }
        });

        streams.AsParallel().ForAll(s =>
        {
            Console.WriteLine($"Writing image {s.stream.Name}");
            s.image.Write(s.stream);
            s.image.Dispose();
            s.stream.Dispose();
        });
    }
}