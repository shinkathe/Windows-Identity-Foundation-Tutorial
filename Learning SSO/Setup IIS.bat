:: Add new binds using current solution dir

set project=STS
set physicalPath=%~dp0%project%
%systemroot%\system32\inetsrv\APPCMD add site /name:"sso.local" /bindings:http://sso.local:80 /physicalPath:"%physicalPath%"

set project=Client 1
set physicalPath=%~dp0%project%
%systemroot%\system32\inetsrv\APPCMD add site /name:"sso.client1" /bindings:http://client1.local:80 /physicalPath:"%physicalPath%"

set project=Client 2
set physicalPath=%~dp0%project%
%systemroot%\system32\inetsrv\APPCMD add site /name:"sso.client2" /bindings:http://client2.local:80 /physicalPath:"%physicalPath%"

@echo off

SET NEWLINE=& echo.

FIND /C /I "sso.local" %WINDIR%\system32\drivers\etc\hosts
IF %ERRORLEVEL% NEQ 0 ECHO %NEWLINE%%NEWLINE%^127.0.0.1 sso.local>>%WINDIR%\System32\drivers\etc\hosts

FIND /C /I "client1.local" %WINDIR%\system32\drivers\etc\hosts
IF %ERRORLEVEL% NEQ 0 ECHO %NEWLINE%%NEWLINE%^127.0.0.1 client1.local>>%WINDIR%\System32\drivers\etc\hosts

FIND /C /I "client2.local" %WINDIR%\system32\drivers\etc\hosts
IF %ERRORLEVEL% NEQ 0 ECHO %NEWLINE%%NEWLINE%^127.0.0.1 client2.local>>%WINDIR%\System32\drivers\etc\hosts
