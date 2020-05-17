using System;
using System.Diagnostics;
using Godot;
using Overlords.helpers.network;

namespace Overlords.helpers.tree.conditionals
{
    public abstract class MultiNetworkNode<TServer, TClient, TCommon>: Node where TServer: Node where TClient: Node where TCommon: Node
    {
        public (NetworkUtils.NetworkMode mode, object instance)? Instance;

        protected abstract TCommon MakeCommon();
        protected abstract TServer MakeServer(TCommon common);
        protected abstract TClient MakeClient(TCommon common);

        public override void _Ready()
        {
            var commonNode = MakeCommon();
            if (commonNode != null)
            {
                commonNode.Name = "common";
                AddChild(commonNode);
            }
            
            var networkMode = GetTree().GetNetworkMode();
            Node networkExclusive;
            switch (networkMode)
            {
                case NetworkUtils.NetworkMode.Server:
                {
                    networkExclusive = MakeServer(commonNode);
                    networkExclusive.Name = "Server";
                    break;
                }
                case NetworkUtils.NetworkMode.Client:
                    networkExclusive = MakeClient(commonNode);
                    networkExclusive.Name = "client";
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

        public TServer AsServer()
        {
            return AsType<TServer>(NetworkUtils.NetworkMode.Server);
        }
        
        public TClient AsClient()
        {
            return AsType<TClient>(NetworkUtils.NetworkMode.Client);
        }
    }
}