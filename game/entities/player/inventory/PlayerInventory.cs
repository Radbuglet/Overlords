using System.Collections.Generic;
using System.Diagnostics;
using Godot;
using Godot.Collections;
using Overlords.helpers.csharp;
using Overlords.helpers.network;
using Overlords.helpers.tree;

namespace Overlords.game.entities.player.inventory
{
    public class PlayerInventory : Node, IRequiresCatchup
    {
        [Export] private int _size;
        [FieldNotNull] [Export] private PackedScene _stackPrefab;
        private ItemStackRoot[] _itemStacks;

        public override void _EnterTree()
        {
            _itemStacks = new ItemStackRoot[_size];
        }

        private IEnumerable<(int, ItemStackRoot)> GetStacks()
        {
            for (var i = 0; i < _size; i++)
            {
                yield return (i, _itemStacks[i]);
            }
        }

        public ItemStackRoot GetStack(int slot)
        {
            return _itemStacks.TryGetValue(slot, out var stack) ? stack : null;
        }
        
        public void PutStack(int slot, ItemStackRoot stack)
        {
            Debug.Assert(_itemStacks[slot] == null);
            _itemStacks[slot] = stack;
            stack.Name = slot.ToString();
            AddChild(stack);
        }

        public bool InsertStack(ItemStackRoot stack)
        {
            int? firstEmptySlot = null;
            
            // Insert into existing stacks
            foreach (var (slot, otherStack) in GetStacks())
            {
                if (stack.IsEmpty())
                    return true;
                
                if (otherStack == null)
                {
                    if (firstEmptySlot == null)
                        firstEmptySlot = slot;
                    continue;
                }
                
                stack.TransferInto(otherStack);
            }

            // Insert into the first empty stack if the stack is still full
            if (stack.IsEmpty())
                return true;

            if (firstEmptySlot == null)
                return false;
            
            PutStack(firstEmptySlot.Value, stack);
            return true;
        }

        public CatchupState CatchupOverNetwork(int peerId)
        {
            var allocatedSlots = new Array();
            foreach (var (slot, stack) in GetStacks())
            {
                if (stack != null)
                    allocatedSlots.Add(slot);
            }
            return new CatchupState(allocatedSlots, true);
        }

        public void HandleCatchupState(object slotsRaw)
        {
            if (!(slotsRaw is Array slots))
            {
                GD.PushWarning("Failed to allocate slots on inventory container: arg wasn't array!");
                return;
            }

            foreach (var slotRaw in slots)
            {
                if (!(slotRaw is int slot))
                {
                    GD.PushWarning("Failed to allocate a slot: entry wasn't an int!");
                    continue;
                }

                if (!_itemStacks.HasIndex(slot))
                {
                    GD.PushWarning("Failed to allocate a slot: slot index is invalid!");
                    continue;
                }

                var stack = (ItemStackRoot) _stackPrefab.Instance();
                PutStack(slot, stack);
            }
        }
    }
}