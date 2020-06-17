using Godot;
using Godot.Collections;

namespace Overlords.helpers.network.catchup
{
    public interface IRequiresCatchup
    {
        void CatchupState(int peerId);
    }

    public static class CatchupUtils
    {
        public static void CatchupToPeer(this Node root, int peerId)
        {
            root.PropagateCall(nameof(IRequiresCatchup.CatchupState), new Array {peerId});
        }
    }
}