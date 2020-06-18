using System;
using System.Linq;
using Godot;
using Overlords.helpers.tree;

namespace Overlords.helpers.network
{
    public class Quarantine : Node
    {
        public delegate void QuarantineChecking();
        public delegate void QuarantineOver();
        
        private const string QuarantinedGroupName = "quarantined";
        public bool IsSafe { get; private set; }
        
        public override void _Ready()
        {
            this.Initialize();
            AddToGroup(QuarantinedGroupName);
        }
        

        public static bool PerformSweep(SceneTree tree, out string problem)
        {
            foreach (var suspect in tree.GetNodesInGroup(QuarantinedGroupName).Cast<Quarantine>())
            {
                var suspectGameObj = suspect.GetParent();
                try
                {
                    suspectGameObj.EmitSignal(nameof(QuarantineChecking));
                }
                catch (QuarantineContamination reason)
                {
                    problem = $"Quarantine violation at {suspect.GetPath()}: {reason.Message}";
                    return false;
                }
                suspect.IsSafe = true;
                suspect.RemoveFromGroup(QuarantinedGroupName);
                suspectGameObj.EmitSignal(nameof(QuarantineOver));
            }

            problem = null;
            return true;
        }
    }

    public class QuarantineContamination : Exception
    {
        public QuarantineContamination(string reason): base(reason)
        { }

        public QuarantineContamination() : base("No reason provided.")
        { }
    }
}