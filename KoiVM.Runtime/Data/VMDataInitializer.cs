#region

using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

#endregion

namespace KoiVM.Runtime.Data
{
    internal unsafe class VMDataInitializer
    {
        internal static VMData GetData(Module module)
        {
            if(!Platform.LittleEndian)
                throw new PlatformNotSupportedException();

            var moduleBase = (byte*) Marshal.GetHINSTANCE(module);
            var fqn = module.FullyQualifiedName;
            var isFlat = fqn.Length > 0 && fqn[0] == '<';
            if(isFlat)
                return new VMData(module, GetKoiStreamFlat(moduleBase));
            return new VMData(module, GetKoiStreamMapped(moduleBase));
        }

        private static void* GetKoiStreamMapped(byte* moduleBase)
        {
            var ptr = moduleBase + 0x3c;
            byte* ptr2;
            ptr = ptr2 = moduleBase + *(uint*) ptr;
            ptr += 0x6;
            var sectNum = *(ushort*) ptr;
            ptr += 14;
            var optSize = *(ushort*) ptr;
            ptr = ptr2 = ptr + 0x4 + optSize;

            var mdDir = moduleBase + *(uint*) (ptr - 16);
            var mdHdr = moduleBase + *(uint*) (mdDir + 8);
            mdHdr += 12;
            mdHdr += *(uint*) mdHdr;
            mdHdr = (byte*) (((ulong) mdHdr + 7) & ~3UL);
            mdHdr += 2;
            ushort numOfStream = *mdHdr;
            mdHdr += 2;
            var streamName = new StringBuilder();
            for(var i = 0; i < numOfStream; i++)
            {
                var offset = *(uint*) mdHdr;
                var len = *(uint*) (mdHdr + 4);
                mdHdr += 8;
                streamName.Length = 0;
                for(var ii = 0; ii < 8; ii++)
                {
                    streamName.Append((char) *mdHdr++);
                    if(*mdHdr == 0)
                    {
                        mdHdr += 3;
                        break;
                    }
                    streamName.Append((char) *mdHdr++);
                    if(*mdHdr == 0)
                    {
                        mdHdr += 2;
                        break;
                    }
                    streamName.Append((char) *mdHdr++);
                    if(*mdHdr == 0)
                    {
                        mdHdr += 1;
                        break;
                    }
                    streamName.Append((char) *mdHdr++);
                    if(*mdHdr == 0)
                        break;
                }
                if(streamName.ToString() == "#Koi")
                    return AllocateKoi(moduleBase + *(uint*) (mdDir + 8) + offset, len);
            }
            return null;
        }

        private static void* GetKoiStreamFlat(byte* moduleBase)
        {
            var ptr = moduleBase + 0x3c;
            byte* ptr2;
            ptr = ptr2 = moduleBase + *(uint*) ptr;
            ptr += 0x6;
            var sectNum = *(ushort*) ptr;
            ptr += 14;
            var optSize = *(ushort*) ptr;
            ptr = ptr2 = ptr + 0x4 + optSize;

            var mdDir = *(uint*) (ptr - 16);

            var vAdrs = new uint[sectNum];
            var vSizes = new uint[sectNum];
            var rAdrs = new uint[sectNum];
            for(var i = 0; i < sectNum; i++)
            {
                vAdrs[i] = *(uint*) (ptr + 12);
                vSizes[i] = *(uint*) (ptr + 8);
                rAdrs[i] = *(uint*) (ptr + 20);
                ptr += 0x28;
            }

            for(var i = 0; i < sectNum; i++)
                if(vAdrs[i] <= mdDir && mdDir < vAdrs[i] + vSizes[i])
                {
                    mdDir = mdDir - vAdrs[i] + rAdrs[i];
                    break;
                }
            var mdDirPtr = moduleBase + mdDir;
            var mdHdr = *(uint*) (mdDirPtr + 8);
            for(var i = 0; i < sectNum; i++)
                if(vAdrs[i] <= mdHdr && mdHdr < vAdrs[i] + vSizes[i])
                {
                    mdHdr = mdHdr - vAdrs[i] + rAdrs[i];
                    break;
                }


            var mdHdrPtr = moduleBase + mdHdr;
            mdHdrPtr += 12;
            mdHdrPtr += *(uint*) mdHdrPtr;
            mdHdrPtr = (byte*) (((ulong) mdHdrPtr + 7) & ~3UL);
            mdHdrPtr += 2;
            ushort numOfStream = *mdHdrPtr;
            mdHdrPtr += 2;
            var streamName = new StringBuilder();
            for(var i = 0; i < numOfStream; i++)
            {
                var offset = *(uint*) mdHdrPtr;
                var len = *(uint*) (mdHdrPtr + 4);
                streamName.Length = 0;
                mdHdrPtr += 8;
                for(var ii = 0; ii < 8; ii++)
                {
                    streamName.Append((char) *mdHdrPtr++);
                    if(*mdHdrPtr == 0)
                    {
                        mdHdrPtr += 3;
                        break;
                    }
                    streamName.Append((char) *mdHdrPtr++);
                    if(*mdHdrPtr == 0)
                    {
                        mdHdrPtr += 2;
                        break;
                    }
                    streamName.Append((char) *mdHdrPtr++);
                    if(*mdHdrPtr == 0)
                    {
                        mdHdrPtr += 1;
                        break;
                    }
                    streamName.Append((char) *mdHdrPtr++);
                    if(*mdHdrPtr == 0)
                        break;
                }
                if(streamName.ToString() == "#Koi")
                    return AllocateKoi(moduleBase + mdHdr + offset, len);
            }
            return null;
        }

        [DllImport("kernel32.dll")]
        private static extern void CopyMemory(void* dest, void* src, uint count);

        private static void* AllocateKoi(void* ptr, uint len)
        {
            var koi = (void*) Marshal.AllocHGlobal((int) len);
            CopyMemory(koi, ptr, len);
            return koi;
        }
    }
}