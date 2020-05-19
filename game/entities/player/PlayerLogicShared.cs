using System;
using Godot;
using Overlords.helpers.network;
using Overlords.helpers.network.replication;
using Overlords.helpers.network.serialization;
using Overlords.helpers.tree;
using Overlords.helpers.tree.behaviors;

namespace Overlords.game.entities.player
{
    public class PlayerLogicShared: Node
    {
        [RequireParent] public Spatial PlayerRoot;
        
        [LinkNodeStatic("../ReplicatedState")]
        public StateReplicator StateReplicator;
        
        public int OwnerPeerId;
        public StateReplicator.StateField<int> BalanceValue;

        public void SetupPreEntry(SceneTree tree, int peerId)
        {
            this.InitializeBehavior();
            
            // Setup shared
            PlayerRoot.Name = $"player_{peerId}";
            OwnerPeerId = peerId;
            
            // Create RVals
            BalanceValue = StateReplicator.MakeField(new PrimitiveSerializer<int>());

            // Setup variant
            var networkMode = tree.GetNetworkMode();
            var gameObject = this.GetGameObject<Node>();
            switch (networkMode)
            {
                case NetworkUtils.NetworkMode.None:
                    GD.PushWarning("Player created on non-networked scene tree!");
                    break;
                case NetworkUtils.NetworkMode.Client:
                    gameObject.GetBehavior<PlayerLogicServer>().Purge();
                    break;
                case NetworkUtils.NetworkMode.Server:
                    gameObject.GetBehavior<PlayerLogicLocal>().Purge();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}