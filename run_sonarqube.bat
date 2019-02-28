@echo off
SonarScanner.MSBuild.exe begin /d:sonar.login="77f863db5997a841fa7270685fcac4fc9fe9b5d4" /k:"EuriborHistory" /d:sonar.host.url="http://192.168.1.26:9000"
"C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\MSBuild.exe" /t:Rebuild
SonarScanner.MSBuild.exe end /d:sonar.login="77f863db5997a841fa7270685fcac4fc9fe9b5d4"