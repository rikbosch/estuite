﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Estuite.Domain;
using NSpec;
using Shouldly;

namespace Estuite.Specs.UnitTests
{
    [Tag("describe_Aggregate")]
    public class describe_AggregateOfInteger_ICanBeHydrated : nspec
    {
        private void before_each()
        {
            _id = 1;
            _target = _aggregate = new AggregateUnderTest(_id);
            _streams = new FakeIHydrateStreams();
        }

        private void when_hydrate()
        {
            actAsync = async () => await _target.HydrateTo(_streams);
            it["provides an id with expected type"] = () => { _streams.ProvidedId.ShouldBeOfType<int>(); };
            it["provides an expected id"] = () => { _streams.ProvidedId.ShouldBe(_id); };
            it["provides itself to hydrate events"] =
                () => { _streams.ProvidedEvents.ShouldBeSameAs(_aggregate); };
            context["and hydrator is null"] = () =>
            {
                before = () => _streams = null;
                it["throw exception"] = expect<ArgumentNullException>(
                    "Value cannot be null.\r\nParameter name: streams"
                );
            };
        }
        private void when_try_hydrate()
        {
            actAsync = async () => _returns = await _target.TryHydrateTo(_streams);
            it["provides an id with expected type"] = () => { _streams.ProvidedId.ShouldBeOfType<int>(); };
            it["provides an expected id"] = () => { _streams.ProvidedId.ShouldBe(_id); };
            it["returns true"] = () => { _returns.ShouldBe(true); };
            it["provides itself to hydrate events"] =
                () => { _streams.ProvidedEvents.ShouldBeSameAs(_aggregate); };
            context["and hydrator is null"] = () =>
            {
                before = () => _streams = null;
                it["throw exception"] = expect<ArgumentNullException>(
                    "Value cannot be null.\r\nParameter name: streams"
                );
            };
        }

        private class FakeIHydrateStreams : IHydrateStreams
        {
            public IHydrateEvents ProvidedEvents { get; private set; }
            public object ProvidedId { get; private set; }

            public async Task Hydrate<TId, TEventStream>(
                TId id,
                TEventStream stream,
                CancellationToken token = new CancellationToken())
                where TEventStream : IHydrateEvents, IFlushEvents
            {
                ProvidedId = id;
                ProvidedEvents = stream;
            }

            public async Task<bool> TryHydrate<TId, TEventStream>(
                TId id,
                TEventStream stream,
                CancellationToken token = new CancellationToken())
                where TEventStream : IHydrateEvents, IFlushEvents
            {
                ProvidedId = id;
                ProvidedEvents = stream;
                return true;
            }
        }

        private class AggregateUnderTest : Aggregate<int>
        {
            public AggregateUnderTest(int id) : base(id)
            {
            }
        }

        private int _id;
        private ICanBeHydrated _target;
        private AggregateUnderTest _aggregate;
        private FakeIHydrateStreams _streams;
        private bool _returns;
    }
}