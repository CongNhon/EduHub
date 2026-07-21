using System.Globalization;
using MongoDB.Bson;
using MongoDB.Driver;
using Serilog.Core;
using Serilog.Events;

namespace EduHub.Infrastructure.Audit;

/// <summary>
/// Ghi chú: MongoAuditSink ghi Serilog event đã redact vào MongoDB audit collection.
/// </summary>
public sealed class MongoAuditSink : ILogEventSink
{
    private readonly IMongoCollection<BsonDocument>? collection;

    /// <summary>
    /// Ghi chú: Constructor tạo Mongo collection với timeout ngắn để lỗi Mongo không phá request chính.
    /// </summary>
    public MongoAuditSink(AuditLogOptions options)
    {
        if (!options.Enabled || string.IsNullOrWhiteSpace(options.ConnectionString))
        {
            return;
        }

        var settings = MongoClientSettings.FromConnectionString(options.ConnectionString);
        settings.ServerSelectionTimeout = TimeSpan.FromMilliseconds(options.ServerSelectionTimeoutMilliseconds);
        var client = new MongoClient(settings);
        collection = client
            .GetDatabase(options.DatabaseName)
            .GetCollection<BsonDocument>(options.CollectionName);
    }

    /// <summary>
    /// Ghi chú: Emit chuyển LogEvent thành Mongo document và nuốt lỗi sink để không rollback business transaction.
    /// </summary>
    public void Emit(LogEvent logEvent)
    {
        if (collection is null)
        {
            return;
        }

        try
        {
            collection.InsertOne(ToDocument(logEvent));
        }
        catch
        {
        }
    }

    /// <summary>
    /// Ghi chú: ToDocument map Serilog LogEvent thành document audit an toàn.
    /// </summary>
    private static BsonDocument ToDocument(LogEvent logEvent)
    {
        var properties = new BsonDocument();
        foreach (var property in logEvent.Properties)
        {
            properties[property.Key] = ToBsonValue(property.Key, property.Value);
        }

        var document = new BsonDocument
        {
            ["timestampUtc"] = logEvent.Timestamp.UtcDateTime,
            ["level"] = logEvent.Level.ToString(),
            ["messageTemplate"] = logEvent.MessageTemplate.Text,
            ["message"] = AuditRedactionPolicy.RedactValue(logEvent.RenderMessage(CultureInfo.InvariantCulture)),
            ["properties"] = properties
        };

        AddIfExists(document, "correlationId", properties, "CorrelationId");
        AddIfExists(document, "actorUserId", properties, "ActorUserId");
        AddIfExists(document, "actorRole", properties, "ActorRole");
        AddIfExists(document, "useCase", properties, "RequestName");
        AddIfExists(document, "entityType", properties, "EntityType");
        AddIfExists(document, "entityId", properties, "EntityId");
        AddIfExists(document, "action", properties, "Action");
        AddIfExists(document, "result", properties, "StatusCode");
        AddIfExists(document, "duration", properties, "ElapsedMilliseconds");

        if (logEvent.Exception is not null)
        {
            document["exception"] = new BsonDocument
            {
                ["type"] = logEvent.Exception.GetType().Name,
                ["message"] = AuditRedactionPolicy.RedactValue(logEvent.Exception.Message)
            };
        }

        return document;
    }

    /// <summary>
    /// Ghi chú: ToBsonValue chuyển property Serilog sang BSON và redact theo key.
    /// </summary>
    private static BsonValue ToBsonValue(string key, LogEventPropertyValue value) =>
        value switch
        {
            ScalarValue { Value: null } => BsonNull.Value,
            ScalarValue { Value: string text } => AuditRedactionPolicy.RedactByKey(key, text),
            ScalarValue { Value: int number } => number,
            ScalarValue { Value: long number } => number,
            ScalarValue { Value: double number } => number,
            ScalarValue { Value: decimal number } => decimal.ToDouble(number),
            ScalarValue { Value: bool boolean } => boolean,
            ScalarValue { Value: Guid id } => id.ToString(),
            ScalarValue { Value: DateTime dateTime } => dateTime,
            ScalarValue { Value: DateTimeOffset dateTimeOffset } => dateTimeOffset.UtcDateTime,
            SequenceValue sequence => new BsonArray(sequence.Elements.Select(item => ToBsonValue(key, item))),
            StructureValue structure => new BsonDocument(structure.Properties.Select(property =>
                new BsonElement(property.Name, ToBsonValue(property.Name, property.Value)))),
            DictionaryValue dictionary => new BsonDocument(dictionary.Elements.Select(pair =>
                new BsonElement(pair.Key.Value?.ToString() ?? "key", ToBsonValue(pair.Key.Value?.ToString() ?? "key", pair.Value)))),
            _ => AuditRedactionPolicy.RedactByKey(key, value.ToString())
        };

    private static void AddIfExists(BsonDocument target, string targetName, BsonDocument source, string sourceName)
    {
        if (source.TryGetValue(sourceName, out var value) && !value.IsBsonNull)
        {
            target[targetName] = value;
        }
    }
}
