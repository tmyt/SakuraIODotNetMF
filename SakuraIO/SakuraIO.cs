using System;
using System.Threading;

namespace SakuraIO
{
    public class SakuraIO
    {
        protected virtual void Begin()
        {
        }

        protected virtual void End()
        {
        }

        protected virtual void SendByte(byte data)
        {
        }

        protected virtual byte StartReceive(byte length)
        {
            return length;
        }

        protected virtual byte ReceiveByte()
        {
            return 0x00;
        }

        protected virtual byte ReceiveByte(bool stop)
        {
            return 0x00;
        }

        protected byte ExecuteCommand(byte cmd, byte[] request, byte responseLength,
            out byte[] response)
        {
            Debug.dbgln("executeCommand");

            Begin();

            // request
            SendByte(cmd);
            SendByte((byte)request.Length);
            var parity = (byte)(cmd ^ (byte)request.Length);
            for (var i = 0; i < (byte)request.Length; i++)
            {
                parity ^= request[i];
                SendByte(request[i]);
            }

            SendByte(parity);

            var reservedResponseLength = responseLength;

            Thread.Sleep(10);

            // response
            StartReceive((byte)(reservedResponseLength + 3));
            var result = ReceiveByte();
            if (result != Commands.CMD_ERROR_NONE)
            {
                Debug.dbgln("Invalid status");
                End();
                response = new byte[0];
                return result;
            }

            var receivedResponseLength = ReceiveByte();
            response = new byte[receivedResponseLength];

            parity = (byte)(result ^ receivedResponseLength);
            for (var i = 0; i < receivedResponseLength; i++)
            {
                var tmpResponse = ReceiveByte();
                parity ^= tmpResponse;
                if (i < reservedResponseLength)
                {
                    response[i] = tmpResponse;
                }
            }

            Debug.dbgln("Parity");
            byte p = ReceiveByte(true);
            parity ^= p;
            Debug.dbg("Parity=");
            Debug.dbgln(p.ToString());
            if (parity != 0x00)
            {
                result = Commands.CMD_ERROR_PARITY;
                Debug.dbgln("Invalid parity");
            }
            else
            {
                Debug.dbgln("Success");
            }

            End();
            return result;
        }


        protected byte ExecuteCommand(byte cmd)
        {
            return ExecuteCommand(cmd, new byte[] { });
        }

        protected byte ExecuteCommand(byte cmd, byte request)
        {
            return ExecuteCommand(cmd, new[] {request});
        }

        protected byte ExecuteCommand(byte cmd, byte[] request)
        {
            byte[] response;
            return ExecuteCommand(cmd, request, 0, out response);
        }

        protected byte ExecuteCommand(byte cmd, byte responseLength, out byte[] response)
        {
            return ExecuteCommand(cmd, new byte[] { }, responseLength, out response);
        }

        protected byte ExecuteTxCommand(byte cmd, byte ch, char type, byte[] data, ulong offset)
        {
            byte[] request = new byte[10];
            request[0] = ch;
            request[1] = (byte)type;
            Array.Copy(data, 0, request, 2, Math.Min(data.Length, 8));
            if (offset != 0)
            {
                var newReq = new byte[18];
                Array.Copy(request, newReq, request.Length);
                Array.Copy(BitConverter.GetBytes(offset), 0, newReq, 10, 8);
                request = newReq;
            }

            return ExecuteCommand(cmd, request);
        }

        protected byte ExecuteRxCommand(byte cmd, out byte ch, out char type, out byte[] value, out ulong offset)
        {
            byte[] response;
            byte ret = ExecuteCommand(cmd, 18, out response);
            if (ret != Commands.CMD_ERROR_NONE)
            {
                ch = 0;
                type = ' ';
                value = new byte[8];
                offset = 0;
                return ret;
            }

            ch = response[0];
            type = (char)response[1];
            value = new byte[8];
            Array.Copy(response, 2, value, 0, 8);
            offset = BitConverter.ToUInt64(response, 10);
            return ret;
        }

        #region Common Commands

        public byte GetConnectionStatus()
        {
            byte[] response;
            if (ExecuteCommand(Commands.CMD_GET_CONNECTION_STATUS, 1, out response) != Commands.CMD_ERROR_NONE)
            {
                return 0x7F;
            }

            return response[0];
        }

        [Obsolete]
        public byte GetSignalQuarity()
        {

            return GetSignalQuality();
        }

        public byte GetSignalQuality()

        {
            byte[] response;
            if (ExecuteCommand(Commands.CMD_GET_SIGNAL_QUALITY, 1, out response) != Commands.CMD_ERROR_NONE)
            {
                return 0x00;
            }

            return response[0];
        }

        public ulong GetUnixtime()
        {
            byte[] response;
            if (ExecuteCommand(Commands.CMD_GET_DATETIME, 8, out response) != Commands.CMD_ERROR_NONE)
            {
                return 0x00;
            }

            return BitConverter.ToUInt64(response, 0);
        }

        public byte Echoback(byte[] data, out byte[] response)
        {
            if (ExecuteCommand(Commands.CMD_ECHO_BACK, data, (byte)data.Length, out response) !=
                Commands.CMD_ERROR_NONE)
            {
                return 0x00;
            }

            return (byte)response.Length;
        }

