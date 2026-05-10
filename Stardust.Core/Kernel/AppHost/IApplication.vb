
Public Interface IApplication
    Property CurrentThread As Threading.Thread
    Property PID As ULong
    Property PPID As ULong
    Sub Init()
    Sub run()
    Sub tick()
End Interface
