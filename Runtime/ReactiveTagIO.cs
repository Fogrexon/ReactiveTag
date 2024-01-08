using System.Collections.Generic;
using System.Text;
using OpenCover.Framework.Model;
using UnityEditor;
using UnityEngine;
using VYaml.Serialization;

namespace ReactiveTag
{
    /// <summary>
    /// タグファイルの読み書きを担当するクラス
    /// </summary>
    public class ReactiveTagIO
    {
        public static YamlTagDefinition DecodeYamlToTag()
        {
            var text = Resources.Load<TextAsset>("ReactiveTagList");
            if (text is null)
            {
                Debug.LogError($"Tag file not found: ReactiveTagList");
                return null;
            }
            var utf8Bytes = Encoding.UTF8.GetBytes(text.text);
            var yaml = YamlSerializer.Deserialize<YamlTagDefinition>(utf8Bytes);
            LinkParentAndChildren(yaml, yaml.Children);
            return yaml;
        }
        
        private static void LinkParentAndChildren(YamlTagDefinition parent, List<YamlTagDefinition> children)
        {
            if (children is null) return;
            foreach (var child in children)
            {
                child.Parent = parent;
                LinkParentAndChildren(child, child.Children);
            }
        }
        
        public static bool EncodeTagToYaml(YamlTagDefinition yaml)
        {
            var utf8Bytes = YamlSerializer.Serialize(yaml).ToArray();
            var text = Encoding.UTF8.GetString(utf8Bytes);
            var filePath = "Assets/Resources/ReactiveTagList.yaml";
            
            if (!System.IO.File.Exists(filePath))
            {
                System.IO.File.Create(filePath).Close();
            }
            
            System.IO.File.WriteAllText(filePath, text);
            
            return true;
        }
    }
}