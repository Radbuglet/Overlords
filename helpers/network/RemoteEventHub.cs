using System;
using Godot;
using Overlords.helpers.network.serialization;

namespace Overlords.helpers.network
{
    public class RemoteEventHub<TInbound, TOutbound>: Node where TInbound: Enum where TOutbound: Enum
    {
        public readonly RemoteEvent RemoteEvent;
        public readonly RemoteEventSplitter<int, TInbound> InboundSplitter;
        public readonly RemoteEventSplitter<int, TOutbound> OutboundSplitter;

        public RemoteEventHub(RemoteEvent remoteEvent)
        {
            Name = "RemoteEventHub";
            RemoteEvent = remoteEvent;
            InboundSplitter = new RemoteEventSplitter<int, TInbound>();
            OutboundSplitter = new RemoteEventSplitter<int, TOutbound>();
            remoteEvent.Connect(nameof(RemoteEvent.FiredRemotely), this, nameof(_ReceivedData));
        }

        private void _ReceivedData(int sender, object data)
        {
            try
            {
                InboundSplitter.ProcessDecoding(sender, data);
            }
            catch (CoreSerialization.DeserializationException e)
            {
                GD.PushWarning($"Failed to deserialize packet from {sender}. Reason:\n{e.Message}");
            }
        }

        public void Send(TOutbound type, object data)
        {
            RemoteEvent.Fire(OutboundSplitter.Encode(type, data));
        }
        
        public void SendUnreliable(TOutbound type, object data)
        {
            RemoteEvent.FireUnreliable(OutboundSplitter.Encode(type, data));
        }
        
        public void Send(int target, TOutbound type, object data)
        {
            RemoteEvent.Fire(target, OutboundSplitter.Encode(type, data));
        }
        
        public void SendUnreliable(int target, TOutbound type, object data)
        {
            RemoteEvent.FireUnreliable(target, OutboundSplitter.Encode(type, data));
        }

        public void BindHandler(TInbound target, Action<int, object> handler)
        {
            InboundSplitter.BindDecodingHandler(target, handler);
        }
    }
}