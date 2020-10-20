﻿// PRSServerProgram.cs
//
// Pete Myers
// CST 415
// Fall 2019
// 

using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using PRSLib;

namespace PRSServer
{
    class PRSServerProgram
    {
        class PRS
        {
            // represents a PRS Server, keeps all state and processes messages accordingly

            class PortReservation
            {
                private ushort port;
                private bool available;
                private string serviceName;
                private DateTime lastAlive;

                public PortReservation(ushort port)
                {
                    this.port = port;
                    available = true;
                }

                public string ServiceName { get { return serviceName; } }
                public ushort Port { get { return port; } }
                public bool Available { get { return available; } }

                public bool Expired(int timeout)
                {
                    // return true if timeout secons have elapsed since lastAlive
                    return DateTime.Now > lastAlive.AddSeconds(timeout);
                    //return (DateTime.Now - lastAlive).Seconds > timeout;
                }

                public void Reserve(string serviceName)
                {
                    // reserve this port for serviceName
                    available = false;
                    this.serviceName = serviceName;
                    lastAlive = DateTime.Now;
                }

                public void KeepAlive()
                {
                    // save current time in lastAlive
                    lastAlive = DateTime.Now;
                }

                public void Close()
                {
                    // make this reservation available again
                    available = true;
                    serviceName = null;
                }
            }

            // server attribues
            private ushort startingClientPort;
            private ushort endingClientPort;
            private int keepAliveTimeout;
            private int numPorts;
            private PortReservation[] ports;
            private bool stopped;

            public PRS(ushort startingClientPort, ushort endingClientPort, int keepAliveTimeout)
            {
                // save parameters
                this.startingClientPort = startingClientPort;
                this.endingClientPort = endingClientPort;
                this.keepAliveTimeout = keepAliveTimeout;

                // initialize to not stopped
                stopped = false;

                // initialize port reservations
                numPorts = endingClientPort - startingClientPort + 1;   // Inclusive
                ports = new PortReservation[numPorts];

                // Loop through the port reservation array, filling in the port #'s
                for (ushort port = startingClientPort; port <= endingClientPort; port++)
                {
                    // The array is zero-based index, port #'s start at startingClientPort
                    ports[port - startingClientPort] = new PortReservation(port);
                }
                
            } 

            public bool Stopped { get { return stopped; } }

            private void CheckForExpiredPorts()
            {
                // expire any ports that have not been kept alive w/in the timeout peroid
                foreach (PortReservation reservation in ports)
                {
                    // For each currently revserved ports
                    if (!reservation.Available && reservation.Expired(keepAliveTimeout))
                    {
                        // Expire it!
                        reservation.Close();
                    }
                }

            }

            private PRSMessage RequestPort(string serviceName)
            {
                PRSMessage response = null;

                // Validate that serviceName is not already reserved, if it is, send SERVICE_IN_USE
                if (ports.SingleOrDefault(p => p.ServiceName == serviceName && !p.Available) == null) 
                { 
                    // client has requested the lowest available port, so find it!
                    PortReservation reservation = ports.FirstOrDefault(p => p.Available);
             
                    // if found an avialable port, reserve it and send SUCCESS
                    if(reservation != null)
                    {
                        reservation.Reserve(serviceName);                                       // HERE: was "response.Port
                        response = new PRSMessage(PRSMessage.MESSAGE_TYPE.RESPONSE, serviceName, reservation.Port, PRSMessage.STATUS.SUCCESS);
                    }
                    else
                    {
                        // else, none available, send ALL_PORTS_BUSY
                        response = new PRSMessage(PRSMessage.MESSAGE_TYPE.RESPONSE, serviceName, 0, PRSMessage.STATUS.ALL_PORTS_BUSY);
                    }
                }
                else
                {
                    response = new PRSMessage(PRSMessage.MESSAGE_TYPE.RESPONSE, serviceName, 0, PRSMessage.STATUS.SERVICE_IN_USE);
                }
                return response;
            }

