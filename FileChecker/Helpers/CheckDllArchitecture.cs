using System;
using System.Diagnostics;
using System.Reflection;

namespace FileVersionChecker.Helpers
{
    internal class CheckDLLArchitecture
    {
        /// <summary>
        /// Check if the DL is 32 / 64 bits
        /// http://stackoverflow.com/questions/1001404/check-if-unmanaged-dll-is-32-bit-or-64-bit
        /// </summary>
        public static Architecture DoesDllIs64Bit(string dllPath)
        {
            // Any CPU = PE32 with 32BIT = 0
            // x86     = PE32 with 32BIT = 1
            // x64/Itanium (IA-64)	= PE32+ with 32BIT = 0

            ImageFileMachine imgFileMachine = 0;
            try
            {
                Assembly assembly = Assembly.LoadFile(dllPath);
                PortableExecutableKinds kinds;

                assembly.ManifestModule.GetPEKind(out kinds, out imgFileMachine);

                switch (imgFileMachine)
                {
                    case ImageFileMachine.AMD64:// A 64-bit AMD processor only.
                    case ImageFileMachine.IA64:// A 64-bit Intel processor only.
                        return Architecture.x64;
                    case ImageFileMachine.I386:
                        if (kinds == PortableExecutableKinds.ILOnly)
                            return Architecture.AnyCPU;
                        return Architecture.x86;
                    default:
                        return Architecture.Unknown;
                }
            }
            catch (Exception)
            {
                Debug.WriteLine(string.Format("Cannot determine architecture for {0}", dllPath));
                //Console.WriteLine();
            }

            return Architecture.Unknown;
        }
    }

    public enum Architecture
    {
        Unknown,
        x86,
        x64,
        AnyCPU,
    }
}
