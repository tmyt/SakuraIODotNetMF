namespace SakuraIO
{
    public enum Command : byte
    {
        // Common
        GetConnectionStatus = 0x01, // OK
        GetSignalQuality = 0x02, // OK
        GetDatetime = 0x03, // OK
        EchoBack = 0x0f, // OK

        // IO
        ReadAdc = 0x10, // OK

        // Transmit
        TxEnqueue = 0x20, // OK
        TxSendImmediately = 0x21, // OK
        TxLength = 0x22, // OK
        TxClear = 0x23, // OK
        TxSend = 0x24, // OK
        TxStat = 0x25, // OK

        // Receive
        RxDequeue = 0x30, // OK
        RxPeek = 0x31, // OK
        RxLength = 0x32, // OK
        RxClear = 0x33, // OK

        // File Download
        StartFileDownload = 0x40, // OK
        GetFileMetadata = 0x41, // OK
        GetFileDownloadStatus = 0x42, // OK
        CancelFileDownload = 0x43, // OK
        GetFileData = 0x44, // OK

        // Operation
        GetProductId = 0xA0, // OK
        GetUniqueId = 0xA1, // OK
        GetFirmwareVersion = 0xA2, // OK
        Unlock = 0xA8, // OK
        UpdateFirmware = 0xA9, // OK
        GetUpdateFirmwareStatus = 0xAA, // OK
        SoftwareReset = 0xAF, // OK

        // Power Save
        SetPowerSaveMode = 0xB0,
        GetPowerSaveMode = 0xB1,
    }

    public enum PowerSaveMode
    {
        Disable = 0x00,
        AutoSleep = 0x01,
        RfOff = 0x02,
        Error = 0xff,
    }

    public enum Error
    {
        // Response
        None = 0x01,
        Parity = 0x02,
        Missing = 0x03,
        InvalidSyntax = 0x04,
        Runtime = 0x05,
        Locked = 0x06,
        Busy = 0x07,
    }


    public enum FileStatus
    {
        // FileStatus
        Error = 0x01,
        InvalidRequest = 0x02,
        NotFound = 0x81,
        ServerError = 0x82,
        InvalidData = 0x83,
    }
}
