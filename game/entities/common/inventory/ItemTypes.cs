using System;
using System.Collections.Generic;
using Overlords.helpers.network.serialization;

namespace Overlords.game.entities.common.inventory
{
    public enum ItemType
    {
        Cobble,
        Stone,
        IronOre,
        Iron,
        GoldOre,
        Gold,
        Diamond,
        EnrichedDiamond,
        DiamondPlate,
        Skull,
        Turret
    }

    public sealed class ItemStackSimple : ItemStack
    {
        private readonly ItemType _type;
        
        public ItemStackSimple(ItemType type, int amount)
        {
            _type = type;
            Amount = Math.Min(amount, GetMaxAmount());
        }

        public override int GetMaxAmount()
        {
            return _type == ItemType.Turret || _type == ItemType.Skull ? 8 : 64;
        }

        public override bool IsSimilar(ItemStack other)
        {
            return other is ItemStackSimple otherStack && otherStack._type == _type;
        }
    }

    public static class ItemDecoding
    {
        public class SerializedItem
        {
            public readonly StructSerializer<SerializedItem> Serializer = new StructSerializer<SerializedItem>(
                () => new SerializedItem(),
                new Dictionary<string, ISerializerRaw>
                {
                    [nameof(Type)] = new EnumSerializer<ItemType>(),
                    [nameof(Amount)] = new PrimitiveSerializer<int>()
                });

            public ItemType Type;
            public int Amount;

            public ItemStackSimple ToStack()
            {
                return new ItemStackSimple(Type, Amount);
            }
        }
    }
}