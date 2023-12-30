using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using VYaml.Serialization;

namespace ReactiveTag
{
    [ExecuteAlways]
    public class ReactiveTagGenerator: MonoBehaviour
    {
        public static ReactiveTagGenerator Instance { get; private set; }
        
        /// <summary>
        /// タグ名からIDを生成するスクリプト
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GenerateIdFromFullTag(string path)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(path);
            byte[] hash = MD5.Create().ComputeHash(bytes);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("x2"));
            }
            return "_" + sb.ToString();
        }
        
        public const string TagRootPath = "Assets/ReactiveTag_Generated/Tags";
        
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
        /// <summary>
        /// タグ用のスクリプトファイルを生成する
        /// inspectorから実行できる
        /// </summary>
        [ContextMenu("Generate Scripts")]
        private void GenerateScripts()
        {
            var text = Resources.Load<TextAsset>("CustomTagList");
            if (text is null)
            {
                Debug.LogError($"Tag file not found: CustomTagList.yaml");
                return;
            }
            var utf8Bytes = System.Text.Encoding.UTF8.GetBytes(text.text);
            var yaml = YamlSerializer.Deserialize<YamlTagDefinition>(utf8Bytes);
            
            // Assets/ReactiveTag/Generated/Tags/以下にファイルを生成する
            // すでにファイルが存在する場合には一旦全部消す
            if (System.IO.Directory.Exists(TagRootPath))
            {
                System.IO.Directory.Delete(TagRootPath, true);
            }
            System.IO.Directory.CreateDirectory(TagRootPath);
            
            // ファイルの生成
            CreateRootTag(yaml.Name, yaml.Children);
            RecursiveYamlInterpreter(yaml.Name, yaml.Children);
        }

        private void RecursiveYamlInterpreter(string baseTag, YamlTagDefinition[] children)
        {
            if (children is null || children.Length == 0)
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tagFullName"></param>
        /// <param name="tagChildren"></param>
        private void GenerateClassFiles(string tagFullName, YamlTagDefinition[] tagChildren)
        {
            var childrenTemplate = "        private {0} _{1};\n";
            var constructorTemplate = "            this._{0} = new {1}(Guid.NewGuid(), \"{2}\", this);\n";
            var childrenGetterTemplate = "        public {0} {1} => this._{2};\n";

            var childrenScript = "";
            var constructorScript = "";
            var childrenGetterScript = "";
            if (tagChildren is not null)
            {
                foreach (var child in tagChildren)
                {
                    var childFullName = $"{tagFullName}{child.Name}";
                    var childId = GenerateIdFromFullTag(childFullName);
                    var lowerKey = child.Name.ToLower();
                    childrenScript += string.Format(childrenTemplate, childId, lowerKey);
                    constructorScript += string.Format(constructorTemplate, lowerKey, childId, child.Name);
                    childrenGetterScript += string.Format(childrenGetterTemplate, childId, child.Name, lowerKey);
                }
            }

            var tagId = GenerateIdFromFullTag(tagFullName);
            var classScript = string.Format(@"using System;

namespace ReactiveTag.Generated.Tags
{{
    public class {0}: Tag
    {{
{1}
        public {0}(Guid id, string name, Tag parent = null): base(id, name, parent)
        {{
{2}
        }}
{3}
    }}
}}
", tagId, childrenScript, constructorScript, childrenGetterScript);
            
            var path = $"{TagRootPath}/{tagId}.cs";
            System.IO.File.WriteAllText(path, classScript);
        }

        /// <summary>
        /// ルートのタグを生成する
        /// ルートはstaticメソッドでタグを取得できるようにする
        /// </summary>
        /// <param name="rootName"></param>
        /// <param name="children"></param>
        private void CreateRootTag(string rootName, YamlTagDefinition[] children)
        {
            var rootClassTemplate = @"using System;

namespace ReactiveTag.Generated.Tags
{{
    public class {0}
    {{
{1}
    }}
}}
";
            var childrenTemplate = @"        private static {0} _{1};
        public static {0} {2} {{ get {{ 
            if (_{1} is null)
            {{
                _{1} = new {0}(Guid.NewGuid(), ""{2}"");
            }}
            return _{1};
        }} }}
";

            var childrenScript = "";
            if (children is not null)
            {
                foreach (var child in children)
                {
                    var childFullName = $"{rootName}{child.Name}";
                    var childId = GenerateIdFromFullTag(childFullName);
                    
                    var lowerKey = child.Name.ToLower();
                    childrenScript += string.Format(childrenTemplate, childId, lowerKey, child.Name);
                }
            }
            var classScript = string.Format(rootClassTemplate, rootName, childrenScript);
            
            var path = $"{TagRootPath}/{rootName}.cs";
            System.IO.File.WriteAllText(path, classScript);
        }
    }
}