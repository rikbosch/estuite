﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Estuite.Domain;

namespace Estuite
{
    public class UnitOfWork :
        IRegisterAggregates,
        IRegisterEventStreams,
        IHydrateAggregates,
        IHydrateEventStreams,
        ICommitAggregates
    {
        private readonly Dictionary<StreamId, IFlushEvents> _aggregates;
        private readonly BucketId _bucketId;
        private readonly ICreateSessions _createSessions;
        private readonly IGenerateIdentities _identities;
        private readonly ICreateStreamIdentities _streamIdentities;
        private readonly IWriteSessions _writeSessions;

        public UnitOfWork(BucketId bucketId, ICreateSessions createSessions, IWriteSessions writeSessions)
        {
            if (bucketId == null) throw new ArgumentNullException(nameof(bucketId));
            _bucketId = bucketId;
            _createSessions = createSessions;
            _writeSessions = writeSessions;
            _aggregates = new Dictionary<StreamId, IFlushEvents>(StreamIdEqualityComparer.Instance);
            _identities = this as IGenerateIdentities ?? new GuidCombGenerator(new UtcDateTimeProvider());
            _streamIdentities = this as ICreateStreamIdentities ?? new DefaultStreamIdentityFactory();
        }

        public async Task Commit(CancellationToken token = new CancellationToken())
        {
            var streamsToWrite = _aggregates
                .Select(x => new {StreamId = x.Key, Events = x.Value.Flush()})
                .Where(x => x.Events.Any())
                .ToArray();

            switch (streamsToWrite.Length)
            {
                case 0:
                    return;
                case 1:
                    var streamId = streamsToWrite[0].StreamId;
                    var events = streamsToWrite[0].Events;
                    await WriteStream(streamId, events, token);
                    break;
                default:
                    var ids = string.Join(", ", streamsToWrite.Select(x => x.StreamId.Value));
                    string message = $"Can't commit multiple streams. Stream ids {ids}";
                    throw new InvalidOperationException(message);
            }
        }

        public void Hydrate(ICanBeHydrated aggregate)
        {
            aggregate.HydrateTo(this);
        }

        public void Hydrate<TId, TEventStream>(TId id, TEventStream events) where TEventStream : IHydrateEvents
        {
            events.Hydrate(new object[0]);
            throw new NotImplementedException();
        }

        public void Register(ICanBeRegistered aggregate)
        {
            aggregate.RegisterTo(this);
        }

        public void Register<TId, TEventStream>(TId id, TEventStream events) where TEventStream : IFlushEvents
        {
            var streamId = _streamIdentities.Create<TId, TEventStream>(_bucketId, id);
            _aggregates.Add(streamId, events);
        }

        private async Task WriteStream(StreamId streamId, IEnumerable<Event> events, CancellationToken token)
        {
            var sessionId = new SessionId($"{_identities.Generate()}");
            var session = _createSessions.Create(streamId, sessionId, events);
            await _writeSessions.Write(session, token);
        }
    }
}