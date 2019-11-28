// <copyright file="EmbeddedEventStore.cs" company="Corsham Science">
// Copyright (c) Corsham Science. All rights reserved.
// </copyright>

namespace CorshamScience.AggregateRepository.EventStore.Tests
{
    using System.Threading;
    using global::EventStore.ClientAPI.Embedded;
    using global::EventStore.Core;
    using global::EventStore.Core.Bus;
    using global::EventStore.Core.Messages;
    using global::EventStore.Core.Services.Monitoring;

    public class EmbeddedEventStore
    {
        public EmbeddedEventStore(int tcpPort, int httpPort)
        {
            TcpEndPoint = tcpPort;
            HttpEndPoint = httpPort;
        }

        public ClusterVNode Node { get; private set; }

        public int HttpEndPoint { get; }

        public int TcpEndPoint { get; }

        public void Start()
        {
            Node = EmbeddedVNodeBuilder.AsSingleNode().RunInMemory()
                .AdvertiseExternalTCPPortAs(TcpEndPoint)
                .AdvertiseExternalHttpPortAs(HttpEndPoint)
                .AddExternalHttpPrefix($"http://127.0.0.1:{HttpEndPoint}/").
                WithStatsStorage(StatsStorage.None).Build();
            var started = new ManualResetEvent(false);

            Node.MainBus.Subscribe(new AdHocHandler<SystemMessage.BecomeMaster>(m => started.Set()));
            Node.Start();

            started.WaitOne();
        }

        public void Stop()
        {
            var stopped = new ManualResetEvent(false);
            Node.MainBus.Subscribe(new AdHocHandler<SystemMessage.BecomeShutdown>(m => stopped.Set()));

            Node?.Stop();
            stopped.WaitOne();
        }
    }
}
