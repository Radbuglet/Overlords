using System;

namespace Overlords.helpers.behaviors
{
    [AttributeUsage(AttributeTargets.Field)]
    public class LinkNodePath : Attribute
    {
        public readonly string ExportedPropName;

        public LinkNodePath(string exportedPropName)
        {
            ExportedPropName = exportedPropName;
        }
    }
}