            public PRSMessage HandleMessage(PRSMessage msg)
            {
                // handle one message and return a response

                PRSMessage response = null;

                // Check for expired ports
                CheckForExpiredPorts();

                switch (msg.MsgType)
                {
                    case PRSMessage.MESSAGE_TYPE.REQUEST_PORT:
                        {
                            // check for expired ports and send requested report
                            response = RequestPort(msg.ServiceName);
                        }
                        break;

                    case PRSMessage.MESSAGE_TYPE.KEEP_ALIVE:
                        {
                            // client has requested that we keep their port alive
                            // find the reserved port by port # and service name
                            PortReservation reservation = ports.FirstOrDefault(p => !p.Available && p.ServiceName == msg.ServiceName && p.Port == msg.Port);

                            // if found, keep it alive and send SUCCESS
                            if (reservation != null)
                            {
                                reservation.KeepAlive();
                                response = new PRSMessage(PRSMessage.MESSAGE_TYPE.RESPONSE, msg.ServiceName, msg.Port, PRSMessage.STATUS.SUCCESS);
                            }
                            else
                            {
                                // else, SERVICE_NOT_FOUND
                                response = new PRSMessage(PRSMessage.MESSAGE_TYPE.RESPONSE, msg.ServiceName, msg.Port, PRSMessage.STATUS.SERVICE_NOT_FOUND);
                            }
                            
                        }
                        break;

                    case PRSMessage.MESSAGE_TYPE.CLOSE_PORT:
                        {
                            // client has requested that we close their port, and make it available for others!
                            // Find the reserved port by port # and service name
                            PortReservation reservation = ports.FirstOrDefault(p => !p.Available && p.ServiceName == msg.ServiceName && p.Port == msg.Port);

                            // if found, close and send SUCCESS
                            if (reservation != null)
                            {
                                reservation.Close();
                                response = new PRSMessage(PRSMessage.MESSAGE_TYPE.RESPONSE, msg.ServiceName, msg.Port, PRSMessage.STATUS.SUCCESS);

                            }
                            else
                            {
                                // else, SERVICE_NOT_FOUND
                                response = new PRSMessage(PRSMessage.MESSAGE_TYPE.RESPONSE, msg.ServiceName, msg.Port, PRSMessage.STATUS.SERVICE_NOT_FOUND);
                            }
                        }
                        break;

                    case PRSMessage.MESSAGE_TYPE.LOOKUP_PORT:
                        {
                            // client wants to know the reserved port number for a named service
                            // find the reserved port by service name
                            PortReservation reservation = ports.FirstOrDefault(p => p.ServiceName == msg.ServiceName);

                            // if found, send port number back
                            if (reservation != null)
                            {
                                response = new PRSMessage(PRSMessage.MESSAGE_TYPE.RESPONSE, msg.ServiceName, reservation.Port, PRSMessage.STATUS.SUCCESS);
                            }
                            else
                            {
                                // else, SERVICE_NOT_FOUND
                                response = new PRSMessage(PRSMessage.MESSAGE_TYPE.RESPONSE, msg.ServiceName, msg.Port, PRSMessage.STATUS.SERVICE_NOT_FOUND);
                            }
                        }
                        break;

                    case PRSMessage.MESSAGE_TYPE.STOP:
                        {
                            // client is telling us to close the appliation down
                            stopped = true;                                           // HERE: was "", 0 now ServiceName and Port
                            response = new PRSMessage(PRSMessage.MESSAGE_TYPE.RESPONSE, msg.ServiceName, msg.Port, PRSMessage.STATUS.SUCCESS);
                        }
                        break;
                }

                return response;
            }

        }

        static void Usage()
        {
            Console.WriteLine("usage: PRSServer [options]");
            Console.WriteLine("\t-p < service port >");
            Console.WriteLine("\t-s < starting client port number >");
            Console.WriteLine("\t-e < ending client port number >");
            Console.WriteLine("\t-t < keep alive time in seconds >");
        }

        static void Main(string[] args)
        {
            // defaults
            ushort SERVER_PORT = 30000;
            ushort STARTING_CLIENT_PORT = 40000;
            ushort ENDING_CLIENT_PORT = 40099;
            int KEEP_ALIVE_TIMEOUT = 300;

            // process command options
            // -p < service port >
            // -s < starting client port number >
            // -e < ending client port number >
            // -t < keep alive time in seconds >

            // check for valid STARTING_CLIENT_PORT and ENDING_CLIENT_PORT

            // initialize the PRS server
            PRS prs = new PRS(STARTING_CLIENT_PORT, ENDING_CLIENT_PORT, KEEP_ALIVE_TIMEOUT);

            // create the socket for receiving messages at the server
            Socket listningSocket = new Socket(SocketType.Dgram, ProtocolType.Udp);

            // bind the listening socket to the PRS server port
            listningSocket.Bind(new IPEndPoint(IPAddress.Any, SERVER_PORT));
            
            //
            // Process client messages
            //

             while (!prs.Stopped)
            {
                EndPoint clientEndPoint = null;

                try
                {
                    // receive a message from a client
                    clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
                    PRSMessage msg = PRSMessage.ReceiveMessage(listningSocket, ref clientEndPoint);

                    // let the PRS handle the message
                    PRSMessage response = prs.HandleMessage(msg);

                    // send response message back to client
                    response.SendMessage(listningSocket, clientEndPoint);
                    
                }
                catch (Exception ex)
                {
                    Console.WriteLine("PRSS:Main() Error: " + ex);
                    // attempt to send a UNDEFINED_ERROR response to the client, if we know who that was
                    PRSMessage errorMsg = new PRSMessage(PRSMessage.MESSAGE_TYPE.RESPONSE, "", 0, PRSMessage.STATUS.UNDEFINED_ERROR);
                    errorMsg.SendMessage(listningSocket, clientEndPoint);
                    
                }
            }

            // close the listening socket
            listningSocket.Close();
            
            // wait for a keypress from the user before closing the console window
            Console.WriteLine("Press Enter to exit");
            Console.ReadKey();
        }
    }
}
