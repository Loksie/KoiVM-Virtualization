#region

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using Confuser.Core.Project;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using dnlib.DotNet.Writer;
using Cr = Confuser.Core;
using SR = System.Reflection;

#endregion

namespace KoiVM.Confuser.Processor
{
    internal class UserProcessor
    {
        private static readonly ThreadLocal<Random> random =
            new ThreadLocal<Random>(() => new Random(Thread.CurrentThread.ManagedThreadId * Environment.TickCount));

        public readonly User User;
        private string binPath;
        private RenameContext ctx;
        private readonly byte[] internalModule;
        private readonly Cr.ILogger logger;
        private string privPath;
        private byte[] rtModule;
        private string version;

        public UserProcessor(User usr, byte[] internalModule, Cr.ILogger logger)
        {
            User = usr;
            this.internalModule = internalModule;
            this.logger = logger;
        }

        public void Process(string binPath, string pubPath)
        {
            this.binPath = binPath;
            privPath = Path.Combine(pubPath, User.GetKoiId());
            if(User.SubscriptionEnd < DateTime.Now || User.Status == Status.Inactive)
                return;

            if(Directory.Exists(privPath))
            {
                Directory.Delete(privPath, true);
                while(Directory.Exists(privPath))
                    Thread.Sleep(10);
            }
            Directory.CreateDirectory(privPath);

            logger.InfoFormat("Private path: {0}", privPath);

            if(User.Status == Status.Revoked)
            {
                logger.Error("License revoked");
                WriteVersion("REVOKED");
            }
            else
            {
                ProcessConfuserModule();
                ProcessRuntimeModule();
                ProcessVMModule();

                Pack();
                logger.Info("Output generated.");
            }
        }

        private void ProcessConfuserModule()
        {
            logger.Info("Processing Confuser Module...");
            var module = ModuleDefMD.Load(internalModule);
            module.LoadPdb();

            var assembly = GetType().Assembly;
            foreach(var attr in assembly.GetCustomAttributes(false))
            {
                var asmAttr = module.Assembly.CustomAttributes.Find(attr.GetType().FullName);
                string ver;
                switch(attr.GetType().FullName)
                {
                    case "System.Reflection.AssemblyVersionAttribute":
                        ver = ((SR.AssemblyVersionAttribute) attr).Version;
                        asmAttr.ConstructorArguments[0] = new CAArgument(module.CorLibTypes.String, ver);
                        break;

                    case "System.Reflection.AssemblyFileVersionAttribute":
                        ver = ((SR.AssemblyFileVersionAttribute) attr).Version;
                        asmAttr.ConstructorArguments[0] = new CAArgument(module.CorLibTypes.String, ver);
                        break;

                    case "System.Reflection.AssemblyInformationalVersionAttribute":
                        ver = ((SR.AssemblyInformationalVersionAttribute) attr).InformationalVersion;
                        asmAttr.ConstructorArguments[0] = new CAArgument(module.CorLibTypes.String, ver);
                        break;
                }
            }

            var fish = module.Find("KoiVM.Confuser.Internal.Fish", true);
            foreach(var instr in fish.FindStaticConstructor().Body.Instructions)
            {
                if(instr.OpCode.Code != Code.Ldstr)
                    continue;

                var str = (string) instr.Operand;
                if(str == "{USERNAME}")
                    instr.Operand = User.UserName;
                else if(str == "{SUBSCRIPTION}")
                    instr.Operand = User.SubscriptionEnd.ToString("dd/MM/yyyy");
                else if(str == "00000000")
                    instr.Operand = User.ID.ToString("x8");
            }
            module.Write(Path.Combine(privPath, "KoiVM.Confuser.Internal.dll"), new ModuleWriterOptions(module) {WritePdb = true});
            logger.Info("Finished Confuser Module.");
        }

