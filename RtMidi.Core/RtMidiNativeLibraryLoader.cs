using System;
using System.Collections.Generic;
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

                var path = FindNativeLibraryPath();

                if (path == null)
                {
                    _initialized = true;
                    return;
                }

                path = EnsureGenericLibraryName(path);

                var handle = dlopen(path, RtlNow | RtlGlobal);
                _initialized = true;

                if (handle == IntPtr.Zero)
                {
                    var message = dlerror();
                    throw new InvalidOperationException($"Failed to preload native RtMidi library '{path}': {GetDlErrorMessage(message)}");
                }
            }
        }

        private static string FindNativeLibraryPath()
        {
            var baseDirectory = AppContext.BaseDirectory;
            var assemblyDirectory = Path.GetDirectoryName(typeof(RtMidiNativeLibraryLoader).Assembly.Location);

            var searchDirectories = new List<string>
            {
                baseDirectory,
                assemblyDirectory,
            };

            if (!string.IsNullOrEmpty(baseDirectory))
            {
                searchDirectories.Add(Path.Combine(baseDirectory, "runtimes", "osx-arm64", "native"));
                searchDirectories.Add(Path.Combine(baseDirectory, "runtimes", "osx-x64", "native"));
            }

            if (!string.IsNullOrEmpty(assemblyDirectory))
            {
                searchDirectories.Add(Path.Combine(assemblyDirectory, "runtimes", "osx-arm64", "native"));
                searchDirectories.Add(Path.Combine(assemblyDirectory, "runtimes", "osx-x64", "native"));
            }

            foreach (var directory in searchDirectories)
            {
                if (string.IsNullOrEmpty(directory))
                {
                    continue;
                }

                foreach (var candidate in GetDarwinLibraryFileNames(RuntimeInformation.ProcessArchitecture))
                {
                    var path = Path.Combine(directory, candidate);
                    if (File.Exists(path))
                    {
                        return path;
                    }
                }
            }

            return null;
        }

        private static string EnsureGenericLibraryName(string selectedPath)
        {
            if (string.IsNullOrEmpty(selectedPath))
            {
                return selectedPath;
            }

            if (string.Equals(Path.GetFileName(selectedPath), "librtmidi.dylib", StringComparison.OrdinalIgnoreCase))
            {
                return selectedPath;
            }

            var directory = Path.GetDirectoryName(selectedPath);
            if (string.IsNullOrEmpty(directory))
            {
                return selectedPath;
            }

            var genericPath = Path.Combine(directory, "librtmidi.dylib");
            try
            {
                File.Copy(selectedPath, genericPath, true);
                return genericPath;
            }
            catch
            {
                return selectedPath;
            }
        }

        private static IEnumerable<string> GetDarwinLibraryFileNames(Architecture architecture)
        {
            switch (architecture)
            {
                case Architecture.Arm64:
                    yield return "librtmidi.darwin-arm64.dylib";
                    break;
                case Architecture.X64:
                    yield return "librtmidi.darwin-x64.dylib";
                    break;
            }

            yield return "librtmidi.dylib";
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
