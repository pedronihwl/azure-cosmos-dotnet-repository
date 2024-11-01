// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

using System.Collections.ObjectModel;

namespace Microsoft.Azure.CosmosRepository.Providers;

class DefaultCosmosVectorSearchProvider(IOptions<RepositoryOptions> options) : ICosmosVectorSearchProvider
{

    public (VectorEmbeddingPolicy? policy, IndexingPolicy? indexing) GetVectorSearchConfiguration(Type itemType)
    {
        ContainerOptionsBuilder? optionsBuilder = options.Value.GetContainerOptions(itemType);

        if (optionsBuilder is { VectorSearchOptions: not null })
        {
            VectorSearchOptions? vectorOptions = optionsBuilder.VectorSearchOptions;

            var policy = new VectorEmbeddingPolicy([
                new Embedding
                {
                    Dimensions = vectorOptions.Dimensions,
                    Path = vectorOptions.Path,
                    DataType = VectorDataType.Float32,
                    DistanceFunction = vectorOptions.DistanceFunction
                }
            ]);

            var indexing = new IndexingPolicy()
            {
                VectorIndexes =
                [
                    new VectorIndexPath()
                    {
                        Path = vectorOptions.Path,
                        Type = vectorOptions.VectorIndexType,
                    }
                ]
            };

            indexing.IncludedPaths.Add(new IncludedPath{ Path = "/" });
            indexing.ExcludedPaths.Add(new ExcludedPath { Path = vectorOptions.Path + "/*" });

            return (policy, indexing);
        }

        return (null, null);
    }

}