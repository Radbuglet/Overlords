using System;
using System.Linq;
using Godot;
using Object = Godot.Object;

namespace Overlords.helpers.network
{
	public interface IQuarantineInfectable
	{
		void _QuarantineChecking();
	}

	public interface IQuarantinedListener
	{
		void _QuarantineOver();
	}
	
	/// <summary>
	/// Since catchup is handled by all affected nodes in no particular order (nor a central authority to validate any packet),
	/// nodes created remotely which require catchup validation should use the Quarantine utility.
	/// 
	/// Nodes that must check an invariant declare themselves as being a possible "contaminant" using `FlagQuarantineInfectable`.
	/// Nodes that rely upon some state being valid declare themselves as being a "quarantine listener". All of these nodes will
	/// be called once all infectable nodes are checked. They will be called in scene tree order in a way almost identical to
	/// `_EnteredTree`. 
	/// </summary>
	public static class Quarantine
	{
		private const string QuarantineInfectableGroupName = "quarantined_infectable";
		private const string QuarantineListenerGroupName = "quarantined_listener";

		public static void FlagQuarantineInfectable<T>(this T target) where T: Node, IQuarantineInfectable
		{
			target.AddToGroup(QuarantineInfectableGroupName);
		}
		
		public static void FlagQuarantineListener<T>(this T target) where T: Node, IQuarantinedListener
		{
			target.AddToGroup(QuarantineListenerGroupName);
		}
		
		public static bool PerformQuarantineSweep(this SceneTree tree, out string problem)
		{
			foreach (var suspectNode in tree.GetNodesInGroup(QuarantineInfectableGroupName).Cast<Node>())
			{
				var suspectIface = (IQuarantineInfectable) suspectNode;
				try
				{
					suspectIface._QuarantineChecking();
				}
				catch (QuarantineContamination reason)
				{
					problem = $"Quarantine violation at {suspectNode.GetPath()}: {reason.Message}";
					return false;
				}
				suspectNode.RemoveFromGroup(QuarantineInfectableGroupName);
			}

			foreach (var listener in tree.GetNodesInGroup(QuarantineListenerGroupName).Cast<Node>())
			{
				if (!Object.IsInstanceValid(listener) || !listener.IsInsideTree()) continue;  // ...to handle Nodes that get purged during quarantine release.
				listener.RemoveFromGroup(QuarantineListenerGroupName);
				((IQuarantinedListener) listener)._QuarantineOver();
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
