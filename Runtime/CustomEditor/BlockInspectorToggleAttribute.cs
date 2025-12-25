using System;

namespace SHUU._CustomEditor
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public class BlockInspectorToggleAttribute : Attribute
    {
        public bool inheritable { get; private set; }




        public BlockInspectorToggleAttribute(bool _inheritable = true)
        {
            inheritable = _inheritable;
        }
    }
}
