// FTConnectedClient.cs
//
// Pete Myers
// CST 415
// Fall 2019
// 

using System;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.IO;
using System.Net.Configuration;

namespace FTServer
{
    class FTConnectedClient
    {
        // represents a single connected ft client, that wants directory contents from the server
        // each client will have its own socket and thread
        // client is given it's socket from the FTServer when the server accepts the connection
        // the client class creates it's own thread
        // the client's thread will process messages on the client's socket

        private Socket clientSocket;
        private NetworkStream stream;
        private StreamReader reader;
        private StreamWriter writer;
        private Thread clientThread;

        public FTConnectedClient(Socket clientSocket)
        {
            // save the client's socket
            this.clientSocket = clientSocket;

            // at this time, there is no stream, reader, write or thread
            stream = null;
            reader = null;
            writer = null;
            clientThread = null;

        }

        public void Start()
        {
            // called by the main thread to start the clientThread and process messages for the client

            // create and start the clientThread, pass in a reference to this class instance as a parameter
            clientThread = new Thread(ThreadProc);
            clientThread.Start(this);
        }

        private static void ThreadProc(Object param)
        {
            // the procedure for the clientThread
            // when this method returns, the clientThread will exit

            // the param is a FTConnectedClient instance
            // start processing messages with the Run() method
            FTConnectedClient client = param as FTConnectedClient;
            client.Run();
        }

        private void Run()
        {
            // this method is executed on the clientThread
            try
            {
                // create network stream, reader and writer over the socket
                stream = new NetworkStream(clientSocket);
                reader = new StreamReader(stream);
                writer = new StreamWriter(stream);

                // process client requests
                bool done = false;
                while (!done)
                {
                    // receive a message from the client
                    string msg = reader.ReadLine();
                    
                    // handle the message
                    if (msg == "get")
                    {
                        // get directoryName
                        string directoryName = reader.ReadLine();
                        Console.WriteLine("FTConnectedClient.Run() - Received get message for " + directoryName);

                        // retrieve directory contents and sending all the files

                        // if directory does not exist! send an error!
                        if (!Directory.Exists(directoryName))
                        {
                            Console.WriteLine("FTConnectedClient.Run() - Directory does not exist! :  " + directoryName); 
                            SendError("Directory does not exist: " + directoryName);
                        }
                        else
                        {
                            // if directory exists, send each file to the client
                            // for each file...
                            foreach (string fName in Directory.GetFiles(directoryName))
                            {
                                Console.WriteLine("FTConnectedClient.Run() - Found a file: " + fName);
                           
                                // make sure it's a txt file
                                FileInfo fi = new FileInfo(fName);
                                Console.WriteLine("FTConnectedClient.Run() - File Extension: " + fi.Extension);
                                if (fi.Extension == ".txt")
                                {
                                    Console.WriteLine("FTConnectedClient.Run() - It's a text file!");

                                    // get the file contents
                                    SendFileName(fi.Name, (int)fi.Length);

                                    // send a file to the client
                                    string contents = File.ReadAllText(fName);

                                    // Send a file's contents to the client
                                    SendFileContents(contents);
                                }                                
                            }
                            // send done after last file
                            SendDone();
                        }       
                    }

                    else if (msg == "exit")
                    {
                        // client is done, close it's socket and quit the thread
                        clientSocket.Disconnect(false);
                        done = true;
                        Console.WriteLine("FTConnectedClient.Run() - Processed Exit Message");                        
                    }
                    
                    else 
                    {
                        // error handling for an invalid message
                        Console.WriteLine("FTConnectedClient.Run() - unrecognized message: " + msg);
                        SendError("Unrecognized message: " + msg);

                        // this client is too broken to waste our time on!
                        // quite processing messages and disconnect
                        clientSocket.Disconnect(false);
                        done = true;
                        
                    }
                }
            }
            catch (SocketException se)
            {
                Console.WriteLine("[" + clientThread.ManagedThreadId.ToString() + "] " + "Error on client socket, closing connection: " + se.Message);
            }
            catch (IOException ioe)
            {
                Console.WriteLine("[" + clientThread.ManagedThreadId.ToString() + "] " + "Error on IO, closing connection: " + ioe.Message);
            }

            // close the client's writer, reader, network stream and socket
            writer.Close();
            reader.Close();
            stream.Close();
            clientSocket.Close();
        }

        private void SendFileName(string fileName, int fileLength)
        {
            // send file name and file length message
            writer.Write(fileName + "\n" + fileLength.ToString() + "\n");
            writer.Flush();
            Console.WriteLine("FTConnectedClient.SendFileName() - Sent! '" + fileName + "', " + fileLength.ToString());

        }

        private void SendFileContents(string fileContents)
        {
            // TODO: FTConnectedClient.SendFileContents()
            // send file contents only
            // NOTE: no \n at end of contents
            writer.Write(fileContents);
            writer.Flush();
            Console.WriteLine("FTConnectedClient.SendFileContents() - Sent!");

        }

        private void SendDone()
        {
            // send done message
            string done = "done\n";
            writer.Write(done);
            writer.Flush();
            Console.WriteLine("FTConnectedClient.SendDone() - Sent!");

        }

        private void SendError(string errorMessage)
        {
            // TODO: FTConnectedClient.SendError()
            // send error message
            string err = "error\n" + errorMessage + "\n";
            writer.Write(err);
            writer.Flush();
            Console.WriteLine("FTConnectedClient.SendError() - Sent!");

        }
    }
}
