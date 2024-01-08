using System.Collections.Generic;
using System.Linq;
using ReactiveTag.Utils;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace ReactiveTag.Editor
{
    public class ReactiveTagTreeView: TreeView
    {
        private YamlTagDefinition _tagDefinition;
        private Dictionary<int, YamlTagDefinition> _idToTagDefinition;
        private Dictionary<YamlTagDefinition, int> _tagDefinitionsToId;
        private int idCount = 0;
        
        public delegate void OnTagSelectionAction(YamlTagDefinition tag);
        public OnTagSelectionAction OnTagSelection;
        public ReactiveTagTreeView(TreeViewState state, YamlTagDefinition tagDefinition) : base(state)
        {
            _tagDefinition = tagDefinition;
            
            _idToTagDefinition = new Dictionary<int, YamlTagDefinition>();
            _tagDefinitionsToId = new Dictionary<YamlTagDefinition, int>();
            
            idCount = 1;
            RecursiveTableCreation(tagDefinition);
            
            Reload();
        }
        
        public void ResetTree(YamlTagDefinition tagDefinition)
        {
            _tagDefinition = tagDefinition;
            _idToTagDefinition = new Dictionary<int, YamlTagDefinition>();
            _tagDefinitionsToId = new Dictionary<YamlTagDefinition, int>();
            
            idCount = 1;
            RecursiveTableCreation(tagDefinition);
            
            Reload();
        }

        private void RecursiveTableCreation(YamlTagDefinition tagElement)
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
                RecursiveTableCreation(child);
            }
        }
        
        protected override TreeViewItem BuildRoot()
        {
            var root = new TreeViewItem { id = 0, depth = -1, displayName = "Root" };
            root.AddChild(RecursiveYamlInterpreter(_tagDefinition, 0));
            return root;
        }
        
        private TreeViewItem RecursiveYamlInterpreter(YamlTagDefinition tagElement, int depth)
        {
            if (!_tagDefinitionsToId.TryGetValue(tagElement, out var id))
            {
                RecursiveTableCreation(tagElement);
                id = _tagDefinitionsToId[tagElement];
            }
            
            
            var item = new TreeViewItem { id = id, depth = depth, displayName = tagElement.Name };
            
            if (tagElement.Children is not null)
            {
                foreach (var child in tagElement.Children)
                {
                    var childItem = RecursiveYamlInterpreter(child, depth + 1);
                    item.AddChild(childItem);
                }
            }

            if (depth == -1) Debug.Log(item);
            return item;
        }

        protected override void SingleClickedItem(int id)
        {
            var selectedTag = _idToTagDefinition[id];
            OnTagSelection?.Invoke(selectedTag);
        }
        
        protected override void ContextClickedItem(int id)
        {
            var ev = Event.current;
            ev.Use();
    
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Add Child"), false, () =>
            {
                var selectedTag = _idToTagDefinition[id];
                var newTag = new YamlTagDefinition()
                {
                    Name = TagValidator.CreateNewTagName(selectedTag),
                    IconPath = "",
                    Parent = selectedTag,
                };
                if (selectedTag.Children is null) selectedTag.Children = new List<YamlTagDefinition>();
                selectedTag.Children.Add(newTag);
                Reload();
            });
            if (id == 1)
            {
                // ルートアイテムなので削除不可
                menu.AddDisabledItem(new GUIContent("Remove"));
            }
            else
            {
                menu.AddItem(new GUIContent("Remove"), false, () =>
                {
                    var selectedTag = _idToTagDefinition[id];
                    var parent = selectedTag.Parent;
                    if (parent != null) parent.Children.Remove(selectedTag);
                    Reload();
                });
            }
            menu.ShowAsContext();
        }
    }
}