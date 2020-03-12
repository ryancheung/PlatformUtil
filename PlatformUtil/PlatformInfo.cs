using System;
using System.Runtime.InteropServices;

namespace PlatformUtil
{
    /// <summary>
    /// Contains information relating to the current platform.
    /// </summary>
    public static class PlatformInfo
    {
        /// <summary>
        /// Initializes the <see cref="PlatformInfo"/> type.
        /// </summary>
        static PlatformInfo()
        {
            CurrentPlatform = DetectCurrentPlatform(out var machineHardwareName);
            CurrentPlatformMachineHardwareName = machineHardwareName;
            CurrentPlatformVersion = Environment.OSVersion.VersionString;
            CurrentRuntime = DetectCurrentRuntime();
        }

        /// <summary>
        /// Gets a value indicating whether runtime code generation is supported on the current platform.
        /// </summary>
        /// <returns><see langword="true"/> if the current platform supports runtime code generation; otherwise, <see langword="false"/>.</returns>
        public static Boolean IsRuntimeCodeGenerationSupported()
        {
            return IsRuntimeCodeGenerationSupported(CurrentPlatform);
        }

        /// <summary>
        /// Gets a value indicating whether runtime code generation is supported on the specified platform.
        /// </summary>
        /// <param name="platform">The platform to evaluate.</param>
        /// <returns><see langword="true"/> if the specified platform supports runtime code generation; otherwise, <see langword="false"/>.</returns>
        public static Boolean IsRuntimeCodeGenerationSupported(Platform platform)
        {
            if (platform == Platform.iOS)
                return false;

            return true;
        }

        /// <summary>
        /// Gets an <see cref="Runtime"/> that is currently executing this application.
        /// </summary>
        public static Runtime CurrentRuntime { get; }

        /// <summary>
        /// Gets an <see cref="Platform"/> that is currently executing this application.
        /// </summary>
        public static Platform CurrentPlatform { get; }

        /// <summary>
        /// Gets the string which contains the machine hardware name for the current platform.
        /// </summary>
        public static String CurrentPlatformMachineHardwareName { get; }

        /// <summary>
        /// Gets the string which contains the version information for the current platform.
        /// </summary>
        public static String CurrentPlatformVersion { get; }

        /// <summary>
        /// Attempts to detect the current runtime.
        /// </summary>
        private static Runtime DetectCurrentRuntime()
        {
            if (String.Equals("System.Private.CoreLib", typeof(Object).Assembly.GetName().Name, StringComparison.Ordinal))
                return Runtime.CoreCLR;

            if (Type.GetType("Mono.Runtime") != null)
                return Runtime.Mono;

            return Runtime.CLR;
        }

        /// <summary>
        /// Attempts to detect the current platform.
        /// </summary>
        private static Platform DetectCurrentPlatform(out String machineHardwareName)
        {
            machineHardwareName = Environment.OSVersion.Platform.ToString();

            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Win32NT:
                case PlatformID.Win32S:
                case PlatformID.Win32Windows:
                case PlatformID.WinCE:
                    return Platform.Windows;
                case PlatformID.Unix:
                    {
                        var buf = IntPtr.Zero;
                        try
                        {
                            buf = Marshal.AllocHGlobal(8192);
                            if (Native.uname(buf) == 0)
                            {
                                machineHardwareName = Marshal.PtrToStringAnsi(buf);
                                if (String.Equals("Darwin", machineHardwareName, StringComparison.OrdinalIgnoreCase))
                                {
                                    if (Type.GetType("UIKit.UIApplicationDelegate, Xamarin.iOS") != null)
                                    {
                                        return Platform.iOS;
                                    }
                                    else
                                    {
                                        return Platform.macOS;
                                    }
                                }
                                else
                                {
                                    if (Type.GetType("Android.App.Activity, Mono.Android") != null)
                                    {
                                        return Platform.Android;
                                    }
                                    else
                                    {
                                        return Platform.Linux;
                                    }
                                }
                            }
                        }
                        finally
                        {
                            if (buf != IntPtr.Zero)
                                Marshal.FreeHGlobal(buf);
                        }
                    }
                    break;
            }

            throw new PlatformNotSupportedException();
        }
    }
}
