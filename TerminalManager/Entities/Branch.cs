﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerminalManager.Entities
{
    public class Branch
    {
        public int BranchID { get; set; }
        public string BranchName { get; set; }
        public string BranchServerIP { get; set; }
        public  string  LocalNetworkIP { get; set; }
    }
}