        #endregion

        #region IO Commands

        [Obsolete]
        public ushort GetADC(byte channel)
        {
            byte[] request = {channel}, response;
            if (ExecuteCommand(Commands.CMD_READ_ADC, request, 2, out response) != Commands.CMD_ERROR_NONE)
            {
                return 0xffff;
            }

            return BitConverter.ToUInt16(response, 0);
        }

        #endregion

        #region TX Commands

        protected byte EnqueueTxRaw(byte ch, char type, byte[] data, ulong offset)
        {
            return ExecuteTxCommand(Commands.CMD_TX_ENQUEUE, ch, type, data, offset);
        }

        public byte EnqueueTx(byte ch, int value, ulong offset)
        {
            return EnqueueTxRaw(ch, 'i', BitConverter.GetBytes(value), offset);
        }

        public byte EnqueueTx(byte ch, uint value, ulong offset)
        {
            return EnqueueTxRaw(ch, 'I', BitConverter.GetBytes(value), offset);
        }

        public byte EnqueueTx(byte ch, long value, ulong offset)
        {
            return EnqueueTxRaw(ch, 'l', BitConverter.GetBytes(value), offset);
        }

        public byte EnqueueTx(byte ch, ulong value, ulong offset)
        {
            return EnqueueTxRaw(ch, 'L', BitConverter.GetBytes(value), offset);
        }

        public byte EnqueueTx(byte ch, float value, ulong offset)
        {
            return EnqueueTxRaw(ch, 'f', BitConverter.GetBytes(value), offset);
        }

        public byte EnqueueTx(byte ch, double value, ulong offset)
        {
            return EnqueueTxRaw(ch, 'd', BitConverter.GetBytes(value), offset);
        }

        public byte EnqueueTx(byte ch, byte[] value, ulong offset)
        {
            return EnqueueTxRaw(ch, 'b', value, offset);
        }

        public byte EnqueueTx(byte ch, int value)
        {
            return EnqueueTx(ch, value, 0);
        }

        public byte EnqueueTx(byte ch, uint value)
        {
            return EnqueueTx(ch, value, 0);
        }

        public byte EnqueueTx(byte ch, long value)
        {
            return EnqueueTx(ch, value, 0);
        }

        public byte EnqueueTx(byte ch, ulong value)
        {
            return EnqueueTx(ch, value, 0);
        }

        public byte EnqueueTx(byte ch, float value)
        {
            return EnqueueTx(ch, value, 0);
        }

        public byte EnqueueTx(byte ch, double value)
        {
            return EnqueueTx(ch, value, 0);
        }

        public byte EnqueueTx(byte ch, byte[] value)
        {
            return EnqueueTx(ch, value, 0);
        }

        protected byte SendImmediatelyRaw(byte ch, char type, byte[] data, ulong offset)
        {
            return ExecuteTxCommand(Commands.CMD_TX_SENDIMMED, ch, type, data, offset);
        }

        public byte SendImmediately(byte ch, int value, ulong offset)
        {
            return SendImmediatelyRaw(ch, 'i', BitConverter.GetBytes(value), offset);
        }

        public byte SendImmediately(byte ch, uint value, ulong offset)
        {
            return SendImmediatelyRaw(ch, 'I', BitConverter.GetBytes(value), offset);
        }

        public byte SendImmediately(byte ch, long value, ulong offset)
        {
            return SendImmediatelyRaw(ch, 'l', BitConverter.GetBytes(value), offset);
        }

        public byte SendImmediately(byte ch, ulong value, ulong offset)
        {
            return SendImmediatelyRaw(ch, 'L', BitConverter.GetBytes(value), offset);
        }

        public byte SendImmediately(byte ch, float value, ulong offset)
        {
            return SendImmediatelyRaw(ch, 'f', BitConverter.GetBytes(value), offset);
        }

        public byte SendImmediately(byte ch, double value, ulong offset)
        {
            return SendImmediatelyRaw(ch, 'd', BitConverter.GetBytes(value), offset);
        }

        public byte SendImmediately(byte ch, byte[] value, ulong offset)
        {
            return SendImmediatelyRaw(ch, 'b', value, offset);
        }

        public byte SendImmediately(byte ch, int value)
        {
            return SendImmediately(ch, value, 0);
        }

        public byte SendImmediately(byte ch, uint value)
        {
            return SendImmediately(ch, value, 0);
        }

        public byte SendImmediately(byte ch, long value)
        {
            return SendImmediately(ch, value, 0);
        }

        public byte SendImmediately(byte ch, ulong value)
        {
            return SendImmediately(ch, value, 0);
        }

        public byte SendImmediately(byte ch, float value)
        {
            return SendImmediately(ch, value, 0);
        }

        public byte SendImmediately(byte ch, double value)
        {
            return SendImmediately(ch, value, 0);
        }

        public byte SendImmediately(byte ch, byte[] value)
        {
            return SendImmediately(ch, value, 0);
        }

