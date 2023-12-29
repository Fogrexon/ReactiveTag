using VYaml.Annotations;

namespace ReactiveTag
{
    [YamlObject]
    public partial class YamlTagDefinition
    {
        public string Name;
        
        public YamlTagDefinition[] Children;
    }
}