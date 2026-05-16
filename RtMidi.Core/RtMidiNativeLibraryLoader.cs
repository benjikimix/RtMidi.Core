using System;
using System.IO;
using System.Runtime.InteropServices;

namespace RtMidi.Core.Unmanaged
{
    internal static class RtMidiNativeLibraryLoader
    {
        private const int RtlNow = 2;
        private const int RtlGlobal = 0x100;
        private static readonly object SyncRoot = new object();
        private static bool _initialized;

        internal static void TryLoad()
        {
            if (_initialized)
            {
                return;
            }

            lock (SyncRoot)
            {
                if (_initialized)
                {
                    return;
                }

                if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    _initialized = true;
                    return;
                }

                var libraryFileName = GetDarwinLibraryFileName(RuntimeInformation.ProcessArchitecture);
                var path = Path.Combine(AppContext.BaseDirectory, libraryFileName);

                if (!File.Exists(path))
                {
                    path = Path.Combine(AppContext.BaseDirectory, "librtmidi.dylib");
                }

                if (!File.Exists(path))
                {
                    _initialized = true;
                    return;
                }

                var handle = dlopen(path, RtlNow | RtlGlobal);
                _initialized = true;

                if (handle == IntPtr.Zero)
                {
                    var message = dlerror();
                    throw new InvalidOperationException($"Failed to preload native RtMidi library '{path}': {GetDlErrorMessage(message)}");
                }
            }
        }

        private static string GetDarwinLibraryFileName(Architecture architecture)
        {
            return architecture switch
            {
                Architecture.Arm64 => "librtmidi.darwin-arm64.dylib",
                Architecture.X64 => "librtmidi.darwin-x64.dylib",
                _ => "librtmidi.dylib",
            };
        }

        private static string GetDlErrorMessage(IntPtr messagePtr)
        {
            return messagePtr == IntPtr.Zero ? "Unknown error" : Marshal.PtrToStringAnsi(messagePtr);
        }

        [DllImport("libdl")]
        private static extern IntPtr dlopen(string path, int flags);

        [DllImport("libdl")]
        private static extern IntPtr dlerror();
    }
}
