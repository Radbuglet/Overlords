using System.Collections.Generic;
using System.Linq;
using Godot;
using Overlords.helpers.tree;

namespace Overlords.helpers.network
{
    public interface ICullsNetwork
    {
        bool IsLocallyVisibleTo(int peerId);
    }
    
    public static class NetworkCullingExtensions
    {
        public static bool IsVisibleTo(this Node target, int peerId)
        {
            return target.EnumerateAncestors().All(
                ancestor => !(ancestor is ICullsNetwork cullsNetwork) || cullsNetwork.IsLocallyVisibleTo(peerId));
        }

        public static IEnumerable<int> EnumerateNetworkViewers(this Node target, IEnumerable<int> filterFrom = null)
        {
            if (filterFrom == null)
                filterFrom = target.GetTree().GetNetworkConnectedPeers();

            foreach (var peerId in filterFrom)
            {
                if (target.IsVisibleTo(peerId))
                    yield return peerId;
            }
        }

        public static IEnumerable<int> EnumerateNetworkViewersLocal(this ICullsNetwork target, IEnumerable<int> filterFrom)
        {
            return filterFrom.Where(target.IsLocallyVisibleTo);
        }
    }
}