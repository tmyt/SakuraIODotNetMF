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

        protected Error ExecuteCommand(Command cmd, byte[] request, byte responseLength,
            out byte[] response)
        {
            Debug.dbgln("executeCommand");

            Begin();

            // request
            SendByte((byte)cmd);
            SendByte((byte)request.Length);
            var parity = (byte)((byte)cmd ^ (byte)request.Length);
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
            var result = (Error)ReceiveByte();
            if (result != Error.None)
            {
                Debug.dbgln("Invalid status");
                End();
                response = new byte[0];
                return result;
            }

            var receivedResponseLength = ReceiveByte();
            response = new byte[receivedResponseLength];

            parity = (byte)((byte)result ^ receivedResponseLength);
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
                result = Error.Parity;
                Debug.dbgln("Invalid parity");
            }
            else
            {
                Debug.dbgln("Success");
            }

            End();
            return result;
        }


        protected Error ExecuteCommand(Command cmd)
        {
            return ExecuteCommand(cmd, new byte[] { });
        }

        protected Error ExecuteCommand(Command cmd, byte request)
        {
            return ExecuteCommand(cmd, new[] {request});
        }

        protected Error ExecuteCommand(Command cmd, byte[] request)
        {
            byte[] response;
            return ExecuteCommand(cmd, request, 0, out response);
        }

        protected Error ExecuteCommand(Command cmd, byte responseLength, out byte[] response)
        {
            return ExecuteCommand(cmd, new byte[] { }, responseLength, out response);
        }

        protected Error ExecuteTxCommand(Command cmd, byte ch, char type, byte[] data, ulong offset)
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

        protected Error ExecuteRxCommand(Command cmd, out byte ch, out char type, out byte[] value, out ulong offset)
        {
            byte[] response;
            var ret = ExecuteCommand(cmd, 18, out response);
            if (ret != Error.None)
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
            if (ExecuteCommand(Command.GetConnectionStatus, 1, out response) != Error.None)
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
            if (ExecuteCommand(Command.GetSignalQuality, 1, out response) != Error.None)
            {
                return 0x00;
            }

            return response[0];
        }

        public ulong GetUnixtime()
        {
            byte[] response;
            if (ExecuteCommand(Command.GetDatetime, 8, out response) != Error.None)
            {
                return 0x00;
            }

            return BitConverter.ToUInt64(response, 0);
        }

        public byte Echoback(byte[] data, out byte[] response)
        {
            if (ExecuteCommand(Command.EchoBack, data, (byte)data.Length, out response) !=
                Error.None)
            {
                return 0x00;
            }

            return (byte)response.Length;
        }

        #endregion

        #region IO Commands

        [Obsolete]
        public ushort GetAdc(byte channel)
        {
            byte[] request = {channel}, response;
            if (ExecuteCommand(Command.ReadAdc, request, 2, out response) != Error.None)
            {
                return 0xffff;
            }

            return BitConverter.ToUInt16(response, 0);
        }

        #endregion

        #region TX Commands

        protected Error EnqueueTxRaw(byte ch, char type, byte[] data, ulong offset)
        {
            return ExecuteTxCommand(Command.TxEnqueue, ch, type, data, offset);
        }

        public Error EnqueueTx(byte ch, int value, ulong offset)
        {
            return EnqueueTxRaw(ch, 'i', BitConverter.GetBytes(value), offset);
        }

        public Error EnqueueTx(byte ch, uint value, ulong offset)
        {
            return EnqueueTxRaw(ch, 'I', BitConverter.GetBytes(value), offset);
        }

        public Error EnqueueTx(byte ch, long value, ulong offset)
        {
            return EnqueueTxRaw(ch, 'l', BitConverter.GetBytes(value), offset);
        }

        public Error EnqueueTx(byte ch, ulong value, ulong offset)
        {
            return EnqueueTxRaw(ch, 'L', BitConverter.GetBytes(value), offset);
        }

        public Error EnqueueTx(byte ch, float value, ulong offset)
        {
            return EnqueueTxRaw(ch, 'f', BitConverter.GetBytes(value), offset);
        }

        public Error EnqueueTx(byte ch, double value, ulong offset)
        {
            return EnqueueTxRaw(ch, 'd', BitConverter.GetBytes(value), offset);
        }

        public Error EnqueueTx(byte ch, byte[] value, ulong offset)
        {
            return EnqueueTxRaw(ch, 'b', value, offset);
        }

        public Error EnqueueTx(byte ch, int value)
        {
            return EnqueueTx(ch, value, 0);
        }

        public Error EnqueueTx(byte ch, uint value)
        {
            return EnqueueTx(ch, value, 0);
        }

        public Error EnqueueTx(byte ch, long value)
        {
            return EnqueueTx(ch, value, 0);
        }

        public Error EnqueueTx(byte ch, ulong value)
        {
            return EnqueueTx(ch, value, 0);
        }

        public Error EnqueueTx(byte ch, float value)
        {
            return EnqueueTx(ch, value, 0);
        }

        public Error EnqueueTx(byte ch, double value)
        {
            return EnqueueTx(ch, value, 0);
        }

        public Error EnqueueTx(byte ch, byte[] value)
        {
            return EnqueueTx(ch, value, 0);
        }

        protected Error SendImmediatelyRaw(byte ch, char type, byte[] data, ulong offset)
        {
            return ExecuteTxCommand(Command.TxSendImmediately, ch, type, data, offset);
        }

        public Error SendImmediately(byte ch, int value, ulong offset)
        {
            return SendImmediatelyRaw(ch, 'i', BitConverter.GetBytes(value), offset);
        }

        public Error SendImmediately(byte ch, uint value, ulong offset)
        {
            return SendImmediatelyRaw(ch, 'I', BitConverter.GetBytes(value), offset);
        }

        public Error SendImmediately(byte ch, long value, ulong offset)
        {
            return SendImmediatelyRaw(ch, 'l', BitConverter.GetBytes(value), offset);
        }

        public Error SendImmediately(byte ch, ulong value, ulong offset)
        {
            return SendImmediatelyRaw(ch, 'L', BitConverter.GetBytes(value), offset);
        }

        public Error SendImmediately(byte ch, float value, ulong offset)
        {
            return SendImmediatelyRaw(ch, 'f', BitConverter.GetBytes(value), offset);
        }

        public Error SendImmediately(byte ch, double value, ulong offset)
        {
            return SendImmediatelyRaw(ch, 'd', BitConverter.GetBytes(value), offset);
        }

        public Error SendImmediately(byte ch, byte[] value, ulong offset)
        {
            return SendImmediatelyRaw(ch, 'b', value, offset);
        }

        public Error SendImmediately(byte ch, int value)
        {
            return SendImmediately(ch, value, 0);
        }

        public Error SendImmediately(byte ch, uint value)
        {
            return SendImmediately(ch, value, 0);
        }

        public Error SendImmediately(byte ch, long value)
        {
            return SendImmediately(ch, value, 0);
        }

        public Error SendImmediately(byte ch, ulong value)
        {
            return SendImmediately(ch, value, 0);
        }

        public Error SendImmediately(byte ch, float value)
        {
            return SendImmediately(ch, value, 0);
        }

        public Error SendImmediately(byte ch, double value)
        {
            return SendImmediately(ch, value, 0);
        }

        public Error SendImmediately(byte ch, byte[] value)
        {
            return SendImmediately(ch, value, 0);
        }

        public Error GetTxQueueLength(out byte available, out byte queued)
        {
            byte[] response;
            var ret = ExecuteCommand(Command.TxLength, 2, out response);
            available = response[0];
            queued = response[1];
            return ret;
        }

        public Error ClearTx()
        {
            return ExecuteCommand(Command.TxClear);
        }

        public Error GetTxStatus(out byte queue, out byte immediate)
        {
            byte[] response;
            var ret = ExecuteCommand(Command.TxStat, 2, out response);
            queue = response[0];
            immediate = response[1];
            return ret;
        }

        public Error Send()
        {
            return ExecuteCommand(Command.TxSend);
        }

        #endregion

        #region RX Commands

        public Error DequeueRx(out byte ch, out char type, out byte[] value, out ulong offset)
        {
            return ExecuteRxCommand(Command.RxDequeue, out ch, out type, out value, out offset);
        }

        public Error PeekRx(out byte ch, out char type, out byte[] value, out ulong offset)
        {
            return ExecuteRxCommand(Command.RxPeek, out ch, out type, out value, out offset);
        }

        public Error GetRxQueueLength(out byte available, out byte queued)
        {
            byte[] response;
            var ret = ExecuteCommand(Command.RxLength, 2, out response);
            available = response[0];
            queued = response[1];
            return ret;
        }

        public Error ClearRx()
        {
            return ExecuteCommand(Command.RxClear);
        }

        #endregion

        #region File command

        public Error StartFileDownload(ushort fileId)
        {
            return ExecuteCommand(Command.StartFileDownload, BitConverter.GetBytes(fileId));
        }

        public Error CancelFileDownload()
        {
            return ExecuteCommand(Command.CancelFileDownload);
        }

        public Error GetFileMetaData(out FileStatus status, out uint totalSize, out ulong timestamp, out uint crc)
        {
            byte[] response;
            var ret = ExecuteCommand(Command.GetFileMetadata, 17, out response);
            status = (FileStatus)response[0];
            totalSize = BitConverter.ToUInt32(response, 1);
            timestamp = BitConverter.ToUInt64(response, 5);
            crc = BitConverter.ToUInt32(response, 13);
            return ret;
        }

        public Error GetFileDownloadStatus(out FileStatus status, out uint currentSize)
        {
            byte[] response;
            var ret = ExecuteCommand(Command.GetFileDownloadStatus, 5, out response);
            status = (FileStatus)response[0];
            currentSize = BitConverter.ToUInt32(response, 1);
            return ret;
        }

        public Error GetFileData(byte size, out byte[] data)
        {
            var ret = ExecuteCommand(Command.GetFileData, new[] {size}, size, out data);
            return ret;
        }

        #endregion

        #region Operation command

        public ushort GetProductId()
        {
            byte[] response;
            var ret = ExecuteCommand(Command.GetProductId, 2, out response);
            if (ret != Error.None)
            {
                return 0x00;
            }

            return BitConverter.ToUInt16(response, 0);
        }

        public Error GetUniqueId(out string data)
        {
            byte[] response;
            var ret = ExecuteCommand(Command.GetUniqueId, 10, out response);
            if (ret != Error.None)
            {
                data = "";
                return ret;
            }

            data = BitConverter.ToString(response);
            return ret;
        }

        public Error GetFirmwareVersion(out string data)
        {
            byte[] response;
            var ret = ExecuteCommand(Command.GetFirmwareVersion, 32, out response);
            if (ret != Error.None)
            {
                data = "";
                return ret;
            }

            data = BitConverter.ToString(response);
            return ret;
        }

        public Error Unlock()
        {
            byte[] request = {0x53, 0x6B, 0x72, 0x61};
            return ExecuteCommand(Command.Unlock, request);
        }

        public Error UpdateFirmware()
        {
            return ExecuteCommand(Command.UpdateFirmware);
        }

        public byte GetFirmwareUpdateStatus()
        {
            byte[] response;
            if (ExecuteCommand(Command.GetUpdateFirmwareStatus, 1, out response) != Error.None)
            {
                return 0xff;
            }

            return response[0];
        }

        public Error Reset()
        {
            return ExecuteCommand(Command.SoftwareReset);
        }

        public Error SetPowerSaveMode(PowerSaveMode mode)
        {
            return ExecuteCommand(Command.SetPowerSaveMode, (byte)mode);
        }

        public PowerSaveMode GetPowerSaveMode()
        {
            byte[] response;
            if (ExecuteCommand(Command.GetPowerSaveMode, 1, out response) != Error.None)
            {
                return PowerSaveMode.Error;
            }

            return (PowerSaveMode)response[0];
        }

        #endregion
    }
}