﻿using System.Collections.Generic;
using System.Diagnostics;
using Godot;
using Godot.Collections;
using Overlords.helpers.csharp;
using Overlords.helpers.network;
using Overlords.helpers.tree;

namespace Overlords.game.player.inventory
{
    // TODO: Culling
    public class PlayerInventory : Node, IRequiresCatchup
    {
        [Signal]
        public delegate void StackMaterialUpdated(int slot, ItemStackRoot root);
        
        [Export] private int _size;
        [Export] private PackedScene _stackPrefab;
        private ItemStackRoot[] _stacks;

        // Scene tree garbage
        private PlayerRoot Player => this.FindFirstAncestor<PlayerRoot>();
        
        public override void _Ready()
        {
            _stacks = new ItemStackRoot[_size];
            this.FlagRequiresCatchup();
        }
        
        // Interaction methods
        public bool IsValidSlot(int slot)
        {
            return _stacks.HasIndex(slot);
        }

        public ItemStackRoot GetStack(int slot)
        {
            return _stacks.TryGetValue(slot, out var value) ? value : null;
        }
        
        public IEnumerable<(int, ItemStackRoot)> GetStacks(bool filterEmpty)
        {
            for (var i = 0; i < _size; i++)
            {
                var stack = _stacks[i];
                if (filterEmpty && stack == null)
                    continue;
                yield return (i, stack);
            }
        }

        // Networking
        private ItemStackRoot AddNewStack(int slot)
        {
            var stack = (ItemStackRoot) _stackPrefab.Instance();
            stack.Name = slot.ToString();
            AddChild(stack);
            return stack;
        }
        
        public object CatchupOverNetwork(int peerId)
        {
            var allocateStacks = new Array();
            foreach (var (slot, _) in GetStacks(true))
            {
                allocateStacks.Add(slot);
            }
            return allocateStacks;
        }

        public void HandleCatchupState(object stacksRaw)
        {
            if (!(stacksRaw is Array stacks))
                throw new InvalidCatchupException("Failed to pre-allocate stacks: argument was not an array.");

            foreach (var slotRaw in stacks)
            {
                if (!(slotRaw is int slot))
                    throw new InvalidCatchupException("Failed to pre-allocate stacks: slot was not an integer.");

                if (_stacks[slot] != null)
                    throw new InvalidCatchupException("Failed to pre-allocate stacks: slot already has an item.");

                AddNewStack(slot);
            }
        }

        [Puppet]
        private void AddStack(int targetSlot, Dictionary catchupInfo)
        {
            Debug.Assert(!GetTree().DoesAnythingRequireCatchup());
            
            if (!IsValidSlot(targetSlot))
            {
                GD.PushWarning($"Failed to {nameof(AddStack)}: invalid {nameof(targetSlot)} index.");
                return;
            }

            if (_stacks[targetSlot] != null)
            {
                GD.PushWarning($"Failed to {nameof(AddStack)}: {nameof(targetSlot)} is not empty.");
                return;
            }

            var newStack = AddNewStack(targetSlot);
            if (GetTree().ApplyCatchupInfo(catchupInfo) == null)
            {
                EmitSignal(nameof(StackMaterialUpdated), targetSlot, newStack);
            }
            else
            {
                newStack.Purge();
            }
        }
        
        [Puppet]
        private void RemoveStack(int targetSlot)
        {
            var stack = _stacks[targetSlot];
            if (stack == null)
            {
                GD.PushWarning($"Failed to {nameof(RemoveStack)}: {nameof(targetSlot)} is empty.");
                return;
            }
            stack.Purge();
            _stacks[targetSlot] = null;
            EmitSignal(nameof(StackMaterialUpdated), targetSlot, null);
        }

        [Puppet]
        private void UpdateStackSlot(int currentSlot, int newSlot)
        {
            // Validate origin and target
            var stack = _stacks[currentSlot];
            if (stack == null)
            {
                GD.PushWarning($"Failed to {nameof(UpdateStackSlot)}: {nameof(currentSlot)} is empty.");
                return;
            }
            
            if (!IsValidSlot(newSlot))
            {
                GD.PushWarning($"Failed to {nameof(UpdateStackSlot)}: invalid {nameof(newSlot)} index.");
                return;
            }
            
            if (_stacks[newSlot] != null)
            {
                GD.PushWarning($"Failed to {nameof(UpdateStackSlot)}: {nameof(newSlot)} is not empty.");
                return;
            }
            
            // Move!
            stack.Name = newSlot.ToString();
            _stacks[newSlot] = stack;
            _stacks[currentSlot] = null;
            EmitSignal(nameof(StackMaterialUpdated), currentSlot, null);
            EmitSignal(nameof(StackMaterialUpdated), newSlot, stack);
        }
    }
}