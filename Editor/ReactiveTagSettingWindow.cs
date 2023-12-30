using System;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace ReactiveTag.Editor
{
    public class ReactiveTagSettingWindow : EditorWindow
    {
        private ReactiveTagTreeView _treeView;

        private TextAsset _textAsset;
        private YamlTagDefinition _yamlTagDefinition;
        // メニューバーからウィンドウを開けるようにする
        [MenuItem("Window/ReactiveTagSetting")]
        private static void Open()
        {
            var window = GetWindow<ReactiveTagSettingWindow>("ReactiveTagSetting");
            window.Show();
        }

        private void ReloadFile()
        {
            _yamlTagDefinition = TagYamlDecoder.LoadYamlFromResource();
            _treeView = new ReactiveTagTreeView(new TreeViewState(), _yamlTagDefinition);
            Repaint();
        }

        private void OnEnable()
        {
            ReloadFile();
        }

        private void OnProjectChange()
        {
            ReloadFile();
        }

        private void OnGUI()
        {
            if (_treeView != null) _treeView.OnGUI(new Rect(0, 0, position.width, position.height));
        }
    }
}
