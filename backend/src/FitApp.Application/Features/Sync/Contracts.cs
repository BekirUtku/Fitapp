using System.Text.Json;

namespace FitApp.Application.Features.Sync;

public enum SyncOperation { Upsert, Delete }

public enum SyncOutcome { Applied, SkippedLww, NotFound, Error }

/// <summary>Tek bir varlık değişikliği (aggregate kök düzeyinde).</summary>
public record SyncChange(
    string EntityType,
    Guid EntityId,
    SyncOperation Operation,
    DateTime UpdatedAt,
    JsonElement? Payload);

public record SyncResult(Guid EntityId, string EntityType, string Status, string? Message = null);

public record PushRequest(IReadOnlyList<SyncChange> Changes);

public record PushResponse(IReadOnlyList<SyncResult> Results, DateTime ServerTime);

public record PullResponse(IReadOnlyList<SyncChange> Changes, DateTime ServerTime);
