README - Assignment 4 Pete Solution
v1.1
11/9/2019

Keep these files in the provided directories.

Note: You will need to have a running PRSServer to use the FTServer, SDServer, SDClient and SDBrowser

All programs accept command line arguments, but will work on a single machine with the default arguments.
Try using -help, to see the usage statements.

Open 3 cmd prompts, one in each of these folders:
Run PRSServer in the first cmd prompt.
Run FTServer in the second cmd prompt.
Run SDServer in the third cmd prompt.

You can then test the SDServer by running SDClient in a 4rd cmd prompt in the SDClient folder:

SDClient -o -get /dir1/hello.txt

...
Connected to server at 127.0.0.1, port 40000
Opening new session
Sent open to server
Accepted, sessionID = 1
Sent get to server for document /dir1/hello.txt
Received success from server!
Received 12 chars: hello there!
Received document contents -->
hello there!
<--
Closing client socket
Closed!


SDClient -r 1 -post /dir1/hello.txt < foo.txt

...
Connected to server at 127.0.0.1, port 40000
Resuming session 1
Sent resume to server for session Id 1
Accepted, sessionID = 1
Sent post to server for document /dir1/hello.txt, length 11, contents -->
foo foo foo
<--
Received success from server!
Closing client socket
Closed!



Run the SDBrowser program and use the following as an address:

Address...
FT:127.0.0.1:foo

Contents wil be...

bar.txt
bar
bar
bar

ha ha.txt
ha ha
ha ha
ha ha


Address...
SD:127.0.0.1:/dir1/hello.txt

hello there!
foo foo foo






