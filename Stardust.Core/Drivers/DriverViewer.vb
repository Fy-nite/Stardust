Imports java.awt

Imports javax.swing
Imports javax.swing.table


Public Class DriverViewer
    Inherits GuiProcessNode

    Private _table As JTable
    Private _tableModel As DefaultTableModel

    Public Sub New()
        MyBase.New("Driver Viewer", 720, 500)
    End Sub

    Public Overrides Sub BuildUI()
        Dim contentPane = Window.getContentPane()
        contentPane.setLayout(New BorderLayout())

        ' ── Header ──
        Dim header = New JLabel(" Installed Drivers")
        'header.setFont(header.getFont().deriveFont(java.awt.Font.BOLD, 16.0F))
        header.setBorder(BorderFactory.createEmptyBorder(8, 8, 8, 8))
        contentPane.add(header, BorderLayout.NORTH)

        ' ── Driver table ──
        _tableModel = New DefaultTableModel(New String() {"Mount Point", "Driver Type", "Assembly"}, 0)
        _table = New JTable(_tableModel)
        _table.setFillsViewportHeight(True)
        _table.setRowHeight(24)
        _table.getColumnModel().getColumn(0).setPreferredWidth(140)
        _table.getColumnModel().getColumn(1).setPreferredWidth(180)
        _table.getColumnModel().getColumn(2).setPreferredWidth(300)
        _table.setAutoCreateRowSorter(True)

        Dim scrollPane = New JScrollPane(_table)
        scrollPane.setBorder(BorderFactory.createEmptyBorder(4, 8, 8, 8))
        contentPane.add(scrollPane, BorderLayout.CENTER)

        ' ── Bottom toolbar ──
        Dim toolbar = New JPanel(New FlowLayout(FlowLayout.LEFT))
        Dim refreshBtn = New JButton("Refresh")
        'refreshBtn.addActionListener(New java.awt.event.ActionListener(Sub() RefreshDrivers())) // TODO: fix this
        toolbar.add(refreshBtn)
        toolbar.setBorder(BorderFactory.createEmptyBorder(0, 8, 8, 8))
        contentPane.add(toolbar, BorderLayout.SOUTH)

        ' ── Load data ──
        RefreshDrivers()
    End Sub

    Private Sub RefreshDrivers()
        ' Clear existing rows
        _tableModel.setRowCount(0)

        ' Populate from the VFS mount table
        Dim mounts = FS.GetMountInfo()
        For Each kv In mounts
            Dim driverType = Type.GetType("Stardust.Core.Drivers.VFS.FileSystem." & kv.Value)
            Dim assemblyName = If(driverType IsNot Nothing,
                                  driverType.Assembly.GetName().Name,
                                  "(unknown)")
            _tableModel.addRow(New Object() {kv.Key, kv.Value, assemblyName})
        Next

        ' Show total in the window title
        Window.setTitle($"Driver Viewer — {mounts.Count} driver(s) mounted")
    End Sub

    Public Overrides Sub init()
    End Sub
End Class
