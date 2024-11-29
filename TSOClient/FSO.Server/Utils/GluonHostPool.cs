using FSO.Server.Clients;
using FSO.Server.Database.DA;
using FSO.Server.Framework.Aries;
using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FSO.Common.Security;
using FSO.Server.Protocol.Aries.Packets;
using FSO.Server.Database.DA.Hosts;
using System.Threading;
using NLog;
using System.Security.Cryptography;
using FSO.Server.Protocol.Gluon.Packets;
using Ninject.Modules;
using FSO.Server.Protocol.Utils;
using Ninject.Parameters;
using FSO.Server.Domain;

namespace FSO.Server.Utils
{
    /// <summary>
    /// Allows servers to communicate with each other
    /// </summary>
    public class GluonHostPool : IGluonHostPool
    {
        private static Logger LOG = LogManager.GetCurrentClassLogger();
        public const int POOL_HEALTH_CHECK_FREQUENCY = 30000;
        public const int POOL_REDISCOVER_INTERVAL = 60;

        internal IDAFactory DAFactory;
        internal IKernel Kernel;

        private GluonHostPoolMonitor Monitor;
        private AutoResetEvent StoppedEvent = new AutoResetEvent(false);

        private Dictionary<string, GluonHost> Pool;
        private Thread PoolHealthWatcher;
        private bool PoolRunning;
        private bool ForcePoolRefresh = true;

        private int PoolDiscoverCounter = 0;

        public string PoolHash
        {
            get
            {
                return Monitor.PoolHash;
            }
        }

        public GluonHostPool(IDAFactory daFactory, IKernel kernel){
            this.DAFactory = daFactory;
            this.Kernel = kernel;
            this.Pool = new Dictionary<string, GluonHost>();
            this.Monitor = new GluonHostPoolMonitor(this);
        }

        /// <summary>
        /// Start host pool
        /// </summary>
        public void Start()
        {
            LOG.Info("starting gluon host pool");

            PoolRunning = true;
            PoolHealthWatcher = new Thread(HealthCheckLoop);
            PoolHealthWatcher.Start();
        }

        /// <summary>
        /// Stop host pool
        /// </summary>
        public void Stop()
        {
            LOG.Info("stopping gluon host pool");

            PoolRunning = false;
            StoppedEvent.Set();
        }

        private void HealthCheckLoop()
        {
            while (PoolRunning)
            {
                PoolDiscoverCounter++;

                if (ForcePoolRefresh || 
                    PoolDiscoverCounter >= POOL_REDISCOVER_INTERVAL)
                {
                    ForcePoolRefresh = false;
                    PoolDiscoverCounter = 0;
                    Monitor.DiscoverHosts();
                }

                var hashSame = Monitor.CheckHosts(Pool.Values);
                if (!hashSame){
                    ForcePoolRefresh = true;
                }

                StoppedEvent.WaitOne(POOL_HEALTH_CHECK_FREQUENCY);
            }
        }

        /// <summary>
        /// Get host by call sign
        /// </summary>
        /// <param name="callSign"></param>
        /// <returns>Call sign's host instance</returns>
        public IGluonHost Get(string callSign)
        {
            lock (Pool)
            {
                if (Pool.ContainsKey(callSign)){
                    return Pool[callSign];
                }else{
                    var newHost = Kernel.Get<GluonHost>(
                        new ConstructorArgument("pool", this), 
                        new ConstructorArgument("callSign", callSign)
                    );
                    Pool[callSign] = newHost;
                    return newHost;
                }
            }
        }

        /// <summary>
        /// Get hosts by roles
        /// </summary>
        /// <param name="role">Host role</param>
        /// <returns>List of matched host instances</returns>
        public IEnumerable<IGluonHost> GetByRole(DbHostRole role)
        {
            lock (Pool)
                return Pool.Values.Where(x => x.Role == role).ToList();
        }

