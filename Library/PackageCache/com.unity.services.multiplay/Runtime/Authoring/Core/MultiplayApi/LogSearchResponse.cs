using System;
using System.Collections.Generic;

namespace Unity.Services.Multiplay.Authoring.Core.MultiplayApi
{
    record LogEntryMetadata(System.Guid FleetId, string MessageId, string ServerId, string Source, DateTime Timestamp);
    record LogEntry(string Message, LogEntryMetadata MetaData);
    record LogSearchResult(decimal Count, List<LogEntry> Entries, string PaginationToken);
}
