namespace TerminalManager.Entities
{
    public class Enums
    {
        public enum TerminalType
        {
            Server = 1,
            POSServer = 2, 
            POS = 3,
            OfficeComputer = 4
        }

        public enum PosType
        {
            None = 1,
            Restaurant = 2,
            Retail = 3,
            TakeOrder = 4,
            PriceChecker = 5
        }

        public enum PosStatus
        {
            None = 1,
            HasPTU = 2
        }

        public enum CompanyAccredType
        {
            Systems = 1,
            Services = 2,
            Technology = 3,
            Others = 4
        }

        public enum MallAccredType
        {
            None = 1,
            SM = 2,
            Fishermall = 3,
            RLC = 4,
            Starmall = 5,
            Greenfield = 6
        }
    }
}
