using Godot;
using Overlords.game.entities.common;
using Overlords.helpers.network;
using Overlords.helpers.network.replication;
using Overlords.helpers.network.serialization;
using Overlords.helpers.tree;
using Overlords.helpers.tree.behaviors;

namespace Overlords.game.entities.player
{
    public class PlayerShared: Node, IWorldReferencer
    {
        [LinkNodeStatic("../StateReplicator")]
        public StateReplicator StateReplicator;
        
        [LinkNodeStatic("../RemoteEvent")]
        public RemoteEvent RemoteEvent;

        public StateFieldBoxed<int> BalanceRVal;
        
        public Node World { get; private set; }
        public int OwnerPeerId { get; private set; }

        public Vector3 Position
        {
            get => GetBody().Translation;
            set => GetBody().Translation = value;
        }

        public KinematicBody GetBody()
        {
            return this.GetGameObject<KinematicBody>();
        }

        public Control GetHud()
        {
            return GetNode<Control>("../Hud");
        }

        public void InitializeLocal(
            Node world, int ownerPeerId,
            NetworkTypeUtils.ObjectVariant variant, PlayerProtocol.InitialState initialState)
        {
            // Setup local tree
            var playerRoot = GetBody();
            this.InitializeBehavior();
            playerRoot.ApplyNetworkVariant(variant, 
                typeof(PlayerServer), null, typeof(PlayerLocal), typeof(PlayerPuppet));
            if (variant != NetworkTypeUtils.ObjectVariant.LocalAuthoritative)
                GetHud().Purge();
            
            // Setup state
            playerRoot.Name = $"player_{ownerPeerId}";
            World = world;
            OwnerPeerId = ownerPeerId;
            Position = initialState.Position;
            BalanceRVal = new StateFieldBoxed<int>(new PrimitiveSerializer<int>(), StateReplicator);
        }

        public void InitializeRemote(SceneTree tree, Node world, PlayerProtocol.NetworkConstructor constructor)
        {
            InitializeLocal(world, constructor.OwnerPeerId,
                tree.GetNetworkVariant(constructor.OwnerPeerId), constructor.InitialState);
            StateReplicator.LoadValuesCatchup(constructor.ReplicatedValues);
        }
    }
}