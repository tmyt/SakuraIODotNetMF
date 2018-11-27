namespace SakuraIO
{
    public static class Commands
    {
        // Common
        public const byte CMD_GET_CONNECTION_STATUS = 0x01;  // OK
        public const byte CMD_GET_SIGNAL_QUALITY = 0x02;  // OK
        public const byte CMD_GET_DATETIME = 0x03;  // OK
        public const byte CMD_ECHO_BACK = 0x0f;  // OK

        // IO
        public const byte CMD_READ_ADC = 0x10;  // OK

        // Transmit
        public const byte CMD_TX_ENQUEUE = 0x20;  // OK
        public const byte CMD_TX_SENDIMMED = 0x21;  // OK
        public const byte CMD_TX_LENGTH = 0x22;  // OK
        public const byte CMD_TX_CLEAR = 0x23;  // OK
        public const byte CMD_TX_SEND = 0x24;  // OK
        public const byte CMD_TX_STAT = 0x25;  // OK

        // Receive
        public const byte CMD_RX_DEQUEUE = 0x30;  // OK
        public const byte CMD_RX_PEEK = 0x31;  // OK
        public const byte CMD_RX_LENGTH = 0x32;  // OK
        public const byte CMD_RX_CLEAR = 0x33;  // OK

        // File Download
        public const byte CMD_START_FILE_DOWNLOAD = 0x40;  // OK
        public const byte CMD_GET_FILE_METADATA = 0x41;  // OK
        public const byte CMD_GET_FILE_DOWNLOAD_STATUS = 0x42;  // OK
        public const byte CMD_CANCEL_FILE_DOWNLOAD = 0x43;  // OK
        public const byte CMD_GET_FILE_DATA = 0x44;  // OK

        // Operation
        public const byte CMD_GET_PRODUCT_ID = 0xA0;  // OK
        public const byte CMD_GET_UNIQUE_ID = 0xA1;  // OK
        public const byte CMD_GET_FIRMWARE_VERSION = 0xA2;  // OK
        public const byte CMD_UNLOCK = 0xA8;  // OK
        public const byte CMD_UPDATE_FIRMWARE = 0xA9;  // OK
        public const byte CMD_GET_UPDATE_FIRMWARE_STATUS = 0xAA;  // OK
        public const byte CMD_SOFTWARE_RESET = 0xAF;  // OK

        // Power Save
        public const byte CMD_SET_POWER_SAVE_MODE = 0xB0;
        public const byte CMD_GET_POWER_SAVE_MODE = 0xB1;

        public const byte POWER_SAVE_MODE_DISABLE = 0x00;
        public const byte POWER_SAVE_MODE_AUTO_SLEEP = 0x01;
        public const byte POWER_SAVE_MODE_RF_OFF = 0x02;

        // Response
        public const byte CMD_ERROR_NONE = 0x01;
        public const byte CMD_ERROR_PARITY = 0x02;
        public const byte CMD_ERROR_MISSING = 0x03;
        public const byte CMD_ERROR_INVALID_SYNTAX = 0x04;
        public const byte CMD_ERROR_RUNTIME = 0x05;
        public const byte CMD_ERROR_LOCKED = 0x06;
        public const byte CMD_ERROR_BUSY = 0x07;


        // FileStatus
        public const byte FILE_STATUS_ERROR = 0x01;
        public const byte FILE_STATUS_INVALID_REQUEST = 0x02;
        public const byte FILE_STATUS_NOTFOUND = 0x81;
        public const byte FILE_STATUS_SERVER_ERROR = 0x82;
        public const byte FILE_STATUS_INVALID_DATA = 0x83;
    }
}
