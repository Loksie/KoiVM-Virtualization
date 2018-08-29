ConfuserEx
========
ConfuserEx is a open-source protector for .NET applications.
It is the successor of [Confuser](http://confuser.codeplex.com) project.

Features
--------
* Supports .NET Framework 2.0/3.0/3.5/4.0/4.5
* Symbol renaming (Support WPF/BAML)
* Protection against debuggers/profilers
* Protection against memory dumping
* Protection against tampering (method encryption)
* Control flow obfuscation
* Constant/resources encryption
* Reference hiding proxies
* Disable decompilers
* Embedding dependency
* Compressing output
* Extensible plugin API
* Many more are coming!

Usage
-----
`Confuser.CLI <path to project file>`

The project file is a ConfuserEx Project (*.crproj).
The format of project file can be found in docs\ProjectFormat.md

Bug Report
----------
See the [Issues Report](http://yck1509.github.io/ConfuserEx/issues/) section of website.


License
-------
See LICENSE file for details.

Credits
-------
**[0xd4d](https://github.com/0xd4d)** for his awesome work and extensive knowledge!  
Members of **[Black Storm Forum](http://board.b-at-s.info/)** for their help!

# KoiVM

Is a virtual machine made to work on ConfuserEx, it turns the .NET opcodes into new ones that only are understood by our machine.
There are multiple ways of using the plugin, first one is certainly ridiculous as it will "merge" with cex and virtualize every single method, including protections from ConfuserEX, however note that this might KILL your performance.
Second one will just virtualize the methods that you decide, this is the best option in all if not all the cases.

Now, I will procceed to list all the limitations that you might find while using this protection:
* Memory leaks, running the app for large ammount of times will cause a crash eventually. This can be prevented by limiting the ammount of loops you virtualize and specially, not having them running permanently.
* Opcodes, some opcodes due to its nature are not supported, we're talking about: Calli, Jmp and few others.
* Old, this project was made few years ago, therefore its compatibility by latest .NET frameworks might be limited, however we're talking about specific methods and not an actual issue, you will still be able to protect your methods if you compile on .NET 4.7.1 for example. but dont expect all of them to work as them would using .NET 2.0
* Other runtimes rather than .Net Framework and CoreCLR are not supported.
* Only supported OS is Windows.
* Cflow limited support, when stacking Cflow and Virtualization you might face issues, Im not sure if this is caused by a modification i made myself or because of the nature of the machine.
... And thats basically it, thats the price you pay for this heavy protection.


How to use KoiVm?
-------
Like I stated before, there are two ways of applying this protection, if you want to use the first method just make a cex project file and aggregate the following text that can be found on official KoiVM page: https://ki-host.appspot.com/KoiVM/documentation.html

This project will execute the protection on the whole assembly, here you can see the parameters that you have available:

rtName:
Indicates the assembly name of the runtime library. Only valid on module.

dbgInfo:
Indicates the emission of debug info. Only valid on module.

merge:
Indicates the runtime library should be merged into the output module. Only valid on module.

stackwalk:
Indicates the exception stack trace generated should be complete. Only valid on module.

Second and last way of using koiVM is by adding yourself attributes to the method you wish to protect:

`[Obfuscation(Exclude = false, Feature = "+koi;-ctrl flow")]`

If you're wondering the reason of -ctrl flow, please check the list of limited functions koi might have currently. Moving on, I'd like to remember that you will have to aggregate koiVM as a plugin in order to get the virtualization running. I also recommend using confuserex CLI and not the GUI, as this one might limit the strength of the protection.
