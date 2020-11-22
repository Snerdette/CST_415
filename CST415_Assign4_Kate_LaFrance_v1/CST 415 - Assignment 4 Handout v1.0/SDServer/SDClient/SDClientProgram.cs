using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.IO;
using SDClientLib;
using PRSLib;

namespace SDClient
{
    class SDClientProgram
    {
        private static void Usage()
        {
            /*
                -prs <PRS IP address>:<PRS port>
                -s <SD server IP address>
		        -o | -r <session id> | -c <session id>
                [-get <document> | -post <document>]
            */
            Console.WriteLine("Usage: SDClient [-prs <PRS IP>:<PRS port>] [-s <SD Server IP>]");
            Console.WriteLine("\t-o | -r <session id> | -c <session id>");
            Console.WriteLine("\t[-get <document> | -post <document>]");
        }

        static void Main(string[] args)
        {
            // defaults
            string PRSSERVER_IPADDRESS = "127.0.0.1";
            ushort PRSSERVER_PORT = 30000;
            string SDSERVICE_NAME = "SD Server";
            string SDSERVER_IPADDRESS = "127.0.0.1";
            ushort SDSERVER_PORT = 40000;
            string SESSION_CMD = null;
            ulong SESSION_ID = 0;
            string DOCUMENT_CMD = null;
            string DOCUMENT_NAME = null;

            // process the command line arguments
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-o")
                {
                    SESSION_CMD = "-o";
                }
                else if (args[i] == "-c")
                {
                    SESSION_CMD = "-c";
                    SESSION_ID = ulong.Parse(args[++i]);
                }
                else if (args[i] == "-r")
                {
                    SESSION_CMD = "-r";
                    SESSION_ID = ulong.Parse(args[++i]);
                }
                else if (args[i] == "-post" || args[i] == "-get")
                {
                    DOCUMENT_CMD = args[i];
                    DOCUMENT_NAME = args[++i];
                }
            }
            

            Console.WriteLine("PRS Address: " + PRSSERVER_IPADDRESS);
            Console.WriteLine("PRS Port: " + PRSSERVER_PORT);
            Console.WriteLine("SD Server Address: " + SDSERVER_IPADDRESS);
            Console.WriteLine("Session Command: " + SESSION_CMD);
            Console.WriteLine("Session Id: " + SESSION_ID);
            Console.WriteLine("Document Command: " + DOCUMENT_CMD);
            Console.WriteLine("Document Name: " + DOCUMENT_NAME);

            try
            {
                // contact the PRS and lookup port for "SD Server"
                PRSClient prs = new PRSClient(PRSSERVER_IPADDRESS, PRSSERVER_PORT, SDSERVICE_NAME);
                SDSERVER_PORT = prs.LookupPort();

                // create an SDClient to use in talking to the server
                SDClientLib.SDClient sd = new SDClientLib.SDClient(SDSERVER_IPADDRESS, SDSERVER_PORT);
                sd.Connect();
                
                // send session command to server
                if (SESSION_CMD == "-o")
                {
                    // open new session
                    sd.OpenSession();                    
                }
                else if (SESSION_CMD == "-r")
                {
                    // resume existing session
                    sd.ResumeSession(SESSION_ID);                    
                }
                else if (SESSION_CMD == "-c")
                {
                    // close existing session
                    sd.SessionID = SESSION_ID;
                    sd.CloseSession();                    
                }
                
                // send document request to server
                if (DOCUMENT_CMD == "-post")
                {
                    // read the document contents from stdin
                    string documentContents = Console.In.ReadToEnd();

                    // send the document to the server
                    sd.PostDocument(DOCUMENT_NAME, documentContents);
                }
                else if (DOCUMENT_CMD == "-get")
                {
                    // get document from the server
                    string documentContents = sd.GetDocument(DOCUMENT_NAME);

                    // print out the received document
                    Console.WriteLine("Received document content: " + documentContents);
                }

                // disconnect from the server
                sd.Disconnect();               
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error " + ex.Message);
                Console.WriteLine(ex.StackTrace);
            }

            // wait for a keypress from the user before closing the console window
            // NOTE: the following commented out as they cannot be used when redirecting input to post a file
            //Console.WriteLine("Press Enter to exit");
            //Console.ReadKey();
        }
    }
}
