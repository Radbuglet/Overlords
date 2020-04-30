namespace Overlords.game.world.shared
{
    public static class Protocol
    {
        public enum ServerBoundPacket
        {
            SendMessage
        }
        
        public enum ClientBoundPacket
        {
            LoggedIn,
            OtherPlayerJoined,
            OtherPlayerDisconnected,
        }
    }
}