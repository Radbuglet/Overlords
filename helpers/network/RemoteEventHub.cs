using System;
using System.Collections.Generic;
using Godot;
using Overlords.helpers.network.serialization;

namespace Overlords.helpers.network
{
    public class RemoteEventHub<TInbound, TOutbound> : Node, IGenericRpc<(TOutbound, object)>
        where TInbound : Enum where TOutbound : Enum
    {
        public delegate void PacketHandler<in TPacket>(int sender, TPacket packet);

        private readonly Dictionary<int, Action<int, object>> _inboundHandlers =
            new Dictionary<int, Action<int, object>>();

        private readonly RemoteEvent _remoteEvent;

        public RemoteEventHub(RemoteEvent boundEvent)
        {
            _remoteEvent = boundEvent;
            _remoteEvent.Connect(nameof(RemoteEvent.FiredRemotely), this, nameof(_ReceivedInbound));
        }

        public void GenericFire(IEnumerable<int> targets, bool reliable, (TOutbound, object) data)
        {
            var (packetType, packedData) = data;
            _remoteEvent.GenericFire(targets, reliable, new HubPacket
            {
                EventType = packetType.SerializeEnum(),
                EventArg = packedData
            }.Serialize());
        }

        private void _ReceivedInbound(int sender, object data)
        {
            HubPacket packetRoot;
            try
            {
                packetRoot = HubPacket.Serializer.Deserialize(data);
            }
            catch (DeserializationException)
            {
                GD.PushWarning($"Failed to deserialize hub packet from {sender}.");
                return;
            }

            if (_inboundHandlers.TryGetValue(packetRoot.EventType, out var handler))
            {
                handler(sender, packetRoot.EventArg);
            }
            else
            {
                GD.PushWarning(
                    $"No packet handler defined for packet received from {sender} (or the packet type is invalid).");
            }
        }

        public void BindHandler<TPacket>(TInbound packetType, ISerializer<TPacket> serializer,
            PacketHandler<TPacket> handler)
        {
            _inboundHandlers.Add(packetType.SerializeEnum(), (sender, packetRaw) =>
            {
                TPacket parsedPacket;
                try
                {
                    parsedPacket = serializer.Deserialize(packetRaw);
                }
                catch (DeserializationException err)
                {
                    GD.PushWarning($"Failed to deserialize hub packet root. Reason: {err.Message}");
                    return;
                }

                handler(sender, parsedPacket);
            });
        }

        private class HubPacket
        {
            public static readonly StructSerializer<HubPacket> Serializer = new StructSerializer<HubPacket>(
                () => new HubPacket(),
                new Dictionary<string, ISerializerRaw>
                {
                    [nameof(EventType)] = new PrimitiveSerializer<int>(),
                    [nameof(EventArg)] = new PrimitiveSerializer<object>()
                });

            public object EventArg;
            public int EventType;

            public object Serialize()
            {
                return Serializer.Serialize(this);
            }
        }
    }
}