// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Azure.CosmosRepository.Processors;

interface ICosmosEmbeddingProcessor
{
    float[] CreateEmbeddings(string text);
}
