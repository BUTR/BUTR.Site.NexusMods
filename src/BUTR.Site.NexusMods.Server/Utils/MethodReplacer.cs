using Mono.Unix.Native;

using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace BUTR.Site.NexusMods.Server.Utils;

public static class MethodReplacer
{
    [DllImport("kernel32.dll")]
    private static extern bool VirtualProtect(IntPtr lpAddress, uint dwSize, Protection flNewProtect, out Protection lpflOldProtect);

    private enum Protection { PAGE_EXECUTE_READWRITE = 0x40, }

    public static void Replace(MethodBase source, MethodBase target)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return;

        RuntimeHelpers.PrepareMethod(source.MethodHandle);
        RuntimeHelpers.PrepareMethod(target.MethodHandle);


        var offset = 2 * IntPtr.Size;
        var sourceAddress = Marshal.ReadIntPtr(source.MethodHandle.Value, offset);
        var targetAddress = Marshal.ReadIntPtr(target.MethodHandle.Value, offset);

        var instruction = IntPtr.Size == 4
            ? new byte[]
                {
                    0x68, // push <value>
                }
                .Concat(BitConverter.GetBytes(targetAddress.ToInt32()))
                .Concat(new byte[]
                {
                    0xC3 //ret
                }).ToArray()
            : new byte[]
                {
                    0x48, 0xB8 // mov rax <value>
                }
                .Concat(BitConverter.GetBytes(targetAddress.ToInt64()))
                .Concat(new byte[]
                {
                    0x50, // push rax
                    0xC3  // ret
                }).ToArray();

        var oldWindows = default(Protection);
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            _ = Syscall.mprotect(sourceAddress, (ulong) instruction.Length, MmapProts.PROT_WRITE | MmapProts.PROT_READ);
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            _ = VirtualProtect(sourceAddress, (uint) instruction.Length, Protection.PAGE_EXECUTE_READWRITE, out oldWindows);

        Marshal.Copy(instruction, 0, sourceAddress, instruction.Length);

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            _ = Syscall.mprotect(sourceAddress, (ulong) instruction.Length, MmapProts.PROT_EXEC | MmapProts.PROT_READ);
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            _ = VirtualProtect(sourceAddress, (uint) instruction.Length, oldWindows, out _);
    }
}