#region

using System.Collections.Generic;
using Confuser.Core;
using dnlib.DotNet;

#endregion

namespace Confuser.Renamer.Analyzers
{
    internal class JsonAnalyzer : IRenamer
    {
        private const string JsonProperty = "Newtonsoft.Json.JsonPropertyAttribute";
        private const string JsonIgnore = "Newtonsoft.Json.JsonIgnoreAttribute";
        private const string JsonObject = "Newtonsoft.Json.JsonObjectAttribute";

        private static readonly HashSet<string> JsonContainers = new HashSet<string>
        {
            "Newtonsoft.Json.JsonArrayAttribute",
            "Newtonsoft.Json.JsonContainerAttribute",
            "Newtonsoft.Json.JsonDictionaryAttribute",
            "Newtonsoft.Json.JsonObjectAttribute"
        };

        public void Analyze(ConfuserContext context, INameService service, ProtectionParameters parameters, IDnlibDef def)
        {
            if(def is TypeDef)
                Analyze(context, service, (TypeDef) def, parameters);
            else if(def is MethodDef)
                Analyze(context, service, (MethodDef) def, parameters);
            else if(def is PropertyDef)
                Analyze(context, service, (PropertyDef) def, parameters);
            else if(def is FieldDef)
                Analyze(context, service, (FieldDef) def, parameters);
        }

        public void PreRename(ConfuserContext context, INameService service, ProtectionParameters parameters, IDnlibDef def)
        {
            //
        }

        public void PostRename(ConfuserContext context, INameService service, ProtectionParameters parameters, IDnlibDef def)
        {
            //
        }

        private static CustomAttribute GetJsonContainerAttribute(IHasCustomAttribute attrs)
        {
            foreach(var attr in attrs.CustomAttributes)
                if(JsonContainers.Contains(attr.TypeFullName))
                    return attr;
            return null;
        }

        private static bool ShouldExclude(TypeDef type, IDnlibDef def)
        {
            CustomAttribute attr;

            if(def.CustomAttributes.IsDefined(JsonProperty))
            {
                attr = def.CustomAttributes.Find(JsonProperty);
                if(attr.HasConstructorArguments || attr.GetProperty("PropertyName") != null)
                    return false;
            }

            attr = GetJsonContainerAttribute(type);
            if(attr == null || attr.TypeFullName != JsonObject)
                return false;

            if(def.CustomAttributes.IsDefined(JsonIgnore))
                return false;

            var serialization = 0;
            if(attr.HasConstructorArguments && attr.ConstructorArguments[0].Type.FullName == "Newtonsoft.Json.MemberSerialization")
                serialization = (int) attr.ConstructorArguments[0].Value;
            else
                foreach(var property in attr.Properties)
                    if(property.Name == "MemberSerialization")
                        serialization = (int) property.Value;

            if(serialization == 0)
                return def is PropertyDef && ((PropertyDef) def).IsPublic() ||
                       def is FieldDef && ((FieldDef) def).IsPublic;
            if(serialization == 1) // OptIn
                return false;
            if(serialization == 2) // Fields
                return def is FieldDef;
            return false;
        }

        private void Analyze(ConfuserContext context, INameService service, TypeDef type, ProtectionParameters parameters)
        {
            var attr = GetJsonContainerAttribute(type);
            if(attr == null)
                return;

            var hasId = false;
            if(attr.HasConstructorArguments && attr.ConstructorArguments[0].Type.FullName == "System.String")
                hasId = true;
            else
                foreach(var property in attr.Properties)
                    if(property.Name == "Id")
                        hasId = true;
            if(!hasId)
                service.SetCanRename(type, false);
        }

        private void Analyze(ConfuserContext context, INameService service, MethodDef method, ProtectionParameters parameters)
        {
            if(GetJsonContainerAttribute(method.DeclaringType) != null && method.IsConstructor) service.SetParam(method, "renameArgs", "false");
        }

        private void Analyze(ConfuserContext context, INameService service, PropertyDef property, ProtectionParameters parameters)
        {
            if(ShouldExclude(property.DeclaringType, property)) service.SetCanRename(property, false);
        }

        private void Analyze(ConfuserContext context, INameService service, FieldDef field, ProtectionParameters parameters)
        {
            if(ShouldExclude(field.DeclaringType, field)) service.SetCanRename(field, false);
        }
    }
}