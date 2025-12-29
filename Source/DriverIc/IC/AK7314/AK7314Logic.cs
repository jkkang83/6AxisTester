using FZ4P.Helper;
using System;

namespace FZ4P
{
    public class AK7314Logic : AK73XXBase
    {
        public AK7314Logic(Process process, DLN dln, int afSlaveAddr, int xSlaveAddr, int y1SlaveAddr, int y2SlaveAddr, int fraBourdAddress) : 
            base(process, dln, afSlaveAddr, xSlaveAddr, y1SlaveAddr, y2SlaveAddr, fraBourdAddress)
        {
        }
        #region public 메서드
        public void AK7314_Mode(int ch, byte mode)
        {
            var logic = new DLNWriteLogic(_process, _dln);
            if (mode == 1) logic.WriteArray(ch, base.AFSlaveAddr, 0x02, new byte[] { 0x00 });
            else if (mode == 2) logic.WriteArray(ch, base.AFSlaveAddr, 0x02, new byte[] { 0x10 });
            else logic.WriteArray(ch, base.AFSlaveAddr, 0x02, new byte[] { 0x40 });
        }
        public void AK7314_IC_reset(int ch)
        {
            byte[] rbuf = new byte[1];

            AK7314_Mode(ch, 0);
            ProcessHelper.Wait(50);
            AK7314_memory_update(ch, 5);
            Move(ch, "AF", 2048);
            AK7314_Mode(ch, 1);
            _dln.ReadArray(ch, base.AFSlaveAddr, 0x03, rbuf);
            _process.AddLog(ch, $"AF14 was reeet, 0x03 = {rbuf[0].ToString("X2")}");
        }
        public void AK7314_IC_Data(int ch)
        {
            int Pcal = 0, Ncal = 0, PVT = 0, NVT = 0;

            byte[] rbuf = new byte[1];
            byte[] rbuf2 = new byte[2];
            _dln.WriteArray(ch, AFSlaveAddr, 0xAE, new byte[] { 0x3B });
            AK7314_memory_update(ch, 5);
            _dln.WriteArray(ch, AFSlaveAddr, 0xAE, new byte[] { 0x00 });
            _dln.ReadArray(ch, AFSlaveAddr, 0x04, rbuf); Pcal = rbuf[0];
            _dln.ReadArray(ch, AFSlaveAddr, 0x06, rbuf); Ncal = (short)(rbuf[0] | 0xFF00);

            AK7314_check_byte(ch, 0x00, 0x0F);
            AK7314_check_byte(ch, 0x10, 0x1F);
            AK7314_check_byte(ch, 0x20, 0x2F);
            AK7314_check_byte(ch, 0x30, 0x3F);
            AK7314_check_byte(ch, 0x90, 0x99);
            AK7314_check_byte(ch, 0xC0, 0xCF);
            AK7314_check_byte(ch, 0xE0, 0xEF);
            AK7314_check_byte(ch, 0xF0, 0xFF);

            _dln.ReadArray(ch, AFSlaveAddr, 0xFB, rbuf);
            byte PIDVer = (byte)(0x0F & rbuf[0]);
            _dln.ReadArray(ch, AFSlaveAddr, 0x03, rbuf);
            byte ProductID = rbuf[0];
            _process.AddLog(ch, $" ====  AK7314 (Addr:{(AFSlaveAddr << 1).ToString("X2")}, PID Ver:{PIDVer}, Pro ID:{ProductID.ToString("X2")}) ===");
            _process.AddLog(ch, "");
            _process.AddLog(ch, $"PCal : {Pcal}, Ncal : {Ncal}");
            _process.AddLog(ch, $"PVT : {PVT}, NVT : {NVT}");
        }
        public void Ak7314_soft_move(int ch, int pos, int loop)
        {
            int i = 0;
            short soft_step, margin_code, old_code = 0, new_code = 0;
            soft_step = (short)((pos - 2048) / 50);
            margin_code = Math.Abs(soft_step);

            if (margin_code == 0) return;
            for (i = 0, new_code = 2048; i < loop; i++)
            {
                old_code = new_code;
                Move(ch, "AF", 2048); ProcessHelper.Wait(50);
                Move(ch, "AF", pos - soft_step * 10); ProcessHelper.Wait(50);
                Move(ch, "AF", pos - soft_step * 2); ProcessHelper.Wait(20);
                Move(ch, "AF", pos - soft_step * 1); ProcessHelper.Wait(20);
                Move(ch, "AF", pos - soft_step * 0); ProcessHelper.Wait(50);
                new_code = (short)(ReadHall(ch, "AF"));
                _process.AddLog(ch, $"af pos(t, c) : {pos}, {new_code}");
                if (Math.Abs((int)(pos - new_code)) <= margin_code)
                    break;
            }
        }
        public void AK7314_EEPROM_Writecheck(int ch, byte address, byte value)
        {
            byte data;
            byte[] rbuf = new byte[1];
            _dln.WriteArray(ch, AFSlaveAddr, 0xAE, new byte[] { 0x3B });
            _dln.WriteArray(ch, AFSlaveAddr, address, new byte[] { value });
            ProcessHelper.Wait(30);
            _dln.WriteArray(ch, AFSlaveAddr, 0xAE, new byte[] { 0x00 });
        }
        public bool AK7314_memory_update(int ch, byte mode)
        {
            byte value = 0, temp;
            ushort time = 0;
            byte[] check_update = new byte[1];

            switch (mode)
            {
                case 0: value = 0x00; time = 0; break;      // null, AK7314
                case 1: value = 0x01; time = 120; break;        // 1:  90 ms (PIDK,PIDU,PCAL,NCAL,SETTING1~2)
                case 2: value = 0x02; time = 270; break;        // 2:  234 ms (PIDA~PIDX			)
                case 3: value = 0x04; time = 170; break;        // 3:  108 ms (PIDAA~PIAJ			)
                case 4: value = 0x08; time = 110; break;     // 4:  AK7314C
                case 5: value = 0x10; time = 40; break;     // 5:  Mload
                default: break;
            }

            for (temp = 0; temp < 5; temp++)
            {
                _dln.WriteArray(ch, AFSlaveAddr, 0x03, new byte[] { value });
                ProcessHelper.Wait(time);

                _dln.ReadArray(ch, AFSlaveAddr, 0x4B, check_update);// AK7314_Read_byte(0x4B) & 0x04;
                if (check_update[0] == 0x00)
                    break;
            }
            if (check_update[0] != 0x00)
                return false;
            return true;
        }
        #endregion

        #region private 메서드

        /// <summary>
        /// slave address 구성은 몰라 외부에서 주입 받는식으로 일단 구현
        /// </summary>
        /// <param name="AFSlaveAddr"></param>
        /// <param name="XSlaveAddr"></param>
        /// <param name="Y1SlaveAddr"></param>
        /// <param name="Y2SlaveAddr"></param>
        /// <param name="ch"></param>
        /// <param name="name"></param>
        /// <param name="pos"></param>
        /// <param name="openLoop"></param>
        /// <returns></returns>
        private void AK7314_check_byte(int ch, byte start, byte end)
        {
            int addr = 0; int index = 0;
            string s = string.Empty;
            byte[] rbuf = new byte[1];
            s += $"0x{start.ToString("X2")}~0x{end.ToString("X2")} : ";

            for (addr = start, index = 0; addr <= end; addr++, index++)
            {
                _dln.ReadArray(ch, AFSlaveAddr, addr, rbuf);
                if ((index & 0x0003) == 0x0000)
                    s += " ";
                s += rbuf[0].ToString("X2");

            }
            _process.AddLog(ch, s);
        }
        
        #endregion
    }
}