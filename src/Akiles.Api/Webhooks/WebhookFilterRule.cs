using Akiles.Api.Events;

namespace Akiles.Api.Webhooks;

public record WebhookFilterRule(EventObjectType ObjectType, EventVerb Verb);
