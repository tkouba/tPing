# TCP Ping
TCP connection checker

## Usage

tping host -p port [-t] [-a] [-n count] [-w timeout] [-4] [-6]

Options:

| Option     |                  |
|------------| -----------------|
| -p port    | Connect to port. |
| -t         | Attempt to connect to the specified host until stopped. To stop - press Control-C or Control-Break. |
| -a         | Resolve address to hostname. |
| -n count   | Number of attempts to connect. |
| -w timeout | Timeout in milliseconds to wait for each reply. |
| -4         | Force using IPv4. |
| -6         | Force using IPv6. |

