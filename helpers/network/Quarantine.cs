using System;
using System.Linq;
using Godot;

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
