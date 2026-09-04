Imports System.Threading

Public MustInherit Class ProcessNode
    Implements IApplication

    Public Property CurrentThread As Thread Implements IApplication.CurrentThread

    Public Property PID As ULong Implements IApplication.PID
    Public Property PPID As ULong Implements IApplication.PPID
    Public Event OnExit()

    Private _exitLock As New Object
    Private _hasExited As Boolean = False

    ''' <summary>
    ''' True once this process has started shutting down. Remains true thereafter.
    ''' </summary>
    Public ReadOnly Property HasExited As Boolean
        Get
            SyncLock _exitLock
                Return _hasExited
            End SyncLock
        End Get
    End Property

    ''' <summary>
    ''' Requests that this program terminate. Safe to call from any thread and any
    ''' number of times; only the first call takes effect. Returns True if this call
    ''' initiated the exit, or False if the process had already exited.
    ''' </summary>
    Public Function RequestExit() As Boolean
        Return CompleteExit()
    End Function
    ''' <summary>
    ''' Initializes the application. This method is called before the application starts running and can be used to set up any necessary resources or configurations.
    ''' </summary>
    Public MustOverride Sub init() Implements IApplication.Init
    ''' <summary>
    ''' Runs the application. This method contains the main logic of the application and is called after initialization. It should contain the code that keeps the application running until it is terminated.
    ''' </summary>
    Public MustOverride Sub run() Implements IApplication.run
    ''' <summary>
    ''' Ticks the application. This method is called periodically while the application is running and can be used to perform any necessary updates or checks. The frequency of ticks can be determined by the application's requirements.
    ''' </summary>
    Public Sub tick() Implements IApplication.tick
    End Sub

    Public Overridable Sub ExitProcess() Implements IApplication.ExitProcess
        CompleteExit()
    End Sub

    ''' <summary>
    ''' Hook for subclasses to run their own shutdown/cleanup once, immediately
    ''' before the OnExit event is raised. Use this instead of overriding
    ''' ExitProcess so idempotency is guaranteed by the base class.
    ''' </summary>
    Protected Overridable Sub OnExiting()
    End Sub

    Private Function CompleteExit() As Boolean
        SyncLock _exitLock
            If _hasExited Then Return False
            _hasExited = True
        End SyncLock
        OnExiting()
        RaiseEvent OnExit()
        Return True
    End Function
End Class
