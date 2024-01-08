using System.Collections.Generic;
using System.Linq;

namespace ReactiveTag.Utils
{
    public enum TagValidationResult
    {
        OK,
        EmptyName,
        DuplicatedName,
        NameIsReservedWord,
        NameHasInvalidCharacter
    }
    public class TagValidator
    {
        public static readonly IReadOnlyList<string> ReservedWords = new []
        {
            "Tag",
            "TagComponent",
            "TagEffect",
            "TagManager",
            "TagManagerComponent",
            "ReactiveTagRoot",
            "Parent",
            "Id",
            "Children",
            "Name",
            "Class",
            "ChildrenOf",
            "Equals",
            "ToString",
            "abstract",
            "as",
            "base",
            "bool",
            "break",
            "byte",
            "case",
            "catch",
            "char",
            "checked",
            "class",
            "const",
            "continue",
            "decimal",
            "default",
            "delegate",
            "do",
            "double",
            "else",
            "enum",
            "event",
            "explicit",
            "extern",
            "false",
            "finally",
            "fixed",
            "float",
            "for",
            "foreach",
            "goto",
            "if",
            "implicit",
            "in",
            "int",
            "interface",
            "internal",
            "is",
            "lock",
            "long",
            "namespace",
            "new",
            "null",
            "object",
            "operator",
            "out",
            "override",
            "params",
            "private",
            "protected",
            "public",
            "readonly",
            "ref",
            "return",
            "sbyte",
            "sealed",
            "short",
            "sizeof",
            "stackalloc",
            "static",
            "string",
            "struct",
            "switch",
            "this",
            "throw",
            "true",
            "try",
            "typeof",
            "uint",
            "ulong",
            "unchecked",
            "unsafe",
            "ushort",
            "using",
            "virtual",
            "void",
            "volatile",
            "while",
        };

        public static TagValidationResult ValidateTagName(string newTagName, YamlTagDefinition tag)
        {
            if (newTagName == "")
            {
                return TagValidationResult.EmptyName;
            }
            if (tag.Parent?.Children.Count(sibling => sibling.Name == newTagName) > 1)
            {
                return TagValidationResult.DuplicatedName;
            }
            if (char.IsDigit(newTagName[0]) || newTagName.Any(c => !char.IsLetterOrDigit(c)))
            {
                return TagValidationResult.NameHasInvalidCharacter;
            }
            if (ReservedWords.Contains(newTagName))
            {
                return TagValidationResult.NameIsReservedWord;
            }
            return TagValidationResult.OK;
        }

        public static string CreateNewTagName(YamlTagDefinition parent)
        {
            var name = "NewTag";
            var i = 1;
            if (parent.Children is null)
            {
                return name;
            }
            while (parent.Children.Any(child => child.Name == name))
            {
                name = $"NewTag{i}";
                i++;
            }
            return name;
        }
    }
}