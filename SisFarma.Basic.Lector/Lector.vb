Public Class Lector
    Public Declare Function IsWindow Lib "user32" (ByVal hWnd As Long) As Long
    Public Declare Function OpenProcess Lib "kernel32" (ByVal dwDesiredAccess As Long, ByVal bInheritHandle As Long, ByVal dwProcessId As Long) As Long
    Public Declare Function TerminateProcess Lib "kernel32" (ByVal hProcess As Long, ByVal uExitCode As Long) As Long
    Public Declare Function CloseHandle Lib "kernel32" (ByVal hObject As Long) As Long
    Public Declare Function GetWindowThreadProcessId Lib "user32" (ByVal hWnd As Long, lpdwProcessId As Long) As Long
    Public Declare Function FindWindow Lib "user32" Alias "FindWindowA" (ByVal lpClassName As String, ByVal lpWindowName As String) As Long
    Public Declare Function SendMessage Lib "user32" Alias "SendMessageA" (ByVal hWnd As Long, ByVal wMsg As Long, ByVal wParam As Long, lParam As IntPtr) As Long
    Public Declare Function EnumWindows Lib "user32" (ByVal lpEnumFunc As Long, ByVal lParam As Long) As Long

    Private Const PROCESS_ALL_ACCESS = &H1F0FFF

    'CODIGO PARA COLOCAR EL FORMULARIO EN PRIMER PLANO
    Public Const HWND_TOPMOST As Long = -1
    Public Const HWND_NOTOPMOST As Long = -2
    Public Const SWP_NOMOVE As Long = 2
    Public Const SWP_NOSIZE As Long = 1
    Public Const FLAGS As Long = SWP_NOMOVE Or SWP_NOSIZE
    Public Declare Function SetWindowPos Lib "user32" (ByVal hWnd As Long, ByVal hWndInsertAfter As Long, ByVal X As Long, ByVal Y As Long, ByVal cx As Long, ByVal cy As Long, ByVal wFlags As Long) As Long
    '*****************************************************

    Private Declare Function SetWindowsHookEx Lib "user32.dll" Alias "SetWindowsHookExA" (ByVal idHook As Long, ByVal lpfn As Long, ByVal hmod As Long, ByVal dwThreadId As Long) As Long
    Private Declare Function UnhookWindowsHookEx Lib "user32.dll" (ByVal hHook As Long) As Long
    Private Declare Function CallNextHookEx Lib "user32.dll" (ByVal hHook As Long, ByVal nCode As Long, ByVal wParam As Long, ByRef lParam As IntPtr) As Long
    Private Declare Sub CopyMemory Lib "kernel32.dll" Alias "RtlMoveMemory" (ByRef Destination As IntPtr, ByRef Source As IntPtr, ByVal Length As Long)
    Private Declare Function GetAsyncKeyState Lib "user32.dll" (ByVal vKey As Long) As Integer
    Private Const WH_KEYBOARD_LL   As Long = 13
 
    Private Declare Function GetForegroundWindow Lib "user32.dll" () As Long
    Private Declare Function GetWindowText Lib "user32.dll" Alias "GetWindowTextA" (ByVal hWnd As Long, ByVal lpString As String, ByVal cch As Long) As Long

    Public Structure KBDLLHOOKSTRUCT
       Public VkCode As Long
       Public ScanCode As Long
       Public FLAGS As Long
       Public Time As Long
       Public DwExtraInfo As Long
    End Structure 

    Private Shared KBHook As Long
    Private Shared KeyData As String
    Private Shared lHwnd As Long

    Private Shared URL As String
    Private Shared URLMensajes As String
    Private Shared mostrador As String
    Dim urlNavegar As String
    Dim pagina As String

    Private Shared arr As Array 
    Private Shared arrS As Array

    Friend Shared Sub LeerFicherosConfiguracion()
        Const URL_INFORMACION_REMOTO As String = "c:\url_informacion_remoto.txt"
        Const MOSTRADOR_VC As String = "c:\mostrador_vc.txt"
        Const URL_MENSAJES_REMOTO As String = "c:\url_mensajes_remoto.txt"

        Dim fileUrlInformacionRemoto As New IO.StreamReader(URL_INFORMACION_REMOTO)
        URL = fileUrlInformacionRemoto.ReadLine()

        If IO.File.Exists(MOSTRADOR_VC) Then
            Dim fileMostradorVc As New IO.StreamReader(MOSTRADOR_VC)
            mostrador = fileMostradorVc.ReadLine()
        End If

        Dim fileUrlMensajesRemoto As New IO.StreamReader(URL_MENSAJES_REMOTO)
        URL = fileUrlMensajesRemoto.ReadLine()
    End Sub

    Friend Shared Sub ManageKeyLogger(enable As Boolean)
        Try
            Select Case enable
                Case True
                    Using db As New Model.SisFarmaLectorEntities
                        Dim cbs = db.medicamentos.Where(Function(med) med.cod_barras IsNot Nothing).Select(Function(med) med.cod_barras.Substring(0,3)).Distinct().ToList()
                        arr = cbs.ToArray() 
                        Dim snms = db.sinonimos.Where(Function(sin) sin.cod_barras IsNot Nothing).Select(Function(sin) sin.cod_barras.Substring(0,3)).Distinct().ToList()
                        arrS = snms.ToArray() 
                    End Using
                    'KBHook = SetWindowsHookEx(WH_KEYBOARD_LL, AddressOf KBProc, _
                    '        Runtime.InteropServices.GetHINSTANCE(GetType(Lector)) , 0)
                Case False 
            End Select

        Catch ex As Exception

        End Try
    End Sub

    Private Shared Function KBProc() As Long
        Throw New NotImplementedException()
    End Function


    Function IsInArray(stringToBeFound As String, arr As Object) As Boolean
        IsInArray = (UBound(Filter(arr, stringToBeFound)) > -1)
    End Function

End Class
