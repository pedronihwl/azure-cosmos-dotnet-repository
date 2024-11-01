// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Azure.CosmosRepository;

public interface IItemWithVectorSearch : IItem
{
    string Text { get; }
    float[] Embeddings { get; set; }
}