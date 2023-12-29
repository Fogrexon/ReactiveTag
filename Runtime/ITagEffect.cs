using UnityEngine;

namespace ReactiveTag
{
    public interface ITagEffect
    {
        public Tag TargetTag { get; }

        public void OnTagAttach();
        
        public void OnTagDetach();
    }
}