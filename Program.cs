using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.CommandLine;
using System.Numerics;
using System.Runtime.InteropServices;
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
            () => 0f,
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


        // Option<bool> exportEXR = new(
        //     "--exportEXR",
        //     () => false,
        //     "Exports an HDR (EXR) image at the specified start exposure");

        // exportEXR.AddAlias("-exr");

        
        Option<int?> maxConcurrency = new(
            "--maxConcurrency",
            () => null,
            "Defines the maximum amount of threads to use (e.g. how many pictures are generated at once) when defined - otherwise auto");

        maxConcurrency.AddAlias("-m");


        Option<float> whitePoint = new(
            "--whitePoint",
            () => 1f,
            "Defines the \"maximum\" white point of the input image - values higher than one will make highlights more saturated");

        whitePoint.AddAlias("-w");


        root.AddArgument(inputFile);
        root.AddOption(startExposure);
        root.AddOption(stepSize);
        root.AddOption(steps);
        root.AddOption(quickFit);
        // root.AddOption(exportEXR);
        root.AddOption(maxConcurrency);
        root.AddOption(whitePoint);


        root.SetHandler(Execute, startExposure, stepSize, steps, quickFit, inputFile, maxConcurrency, whitePoint);

        return root.Invoke(args);
    }



    /// <summary>
    /// Executes the main functionality of the program
    /// </summary>
    /// <param name="startExposure">Exposure to start approximating at</param>
    /// <param name="stepSize">The size of the exposure steps to take</param>
    /// <param name="steps">The amount of exposure steps to take</param>
    /// <param name="quickFit">Whether to perform a quick fit, if available</param>
    /// <param name="inputFile">The input image to round-trip tonemap</param>
    /// <param name="maxConcurrency">The maximum amount of concurrency (image jobs) that can run at once, limited by CPU core count</param>
    /// <param name="white_point">The maximum white point</param>
    // <param name="exportHDR">Whether to export accompanying EXR files for each exposure</param>
    public static void Execute(
        float startExposure,
        float stepSize,
        int steps,
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

        var range = Enumerable.Range(0, steps);
        
        
        IToneMapper from = new ReinhardLuminanceTonemapper(white_point);
        IToneMapper to = new ACESTonemapper(quickFit);
        

        ParallelQuery<int> query = range.AsParallel();

        if (maxConcurrency is int val)
        {
            query = query.WithDegreeOfParallelism(val);
            Console.WriteLine($"Setting max parallelism to {val}");
        }


        using Image source = Image.Load<Rgba32>(inputFile);


        query.ForAll(n =>
        {
            Image<Rgba32> image = source.CloneAs<Rgba32>();
            float exposure = startExposure + (n * stepSize);
            float roundedExp = MathF.Round(exposure, 3);
            
            
            Console.WriteLine($"Performing tonemapping for Image #{n}");
            
            
            image.Mutate(c => c.ProcessPixelRowsAsVector4(row => {
                Span<Vector128<float>> rowValues = MemoryMarshal.Cast<Vector4, Vector128<float>>(row);
                rowValues.PerformInverseImageTonemap(1f, from);
                rowValues.PerformImageTonemap(exposure, to);
            }));
            

            // if (exportHDR)
            // {
            //     using FileStream EXROutStream = File.OpenWrite($"./{OUTPUT_DIR}/#{n} (exposure {roundedExp}).tiff");
            //     var encoder = new TiffEncoder()
            //     {
            //         BitsPerPixel = TiffBitsPerPixel.Bit32
            //     };
            //     image.Save(EXROutStream, encoder);
            // }
            
            

            // image.Mutate(c => c.ProcessPixelRowsAsVector4(row => {
            //     Span<Vector128<float>> rowValues = MemoryMarshal.Cast<Vector4, Vector128<float>>(row);
            //     rowValues.PerformImageTonemap(exposure, to);
            // }));
        

            Console.WriteLine($"Finished Image #{n}");
            using FileStream outStream = File.OpenWrite($"./{OUTPUT_DIR}/#{n} (exposure {roundedExp}).bmp");


            Console.WriteLine($"Writing image {outStream.Name}");
            var encoder = new BmpEncoder();
            image.Save(outStream, encoder);
            Console.WriteLine($"Image #{n} finished processing!");
        });
    }
}