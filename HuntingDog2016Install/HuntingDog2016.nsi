;NSIS Modern User Interface
;--------------------------------
;Include Modern UI
  !include "MUI2.nsh"
;--------------------------------
;General
  ;Name and file
  Name "Hunting Dog for SSMS 2016 generated ${__DATE__} ${__TIME__} by $%username%"
  OutFile "HuntingDog-SSMS2016-Install.exe"

  ;Default installation folder
  InstallDir "c:\Program Files (x86)\Microsoft SQL Server\130\Tools\Binn\ManagementStudio\Extensions\HuntingDog2016\"
  
;--------------------------------
;Common settings
!include include\common.nsi

;--------------------------------
;Installer Sections
Section "Copy files "
  SetOutPath "$INSTDIR"
  File "..\HuntingDog2016\bin\Debug\*.dll"
  File "..\HuntingDog2016\bin\Debug\HuntingDog2016.pkgdef"
  File "..\HuntingDog2016\bin\Debug\extension.vsixmanifest"
SectionEnd

Section "Prepare Registry for SkipLoading"
  WriteRegDWORD HKCU "SOFTWARE\Microsoft\SQL Server Management Studio\13.0\Packages\{5b3a5944-ba3e-449d-8b79-4d194244b643}" "SkipLoading" 0x00000001
SectionEnd