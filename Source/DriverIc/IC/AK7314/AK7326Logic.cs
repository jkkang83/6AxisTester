using FZ4P.Helper;

namespace FZ4P
{
    public class AK7326Logic : AK73XXBase
    {
        public AK7326Logic(Process process, DLN dln, int afSlaveAddr, int xSlaveAddr, int y1SlaveAddr, int y2SlaveAddr, int fraBourdAddress) : 
            base(process, dln, afSlaveAddr, xSlaveAddr, y1SlaveAddr, y2SlaveAddr, fraBourdAddress)
        {
        }

        public void AK7326_IC_Mode(int ch, int axis, byte mode)
        {
            byte option = 0, index;
            if (mode == 0) option = 0x40;
            else if (mode == 1) option = 0x00;
            else if (mode == 2) option = 0x40;
            else if (mode == 3) option = 0x00;
            int slaveaddr = axis == 0 ? XSlaveAddr : Y1SlaveAddr;
            string AxisStr = axis == 0 ? "OIS X" : "OIS Y";
            string modeStr = mode == 0 ? "Standby mode" : "Active mode";
            if (mode == 0 || mode == 1)
            {
                _dln.WriteArray(ch, slaveaddr, 0x02, new byte[] { option });
                _process.AddLog(ch, $"{AxisStr} {modeStr}");
            }
            else
            {
                _dln.WriteArray(ch, XSlaveAddr, 0x02, new byte[] { option });
                _dln.WriteArray(ch, Y1SlaveAddr, 0x02, new byte[] { option });
            }
            if (mode == 2) _process.AddLog(ch, "OIS Standby mode");
            if (mode == 3) _process.AddLog(ch, "OIS Active mode");
        }
        public void AK7326_IC_Data(int ch)
        {
            byte PIDVer, ProductID;
            int[] data = new int[2];

            byte[] rbuf = new byte[1];
            byte[] rbuf2 = new byte[2];
            _process.AddLog(ch, "=============== AK7326 IC Data ===============");
            for (int i = 0; i < 2; i++)
            {
                int slaveAddr = i == 0 ? XSlaveAddr : Y1SlaveAddr;
                AK7326_check_byte(ch, i, 0x00, 0x0F);
                AK7326_check_byte(ch, i, 0x10, 0x1F);
                AK7326_check_byte(ch, i, 0x20, 0x2F);
                AK7326_check_byte(ch, i, 0x30, 0x3F);
                AK7326_check_byte(ch, i, 0xE0, 0xEF);
                AK7326_check_byte(ch, i, 0xF0, 0xFF);

                _dln.ReadArray(ch, slaveAddr, 0x04, rbuf2);
                data[0] = ((rbuf2[0] << 8) + rbuf2[1]) >> 4;
                _dln.ReadArray(ch, slaveAddr, 0x06, rbuf2);
                data[1] = ((rbuf2[0] << 8) + rbuf2[1]) >> 4;
                _process.AddLog(ch, $"PCal : {data[0]}, Ncal : {data[1]}");
            }
        }

        public void AK7326_IC_reset(int ch)
        {
            Move(ch, "X", 2048);
            Move(ch, "Y", 2048);
            OISOn(ch, "X", true);
            OISOn(ch, "Y", true);
        }

        public bool AK7326_memory_update(int ch, byte dir, int mode)
        {
            int index = 0;
            byte[] MemoryUpdataeAddr = new byte[] { 0x00, 0x01, 0x02, 0x04, 0x08, 0x10 };
            int[] MemoryUpdataeTime = new int[] { 0, 160, 270, 160, 100, 60 };
            int slaveaddr = dir == 0 ? XSlaveAddr : Y1SlaveAddr;
            bool res = false;
            byte val = 0;
            byte[] rbuf = new byte[1];
            switch (mode)
            {
                case 0:
                    for (index = 0; index < 5; index++)
                    {
                        _dln.WriteArray(ch, slaveaddr, 0x03, new byte[] { MemoryUpdataeAddr[index + 1] });
                        ProcessHelper.Wait(MemoryUpdataeTime[index]);
                    }
                    for (index = 0; index < 5; index++)
                    {
                        _dln.ReadArray(ch, slaveaddr, 0x4B, rbuf);
                        val = (byte)(rbuf[0] & 0x04);

                        if (val == 0x00)
                            break;
                    }
                    if ((index > 4))
                    {
                        _process.AddLog(ch, $"-- memory update NG (%c) -- {dir}");

                        return false;
                    }

                    break;
                case 1:
                    _dln.WriteArray(ch, slaveaddr, 0x03, new byte[] { MemoryUpdataeAddr[5] });
                    ProcessHelper.Wait(MemoryUpdataeTime[5]);
                    break;
                default:
                    break;
            }
            return true;
        }
        public void AK7326_PM_set_slave(int ch, int axis)
        {
            _dln.WriteArray(ch, _FRA.Addr, 0x00, new byte[] { 0x01 });
            _dln.WriteArray(ch, _FRA.Addr, 0x00, new byte[] { 0x00 });
            if (axis == 0) _dln.WriteArray(ch, _FRA.Addr, 0x6F, new byte[] { (byte)_FRA.SlaveAddress.XSlaveAddr});
            else if (axis == 1) _dln.WriteArray(ch, _FRA.Addr, 0x6F, new byte[] { (byte)_FRA.SlaveAddress.Y1SlaveAddr });
            else
            {
                _dln.WriteArray(ch, _FRA.Addr, 0x6F, new byte[] { (byte)_FRA.SlaveAddress.XSlaveAddr });
                _dln.WriteArray(ch, _FRA.Addr, 0x89, new byte[] { (byte)_FRA.SlaveAddress.Y1SlaveAddr });
            }
        }

        public void AK7326_EEPROM_Writecheck(int ch, byte dir, byte address, byte value)
        {
            byte[] rbuf = new byte[1];
            byte data = 0;
            int slave = dir == 0 ? XSlaveAddr : Y1SlaveAddr;
            while (true)
            {
                _dln.WriteArray(ch, slave, 0xAE, new byte[] { 0x3B });
                _dln.WriteArray(ch, slave, address, new byte[] { value });
                ProcessHelper.Wait(30);

                data++;
                _dln.ReadArray(ch, slave, 0x4B, rbuf);
                if ((rbuf[0] & 0x04) == 0x00)
                    break;
                if (data > 5)
                    break;
            }
            _dln.WriteArray(ch, slave, 0xAE, new byte[] { 0x00 });
        }

        #region private 메서드
        private void AK7326_check_byte(int ch, int axis, byte start, byte end)
        {
            int addr = 0; int index = 0;
            string s = string.Empty;
            byte[] rbuf = new byte[1];
            int slaveaddr = axis == 0 ? XSlaveAddr : Y1SlaveAddr;
            s += $"0x{start.ToString("X2")}~0x{end.ToString("X2")} : ";

            for (addr = start, index = 0; addr <= end; addr++, index++)
            {
                _dln.ReadArray(ch, slaveaddr, addr, rbuf);
                if ((index & 0x0003) == 0x0000)
                    s += " ";
                s += rbuf[0].ToString("X2");

            }
            _process.AddLog(ch, s);
        }
        #endregion
    }
}
