using System;
using System.Collections.Generic;
using ReactiveTag.Editor.Utils;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.UIElements;

namespace ReactiveTag.Editor
{
    public class ReactiveTagSettingWindow : EditorWindow
    {
        private SplitView _splitView = new SplitView(SplitView.Direction.Horizontal);
        private ReactiveTagDetailView _detailView = new ReactiveTagDetailView();
        private ReactiveTagTreeView _treeView;

        private TextAsset _textAsset;
        private YamlTagDefinition _yamlTagDefinition;
        private YamlTagDefinition _selectedTag;
        // メニューバーからウィンドウを開けるようにする
        [MenuItem("Window/ReactiveTagSetting")]
        private static void Open()
        {
            var window = GetWindow<ReactiveTagSettingWindow>("ReactiveTagSetting");
            window.Show();
        }

        private void ReloadFile()
        {
            _yamlTagDefinition = ReactiveTagIO.DecodeYamlToTag();
            _selectedTag = _yamlTagDefinition.Children[0];
            if (_treeView == null) _treeView = new ReactiveTagTreeView(new TreeViewState(), _yamlTagDefinition);
            else _treeView.ResetTree(_yamlTagDefinition);
            Repaint();
        }
        
        private void Save(YamlTagDefinition tag)
        {
            ReactiveTagIO.EncodeTagToYaml(tag);
            // AssetDatabase.Refresh();
            ReactiveTagGenerator.GenerateScripts();
        }

        private void UpdateTag(YamlTagDefinition tag)
        {
            _treeView.Reload();
        }

        private void OnEnable()
        {
            ReloadFile();
            _treeView.OnTagSelection += OnTagSelection;
            _detailView.OnTagUpdated += UpdateTag;
        }
        
        private void OnDisable()
        {
            _treeView.OnTagSelection -= OnTagSelection;
            _detailView.OnTagUpdated -= UpdateTag;
        }

        private void OnProjectChange()
        {
            ReloadFile();
        }

        private void OnTagSelection(YamlTagDefinition tag)
        {
            _selectedTag = tag;
        }
            
        private void OnGUI()
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Save"))
            {
                Save(_yamlTagDefinition);
            }
            
            if (GUILayout.Button("Discard"))
            {
                ReloadFile();
            }
            
            if (GUILayout.Button("Clear"))
            {
                Save(new YamlTagDefinition()
                {
                    Children = new List<YamlTagDefinition>
                    {
                        new YamlTagDefinition()
                        {
                            Name = "DefaultTag1",
                            IconPath = "",
                        }
                    },
                    IconPath = "",
                    Name = "ReactiveTagRoot"
                });
            }
            EditorGUILayout.EndHorizontal();
            
            _splitView.BeginSplitView();
            var splitLeftRect = _splitView.LeftRect;
            var searchString = GUILayout.TextField()
            if (_treeView != null) _treeView.OnGUI(new Rect(0, 0, splitLeftRect.width, splitLeftRect.height));
            _splitView.Split();
            _detailView.OnGUI(_selectedTag);
            _splitView.EndSplitView();
            Resources.Load("")
            
            EditorGUILayout.EndVertical();
        }
    }
}
