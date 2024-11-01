// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

// ReSharper disable once CheckNamespace
namespace Microsoft.Azure.CosmosRepository;

internal sealed partial class DefaultRepository<TItem>
{
    /// <inheritdoc/>
    public async ValueTask<TItem?> TryGetAsync(
        string id,
        string? partitionKeyValue = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await GetAsync(id, partitionKeyValue, cancellationToken);
        }
        catch (CosmosException e) when (e.StatusCode is HttpStatusCode.NotFound)
        {
            logger.LogItemNotFoundHandled<TItem>(id, partitionKeyValue ?? id, e);
            return default;
        }
    }

    /// <inheritdoc/>
    public ValueTask<TItem> GetAsync(
        string id,
        string? partitionKeyValue = null,
        CancellationToken cancellationToken = default) =>
        GetAsync(id, new PartitionKey(partitionKeyValue ?? id), cancellationToken);

    /// <inheritdoc/>
    public async ValueTask<TItem> GetAsync(
        string id,
        PartitionKey partitionKey,
        CancellationToken cancellationToken = default)
    {
        Container container =
            await containerProvider.GetContainerAsync().ConfigureAwait(false);

        if (partitionKey == default)
        {
            partitionKey = new PartitionKey(id);
        }

        logger.LogPointReadStarted<TItem>(id, partitionKey.ToString());

        ItemResponse<TItem> response =
            await container.ReadItemAsync<TItem>(id, partitionKey, cancellationToken: cancellationToken)
                .ConfigureAwait(false);

        TItem item = response.Resource;

        logger.LogPointReadExecuted<TItem>(response.RequestCharge);
        logger.LogItemRead(item);

        return repositoryExpressionProvider.CheckItem(item);
    }

    /// <inheritdoc/>
    public async ValueTask<IEnumerable<TItem>> GetAsync(
        Expression<Func<TItem, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        Container container =
            await containerProvider.GetContainerAsync().ConfigureAwait(false);

        IQueryable<TItem> query =
            container.GetItemLinqQueryable<TItem>(
                    linqSerializerOptions: optionsMonitor.CurrentValue.SerializationOptions)
                .Where(repositoryExpressionProvider.Build(predicate));

        logger.LogQueryConstructed(query);

        (IEnumerable<TItem> items, var charge) =
            await cosmosQueryableProcessor.IterateAsync(query, cancellationToken);

        logger.LogQueryExecuted(query, charge);

        return items;
    }

    /// <inheritdoc/>
    public async ValueTask<IEnumerable<TItem>> GetByQueryAsync(
        string query,
        CancellationToken cancellationToken = default)
    {
        Container container =
            await containerProvider.GetContainerAsync().ConfigureAwait(false);

        TryLogDebugDetails(logger, () => $"Read {query}");

        QueryDefinition queryDefinition = new(query);
        return await cosmosQueryableProcessor.IterateAsync<TItem>(container, queryDefinition, cancellationToken);
    }

    public ValueTask<TItem> GetVectorSearchAsync(string text, int K, bool? force = false)
    {
        // Get the configuration

        if (cosmosEmbeddingProcessor == null)
        {
            throw new Exception();
        }

        ContainerOptionsBuilder? options = optionsMonitor.CurrentValue.GetContainerOptions<TItem>();

        var path = options!.VectorSearchOptions!.Path!.Replace("/", string.Empty);

        var queryText = $"SELECT TOP {K} c.id, VectorDistance(c.{path}, @vectorEmbedding) AS similarityScore" +
                        $"FROM c ORDER BY VectorDistance(c.{path}, @vectorEmbedding, {force})";

        var queryVector = cosmosEmbeddingProcessor.CreateEmbeddings(text);
        QueryDefinition queryDefinition = new QueryDefinition(queryText).WithParameter("@vectorEmbedding", queryVector);

        await GetByQueryAsync(queryDefinition);
    }

    /// <inheritdoc/>
    public async ValueTask<IEnumerable<TItem>> GetByQueryAsync(
        QueryDefinition queryDefinition,
        CancellationToken cancellationToken = default)
    {
        Container container =
            await containerProvider.GetContainerAsync().ConfigureAwait(false);

        TryLogDebugDetails(logger, () => $"Read {queryDefinition.QueryText}");

        return await cosmosQueryableProcessor.IterateAsync<TItem>(container, queryDefinition, cancellationToken);
    }
}
