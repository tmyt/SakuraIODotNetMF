using System.Threading;
using Microsoft.SPOT.Hardware;

namespace SakuraIO
{
    public class SakuraIO_SPI : SakuraIO
    {
        private SPI _spi;

        protected int cs;

        protected override void Begin()
        {
            Debug.dbgln("CS=0");
        }

        protected override void End()
        {
            Debug.dbgln("CS=1");
            Thread.Sleep(1);
        }

        protected override void SendByte(byte data)
        {
            Thread.Sleep(1);
            Debug.dbg("Send=");
            Debug.dbgln(data.ToString("x2"));
            _spi.Write(new[] { data });
        }

        protected override byte ReceiveByte(bool stop)
        {
            return ReceiveByte();
        }

        protected override byte ReceiveByte()
        {
            var ret = new byte[1];
            Thread.Sleep(1);
            _spi.WriteRead(new byte[] { 0x00 }, ret);
            Debug.dbg("Recv=");
            Debug.dbgln(ret[0].ToString("x2"));
            return ret[0];
        }

        public SakuraIO_SPI(Cpu.Pin cs, SPI.SPI_module spi_mod)
        {
            _spi = new SPI(new SPI.Configuration(cs, false, 0, 0, false, false, 350, spi_mod));
        }
    }
}
