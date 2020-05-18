using System;

namespace Overlords.helpers.tree.behaviors
{
    [AttributeUsage(AttributeTargets.Field)]
    public class LinkNodeEditor : Attribute
    {
        public readonly string ExportedPropName;

        public LinkNodeEditor(string exportedPropName)
        {
            ExportedPropName = exportedPropName;
        }
    }
}