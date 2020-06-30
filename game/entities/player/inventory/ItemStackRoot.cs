using System;
using Godot;

namespace Overlords.game.entities.player.inventory
{
    public enum ItemMaterial
    {
        Stick,
        Stone,
        Bone
    }
    
    public class ItemStackRoot : Node
    {
        public int Amount;
        public ItemMaterial Material;

        public bool IsSimilar(ItemStackRoot other)
        {
            return other.Material == Material;
        }

        public bool IsEmpty()
        {
            return Amount == 0;
        }

        public int GetAmountAcceptable()
        {
            return GetMaxAmount() - Amount;
        }
        
        public int GetMaxAmount()
        {
            return 64;
        }

        public void TransferInto(ItemStackRoot other, int? maxAmount = null)
        {
            var amountTransferred = Math.Min(Amount, other.GetAmountAcceptable());
            if (maxAmount != null && amountTransferred > maxAmount)
                amountTransferred = maxAmount.Value;
            
            Amount -= amountTransferred;
            other.Amount += amountTransferred;
        }
    }
}