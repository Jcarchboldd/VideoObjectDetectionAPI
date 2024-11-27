using System.Reflection;
using Domain.Entities;
using Infrastructure.Utils;
using Microsoft.ML.OnnxRuntime;

namespace Infrastructure.ObjectDetection;

public class OnnxObjectDetector
{
    private readonly InferenceSession _session;
    private readonly string[] _labels;

    public OnnxObjectDetector()
    {
        // Get the directory of the executing assembly
        var assemblyDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        if (assemblyDirectory == null)
        {
            throw new InvalidOperationException("Unable to determine the assembly directory.");
        }

        // Locate the Resources folder
        var resourcesPath = Path.Combine(assemblyDirectory, "Resources");

        // Set the ONNX model path
        var modelPath = Path.Combine(resourcesPath, "googlenet-3.onnx");
        if (!File.Exists(modelPath))
        {
            throw new FileNotFoundException("The ONNX model file was not found.", modelPath);
        }

        // Initialize the ONNX Inference Session
        _session = new InferenceSession(modelPath);

        // Set the labels file path
        var labelsPath = Path.Combine(resourcesPath, "imagenet-simple-labels.json");
        if (!File.Exists(labelsPath))
        {
            throw new FileNotFoundException("Labels file not found.", labelsPath);
        }

        // Load labels into memory
        _labels = File.ReadAllLines(labelsPath);
    }

    public Task<IEnumerable<DetectionResult>> DetectObjectsAsync(string framePath)
    {
        // Ensure the frame file exists
        if (!File.Exists(framePath))
        {
            throw new FileNotFoundException("The frame file was not found.", framePath);
        }

        // Preprocess the image into a tensor
        var inputTensor = TensorHelper.PreprocessImage(framePath);

        // Run inference
        using var results = _session.Run(new List<NamedOnnxValue>
        {
            NamedOnnxValue.CreateFromTensor("data_0", inputTensor)
        });

        // Get probabilities
        var probabilities = results.First().AsTensor<float>().ToArray();

        // Apply softmax to normalize probabilities
        var expScores = probabilities.Select(MathF.Exp).ToArray();
        var sumExpScores = expScores.Sum();
        var softmaxProbabilities = expScores.Select(score => score / sumExpScores).ToArray();

        // Get the top predictions
        var topPredictions = softmaxProbabilities
            .Select((value, index) => new { Value = value, Label = _labels[index] })
            .OrderByDescending(p => p.Value)
            .Take(5) // Get the top 5 predictions
            .ToList();

        // Log the predictions for debugging
        Console.WriteLine($"Predictions for frame: {framePath}");
        foreach (var prediction in topPredictions)
        {
            Console.WriteLine($"Label: {prediction.Label}, Probability: {prediction.Value}");
        }

        // Map the top predictions to DetectionResult objects
        var detectionResults = topPredictions.Select(p => new DetectionResult
        {
            ObjectName = p.Label,
            ConfidenceScore = p.Value,
            FrameFilePath = framePath
        });

        return Task.FromResult(detectionResults);
    }
}
