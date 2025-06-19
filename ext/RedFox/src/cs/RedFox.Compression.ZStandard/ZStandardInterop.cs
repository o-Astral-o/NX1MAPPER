using RedFox.Interop;
using System.Runtime.InteropServices;

namespace RedFox.Compression.ZStandard
{
    internal unsafe partial class ZStandardInterop
    {
        #region ZStandard
        const string ZStdLibraryName = "libzstd";

        [LibraryImport(ZStdLibraryName, EntryPoint = "ZSTD_decompress")]
        internal static partial int ZSTD_decompress(void* dst, int dstCapacity, void* src, int srcCapacity);

        [LibraryImport(ZStdLibraryName, EntryPoint = "ZSTD_compress")]
        internal static partial int ZSTD_compress(void* dst, int dstCapacity, void* src, int srcCapacity, int compressionLevel);

        [LibraryImport(ZStdLibraryName, EntryPoint = "ZSTD_getFrameContentSize")]
        internal static partial int ZSTD_getFrameContentSize(void* src, int srcSize);

        [LibraryImport(ZStdLibraryName, EntryPoint = "ZSTD_getErrorName")]
        internal static partial sbyte* ZSTD_getErrorName(int errorCode);

        [LibraryImport(ZStdLibraryName, EntryPoint = "ZSTD_compressBound")]
        internal static partial int ZSTD_compressBound(int srcSize);
        #endregion

        static ZStandardInterop()
        {
            NativeLibrary.SetDllImportResolver(typeof(ZStandardInterop).Assembly, NativeHelpers.DllImportResolver);
        }
    }
}