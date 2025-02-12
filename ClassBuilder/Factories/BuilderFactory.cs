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

            var typeDeclarations = GetTypeDeclarions(root);

            if (typeDeclarations is null || typeDeclarations.Count == 0)
                throw new ValidationException("It wasn't identified class to generate builder");

            GenerateHeader(content, usings, nameSpace);

            for (int i = 0; i<typeDeclarations.Count; i++)
            {
                if (i > 0)
                    content.AppendLine("");

                var typeDeclaration = typeDeclarations[i];

                var originalClassName = typeDeclaration.Identifier.Text;

                var propertiesInfo = GetPropertiesInfo(typeDeclaration);

                if (!propertiesInfo.Any())
                    throw new ValidationException("It wasn't identified public properties to generate builder");

                GenerateBuilder(content, originalClassName, typeDeclaration, propertiesInfo);
            }

            content.AppendLine("}");

            return content.ToString();
        }

        private static List<PropertyInfo> GetPropertiesInfo(TypeDeclarationSyntax typeDeclaration)
        {
            IEnumerable<ConstructorDeclarationSyntax> constructors = GetPublicConstructorsWithParameters(typeDeclaration);

            List<ParameterSyntax> parameters = null;

            if (constructors.Any())
            {
                parameters = constructors.First().ParameterList.Parameters.ToList();
            }
            else if (typeDeclaration.ParameterList != null)
            {
                parameters = typeDeclaration.ParameterList.Parameters.ToList();
            }

            if (!parameters.IsNullOrEmpty())
                return parameters.Select(p => new PropertyInfo(p.Type.ToString(), p.Identifier.Text.GetWordWithFirstLetterUpper())).ToList();


            var properties = typeDeclaration.Members
                                        .OfType<PropertyDeclarationSyntax>()
                                        .Where(prop => IsEligibleForBuilder(prop, typeDeclaration));

            return properties.Select(p => new PropertyInfo(p.Type.ToString(), p.Identifier.Text.GetWordWithFirstLetterUpper())).ToList();
        }

        private static IEnumerable<ConstructorDeclarationSyntax> GetPublicConstructorsWithParameters(TypeDeclarationSyntax typeDeclaration)
        {
            return typeDeclaration.Members
                .OfType<ConstructorDeclarationSyntax>()
                .Where(constructor => constructor.Modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword)) && constructor.ParameterList.Parameters.Count > 0);
        }

        private static void GenerateHeader(StringBuilder content, List<string> usings, string nameSpace)
        {
            if (usings.Any())
            {
                foreach (var us in usings)
                {
                    content.Append(us);
                }

                content.AppendLine();
            }

            if (!string.IsNullOrWhiteSpace(nameSpace))
                content.AppendLine($"namespace {nameSpace}");

            content.AppendLine("{");
        }

        private static List<TypeDeclarationSyntax> GetTypeDeclarions(CompilationUnitSyntax root)
        {
            return root.DescendantNodes()
                                   .OfType<TypeDeclarationSyntax>()
                                   .Where(p => !p.Modifiers.Any(m => m.IsKind(SyntaxKind.AbstractKeyword)))
                                   .ToList();
        }

        private static void GenerateBuilder(StringBuilder content, string originalClassName, TypeDeclarationSyntax typeDeclarationSyntax, List<PropertyInfo> propertiesInfo)
        {
            var newClassName = $"{originalClassName}{Constants.Constants.SuffixClassName}";

            content.AppendLine($"\tpublic class {newClassName}");

            content.AppendLine("\t{");

            GeneratePrivateVariables(content, propertiesInfo);

            GenerateBuilderConstructor(content, newClassName);

            GenerateMethodsToSetValues(content, newClassName, propertiesInfo);

            GenerateMethodBuild(content, originalClassName, typeDeclarationSyntax, propertiesInfo);

            content.AppendLine("\t}");
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

        private static void GenerateMethodBuild(StringBuilder content, string originalClassName, TypeDeclarationSyntax typeDeclarationSyntax, IList<PropertyInfo> properties)
        {
            content.AppendLine($"\t\tpublic {originalClassName} Build()");
            content.AppendLine("\t\t{");
            content.AppendLine($"\t\t\treturn new {originalClassName}");

            if (typeDeclarationSyntax.ParameterList != null || GetPublicConstructorsWithParameters(typeDeclarationSyntax).Any())
            {
                content.AppendLine("\t\t\t(");

                for (int i = 0; i < properties.Count; i++)
                {
                    var setValue = $"\t\t\t\t_{properties[i].Name.GetWordWithFirstLetterDown()}";
                    if (i + 1 != properties.Count)
                        setValue = string.Concat(setValue, ",");

                    content.AppendLine(setValue);
                }

                content.AppendLine("\t\t\t);");
            }
            else
            {
                content.AppendLine("\t\t\t{");

                for (int i = 0; i < properties.Count; i++)
                {
                    var setValue = $"\t\t\t\t{properties[i].Name} = _{properties[i].Name.GetWordWithFirstLetterDown()}";
                    if (i + 1 != properties.Count)
                        setValue = string.Concat(setValue, ",");

                    content.AppendLine(setValue);
                }

                content.AppendLine("\t\t\t};");
            }

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
                content.AppendLine($"\t\tpublic {className} {Constants.Constants.PrefixMethodName}{item.Name}({item.Type} {item.Name.GetWordWithFirstLetterDown()})");
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

        static bool IsEligibleForBuilder(PropertyDeclarationSyntax property, TypeDeclarationSyntax classDeclaration)
        {
            bool isPublic = property.Modifiers.Any(SyntaxKind.PublicKeyword);

            var accessorList = property.AccessorList;
            bool hasPrivateSetter = accessorList?.Accessors
                                                 .Any(acc => acc.IsKind(SyntaxKind.SetAccessorDeclaration) &&
                                                             acc.Modifiers.Any(SyntaxKind.PrivateKeyword)) ?? false;

            bool isSetInConstructor = IsAssignedInConstructor(property, classDeclaration);

            bool hasSetter = accessorList?.Accessors.Any(a => a.IsKind(SyntaxKind.SetAccessorDeclaration) || a.IsKind(SyntaxKind.InitAccessorDeclaration)) ?? false;

            return isPublic && hasSetter && (!hasPrivateSetter || isSetInConstructor);
        }

        static bool IsAssignedInConstructor(PropertyDeclarationSyntax property, TypeDeclarationSyntax classDeclaration)
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