        /// <summary>
        /// Get all hosts in pool
        /// </summary>
        /// <returns>List of all host insances</returns>
        public IEnumerable<IGluonHost> GetAll()
        {
            lock (Pool)
                return Pool.Values.ToList();
        }

        /// <summary>
        /// Get host by Shard ID
        /// </summary>
        /// <param name="shard_id">Host's Shard ID</param>
        /// <returns>Shard's host instance</returns>
        public IGluonHost GetByShardId(int shard_id)
        {
            lock (Pool)
                return Pool.Values.FirstOrDefault(x => x.ShardId == shard_id);
        }
    }

    public class GluonHostPoolMonitor
    {
        private GluonHostPool Pool;
        public string PoolHash { get; set; } = "";

        public GluonHostPoolMonitor(GluonHostPool pool){
            this.Pool = pool;
        }

        /// <summary>
        /// Opens / repairs connections with other servers and pings them
        /// They can respond with a pool hash which helps us know
        /// that we need to refresh our view of the pool
        /// </summary>
        /// <param name="hosts"></param>
        /// <returns></returns>
        public bool CheckHosts(IEnumerable<GluonHost> hosts)
        {
            var callbacks = new List<Task<IGluonCall>>();

            foreach (var host in hosts)
            {
                if (host.Status == GluonHostStatus.DISCONNECTED)
                {
                    host.Connect();
                }
                else if (host.Status == GluonHostStatus.CONNECTED)
                {
                    callbacks.Add(host.Call(new HealthPing()));
                }
            }
            
            try {
                var results = Task.WhenAll(callbacks).Result;
                foreach (var result in results)
                {
                    var response = (HealthPingResponse)result;
                    if (response != null && response.PoolHash != "" && response.PoolHash != PoolHash)
                    {
                        return false;
                    }
                }
            }catch(Exception ex){
            }

            return true;
        }

        /// <summary>
        /// Discover hosts from database
        /// </summary>
        public void DiscoverHosts()
        {
            var hashInput = "";

            using (var db = Pool.DAFactory.Get())
            {
                var all = db.Hosts.All();
                foreach (var host in all){
                    OnHostDiscovered(host);

                    hashInput += host.call_sign;
                    hashInput += "\n";
                    hashInput += host.internal_host;
                    hashInput += "\n";
                    hashInput += host.public_host;
                    hashInput += "\n";
                    hashInput += host.role.ToString();
                    hashInput += "\n";
                }
            }

            MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(hashInput);
            byte[] hash = md5.ComputeHash(inputBytes);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++){
                sb.Append(hash[i].ToString("X2"));
            }
            PoolHash = sb.ToString();
        }

