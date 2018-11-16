using System;

namespace ViewSat
{
    class Package
    {
        public string Name { get; set; }
        public string ChkName { get; set; }
        public string FullName { get; set; }
        public int Size { get; set; }
        public string Size16 { get; set; }
        public bool Used { get; set; }

        public Package()
        {
            string fullname = ToString();
            FullName = fullname;
            ChkName = fullname.Substring(0, 3);
            Name = fullname.Substring(0, 2);
            Size16 = fullname.Substring(2, 3);
            Size = Convert.ToInt32(Size16, 16);
        }
        public Package(string name, int size)
        {
            Name = name;
            Size = size;
        }
        public Package(string fullname)
        {
            FullName = fullname;
            ChkName = fullname.Substring(0, 3);
            Name = fullname.Substring(0,2);
            Size16 = fullname.Substring(2, 3);
            Size = Convert.ToInt32(Size16, 16);
        }

        public override string ToString()
        {
            string[] temp = base.ToString().Split('.');
            return temp[temp.Length-1];
        }

    }
    /// <summary>
    /// Receiver Date
    /// </summary>
    class RD006 : Package          
    {       
        public ushort Year { get; set; }
        public byte Month { get; set; }
        public byte Day { get; set; }
        /// <summary>
        /// Receiver reference time [enumerated]
        /// 0 - GPS,
        /// 1 - UTC_USNO, 
        /// 2 - GLONASS, 
        /// 3 - UTC_SU, 
        /// 4-254 - Reserved
        /// </summary>
        public byte BaseTime { get; set; }  
        public byte Chksum { get; set; }

        public RD006() : base() { }
    };
    /// <summary>
    /// Geodesic Position
    /// </summary>
    class PG01E            
    {
        public double Fi { get; set; }
        public double Lam { get; set; }
        public double H { get; set; }
        /// <summary>
        /// Position SEP
        /// </summary>
        public float Ps { get; set; } 
        public byte Type { get; set; }
        public byte Chksum { get; set; }

        public PG01E() : base() { }
    };


}
