// FTClientProgram.cs
//
// Pete Myers
// CST 415
// Fall 2019
// 

using System;
using PRSLib;

namespace FTClient
{
    class FTClientProgram
    {
        private static void Usage()
        {
            /*
                -prs <PRS IP address>:<PRS port>
                -s <file transfer server IP address>
                -d <directory requested>
            */
            Console.WriteLine("Usage: FTClient -d <directory> [-prs <PRS IP>:<PRS port>] [-s <FT Server IP>]");
        }

        static void Main(string[] args)
        {
            // defaults
            string PRSSERVER_IPADDRESS = "127.0.0.1";
            ushort PSRSERVER_PORT = 30000;
            string FTSERVICE_NAME = "FT Server";
            string FTSERVER_IPADDRESS = "127.0.0.1";
            ushort FTSERVER_PORT = 40000;
            string DIRECTORY_NAME = null;

            // process the command line arguments
            

            try
            {
                for (int i = 0; i < args.Length; i++)
                {
                    if(args[i] == "-d")
                    {
                        DIRECTORY_NAME = args[++i];
                    }
                }

                if (DIRECTORY_NAME == null)
                {
                    throw new Exception("FTClientProgram: Directory name is null");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("FTClientProgram.Main() - Error: " + ex.Message);
                Usage();
                return;
            }


            Console.WriteLine("PRS Address: " + PRSSERVER_IPADDRESS);
            Console.WriteLine("PRS Port: " + PSRSERVER_PORT);
            Console.WriteLine("FT Server Address: " + FTSERVER_IPADDRESS);
            Console.WriteLine("Directory: " + DIRECTORY_NAME);

            try
            {
                // contact the PRS and lookup port for "FT Server"
                PRSClient prs = new PRSClient(PRSSERVER_IPADDRESS, PSRSERVER_PORT, FTSERVICE_NAME);
                FTSERVER_PORT = prs.LookupPort();

                // create an FTClient and connect it to the server
                FTClient ft = new FTClient(FTSERVER_IPADDRESS, FTSERVER_PORT);
                ft.Connect();

                // get the contents of the specified directory
                ft.GetDirectory(DIRECTORY_NAME);

                // disconnect from the server
                ft.Disconnect();
                
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error " + ex.Message);
                Console.WriteLine(ex.StackTrace);
            }

            // wait for a keypress from the user before closing the console window
            Console.WriteLine("Press Enter to exit");
            Console.ReadKey();
        }
    }
}
