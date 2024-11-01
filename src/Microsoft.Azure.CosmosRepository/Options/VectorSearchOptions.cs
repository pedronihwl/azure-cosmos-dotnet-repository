// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Azure.CosmosRepository.Options;

public class VectorSearchOptions
{
    public DistanceFunction DistanceFunction { get; set; }
    public VectorIndexType VectorIndexType { get; set; }
    public int Dimensions { get; set; }

    public string? Path { get; private set; }

    internal void WithPath(string path)
    {
        Path = path;
    }

    public static VectorSearchOptions Default =>
        new()
        {
            DistanceFunction = DistanceFunction.Cosine,
            VectorIndexType = VectorIndexType.DiskANN,
            Dimensions = 4096
        };
}