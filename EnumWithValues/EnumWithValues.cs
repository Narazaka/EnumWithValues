using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EnumWithValues {
    [Generator]
    public class SourceGenerator : ISourceGenerator {
        const string AttributeClassesSource = @"
#nullable enable
using System;

namespace EnumWithValues {
    [AttributeUsage(AttributeTargets.Enum)]
    public class EnumWithValuesAttribute : Attribute {
        public string Name { get; }
        public bool ConvertEnumValue { get; }
        public bool ThrowIfCastFails { get; }
        public EnumWithValuesAttribute(string name, bool convertEnumValue = true, bool throwIfCastFails = false) {
            Name = name;
            ConvertEnumValue = convertEnumValue;
            ThrowIfCastFails = throwIfCastFails;
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class EnumValueAttribute : Attribute {
        public object?[] Value { get; }
        public EnumValueAttribute(params object?[] value) => Value = value;
    }
}
#nullable disable
";

        public void Initialize(GeneratorInitializationContext context) {
#if DEBUG
            if (!System.Diagnostics.Debugger.IsAttached) {
                // System.Diagnostics.Debugger.Launch();
            }
#endif
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context) {
            try {
                ExecuteCore(context);
            } catch (Exception ex) {
                System.Diagnostics.Trace.WriteLine(ex.ToString());
            }
        }
        void ExecuteCore(GeneratorExecutionContext context) {
            context.AddSource("EnumWithValues.cs", SourceText.From(AttributeClassesSource, Encoding.UTF8));
            if (context.SyntaxReceiver is not SyntaxReceiver receiver)
                return;
            if (context.Compilation is not Compilation compilation)
                return;
            var options = (compilation as CSharpCompilation)!.SyntaxTrees[0].Options as CSharpParseOptions;
            compilation = compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(SourceText.From(AttributeClassesSource, Encoding.UTF8), options));
            var enumWithValuesAttributeSymbol = compilation.GetTypeByMetadataName("EnumWithValues.EnumWithValuesAttribute");
            var enumValueAttributeSymbol = compilation.GetTypeByMetadataName("EnumWithValues.EnumValueAttribute");
            var enums = new List<EnumDeclaration>();
            foreach (var syntax in receiver.EnumDeclarationSyntaxes) {
                var model = compilation.GetSemanticModel(syntax.SyntaxTree);
                var symbol = ModelExtensions.GetDeclaredSymbol(model, syntax)!;
                var attrs = symbol.GetAttributes();
                var (name, convertEnumValue, throwIfCastFails) = attrs
                    .Where(attr => attr.AttributeClass!.Equals(enumWithValuesAttributeSymbol, SymbolEqualityComparer.Default))
                    .Select(attr => (
                        (string)attr.ConstructorArguments[0].Value!,
                        (bool)attr.ConstructorArguments[1].Value!,
                        (bool)attr.ConstructorArguments[2].Value!
                        ))
                    .FirstOrDefault();
                if (name is null)
                    continue;
                var enumDeclaration = new EnumDeclaration() {
                    Accessibility = symbol.DeclaredAccessibility,
                    EnumType = GetEnumValueTypeString(syntax),
                    EnumName = symbol.Name,
                    EnumFullname = symbol.ToString(),
                    StructName = name,
                    ConvertEnumValue = convertEnumValue,
                    ThrowIfCastFails = throwIfCastFails,
                };
                // roslyn cannot detect value...?
                long enumValue = 0; // TODO: ulong
                foreach (var memberSyntax in syntax.Members) {
                    var memberModel = compilation.GetSemanticModel(memberSyntax.SyntaxTree);
                    var memberSymbol = ModelExtensions.GetDeclaredSymbol(memberModel, memberSyntax)!;
                    var memberAttrs = memberSymbol.GetAttributes();
                    ImmutableArray<TypedConstant>? valueConstants = memberAttrs.Length == 0 ? null : memberAttrs
                        .Where(attr => attr.AttributeClass!.Equals(enumValueAttributeSymbol, SymbolEqualityComparer.Default))
                        .Select(attr => attr.ConstructorArguments[0].Values!)
                        .FirstOrDefault();
                    if (memberSyntax.EqualsValue is EqualsValueClauseSyntax equalsValue) {
                        enumValue = GetEqualsValue(equalsValue.Value) ?? enumValue;
                    }
                    if (valueConstants is not null)
                        enumDeclaration.Members.Add(new() { EnumName = symbol.Name, Name = memberSymbol.Name, EnumValue = enumValue, Values = valueConstants });
                    ++enumValue;
                }
                enums.Add(enumDeclaration);
            }
            foreach (var e in enums) {
                e.DetectTypes();
                var code = StructCode(e);
                if (e.Namespace is string ns)
                    code = Namespaced(ns, code);
                context.AddSource($"{e.StructFullname}.cs", SourceText.From(code, Encoding.UTF8));
            }
        }

        string GetEnumValueTypeString(EnumDeclarationSyntax syntax) {
            if (syntax.BaseList is not BaseListSyntax baseList) {
                return "int";
            }
            var type = baseList.Types.First().Type.ToString();
            switch (type.Replace("System.", "")) {
                case "Byte": return "byte";
                case "SByte": return "sbyte";
                case "Int16": return "short";
                case "UInt16": return "ushort";
                case "Int32": return "int";
                case "UInt32": return "uint";
                case "Int64": return "long";
                case "UInt64": return "ulong";
            }
            return type;
        }

        long? GetEqualsValue(ExpressionSyntax value) {
            switch (value) {
                case PrefixUnaryExpressionSyntax prefixed when prefixed.Kind() == SyntaxKind.UnaryMinusExpression:
                    if (prefixed.Operand is LiteralExpressionSyntax valueLiteral1 && valueLiteral1.Token.Kind() == SyntaxKind.NumericLiteralToken) {
                        return -GetNumericObjectValue(valueLiteral1.Token.Value!);
                        }
                    break;
                case LiteralExpressionSyntax valueLiteral2 when valueLiteral2.Token.Kind() == SyntaxKind.NumericLiteralToken:
                    return GetNumericObjectValue(valueLiteral2.Token.Value!);
            }
            return null;
        }

        long GetNumericObjectValue(object value) {
            switch (value) {
                case byte casted: return casted;
                case sbyte casted: return casted;
                case ushort casted: return casted;
                case short casted: return casted;
                case uint casted: return casted;
                case int casted: return casted;
                case ulong casted: return (long)casted;
                case long casted: return casted;
                default: throw new InvalidCastException();
            }
        }

        class EnumDeclaration {
            public Accessibility Accessibility { get; set; }
            public string EnumType { get; set; } = "";
            public string EnumFullname { get; set; } = "";
            public string EnumName { get; set; } = "";
            public string? Namespace { get => EnumFullname == EnumName ? null : EnumFullname.Substring(0, EnumFullname.Length - EnumName.Length - 1); }
            public string StructName { get; set; } = "";
            public string StructFullname { get => $"{Namespace}_{StructName}"; }
            public bool ConvertEnumValue { get; set; }
            public bool ThrowIfCastFails { get; set; }
            public List<EnumMember> Members { get; set; } = new();
            public List<string>? Types { get; set; }
            public void DetectTypes() {
                if (Members.Count == 0) {
                    Types = new();
                    return;
                }
                var types = Members[0].Values.Select(value => value.Type!).ToList();
                foreach (var member in Members) {
#pragma warning disable RS1024 // シンボルを正しく比較する
                    if (!member.Values.Select(value => value.Type!).SequenceEqual(types, SymbolEqualityComparer.Default)) {
#pragma warning restore RS1024 // シンボルを正しく比較する
                        return;
                    }
                }
                Types = types.Select(type => type.ToString()).ToList();
            }
            public string Type(int index) {
                switch (index) {
                    case -2:
                        return EnumType;
                    case -1:
                        return EnumName;
                    default:
                        return Types![index];
                }
            }

            public IEnumerable<int> TypeIndexes {
                get => Enumerable.Range(ConvertEnumValue ? -2 : -1, (Types is null ? 0 : Types.Count) + (ConvertEnumValue ? 2 : 1));
            }

            public bool HasStringType {
                get => Types is not null && Types.Contains("string");
            }
        }

        class EnumMember {
            public string EnumName { get; set; } = "";
            public string Name { get; set; } = "";
            public long EnumValue { get; set; }
            public IList<TypedConstant> Values { get; set; } = new List<TypedConstant>();
            public string CSharpValue(int index) {
                switch (index) {
                    case -2:
                        return EnumValue.ToString();
                    case -1:
                        return $"{EnumName}.{Name}";
                    default:
                        return Values[index].ToCSharpString();
                }
            }
        }

        const string I = "    ";

        string Namespaced(string? namespaceName, string code) {
            if (namespaceName is null)
                return code;
            return $@"
namespace {namespaceName} {{
{code}
}}
";
        }

        string AccessibilityToString(Accessibility accessibility) =>
            string.Join(" ", accessibility.ToString().Split(new string[] { "And" }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.ToLower()));

        string StructCode(EnumDeclaration declaration) {
            var structName = declaration.StructName;
            var enumName = declaration.EnumName;
            return $@"
    using System;

    {AccessibilityToString(declaration.Accessibility)} struct {structName} : IEquatable<{structName}> {{
{FieldCode(structName, enumName, declaration.Members)}

{EnumConstCode(declaration)}

        public {enumName} AsEnum {{ get; }}

        {structName}({enumName} asEnum) => AsEnum = asEnum;

        public bool Equals({structName} other) => AsEnum == other.AsEnum;
        public static bool operator ==({structName} a, {structName} b) => a.Equals(b);
        public static bool operator !=({structName} a, {structName} b) => !a.Equals(b);
        public override bool Equals(object obj) => obj is {structName} && Equals(obj);
        public override int GetHashCode() => (int)AsEnum;

{ValueOperatorCode(declaration)}

{ToStringCode(declaration)}
    }}
";
        }

        string FieldCode(string structName, string enumName, IEnumerable<EnumMember> members) {
            var code = new StringBuilder();
            foreach (var member in members)
                code.Append($"{I}{I}public static readonly {structName} {member.Name} = new({enumName}.{member.Name});").AppendLine();
            return code.ToString();
        }

        string EnumConstCode(EnumDeclaration declaration) {
            var code = new StringBuilder();
            code.Append($"{I}{I}public static class Enum {{").AppendLine();
            foreach (var member in declaration.Members)
                code.Append($"{I}{I}{I}public const {declaration.EnumName} {member.Name} = {declaration.EnumName}.{member.Name};").AppendLine();
            code.Append($"{I}{I}}}").AppendLine();
            return code.ToString();

        }

        string ToStringCode(EnumDeclaration declaration) {
            if (!declaration.HasStringType)
                return "";
            return I + I + @"public override string ToString() => (string)this;";
        }

        string ValueOperatorCode(EnumDeclaration declaration) {
            // (string structName, string enumName, IList<string> valueTypes, IEnumerable<EnumMember> members
            var code = new StringBuilder();
            foreach (var i in declaration.TypeIndexes) {
                ToValueOperatorCode(code, i, declaration);
                FromValueOperatorCode(code, i, declaration);
            }
            return code.ToString();
        }

        StringBuilder ToValueOperatorCode(StringBuilder code, int valueIndex, EnumDeclaration declaration) {
            code.Append($"{I}{I}public static implicit operator {declaration.Type(valueIndex)}({declaration.StructName} enumStruct) {{").AppendLine();
            code.Append($"{I}{I}{I}switch (enumStruct.AsEnum) {{").AppendLine();
            foreach (var member in declaration.Members)
                code.Append($"{I}{I}{I}{I}case {declaration.EnumName}.{member.Name}: return {member.CSharpValue(valueIndex)};").AppendLine();
            if (declaration.ThrowIfCastFails) {
                code.Append($"{I}{I}{I}{I}default: throw new InvalidCastException();").AppendLine();
            } else {
                code.Append($"{I}{I}{I}{I}default: return default;").AppendLine();
            }
            code.Append($"{I}{I}{I}}}").AppendLine();
            code.Append($"{I}{I}}}").AppendLine();
            return code;
        }

        StringBuilder FromValueOperatorCode(StringBuilder code, int valueIndex, EnumDeclaration declaration) {
            code.Append($"{I}{I}public static implicit operator {declaration.StructName}({declaration.Type(valueIndex)} value) {{").AppendLine();
            code.Append($"{I}{I}{I}switch (value) {{").AppendLine();
            foreach (var member in declaration.Members)
                code.Append($"{I}{I}{I}{I}case {member.CSharpValue(valueIndex)}: return {member.Name};").AppendLine();
            if (declaration.ThrowIfCastFails) {
                code.Append($"{I}{I}{I}{I}default: throw new InvalidCastException();").AppendLine();
            } else {
                code.Append($"{I}{I}{I}{I}default: return default;").AppendLine();
            }
            code.Append($"{I}{I}{I}}}").AppendLine();
            code.Append($"{I}{I}}}").AppendLine();
            return code;
        }
    }

    internal class SyntaxReceiver : ISyntaxReceiver {
        internal List<EnumDeclarationSyntax> EnumDeclarationSyntaxes { get; } = new();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode) {
            switch (syntaxNode) {
                case EnumDeclarationSyntax syntax when syntax.AttributeLists.Count > 0:
                    EnumDeclarationSyntaxes.Add(syntax);
                    break;
            }
        }
    }
}

