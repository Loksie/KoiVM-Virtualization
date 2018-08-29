#region

using System.IO;
using dnlib.DotNet;

#endregion

namespace Confuser.Core.Services
{
    internal class RuntimeService : IRuntimeService
    {
        private ModuleDef rtModule;

        /// <inheritdoc />
        public TypeDef GetRuntimeType(string fullName)
        {
            if(rtModule == null)
            {
                var module = typeof(RuntimeService).Assembly.ManifestModule;
                var rtPath = "Confuser.Runtime.dll";
                if(module.FullyQualifiedName[0] != '<')
                    rtPath = Path.Combine(Path.GetDirectoryName(module.FullyQualifiedName), rtPath);
                rtModule = ModuleDefMD.Load(rtPath, new ModuleCreationOptions {TryToLoadPdbFromDisk = true});
                rtModule.EnableTypeDefFindCache = true;
            }
            return rtModule.Find(fullName, true);
        }
    }

    /// <summary>
    ///     Provides methods to obtain runtime library injection type.
    /// </summary>
    public interface IRuntimeService
    {
        /// <summary>
        ///     Gets the specified runtime type for injection.
        /// </summary>
        /// <param name="fullName">The full name of the runtime type.</param>
        /// <returns>The requested runtime type.</returns>
        TypeDef GetRuntimeType(string fullName);
    }
}