        /// <summary>
        /// Callback for adding hosts from a database entry
        /// </summary>
        /// <param name="dbHost">Host database entry</param>
        private void OnHostDiscovered(DbHost dbHost)
        {
            var host = (GluonHost)Pool.Get(dbHost.call_sign);
            host.Update(dbHost);
        }
    }

    /// <summary>
    /// Class containing all host information
    /// </summary>
    public class GluonHost : IAriesEventSubscriber, IAriesMessageSubscriber, IGluonHost
    {
        public string CallSign { get; internal set; }
        public string InternalHost { get; set; }
        public string PublicHost { get; set; }
        public DbHostRole Role { get; set; }
        public GluonHostStatus Status { get; internal set; }
        public DateTime BootTime { get; set; }
        public int? ShardId { get; set; }

        private GluonHostPool Pool;
        private AriesClient Client;

        private Dictionary<Guid, TaskCompletionSource<IGluonCall>> Callbacks;
        private AriesPacketRouter Router;

        public GluonHost(GluonHostPool pool, string callSign, IKernel kernel, ServerConfiguration config)
        {
            this.Pool = pool;
            this.CallSign = callSign;
            this.Client = new AriesClient(Pool.Kernel);

            Callbacks = new Dictionary<Guid, TaskCompletionSource<IGluonCall>>();
            Client.AddSubscriber(this);

            Router = new AriesPacketRouter();
            Router.On<RequestClientSession>((session, message) =>
            {
                session.Write(new RequestChallenge() { CallSign = CallSign, PublicHost = PublicHost, InternalHost = InternalHost });
            });
            Router.On<RequestChallengeResponse>((session, message) =>
            {
                var challenge = (RequestChallengeResponse)message;
                var answer = ChallengeResponse.AnswerChallenge(challenge.Challenge, config.Secret);

                session.Write(new AnswerChallenge{
                    Answer = answer
                });
            });
            Router.On<AnswerAccepted>((session, message) =>
            {
                OnAuthenticated();
            });
        }

        /// <summary>
        /// Update host information from database
        /// </summary>
        /// <param name="host">Host information from database</param>
        public void Update(DbHost host)
        {
            var hostChanged = false;

            if(InternalHost != host.internal_host){
                InternalHost = host.internal_host;
                hostChanged = true;
            }

            if(PublicHost != host.public_host){
                PublicHost = host.public_host;
                hostChanged = true;
            }

            if(Role != host.role){
                Role = host.role;
            }

            BootTime = host.time_boot;
            ShardId = host.shard_id;

            if (hostChanged){
                Close();
            }
        }

        /// <summary>
        /// Send message to host
        /// </summary>
        /// <param name="messages">Objects to send</param>
        public void Write(params object[] messages)
        {
            if (Client.IsConnected){
                Client.Write(messages);
            }
        }

        public Task<IGluonCall> Call<IN>(IN input) where IN : IGluonCall
        {
            input.CallId = Guid.NewGuid();

            var timeout = new CancellationTokenSource(6000);
            var task = new TaskCompletionSource<IGluonCall>();
            Callbacks.Add(input.CallId, task);

            timeout.Token.Register(() =>{
                task.TrySetCanceled();
            });

            Write(input);

            return task.Task.ContinueWith(x => {
                Callbacks.Remove(input.CallId);
                if (x.IsFaulted || x.IsCanceled){
                    return null;
                }
                return x.Result;
            });
        }

        /// <summary>
        /// Connect to host
        /// </summary>
        public void Connect()
        {
            if(Status == GluonHostStatus.DISCONNECTED && InternalHost != null)
            {
                Status = GluonHostStatus.CONNECTING;
                //TODO: TLS
                var endpoint = InternalHost + "101";
                Client.Connect(endpoint);
            }
        }
        
        protected virtual void OnAuthenticated(){
            Status = GluonHostStatus.CONNECTED;
        }

        /// <summary>
        /// Callback on Gluon message receive
        /// </summary>
        /// <param name="client">Client instance</param>
        /// <param name="message">Sent object</param>
        public void MessageReceived(AriesClient client, object message)
        {
            if (message is IGluonCall)
            {
                var call = (IGluonCall)message;
                if (Callbacks.ContainsKey(call.CallId))
                {
                    Callbacks[call.CallId].SetResult(call);
                    return;
                }
            }

            Router.Handle(this, message);
        }

        public void SessionCreated(AriesClient client)
        {
        }

        public void SessionOpened(AriesClient client)
        {
        }

        public void SessionClosed(AriesClient client)
        {
            Status = GluonHostStatus.DISCONNECTED;   
        }

        public void SessionIdle(AriesClient client)
        {
        }

        public void InputClosed(AriesClient session)
        {
        }

        /// <summary>
        /// Disconnect from host
        /// </summary>
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

        public bool IsAuthenticated
        {
            get
            {
                return false;
            }
        }
        public uint LastRecv { get; set; }

        public bool Connected
        {
            get
            {
                return Status == GluonHostStatus.CONNECTED;
            }
        }
    }

    public enum GluonHostStatus
    {
        DISCONNECTED,
        CONNECTING,
        CONNECTED,
        FAILED
    }

    public class GluonHostPoolModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IGluonHostPool>().To<GluonHostPool>().InSingletonScope();
        }
    }
}
