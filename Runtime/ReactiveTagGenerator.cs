using UnityEngine;
using VYaml.Serialization;

namespace ReactiveTag
{
    [ExecuteAlways]
    public class ReactiveTagGenerator: MonoBehaviour
    {
        public static ReactiveTagGenerator Instance { get; private set; }
        
        private void Awake()
        {
            if (Instance is not null && Instance != this)
            {
                DestroyImmediate(this);
                return;
            }
            Instance = this;
            
#if !UNITY_EDITOR
            // destroyしない
            DontDestroyOnLoad(this);
#endif
            
            GenerateScripts();
        }
        
        // inspectorから実行
        [ContextMenu("Generate Scripts")]
        private void GenerateScripts()
        {
            // load tag hierarchy from yaml
            // load text from Resources
            var text = Resources.Load<TextAsset>("CustomTagList");
            if (text is null)
            {
                Debug.LogError($"Tag file not found: CustomTagList.yaml");
                return;
            }
            // textをUTF8のバイト列に変換
            Debug.Log(text.text);
            var utf8Bytes = System.Text.Encoding.UTF8.GetBytes(text.text);
            Debug.Log($"{utf8Bytes.Length} bytes");
            var yaml = YamlSerializer.Deserialize<YamlTagDefinition>(utf8Bytes);
            
            // generate scripts
            // Assets/ReactiveTag/Generated/Tags/以下にファイルを生成する
            // すでにファイルが存在する場合には一旦全部消す
            var path = "Assets/ReactiveTag/Generated/Tags";
            if (System.IO.Directory.Exists(path))
            {
                System.IO.Directory.Delete(path, true);
            }
            System.IO.Directory.CreateDirectory(path);
            
            RecursiveYamlInterpreter(yaml.Name, yaml.Children);
        }

        private void RecursiveYamlInterpreter(string baseTag, YamlTagDefinition[] children)
        {
            Debug.Log(baseTag);
            if (children is null)
            {
                return;
            }
            foreach (var child in children)
            {
                var tagName = child.Name;
                var tagFullName = $"{baseTag}{tagName}";
                RecursiveYamlInterpreter(tagFullName, child.Children);

                GenerateClassFiles(tagFullName, child.Children);
            }
        }

        private void GenerateClassFiles(string tagFullName, YamlTagDefinition[] tagChildren)
        {
            // 複数行にまたがる文字列をプログラムに書く
            var classTemplate = @"
using ReactiveTag;
using UnityEngine;

namespace ReactiveTag.Generated
{
    public class {0}: Tag
    {
{1}

        public {0}(Guid id, string name): base(id, name)
        {
            {2}
        }

{3}
    }
}
";
            var childrenTemplate = "         private {0} _{1};\n";
            var constructorTemplate = "this._{0} = new {1}(System.Guid.NewGuid(), \"{2}\");\n";
            var childrenGetterTemplate = "        public {0} {1} => this._{2};\n";

            var childrenScript = "";
            var constructorScript = "";
            var childrenGetterScript = "";
            foreach (var child in tagChildren)
            {
                var childrenFullName = $"{tagFullName}{child.Name}";
                var lowerKey = child.Name.ToLower();
                childrenScript += string.Format(childrenTemplate, child.Name, lowerKey);
                constructorScript += string.Format(constructorTemplate, lowerKey, childrenFullName, child.Name);
                childrenGetterScript += string.Format(childrenGetterTemplate, childrenFullName, child.Name, lowerKey);
            }
            
            var classScript = string.Format(classTemplate, tagFullName, childrenScript, constructorScript, childrenGetterScript);
            
            var path = $"Assets/ReactiveTag/Generated/Tags/{tagFullName}.cs";
            System.IO.File.WriteAllText(path, classScript);
        }
    }
}