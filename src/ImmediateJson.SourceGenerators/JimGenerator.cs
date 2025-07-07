using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

using System.Text;

namespace ImmediateJson.SourceGenerators;

[Generator]
public class JimGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var provider = context.SyntaxProvider.CreateSyntaxProvider(
            predicate: static (syntaxNode, _) =>
                syntaxNode is ClassDeclarationSyntax classDeclaration &&
                classDeclaration.AttributeLists.Count > 0 &&
                classDeclaration.AttributeLists.Any(al => al.Attributes.Any(a => a.Name.ToString() == "GenerateImmediateJsonSerializerAttribute")),
            transform: static (context, _) =>
            {
                var classDeclaration = (ClassDeclarationSyntax)context.Node;
                return classDeclaration;
            }
        );

        // context.RegisterSourceOutput(provider, (sourceProductionContext, calculatorClass)
        //     => Execute(calculatorClass, sourceProductionContext));
    }

    // public void Execute(GeneratorExecutionContext context)
    // {
    //     if (context.SyntaxReceiver is not SyntaxReceiver receiver)
    //         return;

    //     var compilation = context.Compilation;

    //     foreach (var classDeclaration in receiver.CandidateClasses)
    //     {
    //         var model = compilation.GetSemanticModel(classDeclaration.SyntaxTree);
    //         if (model.GetDeclaredSymbol(classDeclaration) is not ITypeSymbol typeSymbol)
    //             continue;

    //         if (typeSymbol.GetAttributes().Any(ad => ad.AttributeClass?.Name == "GenerateImmediateJsonSerializerAttribute"))
    //         {
    //             var source = GenerateJsonMethods(typeSymbol);
    //             context.AddSource($"{typeSymbol.Name}.g.cs", SourceText.From(source, Encoding.UTF8));
    //         }
    //     }
    // }

    private string GenerateJsonMethods(ITypeSymbol typeSymbol)
    {
        var properties = typeSymbol.GetMembers().OfType<IPropertySymbol>()
            .Where(p => p.GetMethod != null && p.SetMethod != null && p.DeclaredAccessibility == Accessibility.Public)
            .ToList();

        var sourceBuilder = new StringBuilder();
        sourceBuilder.Append($@"
using System;
using System.IO;
using System.Collections.Generic;
using System.Threading;

namespace {typeSymbol.ContainingNamespace.ToDisplayString()}
{{
    public partial class {typeSymbol.Name}
    {{
        private static readonly ThreadLocal<char[]> BufferPool = new(() => new char[2048]);

        public string ToJson()
        {{
            var buffer = BufferPool.Value!;
            var span = buffer.AsSpan();
            int written = WriteJson(span);
            return new string(span[..written]);
        }}

        public void ToJson(TextWriter writer)
        {{
            var buffer = BufferPool.Value!;
            var span = buffer.AsSpan();
            int written = WriteJson(span);
            writer.Write(span[..written]);
        }}

        private int WriteJson(Span<char> buffer)
        {{
            int pos = 0;
            buffer[pos++] = '{{';
");

        for (int i = 0; i < properties.Count; i++)
        {
            var property = properties[i];
            sourceBuilder.Append($@"
            pos += WriteProperty(buffer, ref pos, ""{char.ToLower(property.Name[0]) + property.Name.Substring(1)}"", {property.Name});");

            if (i < properties.Count - 1)
            {
                sourceBuilder.Append(@"
            buffer[pos++] = ',';");
            }
        }

        sourceBuilder.Append(@"
            buffer[pos++] = '}';
            return pos;
        }

        // Helper methods for writing different types to the buffer
        private static int WriteProperty(Span<char> buffer, ref int pos, string name, string value)
        {
            int start = pos;
            pos += WriteString(buffer, ref pos, name);
            buffer[pos++] = ':';
            pos += WriteString(buffer, ref pos, value);
            return pos - start;
        }

        private static int WriteProperty(Span<char> buffer, ref int pos, string name, int value)
        {
            int start = pos;
            pos += WriteString(buffer, ref pos, name);
            buffer[pos++] = ':';
            pos += WriteInt(buffer, ref pos, value);
            return pos - start;
        }
        
        private static int WriteProperty(Span<char> buffer, ref int pos, string name, Address value)
        {{
            int start = pos;
            pos += WriteString(buffer, ref pos, name);
            buffer[pos++] = ':';
            pos += value.WriteJson(buffer.Slice(pos));
            return pos - start;
        }}
        
        private static int WriteProperty(Span<char> buffer, ref int pos, string name, List<Person> value)
        {{
            int start = pos;
            pos += WriteString(buffer, ref pos, name);
            buffer[pos++] = ':';
            buffer[pos++] = '[';
            for(int i = 0; i < value.Count; i++)
            {{
                pos += value[i].WriteJson(buffer.Slice(pos));
                if(i < value.Count - 1)
                    buffer[pos++] = ',';
            }}
            buffer[pos++] = ']';
            return pos - start;
        }}

        private static int WriteString(Span<char> buffer, ref int pos, string value)
        {
            int start = pos;
            buffer[pos++] = '""';
            value.AsSpan().CopyTo(buffer.Slice(pos));
            pos += value.Length;
            buffer[pos++] = '""';
            return pos - start;
        }

        private static int WriteInt(Span<char> buffer, ref int pos, int value)
        {
            int start = pos;
            value.TryFormat(buffer.Slice(pos), out int written);
            pos += written;
            return pos - start;
        }
    }}
}}");
        return sourceBuilder.ToString();
    }


    private class SyntaxReceiver : ISyntaxReceiver
    {
        public List<ClassDeclarationSyntax> CandidateClasses { get; } = [];

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is ClassDeclarationSyntax classDeclarationSyntax && classDeclarationSyntax.AttributeLists.Count > 0)
            {
                CandidateClasses.Add(classDeclarationSyntax);
            }
        }
    }
}