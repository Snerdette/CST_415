README - Assignment 3 Pete Solution
v1.1
10/26/2019

Keep these files in the provided directories.

Note: You will need to have a running PRSServer to use the SDServer and SDClient

Open 3 cmd prompts, one in each of the folders.
Run PRSServer in the first cmd prompt.
Run SDServer in the second cmd prompt.
Run SDClient in the third cmd prompt.

The PRSServer and SDServer both accept command line arguments, but will work on a single machine with the default arguments.

The SDClient must be passed at least one of the connection arguments: -o -r or -c.
For example:
	SDClient -o
	SDClient -r 1
	SDClient -c 1

The document arguments, -get and -post, must be used in a valid session, for example with -o or -r.
For example:
	SDClient -o -post foo < foo.txt
	SDClient -r 1 -get foo

Try PRSServer -help, SDServer -help and SDClient -help to see the usage statements.


Example output when the following command lines are run in the SDClient folder:

>SDClient -o -post foo < foo.txt
PRS Address: 127.0.0.1
PRS Port: 30000
SD Server Address: 127.0.0.1
Session Command: -o
Session Id: 0
Document Command: -post
Document Name: foo
Sent 54 bytes: {LOOKUP_PORT, SD Server, 0, SUCCESS}
Received 54 bytes: {RESPONSE, SD Server, 40000, SUCCESS}
Connected to server at 127.0.0.1, port 40000
Opening new session
Sent open to server
Accepted, sessionID = 1
Sent post to server for document foo, length 11, contents -->
foo foo foo
<--
Received success from server!
Closing client socket
Closed!


>SDClient -r 1 -get foo
PRS Address: 127.0.0.1
PRS Port: 30000
SD Server Address: 127.0.0.1
Session Command: -r
Session Id: 1
Document Command: -get
Document Name: foo
Sent 54 bytes: {LOOKUP_PORT, SD Server, 0, SUCCESS}
Received 54 bytes: {RESPONSE, SD Server, 40000, SUCCESS}
Connected to server at 127.0.0.1, port 40000
Resuming session 1
Sent resume to server for session Id 1
Accepted, sessionID = 1
Sent get to server for document foo
Received success from server!
Received 11 chars: foo foo foo
Received document contents -->
foo foo foo
<--
Closing client socket
Closed!


>SDClient -c 1
PRS Address: 127.0.0.1
PRS Port: 30000
SD Server Address: 127.0.0.1
Session Command: -c
Session Id: 1
Document Command:
Document Name:
Sent 54 bytes: {LOOKUP_PORT, SD Server, 0, SUCCESS}
Received 54 bytes: {RESPONSE, SD Server, 40000, SUCCESS}
Connected to server at 127.0.0.1, port 40000
Closing session 1
Sent close to server for session Id 0
Closing client socket
Closed!


