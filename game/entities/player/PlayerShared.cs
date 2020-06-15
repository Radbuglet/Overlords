using System;
using System.Collections.Generic;
using Godot;
using Overlords.game.entities.common;
using Overlords.game.entities.player.common;
using Overlords.game.entities.player.local;
using Overlords.helpers.network;
using Overlords.helpers.network.replication;
using Overlords.helpers.network.serialization;
using Overlords.helpers.tree.behaviors;

namespace Overlords.game.entities.player
{
    public class PlayerShared: Node, IWorldProvider
    {
        [LinkNodeStatic("RemoteEvent")]
        public RemoteEvent RemoteEvent;
        
        [LinkNodeStatic("../FpsCamera/RayCast")]
        public RayCast InteractionRayCast;

        public Node World { get; private set; }
        public int OwnerPeerId { get; private set; }
        public string DisplayName;

        public Vector3 Position
        {
            get => GetBody().Translation;
            set => GetBody().Translation = value;
        }

        public KinematicBody GetBody()
        {
            return this.GetGameObject<KinematicBody>();
        }

        public Inventory GetInventory()
        {
            return GetNode<Inventory>("Inventory");
        }

        public void InitializeLocal(
            Node world, int ownerPeerId,
            NetworkTypeUtils.ObjectVariant variant, PlayerProtocol.InitialState initialState)
        {
            // Setup local tree
            var playerRoot = GetBody();
            this.InitializeBehavior();
            variant.ApplyToTree(new Dictionary<NetworkTypeUtils.ObjectVariant, IEnumerable<Func<Node>>>
            {
                [NetworkTypeUtils.ObjectVariant.FlagAuthoritative] = new Func<Node>[]
                {
                    GetInventory
                },
                [NetworkTypeUtils.ObjectVariant.Server] = new []
                {
                    playerRoot.GetBehaviorWrapped<PlayerServer>()
                },
                [NetworkTypeUtils.ObjectVariant.LocalAuthoritative] = new []
                {
                    playerRoot.GetBehaviorWrapped<PlayerLocal>()
                },
                [NetworkTypeUtils.ObjectVariant.LocalPuppet] = new []
                {
                    playerRoot.GetBehaviorWrapped<PlayerPuppet>()
                }
            });

            // Setup state
            playerRoot.Name = $"player_{ownerPeerId}";
            World = world;
            OwnerPeerId = ownerPeerId;
            Position = initialState.Position;
            DisplayName = initialState.DisplayName;
        }
        
        public void InitializeRemote(SceneTree tree, Node world, PlayerProtocol.NetworkConstructor constructor)
        {
            InitializeLocal(world, constructor.OwnerPeerId,
                tree.GetNetworkVariant(constructor.OwnerPeerId), constructor.InitialState);
        }

        public bool IsOverlord()
        {
            return this.GetWorldShared().ActiveOverlordPeer == OwnerPeerId;
        }
    }
}