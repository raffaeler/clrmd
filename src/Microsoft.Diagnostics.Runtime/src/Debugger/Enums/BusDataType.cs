﻿namespace Microsoft.Diagnostics.Runtime.Interop
{
    public enum BUS_DATA_TYPE : int
    {
        ConfigurationSpaceUndefined = -1,
        Cmos,
        EisaConfiguration,
        Pos,
        CbusConfiguration,
        PCIConfiguration,
        VMEConfiguration,
        NuBusConfiguration,
        PCMCIAConfiguration,
        MPIConfiguration,
        MPSAConfiguration,
        PNPISAConfiguration,
        SgiInternalConfiguration,
        MaximumBusDataType
    }
}