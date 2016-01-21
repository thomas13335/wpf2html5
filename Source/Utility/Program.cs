using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wpf2Html5.Utility
{
    class Program
    {
        static int Main(string [] args)
        {
            try
            {
                var tool = new ConverterTool();
                return tool.Execute(args.ToSeparatorList(" "));
            }
            catch(Exception ex)
            {
                Console.WriteLine("error: " + ex.Message);
                return 2;
            }
        }
    }
}