        public byte GetTxQueueLength(out byte available, out byte queued)
        {
            byte[] response;
            var ret = ExecuteCommand(Commands.CMD_TX_LENGTH, 2, out response);
            available = response[0];
            queued = response[1];
            return ret;
        }

        public byte ClearTx()
        {
            return ExecuteCommand(Commands.CMD_TX_CLEAR);
        }

        public byte GetTxStatus(out byte queue, out byte immediate)
        {
            byte[] response;
            var ret = ExecuteCommand(Commands.CMD_TX_STAT, 2, out response);
            queue = response[0];
            immediate = response[1];
            return ret;
        }

        public byte Send()
        {
            return ExecuteCommand(Commands.CMD_TX_SEND);
        }

        #endregion

        #region RX Commands

        public byte DequeueRx(out byte ch, out char type, out byte[] value, out ulong offset)
        {
            return ExecuteRxCommand(Commands.CMD_RX_DEQUEUE, out ch, out type, out value, out offset);
        }

        public byte PeekRx(out byte ch, out char type, out byte[] value, out ulong offset)
        {
            return ExecuteRxCommand(Commands.CMD_RX_PEEK, out ch, out type, out value, out offset);
        }

        public byte GetRxQueueLength(out byte available, out byte queued)
        {
            byte[] response;
            var ret = ExecuteCommand(Commands.CMD_RX_LENGTH, 2, out response);
            available = response[0];
            queued = response[1];
            return ret;
        }

        public byte ClearRx()
        {
            return ExecuteCommand(Commands.CMD_RX_CLEAR);
        }

        #endregion

        #region File command

        public byte StartFileDownload(ushort fileId)
        {
            return ExecuteCommand(Commands.CMD_START_FILE_DOWNLOAD, BitConverter.GetBytes(fileId));
        }

        public byte CancelFileDownload()
        {
            return ExecuteCommand(Commands.CMD_CANCEL_FILE_DOWNLOAD);
        }

        public byte GetFileMetaData(out byte status, out uint totalSize, out ulong timestamp, out uint crc)
        {
            byte[] response;
            var ret = ExecuteCommand(Commands.CMD_GET_FILE_METADATA, 17, out response);
            status = response[0];
            totalSize = BitConverter.ToUInt32(response, 1);
            timestamp = BitConverter.ToUInt64(response, 5);
            crc = BitConverter.ToUInt32(response, 13);
            return ret;
        }

        public byte GetFileDownloadStatus(out byte status, out uint currentSize)
        {
            byte[] response;
            var ret = ExecuteCommand(Commands.CMD_GET_FILE_DOWNLOAD_STATUS, 5, out response);
            status = response[0];
            currentSize = BitConverter.ToUInt32(response, 1);
            return ret;
        }

        public byte GetFileData(byte size, out byte[] data)
        {
            var ret = ExecuteCommand(Commands.CMD_GET_FILE_DATA, new[] {size}, size, out data);
            return ret;
        }

        #endregion

        #region Operation command

        public ushort GetProductID()
        {
            byte[] response;
            var ret = ExecuteCommand(Commands.CMD_GET_PRODUCT_ID, 2, out response);
            if (ret != Commands.CMD_ERROR_NONE)
            {
                return 0x00;
            }

            return BitConverter.ToUInt16(response, 0);
        }

        public byte GetUniqueID(out string data)
        {
            byte[] response;
            byte ret = ExecuteCommand(Commands.CMD_GET_UNIQUE_ID, 10, out response);
            if (ret != Commands.CMD_ERROR_NONE)
            {
                data = "";
                return ret;
            }

            data = BitConverter.ToString(response);
            return ret;
        }

        public byte GetFirmwareVersion(out string data)
        {
            byte[] response;
            byte ret = ExecuteCommand(Commands.CMD_GET_FIRMWARE_VERSION, 32, out response);
            if (ret != Commands.CMD_ERROR_NONE)
            {
                data = "";
                return ret;
            }

            data = BitConverter.ToString(response);
            return ret;
        }

        public byte Unlock()
        {
            byte[] request = {0x53, 0x6B, 0x72, 0x61};
            return ExecuteCommand(Commands.CMD_UNLOCK, request);
        }

        public byte UpdateFirmware()
        {
            return ExecuteCommand(Commands.CMD_UPDATE_FIRMWARE);
        }

        public byte GetFirmwareUpdateStatus()
        {
            byte[] response;
            if (ExecuteCommand(Commands.CMD_GET_UPDATE_FIRMWARE_STATUS, 1, out response) != Commands.CMD_ERROR_NONE)
            {
                return 0xff;
            }

            return response[0];
        }

        public byte Reset()
        {
            return ExecuteCommand(Commands.CMD_SOFTWARE_RESET);
        }

        public byte SetPowerSaveMode(byte mode)
        {
            return ExecuteCommand(Commands.CMD_SET_POWER_SAVE_MODE, mode);
        }

        public byte GetPowerSaveMode()
        {
            byte[] response;
            if (ExecuteCommand(Commands.CMD_GET_POWER_SAVE_MODE, 1, out response) != Commands.CMD_ERROR_NONE)
            {
                return 0xff;
            }

            return response[0];
        }

        #endregion
    }
}