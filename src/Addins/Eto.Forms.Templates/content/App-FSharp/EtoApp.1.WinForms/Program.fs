﻿namespace EtoApp.1.WinForms
module Program =

    open System
    open EtoApp.1

    [<EntryPoint>]
    [<STAThread>]
    let Main(args) = 
        let app = new Eto.Forms.Application(Eto.Platforms.WinForms)
        app.Run(new MainForm())
        0