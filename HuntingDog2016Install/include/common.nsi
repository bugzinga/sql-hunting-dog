  SetCompressor /SOLID LZMA
;--------------------------------
;Interface Settings

  !define MUI_ABORTWARNING

;--------------------------------
;Pages

  !insertmacro MUI_PAGE_DIRECTORY
  !insertmacro MUI_PAGE_INSTFILES
   
;--------------------------------
;Languages
 
  !insertmacro MUI_LANGUAGE "English"