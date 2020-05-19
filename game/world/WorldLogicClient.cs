using Godot;
using Overlords.game.constants;
using Overlords.helpers.network.replication;
using Overlords.helpers.network.serialization;
using Overlords.helpers.tree.behaviors;
using Overlords.helpers.tree.interfaceBehaviors;

namespace Overlords.game.world
{
    public interface IEntityCatchupConsumer
    {
        bool CreateFromNetConstructor(object constructorData);
    }
    
    public class WorldLogicClient: Node
    {
        [RequireBehavior] public WorldLogicShared LogicShared;
        
        [LinkNodeStatic("../EntityContainer")]
        public ListReplicator EntityContainer;
        
        public override void _Ready()
        {
            this.InitializeBehavior();
            EntityContainer.BuildRemoteInstance = instanceRaw =>
            {
                if (!Protocol.ReplicatedEntity.Serializer.TryDeserializedOrWarn(instanceRaw, out var instanceConfig))
                {
                    GD.PushWarning("Invalid instance: failed to deserialize");
                    return;
                }

                var instanceType = LogicShared.TypeRegistrar.GetTypeFromIndex(instanceConfig.TypeIndex);
                if (instanceType == null)
                {
                    GD.PushWarning("Unknown instance type!");
                    return;
                }

                var instance = instanceType.Scene.Instance();
                if (instance.GetImplementation<IEntityCatchupConsumer>().CreateFromNetConstructor(instanceConfig.Constructor))
                    AddChild(instance);
                else
                    instance.QueueFree();
            };
        }
    }
}