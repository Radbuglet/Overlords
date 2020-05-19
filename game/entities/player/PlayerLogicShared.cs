using System.Collections.Generic;
using Godot;
using Overlords.helpers.network.serialization;
using Overlords.helpers.tree.behaviors;

namespace Overlords.game.entities.player
{
    public class PlayerLogicShared: Node
    {
        public class NetworkConstructor
        {
            public static readonly StructSerializer<NetworkConstructor> Serializer = new StructSerializer<NetworkConstructor>(
                () => new NetworkConstructor(),
                new Dictionary<string, ISerializerRaw>
                {
                    [nameof(OwnerPeerId)] = new PrimitiveSerializer<int>()
                });

            public object Serialize()
            {
                return Serializer.Serialize(this);
            }

            public int OwnerPeerId;
        }
        
        [RequireParent] public Spatial PlayerRoot;
        
        public int OwnerPeerId;
        
        public void InitializeShared(int peerId)
        {
            this.InitializeBehavior();
            PlayerRoot.Name = $"player_{peerId}";
            OwnerPeerId = peerId;
        }
    }
}