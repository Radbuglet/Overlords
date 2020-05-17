using System;
using System.Diagnostics;
using Godot;
using Overlords.helpers.network;

namespace Overlords.helpers.tree.conditionals
{
    public abstract class MultiNetworkNode<TServer, TClient, TCommon>: Node where TServer: Node where TClient: Node where TCommon: Node
    {
        public (NetworkUtils.NetworkMode mode, object instance)? Instance;

        protected abstract void InitializeCommon();
        protected abstract TServer MakeServer();
        protected abstract TClient MakeClient();

        public override void _Ready()
        {
            InitializeCommon();
            
            var networkMode = GetTree().GetNetworkMode();
            Node networkExclusive;
            switch (networkMode)
            {
                case NetworkUtils.NetworkMode.Server:
                {
                    networkExclusive = MakeServer();
                    networkExclusive.Name = "Server";
                    break;
                }
                case NetworkUtils.NetworkMode.Client:
                    networkExclusive = MakeClient();
                    networkExclusive.Name = "Client";
                    break;
                default:
                    throw new InvalidOperationException("MultiNetworkNode called while the tree didn't have networking configured");
            }
            
            AddChild(networkExclusive);
            
            Instance = (networkMode, networkExclusive);
        }

        private TType AsType<TType>(NetworkUtils.NetworkMode mode)
        {
            Debug.Assert(Instance != null && Instance.Value.mode == mode);
            return (TType) Instance.Value.instance;
        }

        public TServer Server => AsType<TServer>(NetworkUtils.NetworkMode.Server);

        public TClient Client => AsType<TClient>(NetworkUtils.NetworkMode.Client);
    }
}