using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace CSharp_MPI_RUN
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                bool USE_MASTER_SHARED_FOLDER = false;
                bool ACTUALLY_RUN_THE_KERNEL = true;
                const string SPACE = " ";

                string bin_folder_on_host = @"\\192.168.48.1\Debug"; //host isn't involved in computing
                string executable_kernel = @"CSharp_MPI_Kernel.exe";

                string MPI_Shared_Work_Directory = @"MPI_Work_Directory\";
                var now = DateTime.Now;
                var Year = now.Year.ToString();
                var Month = now.Month < 10 ? "0" + now.Month : now.Month.ToString();
                var Day = now.Day < 10 ? "0" + now.Day : now.Day.ToString();
                var Hour = now.Hour < 10 ? "0" + now.Hour : now.Hour.ToString();
                var Minute = now.Minute < 10 ? "0" + now.Minute : now.Minute.ToString();
                var Second = now.Second < 10 ? "0" + now.Second : now.Second.ToString();
                var unique_name = $"{Year}_{Month}_{Day}_{Hour}_{Minute}_{Second}" + @"\";
                string TargetTempFolder = MPI_Shared_Work_Directory + (USE_MASTER_SHARED_FOLDER ? "" : unique_name);

                var master = "192.168.48.128";
                var slaves = new List<string>(new string[] {
                    "192.168.48.130",
                    "192.168.48.129",
                });

                Func<string,string> get_network_shared_folder = (ip) => @"\\" + ip + @"\" + TargetTempFolder;
                string system_shared_folder = @"C:\\" + TargetTempFolder;

                Action<string> ClearDirectory = (SourcePath) =>
                {
                    DirectoryInfo di = new DirectoryInfo(SourcePath);

                    foreach (FileInfo file in di.GetFiles())
                    {
                        file.Delete();
                    }
                    foreach (DirectoryInfo dir in di.GetDirectories())
                    {
                        dir.Delete(true);
                    }
                };

                Action<string, string> CopyDirectory = (SourcePath, DestinationPath) =>
                 {
                     if (USE_MASTER_SHARED_FOLDER)
                         ClearDirectory(DestinationPath);
                     else
                         Directory.CreateDirectory(DestinationPath);

                     //Now Create all of the directories
                     foreach (string dirPath in Directory.GetDirectories(SourcePath, "*",
                             SearchOption.AllDirectories))
                         Directory.CreateDirectory(dirPath.Replace(SourcePath, DestinationPath));

                     //Copy all the files & Replaces any files with the same name
                     foreach (string newPath in Directory.GetFiles(SourcePath, "*.*",
                              SearchOption.AllDirectories))
                         File.Copy(newPath, newPath.Replace(SourcePath, DestinationPath), true);
                 };

                Action<string> robocopy = (slave) =>
                {
                    string argument_robocopy;

                    argument_robocopy =
                        get_network_shared_folder(master) + SPACE + get_network_shared_folder(slave) + " /MIR /PURGE";
                    
                    Process robocopy_process = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            CreateNoWindow = true,
                            UseShellExecute = true,
                            FileName = "robocopy",
                            WindowStyle = ProcessWindowStyle.Hidden,
                            Arguments = argument_robocopy,
                        }
                    };

                    robocopy_process.Start();
                    robocopy_process.WaitForExit();
                };

                ImpersonationHelper.Impersonate("WORKGROUP", "HostUsername", "HostPassword", delegate
                {
                    CopyDirectory(bin_folder_on_host, system_shared_folder);
                });

                foreach (var slave in slaves)
                {
                    robocopy(slave);
                }

                if (ACTUALLY_RUN_THE_KERNEL)
                {
                    var allNodes = new string[slaves.Count + 1];
                    allNodes[0] = master;
                    slaves.CopyTo(allNodes, 1);

                    if (args == null || args.Length == 0) args = new string[] { new Random().Next(1, 10).ToString() };
                    //args = new string[] { "1" };

                    string Arguments = $"-d 0 -env MPICH_NETMASK 192.168.0.0/255.255.0.0 -hosts {allNodes.Count()} {allNodes.Select(node => node + SPACE + args[0]).Aggregate((a, b) => a + SPACE + b)} {executable_kernel}";

                    Process mpiexec_process = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            WorkingDirectory = system_shared_folder,
                            CreateNoWindow = false,
                            UseShellExecute = false,
                            FileName = "mpiexec",
                            WindowStyle = ProcessWindowStyle.Normal,
                            Arguments = Arguments,
                        }
                    };

                    mpiexec_process.Start();
                    mpiexec_process.WaitForExit();
                }

                ClearDirectory(system_shared_folder);

                Action<string> rmdir = (slave) =>
                {
                    if (false == USE_MASTER_SHARED_FOLDER)
                    {
                        string argument_rmdir = @"/C rmdir /s /q \\" + slave + @"\" + TargetTempFolder + @"\";

                        Process rmdir_process = new Process
                        {
                            StartInfo = new ProcessStartInfo
                            {
                                CreateNoWindow = true,
                                UseShellExecute = true,
                                FileName = "cmd",
                                WindowStyle = ProcessWindowStyle.Hidden,
                                Arguments = argument_rmdir,
                            }
                        };

                        rmdir_process.Start();
                        rmdir_process.WaitForExit();
                    }
                };

                if (false == USE_MASTER_SHARED_FOLDER)
                {
                    Directory.Delete(system_shared_folder, true);
                    slaves.ForEach(slave => rmdir(slave)); //no need to robocopy first! :)
                }
            }
            catch (Exception err)
            {
                Console.WriteLine(err.ToString());
            }
        }
    }
}
