using System.Collections.Generic;
using Godot;
using Godot.Collections;
using Overlords.helpers.network;

namespace Overlords.game.entities.player.inventory
{
    public class PlayerInventory : Node, ICatchesUpSelf
    {
        [Export] private int _size;
        [Export] private PackedScene _stackPrefab;
        private ItemStackRoot[] _items;
        
        // Scene tree garbage
        private PlayerRoot PlayerRoot => GetParent<PlayerRoot>();
        
        public override void _Ready()
        {
            _items = new ItemStackRoot[_size];
        }
        
        // Interaction methods  TODO: Implement on server and replicate
        public IEnumerable<(int, ItemStackRoot)> GetStacks(bool filterEmpty)
        {
            for (var i = 0; i < _size; i++)
            {
                var stack = _items[i];
                if (filterEmpty && stack == null)
                    continue;
                yield return (i, stack);
            }
        }
        
        // Networking
        public CatchupState CatchupOverNetwork(int peerId)
        {
            var allocateStacks = new Array();
            foreach (var (slot, _) in GetStacks(true))
            {
                allocateStacks.Add(slot);
            }
            
            return peerId == PlayerRoot.State.OwnerPeerId.Value ?
                new CatchupState(true, allocateStacks) :
                new CatchupState(false);
        }

        public void HandleCatchupState(object stacksRaw)
        {
            if (!(stacksRaw is Array stacks))
                throw new InvalidCatchupException("Failed to pre-allocate stacks: argument was not an array.");

            foreach (var slotRaw in stacks)
            {
                if (!(slotRaw is int slot))
                    throw new InvalidCatchupException("Failed to pre-allocate stacks: slot was not an integer.");

                if (_items[slot] != null)
                    throw new InvalidCatchupException("Failed to pre-allocate stacks: slot already has an item.");
                
                var stack = _stackPrefab.Instance();
                stack.Name = slot.ToString();
                AddChild(stack);
            }
        }
    }
}