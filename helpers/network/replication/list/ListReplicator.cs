using Godot;
using Overlords.helpers.tree.conditionals;

namespace Overlords.helpers.network.replication.list
{
    // TODO: Implement
    public class ListReplicator: MultiNetworkNode<ListReplicatorServer, ListReplicatorClient, Node>
    {
        protected override Node MakeCommon()
        {
            return null;
        }

        protected override ListReplicatorServer MakeServer(Node common)
        {
            return new ListReplicatorServer();
        }

        protected override ListReplicatorClient MakeClient(Node common)
        {
            return new ListReplicatorClient();
        }
    }
}