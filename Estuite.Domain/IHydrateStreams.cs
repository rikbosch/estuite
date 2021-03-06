﻿using System.Threading;
using System.Threading.Tasks;

namespace Estuite.Domain
{
    public interface IHydrateStreams
    {
        Task Hydrate<TId, TStream>(TId id, TStream stream, CancellationToken token = new CancellationToken())
            where TStream : IHydrateEvents, IFlushEvents;

        Task<bool> TryHydrate<TId, TStream>(TId id, TStream stream, CancellationToken token = new CancellationToken())
            where TStream : IHydrateEvents, IFlushEvents;
    }
}