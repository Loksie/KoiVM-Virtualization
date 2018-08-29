#region

using System;
using System.IO;
using System.Linq;
using Confuser.Core.Project;
using dnlib.DotNet;
using Cr = Confuser.Core;

#endregion

namespace KoiVM.Confuser.Processor
{
    public static class StubProcessor
    {
        public static byte[] Process(string binPath, string pubPath, Cr.ILogger logger)
        {
            logger.Info("Processing Stub assembly...");

            var input = Path.Combine(@"C:\Users\Nybher\Desktop\koiVM\Debug\bin", "KoiVM.Confuser.exe");
            var output = Path.Combine(pubPath, "KoiVM.Confuser.exe");
            logger.InfoFormat("Input path: {0}", input);
            logger.InfoFormat("Output path: {0}", output);

            var inputModule = File.ReadAllBytes(input);

            var internalModule = ModuleDefMD.Load(inputModule);
            internalModule.Name = "KoiVM.Confuser.Internal.dll";
            internalModule.EntryPoint = null;
            internalModule.Kind = ModuleKind.Dll;
            internalModule.Assembly.Name = "KoiVM.Confuser.Internal";
            foreach(var type in internalModule.Types.ToList())
            {
                if(type.IsGlobalModuleType)
                    continue;
                if(!type.Namespace.StartsWith("KoiVM.Confuser.Internal"))
                    internalModule.Types.Remove(type);
            }

            var stubModule = ModuleDefMD.Load(inputModule);
            foreach(var type in stubModule.Types.ToList())
            {
                if(type.IsGlobalModuleType)
                    continue;
                if(type.Namespace.StartsWith("KoiVM.Confuser.Internal"))
                    stubModule.Types.Remove(type);
            }

            PatchReferences(internalModule, stubModule);
            PatchReferences(stubModule, internalModule);

            logger.Info("Saving modules...");
            stubModule.Write(output);

            byte[] buf;
            using(var stream = new MemoryStream())
            {
                internalModule.Write(stream);
                buf = stream.ToArray();
            }
            var internalPath = Path.Combine(Path.GetDirectoryName(output), "KoiVM.Confuser.Internal.dll");
            File.WriteAllBytes(internalPath, buf);

            var proj = new ConfuserProject();
            proj.Add(new ProjectModule {Path = output});
            proj.OutputDirectory = Path.GetDirectoryName(output);
            proj.BaseDirectory = proj.OutputDirectory;
            proj.ProbePaths.Add(binPath);

            var parameters = new Cr.ConfuserParameters();
            parameters.Project = proj;
            parameters.Logger = logger;
            Cr.ConfuserEngine.Run(parameters).Wait();

            var symMap = Path.Combine(proj.OutputDirectory, "symbols.map");
            if(File.Exists(symMap))
                File.Delete(symMap);
            File.Delete(internalPath);

            logger.Info("Finish Stub creation.");
            return buf;
        }

        private static void PatchReferences(ModuleDef src, ModuleDef dst)
        {
            foreach(var type in src.Types)
            foreach(var method in type.Methods)
            {
                if(!method.HasBody)
                    continue;

                foreach(var instr in method.Body.Instructions)
                    if(instr.Operand is IMemberRef && ((IMemberRef) instr.Operand).DeclaringType != null &&
                       ((IMemberRef) instr.Operand).DeclaringType.Module == null)
                    {
                        var memberRef = (IMemberRef) instr.Operand;
                        var declType = src.Import(dst.Find(memberRef.DeclaringType.FullName, false));
                        if(memberRef.IsField)
                            memberRef = new MemberRefUser(src, memberRef.Name, ((IField) memberRef).FieldSig, declType);
                        else if(memberRef.IsMethod)
                            memberRef = new MemberRefUser(src, memberRef.Name, ((IMethod) memberRef).MethodSig, declType);
                        else
                            throw new NotSupportedException();
                        instr.Operand = memberRef;
                    }
            }
        }
    }
}