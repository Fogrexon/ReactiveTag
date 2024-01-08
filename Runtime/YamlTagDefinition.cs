using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using JetBrains.Annotations;
using VYaml.Annotations;

namespace ReactiveTag
{
    [YamlObject]
    public partial class YamlTagDefinition
    {
        public string Name;
        
        public string IconPath;

        public string Id = Guid.NewGuid().ToString("N");
        
        public List<YamlTagDefinition> Children;

        [YamlIgnore][CanBeNull] public YamlTagDefinition Parent;
    }
}