#region

using System.ServiceProcess;

#endregion

namespace Topaz.LBK766
{
    internal static class Program
    {
        /// <summary>
        ///     The main entry point for the application.
        /// </summary>
        private static void Main()
        {
            var servicesToRun = new ServiceBase[]
            {
                new SigCapture()
            };
            ServiceBase.Run(servicesToRun);
        }
    }
}