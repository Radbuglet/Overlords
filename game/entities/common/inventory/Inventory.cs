using System;
using System.Collections.Generic;
using System.Diagnostics;
using Godot;
using Overlords.helpers.tree;

namespace Overlords.game.entities.common.inventory
{
    public class Inventory : Node
    {
        [Export] private int _size = 9 * 4;
        private ItemStack[] _stacks;
        
        public override void _Ready()
        {
            _stacks = new ItemStack[_size];
        }
        
        public ItemStack GetStackInSlot(int slot)
        {
            return _stacks[slot];
        }

        public IEnumerable<(int slot, ItemStack stack)> GetStacks()
        {
            for (var index = 0; index < _stacks.Length; index++)
            {
                var stack = _stacks[index];
                if (stack == null) continue;
                yield return (index, stack);
            }
        }

        public void Clear()
        {
            for (var index = 0; index < _stacks.Length; index++)
            {
                _stacks[index]?.Purge();
                _stacks[index] = null;
            }
        }

        public void RemoveStack(int slot)
        {
            _stacks[slot]?.Purge();
            _stacks[slot] = null;
        }
        
        public void PutStack(int slot, ItemStack stack)
        {
            Debug.Assert(_stacks[slot] == null);
            _stacks[slot] = stack;
            stack.Name = $"Stack_{slot}";
            AddChild(stack);
        }
        
        public bool InsertStack(ItemStack stack)
        {
            // First pass (merging only)
            int? firstEmptySlot = null;
            for (var index = 0; index < _stacks.Length; index++)
            {
                if (stack.IsEmpty()) return true;
                
                var otherStack = _stacks[index];
                if (otherStack == null)
                {
                    if (firstEmptySlot == null)
                        firstEmptySlot = index;
                    continue;
                }
                
                if (!otherStack.IsSimilar(stack)) continue;
                var inserted = Math.Min(stack.Amount, otherStack.GetMaxAmount() - otherStack.Amount);
                stack.Amount -= inserted;
                otherStack.Amount += inserted;
            }
            
            // Stick the rest of the stack into the empty slot
            if (firstEmptySlot == null) return false;
            PutStack(firstEmptySlot.Value, stack);
            return true;
        }

        public StackConsumptionResult CheckRequiredStack(ItemStack stack, int requiredAmount)
        {
            var receipt = new StackConsumptionResult();
            for (var index = 0; index < _stacks.Length; index++)
            {
                if (requiredAmount <= 0)
                {
                    receipt.WasFullyConsumed = true;
                    return receipt;
                }
                
                var otherStack = _stacks[index];
                if (otherStack == null || !otherStack.IsSimilar(stack)) continue;
                
                var removed = Math.Min(otherStack.Amount, requiredAmount);
                otherStack.Amount -= removed;
                requiredAmount -= removed;
                receipt.ConsumedAmount += removed;
                receipt.ModifiedSlots.Add(index);
            }
            
            return receipt;
        }

        public void ConsumeRequiredStack(StackConsumptionResult receipt)
        {
            var amountLeft = receipt.ConsumedAmount;
            foreach (var slot in receipt.ModifiedSlots)
            {
                var stack = _stacks[slot];
                var removed = Math.Min(stack.Amount, amountLeft);
                stack.Amount -= removed;
                amountLeft -= removed;
                if (stack.IsEmpty()) RemoveStack(slot);
            }
        }
    }

    public class StackConsumptionResult
    {
        public bool WasFullyConsumed;
        public int ConsumedAmount;
        public readonly List<int> ModifiedSlots = new List<int>();
    }
}