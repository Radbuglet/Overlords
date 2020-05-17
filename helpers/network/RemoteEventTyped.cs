using Godot;
using Overlords.helpers.network.serialization;

namespace Overlords.helpers.network
{
    public class RemoteEventTyped<TArgument>: Node, IGenericRpc<TArgument>
    {
        private readonly ISerializer<TArgument> _argSerializer;
        private readonly RemoteEvent _remoteEvent;

        public delegate void EventHandleRemote(int sender, TArgument argument);
        public event EventHandleRemote OnRemoteMessage;

        public RemoteEventTyped(ISerializer<TArgument> argSerializer, RemoteEvent remoteEvent)
        {
            _argSerializer = argSerializer;
            _remoteEvent = remoteEvent;
            remoteEvent.Connect(nameof(remoteEvent.FiredRemotely), this, nameof(_PackedReceived));
        }

        private void _PackedReceived(int sender, object dataRaw)
        {
            TArgument deserializedArg;
            try
            {
                deserializedArg = _argSerializer.Deserialize(dataRaw);
            }
            catch (DeserializationException e)
            {
                GD.PushWarning($"Failed to deserialize typed remote event argument: {e.Message}");
                return;
            }

            OnRemoteMessage?.Invoke(sender, deserializedArg);
        }

        public void GenericFire(int? target, bool reliable, TArgument data)
        {
            try
            {
                _remoteEvent.GenericFire(target, reliable, _argSerializer.Serialize(data));
            }
            catch (SerializationException e)
            {
                GD.PushWarning($"Failed to serialize typed remote event argument: {e.Message}");
            }
        }
        
        public static RemoteEventTyped<TArgument> Attach(Node self, string remoteName, ISerializer<TArgument> argSerializer)
        {
            var remoteEvent = new RemoteEvent
            {
                Name = remoteName,
            };

            var typedWrapper = new RemoteEventTyped<TArgument>(argSerializer, remoteEvent)
            {
                Name = "typedWrapper"
            };
            
            remoteEvent.AddChild(typedWrapper);
            self.AddChild(remoteEvent);

            return typedWrapper;
        }
    }
}