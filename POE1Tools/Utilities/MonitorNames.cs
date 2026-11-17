using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace POE1Tools.Utilities
{
    /// <summary>Resolves monitor model names (e.g. "VX2758-2K-PRO") per GDI device name (e.g. "\\.\DISPLAY1").</summary>
    public static class MonitorNames
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct LUID { public uint Low; public int High; }

        [StructLayout(LayoutKind.Sequential, Size = 72)]
        private struct PathInfo
        {
            public LUID SourceAdapter;
            public uint SourceId;
            public uint SourceModeIdx;
            public uint SourceStatus;
            public LUID TargetAdapter;
            public uint TargetId;
        }

        [StructLayout(LayoutKind.Sequential, Size = 64)]
        private struct ModeInfo { public uint InfoType; }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct SourceName
        {
            public uint Type;
            public uint Size;
            public LUID AdapterId;
            public uint Id;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string GdiName;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct TargetName
        {
            public uint Type;
            public uint Size;
            public LUID AdapterId;
            public uint Id;
            public uint Flags;
            public uint OutputTechnology;
            public ushort EdidManufactureId;
            public ushort EdidProductCodeId;
            public uint ConnectorInstance;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
            public string FriendlyName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DevicePath;
        }

        [DllImport("user32.dll")]
        private static extern int GetDisplayConfigBufferSizes(uint flags, out uint pathCount, out uint modeCount);

        [DllImport("user32.dll")]
        private static extern int QueryDisplayConfig(uint flags, ref uint pathCount, [Out] PathInfo[] paths,
            ref uint modeCount, [Out] ModeInfo[] modes, IntPtr topologyId);

        [DllImport("user32.dll")]
        private static extern int DisplayConfigGetDeviceInfo(ref SourceName request);

        [DllImport("user32.dll")]
        private static extern int DisplayConfigGetDeviceInfo(ref TargetName request);

        private const uint QDC_ONLY_ACTIVE_PATHS = 2;

        public static Dictionary<string, string> GetAll()
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            try
            {
                if (GetDisplayConfigBufferSizes(QDC_ONLY_ACTIVE_PATHS, out uint pc, out uint mc) != 0) return result;
                var paths = new PathInfo[pc];
                var modes = new ModeInfo[mc];
                if (QueryDisplayConfig(QDC_ONLY_ACTIVE_PATHS, ref pc, paths, ref mc, modes, IntPtr.Zero) != 0) return result;

                for (int i = 0; i < pc; i++)
                {
                    var src = new SourceName
                    {
                        Type = 1,
                        Size = (uint)Marshal.SizeOf<SourceName>(),
                        AdapterId = paths[i].SourceAdapter,
                        Id = paths[i].SourceId
                    };
                    var tgt = new TargetName
                    {
                        Type = 2,
                        Size = (uint)Marshal.SizeOf<TargetName>(),
                        AdapterId = paths[i].TargetAdapter,
                        Id = paths[i].TargetId
                    };
                    if (DisplayConfigGetDeviceInfo(ref src) != 0) continue;
                    if (DisplayConfigGetDeviceInfo(ref tgt) != 0) continue;
                    if (!string.IsNullOrWhiteSpace(tgt.FriendlyName))
                        result[src.GdiName] = tgt.FriendlyName;
                }
            }
            catch { }
            return result;
        }
    }
}
