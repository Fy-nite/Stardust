Imports System.Threading

Public MustInherit Class ProcessNode
    Implements IApplication

    Public Property CurrentThread As Thread Implements IApplication.CurrentThread

    Public Property PID As ULong Implements IApplication.PID
    Public Property PPID As ULong Implements IApplication.PPID
    Public Event OnExit()
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
End Class
