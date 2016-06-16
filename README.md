# Flabbergast Fiddle
![](https://rawgithub.com/flabbergast-config/flabbergast/master/flabbergast.svg)
This provides a server capable of providing a web interface to run Flabbergast
code. If you want to try it, visit [http://fiddle.flabbergast.org/]. These
instructions are for building your own server.

## Building
The application is an ASP.NET application, requiring a working Flabbergast CLR edition.

Install the Mono XSP4 server and then build the project:

    apt-get install mono-xsp4
    ln -s /usr/share/flabbergast/cli/Flabbergast.*.dll bin
    xbuild

## Deploying
There are several deployment options, consult [Mono
ASP.NET](http://www.mono-project.com/docs/web/aspnet/) for details. For
testing, the easiest is to:

   xsp4

and then view in a web browser.

For real deployment, the FastCGI or `mod_mono` options make better sense.

## Security
The Flabbergast environment created is a bit unusual. First, the SQL, and JSON
URI schemes are disabled. Also, the library loader will only use pre-compiled
libraries, so running `sudo update-flabbergast` is essential.

The `current:` URI scheme exposes the login name of the account running the
server.

The previously compiled code for every run are held in memory in a per-user
session variable. They should be reaped when the session end, since there is no
permgen like Java.

There is a timeout for page processing enforced by ASP.NET.
