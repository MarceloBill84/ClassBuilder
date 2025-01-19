﻿using ClassBuilder.Dto;
using ClassBuilder.Exceptions;
using ClassBuilder.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClassBuilder.Factories
{
    public static class BuilderFactory
    {
        public static string Create(string fileContent)
        {

            var content = new StringBuilder();

            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(fileContent);

            CompilationUnitSyntax root = syntaxTree.GetCompilationUnitRoot();

            var usings = GetUsings(root);

            string nameSpace = GetNameSpace(syntaxTree);

            if (string.IsNullOrWhiteSpace(nameSpace))
                throw new ValidationException("It wasn't identified the namespace to generate builder class");

            ClassDeclarationSyntax classDeclaration = root.DescendantNodes()
                                   .OfType<ClassDeclarationSyntax>()
                                   .FirstOrDefault();

            if (classDeclaration is null)
                throw new ValidationException("It wasn't identified the class to generate builder class");

            var originalClassName = classDeclaration.Identifier.Text;

            var properties = classDeclaration.Members
                                                .OfType<PropertyDeclarationSyntax>()
                                                .Where(prop => IsEligibleForBuilder(prop, classDeclaration));



            var propertiesInfo = properties.Select(p => new PropertyInfo(p.Type.ToString(), p.Identifier.Text)).ToList();

            if (!propertiesInfo.Any())
                throw new ValidationException("It wasn't identified public properties to generate builder class");


            var newClassName = $"{originalClassName}Builder";

            if (usings.Any())
            {
                foreach (var us in usings)
                {
                    content.Append(us);
                }

                content.AppendLine();
            }

            content.AppendLine(nameSpace);

            content.AppendLine("{");

            content.AppendLine($"\tpublic class {newClassName}");

            content.AppendLine("\t{");

            GeneratePrivateVariables(content, propertiesInfo);

            GenerateBuilderConstructor(content, newClassName);

            GenerateMethodsToSetValues(content, newClassName, propertiesInfo);

            GenerateMethodBuild(content, originalClassName, propertiesInfo);

            content.AppendLine("\t}");

            content.AppendLine("}");

            return content.ToString();
        }

        private static string GetNameSpace(SyntaxTree syntaxTree)
        {
            var root = syntaxTree.GetRoot();

            var namespaceDeclaration = root.DescendantNodes()
                                           .OfType<NamespaceDeclarationSyntax>()
                                           .FirstOrDefault();

            if (namespaceDeclaration != null)
                return namespaceDeclaration.Name.ToString();

            var fileScoepedNamespaceDeclaration = root.DescendantNodes()
                                           .OfType<FileScopedNamespaceDeclarationSyntax>()
                                           .FirstOrDefault();

            return fileScoepedNamespaceDeclaration?.Name?.ToString();
        }

        private static List<string> GetUsings(CompilationUnitSyntax root)
        {
            var usingDirectives = root.Usings;

            return usingDirectives.Select(p => p.ToFullString()).ToList();
        }

        private static void GenerateMethodBuild(StringBuilder content, string originalClassName, IList<PropertyInfo> properties)
        {
            content.AppendLine($"\t\tpublic {originalClassName} Build()");
            content.AppendLine("\t\t{");
            content.AppendLine($"\t\t\treturn new {originalClassName}");
            content.AppendLine("\t\t\t{");

            for (int i = 0; i < properties.Count; i++)
            {
                var setValue = $"\t\t\t\t{properties[i].Name} = _{properties[i].Name.GetWordWithFirstLetterDown()}";
                if (i + 1 != properties.Count)
                    setValue = string.Concat(setValue, ",");

                content.AppendLine(setValue);
            }

            content.AppendLine("\t\t\t};");
            content.AppendLine("\t\t}");
        }

        private static void GenerateBuilderConstructor(StringBuilder content, string newClassName)
        {
            content.AppendLine();
            content.AppendLine($"\t\tpublic {newClassName}()" + " { }");
            content.AppendLine();
        }

        private static void GenerateMethodsToSetValues(StringBuilder content, string className, IList<PropertyInfo> properties)
        {
            foreach (var item in properties)
            {
                content.AppendLine($"\t\tpublic {className} With{item.Name}({item.Type} {item.Name.GetWordWithFirstLetterDown()})");
                content.AppendLine("\t\t{");
                content.AppendLine($"\t\t\t_{item.Name.GetWordWithFirstLetterDown()} = {item.Name.GetWordWithFirstLetterDown()};");
                content.AppendLine("\t\t\treturn this;");
                content.AppendLine("\t\t}");
                content.AppendLine();
            }
        }

        private static void GeneratePrivateVariables(StringBuilder content, IList<PropertyInfo> properties)
        {
            foreach (var item in properties)
            {
                content.AppendLine($"\t\tprivate {item.Type} _{item.Name.GetWordWithFirstLetterDown()};");
            }
        }

        static bool IsEligibleForBuilder(PropertyDeclarationSyntax property, ClassDeclarationSyntax classDeclaration)
        {
            // Verifica se a propriedade é pública
            bool isPublic = property.Modifiers.Any(SyntaxKind.PublicKeyword);

            // Verifica se o set é privado ou ausente
            var accessorList = property.AccessorList;
            bool hasPrivateSetter = accessorList?.Accessors
                                                 .Any(acc => acc.IsKind(SyntaxKind.SetAccessorDeclaration) &&
                                                             acc.Modifiers.Any(SyntaxKind.PrivateKeyword)) ?? false;

            // Verifica se a propriedade é preenchida via construtor
            bool isSetInConstructor = IsAssignedInConstructor(property, classDeclaration);

            // Retorna true apenas para as propriedades elegíveis
            return isPublic && (!hasPrivateSetter || isSetInConstructor);
        }

        static bool IsAssignedInConstructor(PropertyDeclarationSyntax property, ClassDeclarationSyntax classDeclaration)
        {
            var constructors = classDeclaration.Members.OfType<ConstructorDeclarationSyntax>();
            var propertyName = property.Identifier.Text;

            return constructors.Any(constructor =>
                constructor.Body?.Statements.OfType<ExpressionStatementSyntax>()
                           .Any(statement =>
                               statement.ToString().Contains($"{propertyName} =")) ?? false);
        }
    }
}
