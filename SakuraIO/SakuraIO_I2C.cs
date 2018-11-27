using System;
using Microsoft.SPOT.Hardware;

namespace SakuraIO
{
    public class SakuraIO_I2C : SakuraIO
    {
        private const byte SAKURAIO_SLAVE_ADDR = 0x4F;

        private const byte MODE_IDLE = 0x00;
        private const byte MODE_WRITE = 0x01;
        private const byte MODE_READ = 0x02;

        private I2CDevice _i2c;
        private byte[] _txbuffer;
        private byte[] _rxbuffer;
        private int _txindex;
        private int _rxindex;

        protected byte mode;

        private byte[] GetTxBuffer()
        {
            var b = new byte[_txindex];
            Array.Copy(_txbuffer, b, _txindex);
            return b;
        }

        protected override void Begin()
        {
            mode = MODE_IDLE;
        }

        protected override void End()
        {
            switch (mode)
            {
                case MODE_WRITE:
                    _i2c.Execute(new[]
                    {
                        (I2CDevice.I2CTransaction)I2CDevice.CreateWriteTransaction(GetTxBuffer())
                    }, 200);
                    break;
                case MODE_READ:
                    break;
            }

            mode = MODE_IDLE;
        }

        protected override void SendByte(byte data)
        {
            if (mode != MODE_WRITE)
            {
                Debug.dbgln("beginTr");
                _txbuffer = new byte[64];
                _txindex = 0;
                mode = MODE_WRITE;
            }
            Debug.dbg("Write=");
            Debug.dbgln(data.ToString("x2"));
            _txbuffer[_txindex++] = data;
        }

        protected override byte StartReceive(byte length)
        {
            End();
            Debug.dbg("requestForm=");
            Debug.dbgln(length.ToString());
            mode = MODE_READ;
            _rxindex = 0;
            _rxbuffer = new byte[length];
            _i2c.Execute(new[]
            {
                (I2CDevice.I2CTransaction)I2CDevice.CreateReadTransaction(_rxbuffer)
            }, 200);
            return 0;
        }

        protected override byte ReceiveByte(bool stop)
        {
            var ret = _rxbuffer[_rxindex++];
            if (stop)
            {
                mode = MODE_IDLE;
            }
            Debug.dbg("Read=");
            Debug.dbgln(ret.ToString("x2"));
            return ret;
        }

        protected override byte ReceiveByte()
        {
            return ReceiveByte(false);
        }

        public SakuraIO_I2C()
        {
            _i2c = new I2CDevice(new I2CDevice.Configuration(SAKURAIO_SLAVE_ADDR, 100));
            mode = MODE_IDLE;
        }
    }
}
