FROM mcr.microsoft.com/windows/servercore:ltsc2019

SHELL ["powershell", "-Command", "$ErrorActionPreference = 'Stop'; $ProgressPreference = 'Continue'; $verbosePreference='Continue';"] 

COPY Model/ /model/
COPY Agent/ /agent/
ADD ModelStartContainerEntrypoint.ps1 /

ENTRYPOINT [ "powershell", "C:\\ModelStartContainerEntrypoint.ps1" ]