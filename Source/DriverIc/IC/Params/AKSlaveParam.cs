namespace FZ4P
{
    public class AKSlaveFRAParam
    {
        public int Addr { get; set; }
        public AKSlaveParam SlaveAddress { get; set; }
    }
    public class AKSlaveParam
    {
        public int AFSlaveAddr { get; set; }
        public int XSlaveAddr { get; set; }
        public int Y1SlaveAddr { get; set; }
        public int Y2SlaveAddr { get; set; }
    }
}
