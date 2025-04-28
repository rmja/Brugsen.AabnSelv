using Refit;

namespace Akiles.ApiClient.Webhooks;

public interface IWebhooks
{
    [Get("/webhooks")]
    Task<PagedList<Webhook>> ListWebhooksAsync(
        string? cursor,
        int? limit,
        string? sort,
        CancellationToken cancellationToken = default
    );

    IAsyncEnumerable<Webhook> ListWebhooksAsync(string? sort = null) =>
        new PaginationEnumerable<Webhook>(
            (cursor, cancellationToken) =>
                ListWebhooksAsync(cursor, Constants.DefaultPaginationLimit, sort, cancellationToken)
        );

    [Get("/webhooks/{webhookId}")]
    Task<Webhook> GetWebhookAsync(string webhookId, CancellationToken cancellationToken = default);

    [Post("/webhooks")]
    Task<Webhook> CreateWebhookAsync(
        WebhookInit webhook,
        CancellationToken cancellationToken = default
    );
}
