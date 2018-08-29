#region

using System.Diagnostics;
using System.Linq;
using Confuser.Core;
using Confuser.Renamer.References;
using dnlib.DotNet;
using dnlib.DotNet.MD;

#endregion

namespace Confuser.Renamer.Analyzers
{
    internal class TypeBlobAnalyzer : IRenamer
    {
        public void Analyze(ConfuserContext context, INameService service, ProtectionParameters parameters, IDnlibDef def)
        {
            var module = def as ModuleDefMD;
            if(module == null) return;

            MDTable table;
            uint len;

            // MemberRef
            table = module.TablesStream.Get(Table.Method);
            len = table.Rows;
            var methods = module.GetTypes().SelectMany(type => type.Methods);
            foreach(var method in methods)
            {
                foreach(var methodImpl in method.Overrides)
                {
                    if(methodImpl.MethodBody is MemberRef)
                        AnalyzeMemberRef(context, service, (MemberRef) methodImpl.MethodBody);
                    if(methodImpl.MethodDeclaration is MemberRef)
                        AnalyzeMemberRef(context, service, (MemberRef) methodImpl.MethodDeclaration);
                }
                if(!method.HasBody)
                    continue;
                foreach(var instr in method.Body.Instructions)
                    if(instr.Operand is MemberRef)
                    {
                        AnalyzeMemberRef(context, service, (MemberRef) instr.Operand);
                    }
                    else if(instr.Operand is MethodSpec)
                    {
                        var spec = (MethodSpec) instr.Operand;
                        if(spec.Method is MemberRef)
                            AnalyzeMemberRef(context, service, (MemberRef) spec.Method);
                    }
            }


            // CustomAttribute
            table = module.TablesStream.Get(Table.CustomAttribute);
            len = table.Rows;
            var attrs = Enumerable.Range(1, (int) len)
                .Select(rid => module.ResolveHasCustomAttribute(module.TablesStream.ReadCustomAttributeRow((uint) rid).Parent))
                .Distinct()
                .SelectMany(owner => owner.CustomAttributes);
            foreach(var attr in attrs)
            {
                if(attr.Constructor is MemberRef)
                    AnalyzeMemberRef(context, service, (MemberRef) attr.Constructor);

                foreach(var arg in attr.ConstructorArguments)
                    AnalyzeCAArgument(context, service, arg);

                foreach(var arg in attr.Fields)
                    AnalyzeCAArgument(context, service, arg.Argument);

                foreach(var arg in attr.Properties)
                    AnalyzeCAArgument(context, service, arg.Argument);

                var attrType = attr.AttributeType.ResolveTypeDefThrow();
                if(!context.Modules.Contains((ModuleDefMD) attrType.Module))
                    continue;

                foreach(var fieldArg in attr.Fields)
                {
                    var field = attrType.FindField(fieldArg.Name, new FieldSig(fieldArg.Type));
                    if(field == null)
                        context.Logger.WarnFormat("Failed to resolve CA field '{0}::{1} : {2}'.", attrType, fieldArg.Name, fieldArg.Type);
                    else
                        service.AddReference(field, new CAMemberReference(fieldArg, field));
                }
                foreach(var propertyArg in attr.Properties)
                {
                    var property = attrType.FindProperty(propertyArg.Name, new PropertySig(true, propertyArg.Type));
                    if(property == null)
                        context.Logger.WarnFormat("Failed to resolve CA property '{0}::{1} : {2}'.", attrType, propertyArg.Name, propertyArg.Type);
                    else
                        service.AddReference(property, new CAMemberReference(propertyArg, property));
                }
            }
        }

        public void PreRename(ConfuserContext context, INameService service, ProtectionParameters parameters, IDnlibDef def)
        {
            //
        }

        public void PostRename(ConfuserContext context, INameService service, ProtectionParameters parameters, IDnlibDef def)
        {
            //
        }

        private void AnalyzeCAArgument(ConfuserContext context, INameService service, CAArgument arg)
        {
            if(arg.Type.DefinitionAssembly.IsCorLib() && arg.Type.FullName == "System.Type")
            {
                var typeSig = (TypeSig) arg.Value;
                foreach(var typeRef in typeSig.FindTypeRefs())
                {
                    var typeDef = typeRef.ResolveTypeDefThrow();
                    if(context.Modules.Contains((ModuleDefMD) typeDef.Module))
                    {
                        if(typeRef is TypeRef)
                            service.AddReference(typeDef, new TypeRefReference((TypeRef) typeRef, typeDef));
                        service.ReduceRenameMode(typeDef, RenameMode.ASCII);
                    }
                }
            }
            else if(arg.Value is CAArgument[])
            {
                foreach(var elem in (CAArgument[]) arg.Value)
                    AnalyzeCAArgument(context, service, elem);
            }
        }

        private void AnalyzeMemberRef(ConfuserContext context, INameService service, MemberRef memberRef)
        {
            var declType = memberRef.DeclaringType;
            var typeSpec = declType as TypeSpec;
            if(typeSpec == null || typeSpec.TypeSig.IsArray || typeSpec.TypeSig.IsSZArray)
                return;

            var sig = typeSpec.TypeSig;
            while(sig.Next != null)
                sig = sig.Next;


            Debug.Assert(sig is TypeDefOrRefSig || sig is GenericInstSig || sig is GenericSig);
            if(sig is GenericInstSig)
            {
                var inst = (GenericInstSig) sig;
                Debug.Assert(!(inst.GenericType.TypeDefOrRef is TypeSpec));
                var openType = inst.GenericType.TypeDefOrRef.ResolveTypeDefThrow();
                if(!context.Modules.Contains((ModuleDefMD) openType.Module) ||
                   memberRef.IsArrayAccessors())
                    return;

                IDnlibDef member;
                if(memberRef.IsFieldRef) member = memberRef.ResolveFieldThrow();
                else if(memberRef.IsMethodRef) member = memberRef.ResolveMethodThrow();
                else throw new UnreachableException();

                service.AddReference(member, new MemberRefReference(memberRef, member));
            }
        }
    }
}