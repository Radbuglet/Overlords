﻿using Godot;
using Overlords.game.entities.common;
using Overlords.game.entities.player.common;
using Overlords.game.world;
using Overlords.helpers.network.serialization;
using Overlords.helpers.tree.behaviors;
using _EventHub = Overlords.helpers.network.RemoteEventHub<
    Overlords.game.entities.player.common.PlayerProtocol.ClientBound,
    Overlords.game.entities.player.common.PlayerProtocol.ServerBound>;

namespace Overlords.game.entities.player
{
    public class PlayerPuppet: Node
    {
        [RequireBehavior] public PlayerShared LogicShared;
        private _EventHub _remoteEventHub;

        public override void _Ready()
        {
            this.InitializeBehavior();
            _remoteEventHub = new _EventHub(LogicShared.RemoteEvent);
            _remoteEventHub.BindHandler(PlayerProtocol.ClientBound.PuppetSetPos, new PrimitiveSerializer<Vector3>(),
                (sender, position) =>
                {
                    LogicShared.Position = position;
                });
            AddChild(_remoteEventHub);
            
            LogicShared.GetWorldClient().EmitSignal(nameof(WorldClient.PuppetPlayerAdded), this.GetGameObject<Node>());
        }

        public override void _ExitTree()
        {
            LogicShared.GetWorldClient().EmitSignal(nameof(WorldClient.PuppetPlayerRemoved), this.GetGameObject<Node>());
        }
    }
}