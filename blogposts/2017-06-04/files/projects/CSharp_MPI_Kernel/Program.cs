using System;

namespace CSharp_MPI_Kernel
{
    class Program
    {
        static void Main(string[] args)
        {
            using (new MPI.Environment(ref args))
            {
                Console.WriteLine($"{Comm.Rank} - {Environment.MachineName}");
            }
        }

        static MPI.Intracommunicator Comm { get { return MPI.Communicator.world; } }
    }
}