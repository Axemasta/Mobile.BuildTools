﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using CodeGenHelpers;
using Microsoft.CodeAnalysis;
using Mobile.BuildTools.AppSettings.Diagnostics;
using Mobile.BuildTools.AppSettings.Extensions;
using Mobile.BuildTools.Models.Settings;
using Mobile.BuildTools.Utils;

namespace Mobile.BuildTools.AppSettings.Generators
{
    [Generator]
    public sealed class AppSettingsGenerator : GeneratorBase
    {
        private const string _autoGeneratedMessage = @"This code was generated by Mobile.BuildTools. For more information please visit
https://mobilebuildtools.com or to file an issue please see
https://github.com/dansiegel/Mobile.BuildTools

Changes to this file may cause incorrect behavior and will be lost when
the code is regenerated.

When I wrote this, only God and I understood what I was doing
Now, God only knows.

NOTE: This file should be excluded from source control.";

        protected override void Generate()
        {
            var settings = ConfigHelper.GetSettingsConfig(this);
            if (settings is null || !settings.Any())
                return;
            
            int i = 0;
            var assembly = typeof(AppSettingsGenerator).Assembly;
            var toolVersion = FileVersionInfo.GetVersionInfo(assembly.Location).ProductVersion;
            var compileGeneratedAttribute = @$"[GeneratedCodeAttribute(""{typeof(AppSettingsGenerator).FullName}"", ""{toolVersion}"")]";
            foreach (var settingsConfig in settings)
            {
                if (string.IsNullOrEmpty(settingsConfig.ClassName))
                    settingsConfig.ClassName = i++ > 0 ? $"AppSettings{i}" : "AppSettings";
                else
                {
                    settingsConfig.ClassName = settingsConfig.ClassName.Trim();
                    if (settingsConfig.ClassName == "AppSettings")
                        i++;
                }

                if (string.IsNullOrEmpty(settingsConfig.Namespace))
                    settingsConfig.Namespace = "Helpers";
                else
                    settingsConfig.Namespace = settingsConfig.Namespace.Trim();

                if (string.IsNullOrEmpty(settingsConfig.Delimiter))
                    settingsConfig.Delimiter = ";";
                else
                    settingsConfig.Delimiter = settingsConfig.Delimiter.Trim();

                if (string.IsNullOrEmpty(settingsConfig.Prefix))
                    settingsConfig.Prefix = "BuildTools_";
                else
                    settingsConfig.Prefix = settingsConfig.Prefix.Trim();

                if (string.IsNullOrEmpty(settingsConfig.RootNamespace))
                    settingsConfig.RootNamespace = RootNamespace;
                else
                    settingsConfig.RootNamespace = settingsConfig.RootNamespace.Trim();

                var mergedSecrets = GetMergedSecrets(settingsConfig, out var hasErrors);
                if (hasErrors)
                    continue;

                var namespaceParts = new[]
                {
                    settingsConfig.RootNamespace,
                    settingsConfig.Namespace == "." ? string.Empty : settingsConfig.Namespace
                };
                var fullyQualifiedNamespace = string.Join(".", namespaceParts.Where(x => !string.IsNullOrEmpty(x)));

                var builder = TryGetTypeSymbol($"{fullyQualifiedNamespace}.{settingsConfig.ClassName}", out var typeSymbol)
                    ? CodeBuilder.Create(typeSymbol)
                    : CodeBuilder.Create(fullyQualifiedNamespace)
                                 .AddClass(settingsConfig.ClassName)
                                 .WithAccessModifier(settingsConfig.Accessibility.ToRoslynAccessibility())
                                 .MakeStaticClass();

                IEnumerable<INamedTypeSymbol> interfaces = Array.Empty<INamedTypeSymbol>();
                if(typeSymbol != null)
                {
                    if (typeSymbol.IsStatic)
                        builder.MakeStaticClass();

                    interfaces = typeSymbol.Interfaces;
                }

                builder.AddNamespaceImport("System")
                    .AddNamespaceImport("GeneratedCodeAttribute = System.CodeDom.Compiler.GeneratedCodeAttribute")
                    .Builder
                    .WithAutoGeneratedMessage(_autoGeneratedMessage);

                foreach(var valueConfig in settingsConfig.Properties)
                {
                    AddProperty(ref builder, mergedSecrets, valueConfig, interfaces, settingsConfig.Delimiter);
                }

                AddSource(builder);
            }
        }

