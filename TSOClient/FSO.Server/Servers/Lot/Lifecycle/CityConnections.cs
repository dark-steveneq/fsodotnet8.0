﻿using FSO.Server.Clients;
using FSO.Server.Framework.Aries;
using FSO.Server.Framework.Gluon;
using FSO.Server.Protocol.Gluon.Packets;
using Ninject;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using FSO.Common.Security;
using FSO.Common.Domain.Shards;

namespace FSO.Server.Servers.Lot.Lifecycle
{
    public class CityConnections
    {
        private static Logger LOG = LogManager.GetCurrentClassLogger();
        private Dictionary<LotServerConfigurationCity, CityConnection> Connections;
        private Thread ConnectionWatcher;
        private AutoResetEvent ConnectionChanged = new AutoResetEvent(false);
        private bool _Running;
        public short LotCount;

        private PerformanceCounter CpuCounter;
        private LotServerConfiguration Config;

        public event CityConnectionEvent OnCityDisconnected;
        
        public CityConnections(LotServerConfiguration config, IKernel kernel)
        {
            Config = config;
            if (OperatingSystem.IsWindows())
            {
                CpuCounter = new PerformanceCounter();
                CpuCounter.CategoryName = "Processor";
                CpuCounter.CounterName = "% Processor Time";
                CpuCounter.InstanceName = "_Total";

                if (PerformanceCounterCategory.Exists("Processor"))
                    CpuCounter.NextValue();
            }
            else
                LOG.Debug("Performance counters are not supported on this platform, running without.");

            Connections = new Dictionary<LotServerConfigurationCity, CityConnection>();
            foreach (var city in config.Cities)
            {
                var connection = new CityConnection(kernel, city, config);
                Connections.Add(city, connection);
                connection.OnDisconnected += Connection_OnDisconnected;
            }
        }

        private void Connection_OnDisconnected(CityConnection connection)
        {
            if (OnCityDisconnected != null)
                OnCityDisconnected(connection);
        }

        public IGluonSession GetByShardId(int shard_id)
        {
            return Connections.Values.FirstOrDefault(x => x.CityConfig.ID == shard_id);
        }

        public void Start()
        {
            Stop();

            _Running = true;
            ConnectionWatcher = new Thread(CheckConnections);
            ConnectionWatcher.Start();
        }

        public void Stop()
        {
            _Running = false;
            ConnectionChanged.Set(); //wakes the thread a bit quicker
        }

        private void CheckConnections()
        {
            bool nextSleep = false;
            while (_Running)
            {
                float cpu = 1;
                if (CpuCounter != null && PerformanceCounterCategory.Exists("Processor"))
                    cpu = CpuCounter.NextValue();
                var capacity = new AdvertiseCapacity
                {
                    CpuPercentAvg = (byte)(cpu * 100),
                    CurrentLots = LotCount,
                    MaxLots = (short)Config.Max_Lots,
                    RamAvaliable = 0,
                    RamUsed = 0
                };

                //Repair & advertise connections
                foreach (var connection in Connections.Values)
                {
                    if (!connection.Connected)
                    {
                        LOG.Debug("Attempting connection!");
                        connection.Connect();
                    }
                    else
                        connection.Write(capacity);
                }

                if (nextSleep)
                    Thread.Sleep(Config.CityReportingInterval);
                else
                    nextSleep = ConnectionChanged.WaitOne(Config.CityReportingInterval);
            }
        }
    }

    public class CityConnection : IAriesEventSubscriber, IAriesMessageSubscriber, IGluonSession
    {
        private static Logger LOG;
        
        private AriesClient Client;
        public LotServerConfigurationCity CityConfig;
        public bool Connected { get; internal set; }

        public bool IsAuthenticated
        {
            get
            {
                return false;
            }
        }
        public uint LastRecv { get; set; }

        private bool _Connecting = false;
        private DateTime _ConnectingStart;
        private IAriesPacketRouter _Router;
        private LotServerConfiguration LotServerConfig;
        private IKernel Kernel;

        public event CityConnectionEvent OnConnected;
        public event CityConnectionEvent OnDisconnected;

        public CityConnection(IKernel kernel, LotServerConfigurationCity config, LotServerConfiguration lotServerConfig)
        {
            LOG = LogManager.GetLogger("CityConnection[" + lotServerConfig.Call_Sign + "]");

            Kernel = kernel;
            LotServerConfig = lotServerConfig;
            CityConfig = config;
            Client = new AriesClient(kernel);
            Client.AddSubscriber(this);
            _Router = kernel.Get<IAriesPacketRouter>();
        }
        
        public void Connect()
        {
            if (Connected) {
                return;
            }

            if (_Connecting)
            {
                if (DateTime.UtcNow - _ConnectingStart > TimeSpan.FromSeconds(30)){
                    _Connecting = false;
                }
                return;
            }
            else
            {
                _ConnectingStart = DateTime.UtcNow;
                _Connecting = true;
            }

            //TODO: Fix TLS support
            var endpoint = CityConfig.Host.Replace("100", "101");
            Client.Connect(endpoint);
        }

        public void AuthenticationEstablished()
        {
            Connected = true;
            _Connecting = false;

            var shards = Kernel.Get<IShardsDomain>();
            var shard = shards?.GetById(CityConfig.ID);

            LOG.Info("Successfully connected to {0} (ID: {1})", shard.Name ?? "[Unknown]", CityConfig.ID);

            if (OnConnected != null)
                OnConnected(this);
        }

        public void MessageReceived(AriesClient client, object message)
        {
            ((AriesPacketRouter)_Router).Handle(this, message);
        }

        public void SessionCreated(AriesClient client)
        {
        }

        public void SessionOpened(AriesClient client)
        {
        }

        public void SessionClosed(AriesClient client)
        {
            LOG.Info("Disconnected from city server!");
            Connected = false;
            _Connecting = false;

            if (OnDisconnected != null)
                OnDisconnected(this);
        }

        public void SessionIdle(AriesClient client)
        {
        }

        public void InputClosed(AriesClient session)
        {
            LOG.Info("Disconnected from city server! (input closed)");
        }

        public void Write(params object[] messages)
        {
            Client.Write(messages);
        }

        public void Close()
        {
            Client.Disconnect();
        }

        public object GetAttribute(string key)
        {
            return null;
        }

        public void SetAttribute(string key, object value)
        {
        }

        public void DemandAvatar(uint id, AvatarPermissions permission)
        {
        }

        public void DemandAvatars(IEnumerable<uint> id, AvatarPermissions permission)
        {
        }

        public void DemandInternalSystem()
        {
        }

        public string CallSign
        {
            get
            {
                return LotServerConfig.Call_Sign;
            }
        }

        public string PublicHost
        {
            get
            {
                return LotServerConfig.Public_Host;
            }
        }

        public string InternalHost
        {
            get
            {
                return LotServerConfig.Internal_Host;
            }
        }
    }

    public delegate void CityConnectionEvent(CityConnection connection);
}
