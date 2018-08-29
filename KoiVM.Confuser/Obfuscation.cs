#region

using System.Reflection;

#endregion

[assembly: Obfuscation(Exclude = true, Feature =
    "name('KoiVM.Confuser.exe'):+anti debug(mode=antinet)"
)]

[assembly: Obfuscation(Exclude = true, Feature =
        "preset(aggressive);+constants(mode=dynamic,decoderCount=10,cfg=true);" +
        "+ctrl flow(predicate=expression,intensity=100);+rename(renPublic=false,mode=sequential);" +
        "+resources(mode=dynamic);+ref proxy(typeErasure=true,internal=true);"
#if DEBUG
        + "-anti debug;-rename;"
#endif
)]

[assembly: Obfuscation(Exclude = true, Feature =
    "module('KoiVM.Confuser.Internal.dll'):-ref proxy"
)]