        private void ProcessRuntimeModule()
        {
            logger.Info("Processing Runtime Module...");

            var module = ModuleDefMD.Load(Path.Combine(@"C:\Users\Nybher\Desktop\koiVM\Debug\bin", "KoiVM.Runtime.dll"));
            module.Assembly.Name += "." + User.LongID;
            module.Name = string.Format("KoiVM.Runtime.{0}.dll", User.LongID);

            var renamer = new Renamer(random.Value.Next(), true, '#');
           renamer.Process(module);

            ctx = new RenameContext();

            ctx.VMEntry = renamer.GetNewName(ctx.VMEntry);
            ctx.VMRun = renamer.GetNewName(ctx.VMRun);

            ctx.VMDispatcher = renamer.GetNewName(ctx.VMDispatcher);
            ctx.VMDispatcherDothrow = renamer.GetNewName(ctx.VMDispatcherDothrow);
            ctx.VMDispatcherThrow = renamer.GetNewName(ctx.VMDispatcherThrow);
            ctx.VMDispatcherGetIP = renamer.GetNewName(ctx.VMDispatcherGetIP);
            ctx.VMDispatcherStackwalk = renamer.GetNewName(ctx.VMDispatcherStackwalk);

            ctx.VMConstants = renamer.GetNewName(ctx.VMConstants);

            using(var reader = new StringReader(ctx.VMConstMapText))
            {
                while(reader.Peek() > 0)
                {
                    var line = reader.ReadLine().Trim();
                    if(string.IsNullOrEmpty(line))
                        continue;
                    var entry = line.Split(new[] {'\t'}, StringSplitOptions.RemoveEmptyEntries);
                    var key = renamer.GetNewName(entry[0]);
                    ctx.VMConstMap.Add(Tuple.Create(key, entry[1]));
                }
            }

            random.Value.Shuffle(ctx.VMConstMap);

            using(var stream = new MemoryStream())
            {
                module.Write(stream);
                rtModule = stream.ToArray();
            }
            logger.Info("Finished Runtime Module.");
        }

        private void ProcessVMModule()
        {
            logger.Info("Processing VM Module...");

            var module = ModuleDefMD.Load(Path.Combine(@"C:\Users\Nybher\Desktop\koiVM\Debug\bin", "KoiVM.dll"));
            module.LoadPdb();

            var ca = module.Assembly.CustomAttributes.Find("System.Reflection.AssemblyInformationalVersionAttribute");
            version = (UTF8String) ca.ConstructorArguments[0].Value;

            foreach(var instr in module.Find("KoiVM.Watermark", true).FindMethod("GenerateWatermark").Body.Instructions)
                if(instr.OpCode.Code == Code.Ldc_I4 && (int) instr.Operand == 0x10000)
                    instr.Operand = (int) (User.Watermark * 0xaeaf10f7); // mod-inverse = 0xa7c0b0c7

            var initCctor = module.Find("KoiVM.RT.Mutation.RTMap", true).FindStaticConstructor();
            var renamer = new Renamer(random.Value.Next(), false, '$');
            renamer.Process(module);

            var sb = new StringBuilder();
            foreach(var entry in ctx.VMConstMap)
                sb.AppendLine(string.Format("{0}\t{1}", entry.Item1, renamer.GetNewName(entry.Item2)));
            ctx.VMConstMapText = sb.ToString();

            foreach(var instr in initCctor.Body.Instructions)
            {
                var value = instr.Operand as string;
                if(value == null)
                    continue;
                switch(value)
                {
                    case "KoiVM.Runtime.VMEntry":
                        instr.Operand = ctx.VMEntry;
                        break;
                    case "Run":
                        instr.Operand = ctx.VMRun;
                        break;
                    case "KoiVM.Runtime.Execution.VMDispatcher":
                        instr.Operand = ctx.VMDispatcher;
                        break;
                    case "DoThrow":
                        instr.Operand = ctx.VMDispatcherDothrow;
                        break;
                    case "Throw":
                        instr.Operand = ctx.VMDispatcherThrow;
                        break;
                    case "GetIP":
                        instr.Operand = ctx.VMDispatcherGetIP;
                        break;
                    case "StackWalk":
                        instr.Operand = ctx.VMDispatcherStackwalk;
                        break;
                    case "KoiVM.Runtime.Dynamic.Constants":
                        instr.Operand = ctx.VMConstants;
                        break;
                    default:
                        instr.Operand = ctx.VMConstMapText;
                        break;
                }
            }

            module.Resources.Add(new EmbeddedResource("KoiVM.Runtime.dll", rtModule));
            module.Write(Path.Combine(privPath, "KoiVM.dll"), new ModuleWriterOptions(module) {WritePdb = true});
            logger.Info("Finished VM Module.");
        }

