
; Assembly Editor DLL interface for Win32
; Copyright (c) 2001-2010, Tomasz Grysztar.
; All rights reserved.

format PE DLL GUI 4.0
entry DLLEntryPoint

include 'win32a.inc'

include 'asmedit.ash'

section '.data' data readable writeable

  _asmedit_class db 'ASMEDIT',0

  align 4

  wheel_scroll_lines dd 3

  asmedit_font dd ?

  char rb 4
  kbstate rb 100h
  line_colors rb 100h
  line_buffer rb 100h
  text_buffer rb 100h
  upper_case_table rb 100h

  wc WNDCLASS
  ps PAINTSTRUCT
  tm TEXTMETRIC
  sc SCROLLINFO
  rect RECT

section '.text' code readable executable

proc DLLEntryPoint hinstDLL,fdwReason,lpvReserved
	cmp	[fdwReason],DLL_PROCESS_ATTACH
	jne	.done
	invoke	LoadCursor,0,IDC_IBEAM
	mov	[wc.hCursor],eax
	mov	[wc.style],CS_GLOBALCLASS+CS_DBLCLKS
	mov	[wc.lpfnWndProc],AsmEdit
	mov	eax,[hinstDLL]
	mov	[wc.hInstance],eax
	mov	[wc.cbWndExtra],4
	xor	eax,eax
	mov	[wc.hbrBackground],eax
	mov	[wc.cbClsExtra],eax
	mov	[wc.lpszMenuName],eax
	mov	[wc.lpszClassName],_asmedit_class
	invoke	RegisterClass,wc
	or	eax,eax
	jz	.failed
	invoke	CreateFont,0,0,0,0,0,FALSE,FALSE,FALSE,ANSI_CHARSET,OUT_RASTER_PRECIS,CLIP_DEFAULT_PRECIS,DEFAULT_QUALITY,FIXED_PITCH+FF_DONTCARE,NULL
	or	eax,eax
	jz	.failed
	mov	[asmedit_font],eax
	push	ebx esi edi
	mov	edi,upper_case_table
	xor	ebx,ebx
	mov	esi,100h
    .make_upper_case_table:
	invoke	CharUpper,ebx
	stosb
	inc	bl
	dec	esi
	jnz	.make_upper_case_table
	pop	edi esi ebx
    .done:
	mov	eax,TRUE
	ret
    .failed:
	mov	eax,FALSE
	ret
endp

include 'asmedit.inc'


section '.idata' import data readable writeable

  library kernel,'KERNEL32.DLL',\
	  user,'USER32.DLL',\
	  gdi,'GDI32.DLL'

  import kernel,\
	 GetModuleHandle,'GetModuleHandleA',\
	 GlobalAlloc,'GlobalAlloc',\
	 GlobalReAlloc,'GlobalReAlloc',\
	 GlobalLock,'GlobalLock',\
	 GlobalUnlock,'GlobalUnlock',\
	 GlobalFree,'GlobalFree',\
	 VirtualAlloc,'VirtualAlloc',\
	 VirtualFree,'VirtualFree',\
	 ExitProcess,'ExitProcess'

  import user,\
	 RegisterClass,'RegisterClassA',\
	 CreateCaret,'CreateCaret',\
	 ShowCaret,'ShowCaret',\
	 HideCaret,'HideCaret',\
	 SetCaretPos,'SetCaretPos',\
	 DestroyCaret,'DestroyCaret',\
	 BeginPaint,'BeginPaint',\
	 EndPaint,'EndPaint',\
	 GetDC,'GetDC',\
	 GetUpdateRect,'GetUpdateRect',\
	 ReleaseDC,'ReleaseDC',\
	 GetCursorPos,'GetCursorPos',\
	 ClientToScreen,'ClientToScreen',\
	 TrackPopupMenu,'TrackPopupMenu',\
	 DrawText,'DrawTextA',\
	 FillRect,'FillRect',\
	 InvalidateRect,'InvalidateRect',\
	 GetKeyboardState,'GetKeyboardState',\
	 ToAscii,'ToAscii',\
	 GetScrollInfo,'GetScrollInfo',\
	 SetScrollInfo,'SetScrollInfo',\
	 SetCapture,'SetCapture',\
	 ReleaseCapture,'ReleaseCapture',\
	 OpenClipboard,'OpenClipboard',\
	 CloseClipboard,'CloseClipboard',\
	 EmptyClipboard,'EmptyClipboard',\
	 GetClipboardData,'GetClipboardData',\
	 SetClipboardData,'SetClipboardData',\
	 LoadCursor,'LoadCursorA',\
	 IsClipboardFormatAvailable,'IsClipboardFormatAvailable',\
	 CharUpper,'CharUpperA',\
	 GetWindowLong,'GetWindowLongA',\
	 SetWindowLong,'SetWindowLongA',\
	 DefWindowProc,'DefWindowProcA',\
	 GetClientRect,'GetClientRect',\
	 UpdateWindow,'UpdateWindow',\
	 SetFocus,'SetFocus',\
	 GetSysColor,'GetSysColor',\
	 MessageBox,'MessageBoxA',\
	 SendMessage,'SendMessageA',\
	 PostMessage,'PostMessageA'

  import gdi,\
	 SetBkColor,'SetBkColor',\
	 SetTextColor,'SetTextColor',\
	 CreateSolidBrush,'CreateSolidBrush',\
	 CreateFont,'CreateFontA',\
	 GetTextMetrics,'GetTextMetricsA',\
	 GetTextExtentPoint32,'GetTextExtentPoint32A',\
	 DeleteDC,'DeleteDC',\
	 SelectObject,'SelectObject',\
	 DeleteObject,'DeleteObject'

section '.reloc' fixups data discardable

section '.rsrc' resource data readable

  directory RT_VERSION,versions

  resource versions,\
	   1,LANG_NEUTRAL,version

  versioninfo version,VOS__WINDOWS32,VFT_APP,VFT2_UNKNOWN,LANG_ENGLISH+SUBLANG_DEFAULT,0,\
	      'FileDescription','Assembly Editor Control',\
	      'LegalCopyright',<'Copyright ',0A9h,' 2001-2010 Tomasz Grysztar.'>,\
	      'FileVersion','1.0',\
	      'ProductVersion','1.0',\
	      'OriginalFilename','ASMEDIT.DLL'
