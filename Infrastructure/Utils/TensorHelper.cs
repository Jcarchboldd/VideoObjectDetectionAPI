using Microsoft.ML.OnnxRuntime.Tensors;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Infrastructure.Utils;

public static class TensorHelper
{
    public static DenseTensor<float> PreprocessImage(string imagePath)
    {
        // Load the image
        using var image = Image.Load<Rgb24>(imagePath);

        // Resize to the model's input size (e.g., 224x224)
        image.Mutate(x => x.Resize(224, 224));

        // Prepare tensor [1, 3, 224, 224]
        var tensor = new DenseTensor<float>(new[] { 1, 3, 224, 224 });

        // Populate the tensor with normalized pixel values (mean-subtracted and BGR reordered)
        for (int y = 0; y < 224; y++)
        {
            for (int x = 0; x < 224; x++)
            {
                var pixel = image[x, y];
                tensor[0, 0, y, x] = pixel.B - 103.939f; // Blue channel (BGR order)
                tensor[0, 1, y, x] = pixel.G - 116.779f; // Green channel
                tensor[0, 2, y, x] = pixel.R - 123.68f;  // Red channel
            }
        }

        return tensor;
    }
}