        private void AddProperty(ref ClassBuilder builder, IDictionary<string, string> secrets, ValueConfig valueConfig, IEnumerable<INamedTypeSymbol> interfaces, string delimeter)
        {
            if (!secrets.ContainsKey(valueConfig.Name))
                return;

            var value = secrets[valueConfig.Name];
            var output = string.Empty;
            var isArray = valueConfig.IsArray.HasValue ? valueConfig.IsArray.Value : false;
            var mapping = valueConfig.PropertyType.GetPropertyTypeMapping();
            var valueHandler = mapping.Handler;
            var typeDeclaration = mapping.Type.GetStandardTypeName();
            var type = isArray ? mapping.Type.MakeArrayType() : mapping.Type;
            var propBuilder = builder.AddProperty(valueConfig.Name)
                .SetType(type)
                .MakePublicProperty();

            var symbol = interfaces.SelectMany(x => x.GetMembers())
                .OfType<IPropertySymbol>()
                .Where(x => x.Name == valueConfig.Name)
                .FirstOrDefault();

            var valueType = GetValueType(value, valueConfig.PropertyType);
            if (value is null || value.ToLower() == "null" || value.ToLower() == "default")
            {
                if (type == typeof(bool) && !isArray)
                {
                    output = bool.FalseString.ToLower();
                }
                else if (isArray)
                {
                    output = $"global::System.Array.Empty<{typeDeclaration}>()";
                }
                else
                {
                    output = "default";
                }
            }
            else if (isArray)
            {
                var valueArray = GetValueArray(value, delimeter).Select(x => valueHandler.Format(x, false));
                output = "new " + typeDeclaration + "[] { " + string.Join(", ", valueArray) + " }";
                if (type == typeof(bool))
                {
                    output = output.ToLower();
                }
            }
            else
            {
                output = valueHandler.Format(value, false);
                if (type == typeof(bool))
                {
                    output = output.ToLower();
                }
            }

            if (symbol is not null)
            {
                propBuilder.WithGetterExpression(output);
            }
            else if (!isArray && valueConfig.PropertyType == PropertyType.String)
            {
                propBuilder.WithConstValue(output);
            }
            else
            {
                propBuilder.MakeStatic()
                    .WithReadonlyValue(output, output, valueType: valueType);
            }
        }

        private string[] GetValueArray(string value, string delimeter)
        {
            return value.Split(delimeter[0])
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrEmpty(x))
                .ToArray();
        }

        private CodeGenHelpers.ValueType GetValueType(string value, PropertyType propertyType)
        {
            switch(value?.ToLower())
            {
                case null when propertyType == PropertyType.String:
                case "null" when propertyType == PropertyType.String:
                    return CodeGenHelpers.ValueType.Null;
                case null:
                case "null":
                case "default":
                    return CodeGenHelpers.ValueType.Default;
                default:
                    return CodeGenHelpers.ValueType.UserSpecified;
            }
        }

        internal IDictionary<string, string> GetMergedSecrets(SettingsConfig settingsConfig, out bool hasErrors)
        {
            if (string.IsNullOrEmpty(settingsConfig.Prefix))
                settingsConfig.Prefix = "BuildTools_";

            var env = EnvironmentAnalyzer.GatherEnvironmentVariables(this);
            var secrets = new Dictionary<string, string>();
            hasErrors = false;
            foreach (var prop in settingsConfig.Properties)
            {
                var searchKeys = new[]
                {
                    $"{settingsConfig.Prefix}{prop.Name}",
                    $"{settingsConfig.Prefix}_{prop.Name}",
                    prop.Name,
                };

                string key = null;
                foreach (var searchKey in searchKeys)
                {
                    if (!string.IsNullOrEmpty(key))
                        break;

                    key = env.Keys.FirstOrDefault(x =>
                        x.Equals(searchKey, StringComparison.InvariantCultureIgnoreCase));
                }

                if (string.IsNullOrEmpty(key))
                {
                    if (string.IsNullOrEmpty(prop.DefaultValue))
                    {
                        ReportDiagnostic(Descriptors.MissingAppSettingsProperty, prop.Name);
                        hasErrors = true;
                        continue;
                    }

                    secrets[prop.Name] = prop.DefaultValue == "null" || prop.DefaultValue == "default" ? null : prop.DefaultValue;
                    continue;
                }

                secrets[prop.Name] = env[key];
            }

            return secrets;
        }
    }
}
