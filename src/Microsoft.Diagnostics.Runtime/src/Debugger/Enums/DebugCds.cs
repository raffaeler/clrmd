﻿using System;

namespace Microsoft.Diagnostics.Runtime.Interop
{
    [Flags]
    public enum DEBUG_CDS : uint
    {
        ALL = 0xffffffff,
        REGISTERS = 1,
        DATA = 2,
        REFRESH = 4, // Inform the GUI clients to refresh debugger windows.
    }
}