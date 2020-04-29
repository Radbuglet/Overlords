namespace Overlords.game.world
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