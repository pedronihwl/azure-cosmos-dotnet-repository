// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Azure.CosmosRepository;

public abstract class VectorSearchItem : Item, IItemWithVectorSearch
{

    public float[] Embeddings { get; set; } = [];

    string IItemWithVectorSearch.Text => GetText();
    protected abstract string GetText();
}