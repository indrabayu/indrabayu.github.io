# Step by Step creating Windows Server VM instances that can communicate with each other, in order to run .NET MPI jobs

<a href=''></a>

<ol>
<li>Have the <a href='https://www.vmware.com/go/downloadplayer'>VMWare</a> (Player/Workstation/etc) installed.</li>

<li>Create a VM instance of <a href='https://www.microsoft.com/en-us/download/details.aspx?id=11093'>Win Server 2008 R2</a>. Insert the key. You can choose the Datacenter edition in the dropdown. Instance name doesn't matter (because later we'll change this). Username is "Administrator" and don't forget to use a strong password. Tick the automatic login beneath. Press "Next". For networking, change from "NAT" to "Bridged". Wait until the installation of Windows finishes.</li>

<li>Search "group" di Windows apps/file search box. Find Windows Defender, then turn it off.</li>

<li>Activate the Windows.</li>

<li>Install <a href='https://www.microsoft.com/en-us/download/details.aspx?id=40779'>.NET Framework</a> version "4.5.1", <a href='https://msdn.microsoft.com/en-us/library/bb524831(v=vs.85).aspx'>MSMPI</a>, and <a href='http://www.osl.iu.edu/research/mpi.net/software/'>MPI.NET</a>.</li>

<li>Go to Control Panel, search by keyword "Firewall", then choose "Windows Firewall". On the left-pane click the link "Advance Settings". Press the green arrow button "Windows Firewall Properties" just under the "Public Profile". Change the dropdown value of the firewall state from "On (recommended") to "Off" for the first 3 tabs: "Domain Profile", "Public Profile" and "Private Profile". Press OK. Apply this same thing for the Host OS too.</li>

<li>Right click on My Computer -> Properties. Choose "Advanced System Settings" on the right. To the "Remote" tab, then choose the radiobutton of "Allow connections from computers running any version of Remote Desktop (less secure)".</li>

<li>Copy the Debug folder (or whatever folder name you have) of your MPI project to any place, e.g. "C:\". Send the shortcut for that Debug folder to Desktop. Put also the shortcut for Command Prompt (CMD) to the Desktop. We will share this folder. Right click on the folder, then choose Properties. To the Sharing tab. Click the button "Share...", on the dropdown choose "Everyone", press the button "Add" on its right, change the permission from "Read" to "Read/Write", press the button "Share" below, press the button "Done". Press the button "Advanced Sharing", tick "Share this folder", press the button "OK" below. Press the button "Close" below.

<li>Rename Computer Description and Machine Name from the default name into "mpi1". It will ask for restart. Before restarting, right click on the taskbar, change the value "Combine taskbar buttons" to "Never".</li>

<li>Shut down the VM instance, then rename its folder from the default name into "mpi1".</li>

<li>Open then VM instance, but don't run it just yet. Rename the instance from its default name into "mpi1".</li>

<li>Copy the folder "mpi1" (to the same folder where it's at). Rename its copy from "mpi1 - Copy" menjadi "mpi2".</li>

<li>Open the VM instance within the folder "mpi2" using VMWare, but don't run it just yet. Rename the instance from "mpi1" into "mpi2".</li>

<li>Run the VM instance "mpi2". Rename the Computer Description and Machine Name from "mpi1" into "mpi2".</li>

<li>Now make sure the VM instances "mpi1" & "mpi2" are running. For these upcoming steps, you can apply them for both (or all) instances.</li>

<li>Open CMD. Run IPCONFIG. Say that the IP for "mpi1" adalah "192.168.1.11" and the IP for "mpi2" is "192.168.1.12". Let this window opened if you want to know the IP later on.</li>

<li>Open another CMD. Run CD (change directory) to "C:\Debug". Then run "smpd -d 3". Let the executable run so (it will print out all events). Don't close this window.</li>

<li>Open another CMD. Run CD (change directory) to "C:\Debug". For "mpi1" run "mpiexec -d 3 -host 192.168.1.11 -n 2 AppNameOfMPI", and for "mpi2" run "mpiexec -d 3 -host 192.168.1.12 -n 2 AppNameOfMPI". The results (in case your AppNameOfMPI prints the Rank dan MachineName for all ranks) should be 2 lines, for ranks 0 & 1, but the machine names are the same (either it's mpi1 or mpi2).</li>

<li>Still in the same window (or a new one, up to you). For "mpi1" run "mpiexec -d 3 -host 192.168.1.12 -n 2 AppNameOfMPI", and for "mpi2" run "mpiexec -d 3 -host 192.168.1.11 -n 2 AppNameOfMPI". The results (in case your AppNameOfMPI prints the Rank dan MachineName for all ranks) should be 2 lines, for ranks 0 & 1, but the machine names are the machine name from the other VM Instace! So if you run this from "mpi1" it outputs "mpi2", and from "mpi2" it outputs "mpi1". In case you have successfuly experimented and get such results, then you know this experiment can be run on more than 1 VM Instance, and certainly can be run natively without virtualisation.</li>

<li>You can also try to run "mpiexec -d 3 -hosts 2 192.168.1.11 5 192.168.1.12 7 AppNameOfMPI". The argument "2" after "-hosts" means we're using 2 IP addresses. "5" means on "192.168.1.11" (mpi1) we want to have 5 ranks to run (index 0-4), and "7" means on "192.168.1.12" (mpi2) we want to have 7 ranks to run (index 5-11). We don't need to specify that the total of ranks (the size) is 12 ranks (5 ranks + 7 ranks).</li>
</ol>

# CONCLUSION

<ol>
<li>Make sure: >> Firewall is totally off >> Enable remote desktop >> { Instance Name, IP, Computer Description, and Computer Name} from all VM instances are different >> Shutdown the original VM instance before copying it many times (do not just suspending the template instance via VmWare because it will cause IP conflict later on).</li>

<li>Make sure the SMPD is run using "-d 3" on all VM instances.</li>

<li>For the first testing, run MPIEXEC followed by the argument "-d 3" before all other arguments such as "-host" or "-hosts". If this runs well, next time you can switch it to "-d 0" (on SMPD & MPIEXEC) so the output is purely from your kernel ("AppNameOfMPI"), and you won't see any information from SMPD when running the MPIEXEC.</li>

<li>To automate the SMPD, put the shortcut to SMPD at this folder "shell:startup" (open the folder using RUN). For Windows Server 2008, this is at "C:\Users\Administrator\AppData\Roaming\Microsoft\Windows\Start Menu\Programs\Startup". Right click at the shortcut, go to "Settings" tab, and append this at the end of the "Target" text box: " -d 0". Change the value of "Run" dropdownlist to "Minimized". Click the "Advanced" button, and tick the "Run as administrator". Good! Now each time you login to Windows, the SMPD will run automatically. You can recheck this from MSCONFIG; make sure it's ticked.</li>
</ol>

# Images

<li>The kernel ("AppNameOfMPI" C# Project, referencing the MPI.dll)<br/><img width="100%" src="files/images/Image1.PNG?raw=true" /></li>

<br/>
<br/>
<a href='https://indrabayu.github.io/'>Return to home page</a>