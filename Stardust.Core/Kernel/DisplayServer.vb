Imports System.Reflection
Imports java.awt
Imports java.lang
Imports javax.swing

Public Class SwingRunner
    Implements Runnable
    Private _action As System.Action
    Public Sub New(action As System.Action)
        _action = action
    End Sub
    Public Sub run() Implements Runnable.run
        _action.Invoke()
    End Sub
End Class

Public Class DisplayServer
    Private Shared _instance As DisplayServer
    Private _frame As JFrame
    Private _desktop As JDesktopPane

    Private Sub New()
        _desktop = New JDesktopPane()
        _frame = New JFrame($"Stardust DisplayServer {Assembly.GetExecutingAssembly().GetName().Version}")
        _frame.setDefaultCloseOperation(WindowConstants.EXIT_ON_CLOSE)
        _frame.setContentPane(_desktop)
        _frame.setSize(1024, 768)
        _frame.setLocationRelativeTo(Nothing)

    End Sub

    Public Shared ReadOnly Property Instance As DisplayServer
        Get
            If _instance Is Nothing Then
                _instance = New DisplayServer()
            End If
            Return _instance
        End Get
    End Property

    Public Sub Run()
        PicoTronTheme.Install()
        SwingUtilities.invokeAndWait(New SwingRunner(Sub()
                                                       _frame.setVisible(True)
                                                   End Sub))
    End Sub

    Public ReadOnly Property FrameCount As Integer
        Get
            Return _desktop.getAllFrames().Length
        End Get
    End Property

    ''' <summary>
    ''' Adds a JInternalFrame to the desktop on the current thread.
    ''' Must be called from the EDT (e.g. inside SwingUtilities.invokeAndWait).
    ''' </summary>
    Public Sub AddFrameDirect(frame As JInternalFrame)
        _desktop.add(frame)
        frame.setVisible(True)
        frame.toFront()
    End Sub

    ''' <summary>
    ''' Adds a JInternalFrame to the desktop via invokeLater.
    ''' Safe to call from any thread.
    ''' </summary>
    Public Sub AddFrame(frame As JInternalFrame)
        SwingUtilities.invokeLater(New SwingRunner(Sub()
                                                       _desktop.add(frame)
                                                       frame.setVisible(True)
                                                       frame.toFront()
                                                   End Sub))
    End Sub

    Public Function CreateInternalFrame(title As String, Optional w As Integer = 480, Optional h As Integer = 320) As JInternalFrame
        Dim f = New JInternalFrame(title, True, True, True, True)
        f.setSize(w, h)
        f.setLocation(30 + (_desktop.getAllFrames().Length * 20), 30 + (_desktop.getAllFrames().Length * 20))
        Return f
    End Function
End Class
