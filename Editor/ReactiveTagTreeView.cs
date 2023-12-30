using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;

namespace ReactiveTag.Editor
{
    public class ReactiveTagTreeView: TreeView
    {
        private YamlTagDefinition _tagDefinition;
        private Dictionary<int, YamlTagDefinition> _idToTagDefinition;
        private Dictionary<YamlTagDefinition, int> _tagDefinitionsToId;
        private int idCount = 0;
        public ReactiveTagTreeView(TreeViewState state, YamlTagDefinition tagDefinition) : base(state)
        {
            _tagDefinition = tagDefinition;
            
            _idToTagDefinition = new Dictionary<int, YamlTagDefinition>();
            _tagDefinitionsToId = new Dictionary<YamlTagDefinition, int>();
            
            idCount = 0;
            RecursiveIdToTagDefinition(tagDefinition);
            
            Reload();
        }
        
        private void RecursiveIdToTagDefinition(YamlTagDefinition tagElement)
        {
            _idToTagDefinition.Add(idCount, tagElement);
            _tagDefinitionsToId.Add(tagElement, idCount);
            idCount++;
            
            if (tagElement.Children is null)
            {
                return;
            }
            foreach (var child in tagElement.Children)
            {
                RecursiveIdToTagDefinition(child);
            }
        }
        
        protected override TreeViewItem BuildRoot()
        {
            return RecursiveYamlInterpreter(_tagDefinition, -1);
        }
        
        private TreeViewItem RecursiveYamlInterpreter(YamlTagDefinition tagElement, int depth)
        {
            var item = new TreeViewItem { id = _tagDefinitionsToId[tagElement], depth = depth, displayName = tagElement.Name };
            
            if (tagElement.Children is not null)
            {
                foreach (var child in tagElement.Children)
                {
                    var childItem = RecursiveYamlInterpreter(child, depth + 1);
                    item.AddChild(childItem);
                }
            }

            return item;
        }
    }
}