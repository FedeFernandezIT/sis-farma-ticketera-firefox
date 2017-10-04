Public Class Form2
    Private Sub Form2_Load(sender As Object, e As EventArgs) Handles Me.Load
        ColocarEnTop(True)
    End Sub

    Private Sub Form2_Closed(sender As Object, e As EventArgs) Handles Me.Closed
        ColocarEnTop(False)
    End Sub

    Private Sub WebBrowser1_DocumentCompleted(sender As Object, e As WebBrowserDocumentCompletedEventArgs) Handles WebBrowser1.DocumentCompleted
        WebBrowser1.ScrollBarsEnabled = False
    End Sub

    Private Function ColocarEnTop(condicion As Boolean) As Boolean
        Dim f As Boolean
            
        f = (Lector.SetWindowPos(Me.Handle, IIf(condicion  = True, Lector.HWND_TOPMOST, Lector.HWND_NOTOPMOST), 0, 0, 0, 0, Lector.FLAGS) <> 0)
        'fEstaEnTop = (condicion And (f = True))
        ColocarEnTop = f
    End Function    
End Class