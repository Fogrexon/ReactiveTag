using System.Text;
using OpenCover.Framework.Model;
using UnityEditor;
using UnityEngine;
using VYaml.Serialization;

namespace ReactiveTag
{
    /// <summary>
    /// yamlファイルを読み込んで、タグを生成するクラス
    /// </summary>
    public class TagYamlDecoder
    {
        public static YamlTagDefinition LoadYamlFromResource()
        {
            var text = Resources.Load<TextAsset>("ReactiveTagList");
            if (text is null)
            {
                Debug.LogError($"Tag file not found: ReactiveTagList");
                return null;
            }
            var utf8Bytes = Encoding.UTF8.GetBytes(text.text);
            var yaml = YamlSerializer.Deserialize<YamlTagDefinition>(utf8Bytes);
            return yaml;
        }
    }
}