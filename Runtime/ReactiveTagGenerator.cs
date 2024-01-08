using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using VYaml.Serialization;

namespace ReactiveTag
{
    [ExecuteAlways]
    public class ReactiveTagGenerator: MonoBehaviour
    {
        public const string TagRootPath = "Assets/ReactiveTag_Generated/Tags";
        
        /// <summary>
        /// タグ用のスクリプトファイルを生成する
        /// </summary>
        public static void GenerateScripts()
        {
            var yaml = ReactiveTagIO.DecodeYamlToTag();
            if (yaml is null)
            {
                Debug.LogError($"Tag file not found: CustomTagList");
                return;
            }
            // Assets/ReactiveTag/Generated/Tags/以下にファイルを生成する
            // すでにファイルが存在する場合には一旦全部消す
            if (System.IO.Directory.Exists(TagRootPath))
            {
                System.IO.Directory.Delete(TagRootPath, true);
            }
            System.IO.Directory.CreateDirectory(TagRootPath);
            
            // ファイルの生成
            CreateRootTag(yaml);
            foreach (var child in yaml.Children)
            {
                RecursiveYamlInterpreter(child);
            }
        }

        private static void RecursiveYamlInterpreter(YamlTagDefinition tagElement)
        {
            if (tagElement.Children is not null)
            {
                foreach (var child in tagElement.Children)
                {
                    RecursiveYamlInterpreter(child);
                }
            }
            GenerateClassFiles(tagElement);
        }

        /// <summary>
        /// クラスファイルを生成する
        /// </summary>
        /// <param name="tagElement"></param>
        private static void GenerateClassFiles(YamlTagDefinition tagElement)
        {
            var tagChildren = tagElement.Children;
            
            var childrenTemplate = "        private _{0} _{1};\n";
            var constructorTemplate = "            this._{0} = new _{1}(\"{1}\", \"{2}\", this);\n";
            var childrenGetterTemplate = "        public _{0} {1} => this._{2};\n";

            var childrenScript = "";
            var constructorScript = "";
            var childrenGetterScript = "";
            if (tagChildren is not null)
            {
                foreach (var child in tagChildren)
                {
                    var lowerKey = child.Name.ToLower();
                    childrenScript += string.Format(childrenTemplate, child.Id, lowerKey);
                    constructorScript += string.Format(constructorTemplate, lowerKey, child.Id, child.Name);
                    childrenGetterScript += string.Format(childrenGetterTemplate, child.Id, child.Name, lowerKey);
                }
            }

            var classScript = string.Format(@"using System;

namespace ReactiveTag.Generated.Tags
{{
    public class _{0}: Tag
    {{
{1}
        public _{0}(string id, string name, Tag parent = null): base(id, name, parent)
        {{
{2}
        }}
{3}
    }}
}}
", tagElement.Id, childrenScript, constructorScript, childrenGetterScript);
            
            var path = $"{TagRootPath}/{tagElement.Id}.cs";
            System.IO.File.WriteAllText(path, classScript);
        }

        /// <summary>
        /// ルートのタグを生成する
        /// ルートはstaticメソッドでタグを取得できるようにする
        /// </summary>
        /// <param name="rootDef"></param>
        private static void CreateRootTag(YamlTagDefinition rootDef)
        {
            var rootName = rootDef.Name;
            var children = rootDef.Children;
            
            var rootClassTemplate = @"using System;

namespace ReactiveTag.Generated.Tags
{{
    public class {0}
    {{
{1}
    }}
}}
";
            var childrenTemplate = @"        private static _{0} _{1};
        public static _{0} {2} {{ get {{ 
            if (_{1} is null)
            {{
                _{1} = new _{0}(""{0}"", ""{2}"");
            }}
            return _{1};
        }} }}
";

            var childrenScript = "";
            if (children is not null)
            {
                foreach (var child in children)
                {
                    var lowerKey = child.Name.ToLower();
                    childrenScript += string.Format(childrenTemplate, child.Id, lowerKey, child.Name);
                }
            }
            var classScript = string.Format(rootClassTemplate, rootName, childrenScript);
            
            var path = $"{TagRootPath}/{rootName}.cs";
            System.IO.File.WriteAllText(path, classScript);
        }
    }
}