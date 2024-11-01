// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Azure.CosmosRepository.Providers;

public interface ICosmosVectorSearchProvider
{
    (VectorEmbeddingPolicy? policy, IndexingPolicy? indexing) GetVectorSearchConfiguration(Type itemType);
}