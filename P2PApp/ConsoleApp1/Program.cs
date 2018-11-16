using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PApp
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
            => new StartUp(args).Run();
    }
}