        private void Pack()
        {
            logger.Info("Obfuscating...");

            var proj = new ConfuserProject();
            foreach(var file in Directory.GetFiles(privPath).OrderByDescending(x => x).Where(x => x.EndsWith(".exe") || x.EndsWith(".dll")))
                proj.Add(new ProjectModule {Path = file});
#if DEBUG || __TRACE
            proj.OutputDirectory = Path.Combine(privPath.Trim('\\'), "Confused");
#else
			proj.OutputDirectory = privPath;
#endif
            proj.Debug = true;
            proj.BaseDirectory = privPath;
            proj.ProbePaths.Add(binPath);
            proj.PluginPaths.Add(Path.Combine(binPath, "KoiVM.Confuser.exe"));

            var parameters = new Cr.ConfuserParameters();
            parameters.Project = proj;
            parameters.Logger = logger;
            Cr.ConfuserEngine.Run(parameters).Wait();

            logger.Info("Packing...");
            using(var stream = new MemoryStream())
            {
                var rc4 = new RC4(Convert.FromBase64String("S29pVk0gaXMgYfD4Da3V0ZSEhIQ=="));//S29pVk0gaXMgY3V0ZSEhIQ==
                using (var deflate = new DeflateStream(stream, CompressionMode.Compress))
                using(var writer = new BinaryWriter(deflate))
                {
#if DEBUG || __TRACE
                    var fileList = Directory.GetFiles(@"C:\Users\Nybher\Desktop\koiVM\Debug\bin\pub\ann\");
#else
					var fileList = Directory.GetFiles(privPath, "*.dll");
#endif

                    writer.Write(fileList.Length);
                    foreach(var file in fileList)
                    {
                        var fileBuf = File.ReadAllBytes(file);
                        writer.Write(fileBuf.Length);
                        writer.Write(fileBuf);
#if !DEBUG && !__TRACE
						File.Delete(file);
#endif
                    }
                }
                var buf = stream.ToArray();
                rc4.Crypt(buf, 0, buf.Length);
                File.WriteAllBytes(Path.Combine(privPath, "koi.pack"), buf);

                WriteVersion(version);
            }
        }

        private void WriteVersion(string version)
        {
            var rc4 = new RC4(Encoding.UTF8.GetBytes(User.GetKoiId()));
            var buf = Encoding.UTF8.GetBytes(version);
            rc4.Crypt(buf, 0, buf.Length);
            File.WriteAllBytes(Path.Combine(privPath, "VERSION"), buf);
        }

        private class RenameContext
        {
            public string VMConstants = "KoiVM.Runtime.Dynamic.Constants";

            public readonly List<Tuple<string, string>> VMConstMap = new List<Tuple<string, string>>();

            public string VMConstMapText = @"
REG_R0								R0
REG_R1								R1
REG_R2								R2
REG_R3								R3
REG_R4								R4
REG_R5								R5
REG_R6								R6
REG_R7								R7
REG_BP								BP
REG_SP								SP
REG_IP								IP
REG_FL								FL
REG_K1								K1
REG_K2								K2
REG_M1								M1
REG_M2								M2
FL_OVERFLOW							OVERFLOW
FL_CARRY							CARRY
FL_ZERO								ZERO
FL_SIGN								SIGN
FL_UNSIGNED							UNSIGNED
FL_BEHAV1							BEHAV1
FL_BEHAV2							BEHAV2
FL_BEHAV3							BEHAV3
OP_NOP								NOP
OP_LIND_PTR							LIND_PTR
OP_LIND_OBJECT						LIND_OBJECT
OP_LIND_BYTE						LIND_BYTE
OP_LIND_WORD						LIND_WORD
OP_LIND_DWORD						LIND_DWORD
OP_LIND_QWORD						LIND_QWORD
OP_SIND_PTR							SIND_PTR
OP_SIND_OBJECT						SIND_OBJECT
OP_SIND_BYTE						SIND_BYTE
OP_SIND_WORD						SIND_WORD
OP_SIND_DWORD						SIND_DWORD
OP_SIND_QWORD						SIND_QWORD
OP_POP								POP
OP_PUSHR_OBJECT						PUSHR_OBJECT
OP_PUSHR_BYTE						PUSHR_BYTE
OP_PUSHR_WORD						PUSHR_WORD
OP_PUSHR_DWORD						PUSHR_DWORD
OP_PUSHR_QWORD						PUSHR_QWORD
OP_PUSHI_DWORD						PUSHI_DWORD
OP_PUSHI_QWORD						PUSHI_QWORD
OP_SX_BYTE							SX_BYTE
OP_SX_WORD							SX_WORD
OP_SX_DWORD							SX_DWORD
OP_CALL								CALL
OP_RET								RET
OP_NOR_DWORD						NOR_DWORD
OP_NOR_QWORD						NOR_QWORD
OP_CMP								CMP
OP_CMP_DWORD						CMP_DWORD
OP_CMP_QWORD						CMP_QWORD
OP_CMP_R32							CMP_R32
OP_CMP_R64							CMP_R64
OP_JZ								JZ
OP_JNZ								JNZ
OP_JMP								JMP
OP_SWT								SWT
OP_ADD_DWORD						ADD_DWORD
OP_ADD_QWORD						ADD_QWORD
OP_ADD_R32							ADD_R32
OP_ADD_R64							ADD_R64
OP_SUB_R32							SUB_R32
OP_SUB_R64							SUB_R64
OP_MUL_DWORD						MUL_DWORD
OP_MUL_QWORD						MUL_QWORD
OP_MUL_R32							MUL_R32
OP_MUL_R64							MUL_R64
OP_DIV_DWORD						DIV_DWORD
OP_DIV_QWORD						DIV_QWORD
OP_DIV_R32							DIV_R32
OP_DIV_R64							DIV_R64
OP_REM_DWORD						REM_DWORD
OP_REM_QWORD						REM_QWORD
OP_REM_R32							REM_R32
OP_REM_R64							REM_R64
OP_SHR_DWORD						SHR_DWORD
OP_SHR_QWORD						SHR_QWORD
OP_SHL_DWORD						SHL_DWORD
OP_SHL_QWORD						SHL_QWORD
OP_FCONV_R32_R64					FCONV_R32_R64
OP_FCONV_R64_R32					FCONV_R64_R32
OP_FCONV_R32						FCONV_R32
OP_FCONV_R64						FCONV_R64
OP_ICONV_PTR						ICONV_PTR
OP_ICONV_R64						ICONV_R64
OP_VCALL							VCALL
OP_TRY								TRY
OP_LEAVE							LEAVE
VCALL_EXIT							EXIT
VCALL_BREAK							BREAK
VCALL_ECALL							ECALL
VCALL_CAST							CAST
VCALL_CKFINITE						CKFINITE
VCALL_CKOVERFLOW					CKOVERFLOW
VCALL_RANGECHK						RANGECHK
VCALL_INITOBJ						INITOBJ
VCALL_LDFLD							LDFLD
VCALL_LDFTN							LDFTN
VCALL_TOKEN							TOKEN
VCALL_THROW							THROW
VCALL_SIZEOF						SIZEOF
VCALL_STFLD							STFLD
VCALL_BOX							BOX
VCALL_UNBOX							UNBOX
VCALL_LOCALLOC						LOCALLOC
HELPER_INIT							INIT
ECALL_CALL							E_CALL
ECALL_CALLVIRT						E_CALLVIRT
ECALL_NEWOBJ						E_NEWOBJ
ECALL_CALLVIRT_CONSTRAINED			E_CALLVIRT_CONSTRAINED
FLAG_INSTANCE						INSTANCE
EH_CATCH							CATCH
EH_FILTER							FILTER
EH_FAULT							FAULT
EH_FINALLY							FINALLY
";

            public string VMDispatcher = "KoiVM.Runtime.Execution.VMDispatcher";
            public string VMDispatcherDothrow = "DoThrow";
            public string VMDispatcherGetIP = "GetIP";
            public string VMDispatcherStackwalk = "StackWalk";
            public string VMDispatcherThrow = "Throw";
            public string VMEntry = "KoiVM.Runtime.VMEntry";
            public string VMRun = "Run";
        }
    }
}