using System.Security.Cryptography;
using System.Text;
using VYaml.Annotations;

namespace ReactiveTag
{
    [YamlObject]
    public partial class YamlTagDefinition
    {
        /// <summary>
        /// タグ名からIDを生成するスクリプト
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GenerateIdFromFullTag(string path)
        {
            var bytes = Encoding.UTF8.GetBytes(path);
            var hash = MD5.Create().ComputeHash(bytes);
            var sb = new StringBuilder();
            foreach (var t in hash)
            {
                sb.Append(t.ToString("x2"));
            }
            return "_" + sb.ToString();
        }
        
        public string Name;

        public YamlTagDefinition[] Children;

        [YamlIgnore] private string _id;
        
        [YamlIgnore]
        public string Id
        {
            get
            {
                if (string.IsNullOrEmpty(_id))
                {
                    _id = GenerateIdFromFullTag(Name);
                }
                return _id;
            }
            set => _id = value;
        }
    }
}