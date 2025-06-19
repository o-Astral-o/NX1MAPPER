using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using RedFox.Interop;

namespace RedFox.Compression.ZLIB
{
    internal unsafe partial class ZLIBInterop
    {
        internal enum MiniZReturnStatus
        {
            OK = 0,
            StreamEnd = 1,
            NeedDict = 2,
            ErrorNo = -1,
            StreamError = -2,
            DataError = -3,
            MemoryError = -4,
            BufferError = -5,
            VersionError = -6,
            ParamError = -10000
        };

        const string MiniZLibraryName = "MiniZ";

        [LibraryImport(MiniZLibraryName, EntryPoint = "mz_uncompress")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial int MZ_uncompress(byte* dest, ref int destLen, byte* source, int sourceLen, int windowBits);

        [LibraryImport(MiniZLibraryName, EntryPoint = "mz_deflateBound")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial int MZ_deflateBound(IntPtr stream, int inputSize);

        [LibraryImport(MiniZLibraryName, EntryPoint = "mz_compress")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial int MZ_compress(byte* dest, ref int destLen, byte* source, int sourceLen);

        static ZLIBInterop()
        {
            NativeLibrary.SetDllImportResolver(typeof(ZLIBInterop).Assembly, NativeHelpers.DllImportResolver);
        }
    }
}
