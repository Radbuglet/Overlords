﻿using Godot;
using Overlords.helpers.tree.behaviors;

namespace Overlords.game.entities.player.client
{
    public class PlayerLogicPuppet: Node
    {
        public override void _Ready()
        {
            this.InitializeBehavior();
        }
    }
}