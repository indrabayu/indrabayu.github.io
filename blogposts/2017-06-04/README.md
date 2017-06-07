# Automating the MPIEXEC 

<a href=''></a>

Here I'm going to semi-automate the execution of MPIEXEC so that I don't have to have to type everything in the command prompt, but set everything from the source code editor where it's comfortable to do so.

In this experiment I'm using a host PC, a VM instance for the master node, and a number of VM instances for the slave nodes (in this case I use 2 slaves).

IP addresses:
<ol>
<li>Host: 192.168.1.8</li>
<li>Master node: 192.168.1.21</li>
<li>Slave node #1: 192.168.1.22</li>
<li>Slave node #2: 192.168.1.26</li>
</ol>

I use the settings just like in my previous blog, such as the firewall settings, etc. The difference here is that, in each VM instance I have a shared folder ready, located at "\\IP address\Debug". That's the folder where I put the main executable (of the MPI kernel) and all the necessary binaries to run it. This is achieved my copying exactly the folder content in the development PC's shared folder (the host), hence making sure all compute nodes have that exact folder. After the MPIEXEC is done, the master node does the cleanup by deleting the content of the shared folder within the slaves, and eventually its own.

The kernel itself only prints its rank and machine name, so there's nothing special here.

In my case, the host isn't a VM instance, and it doesn't take part as a compute node.

I randomise the number of processors/ranks (between 1 to 10) that's going to be run on each of the slaves.

The program to run this (let us call it "MPI_RUN") is in the <a href="files/projects/CSharp_MPI_RUN">CSharp_MPI_RUN</a> folder, and the kernel itself is in the <a href="files/projects/CSharp_MPI_Kernel">CSharp_MPI_Kernel</a> folder.

# Remarks

<ol>
<li>You can improve this MPI_RUN program by having the it read from a config file containing the IP addresses (compute nodes) with the corresponding number of processors/ranks the MPIEXEC has to spawn for each of the compute node, instead of hardcoding it in the MPI_RUN source code.</li>
<li>I encountered a strange error each time I want to run the MPI_RUN after rebooting the compute nodes. This error happens in the master node, and following is the dump: <i><b>Fatal error in MPI_Comm_dup: Other MPI error, error stack: MPI_Comm_dup(MPI_COMM_WORLD, new_comm=0x00000000022D2E8C) failed [ch3:sock] rank 0 unable to connect to rank 4 using business card <port=49170 description="AAA.BBB.CCC.DDD mpi3 " shm_host=mpi3 shm_queue=2608:516 > unable to connect to AAA.BBB.CCC.DDD mpi3  on port 49170, no endpoint matches the netmask 192.168.0.0/255.255.0.0</b></i>. Please note that "AAA.BBB.CCC.DDD" is the IP of my internet provider. So I don't know why this keeps happening, but I can say that the second time onward of running the MPI_RUN this error occurs no more (except if you reboot all instances again).
</li>
</ol>

# Images

<li>The big picture. The MPI_RUN is executed only on the master node, so you have to copy the program that one particular compute node first. Click to enlarge.<br/><img width="100%" src="files/images/Image1.png?raw=true" /></li>

<br/>
<br/>
<a href='https://indrabayu.github.io/'>Return to home page</a>