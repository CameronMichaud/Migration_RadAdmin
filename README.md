### Migration_Admin.exe  

Download Migration_Admin.exe and "Skyview-Services-x.x.xxx.msi" to the same directory on target computer.  
  
> Hit "Start Migration":
>
> - Removes ServiceCentral logs (C:\ProgramData\Radianse)
> - Installs .NET 6 & 8 SDK  
> - Installs Google Chrome  
> - Removes old services  
> - Installs latest Skyview services located in the directory of the program  
> - Configues Chrome shortcuts for Radianse.io 
> - Removes user "Kiosk"  
> - Renames current user to "Radianse"  
> - Removes password from user "Radianse"  

Once finished exit the exe and delete Migration_Admin.exe from the target computer.  
<br>
Run in terminal to download:
```
curl -L -o Migration_Admin.exe "https://github.com/CameronMichaud/Migration_RadAdmin/releases/download/v14/Migration_Admin.exe" && echo. && echo Downloaded to: && cd
```
