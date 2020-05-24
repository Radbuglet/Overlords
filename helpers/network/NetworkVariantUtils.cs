using System;
using Godot;
using Overlords.helpers.tree;
using Overlords.helpers.tree.behaviors;

namespace Overlords.helpers.network
{
    public static class NetworkVariantUtils
    {
        public enum ObjectVariant
        {
            Server,
            LocalAuthoritative,
            LocalPuppet
        }

        public static ObjectVariant GetNetworkVariant(this SceneTree tree, int ownerPeerId)
        {
            return tree.GetNetworkMode() == NetworkUtils.NetworkMode.Server
                ? ObjectVariant.Server
                : ownerPeerId == tree.GetNetworkUniqueId()
                    ? ObjectVariant.LocalAuthoritative
                    : ObjectVariant.LocalPuppet;
        }
        
        public static void ApplyNetworkVariant(this Node gameObject, ObjectVariant variant, Type behaviorServer, Type behaviorClientShared, Type behaviorClientLocal, Type behaviorClientPuppet) {
            void RemoveBehavior(Type type)
            {
                if (type == null) return;
                gameObject.GetBehaviorDynamic(type).Purge();
            }
            
            if (variant == ObjectVariant.Server)
            {
                RemoveBehavior(behaviorClientShared);
                RemoveBehavior(behaviorClientLocal);
                RemoveBehavior(behaviorClientPuppet);
            }
            else
            {
                RemoveBehavior(behaviorServer);
                RemoveBehavior(variant == ObjectVariant.LocalAuthoritative
                    ? behaviorClientPuppet
                    : behaviorClientLocal);
            }
        }
    }
}