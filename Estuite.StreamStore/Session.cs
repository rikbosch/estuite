﻿using System;
using Estuite.Domain;

namespace Estuite.StreamStore
{
    public class Session
    {
        public Session(StreamId streamId, SessionId sessionId, DateTime created, EventRecord[] records)
        {
            if (streamId == null) throw new ArgumentNullException(nameof(streamId));
            if (sessionId == null) throw new ArgumentNullException(nameof(sessionId));
            if (created.IsNullOrEmpty()) throw new ArgumentOutOfRangeException(nameof(created));
            if (records == null) throw new ArgumentNullException(nameof(records));
            StreamId = streamId;
            SessionId = sessionId;
            Records = records;
            Created = created;
        }

        public SessionId SessionId { get; }
        public StreamId StreamId { get; }
        public EventRecord[] Records { get; }
        public DateTime Created { get; }
    }
}