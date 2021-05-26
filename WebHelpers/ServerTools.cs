using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace Common
{
    public class ServerTools
    {
        public static void ExecuteCommandAsync(string command, string workdir)
        {                                                                                                              
            try
            {
                var objThread = new Thread(new ParameterizedThreadStart(ExecuteCommandSync));
                objThread.IsBackground = true;
                objThread.Priority = ThreadPriority.AboveNormal;
                objThread.Start(new List<string> { command, workdir });
            }
            catch (Exception)
            {
            }
        }
        public static void ExecuteCommandSync(object strcmd)
        {
            
            try
            {
                var arrcmd = strcmd as List<string>;
                var command = new Process();
                var commandInfo = new ProcessStartInfo("cmd.exe");
                commandInfo.WorkingDirectory = arrcmd.ElementAt(1);
                commandInfo.UseShellExecute = false;
                commandInfo.RedirectStandardInput = true;
                commandInfo.RedirectStandardOutput = true;
                commandInfo.CreateNoWindow = true;
                command.StartInfo = commandInfo; 
                if (command.Start())
                {
                    command.StandardInput.WriteLine(arrcmd.ElementAt(0)); 
                    command.WaitForExit();
                    command.Close();  
                } 
            }
            catch (Exception ex)
            {

            }
        }
        public static void PDF2SWFAsync(string pdf2swf, string arg)
        {
            try
            {
                var objThread = new Thread(new ParameterizedThreadStart(PDF2SWFSync));
                objThread.IsBackground = true;
                objThread.Priority = ThreadPriority.AboveNormal;
                objThread.Start(new List<string> { pdf2swf, arg });
            }
            catch (Exception)
            {
            }
        }
        public static void PDF2SWFSync(object args)
        {
            try
            {
                var lstargs = args as List<string>;
                System.Diagnostics.Process proc = new System.Diagnostics.Process();
                proc.StartInfo.FileName = lstargs.ElementAt(0);
                proc.StartInfo.Arguments = lstargs.ElementAt(1);
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                proc.StartInfo.CreateNoWindow = true;
                proc.Start();
                string output = proc.StandardOutput.ReadToEnd();
                proc.WaitForExit();
                proc.Close(); 
            }
            catch
            { 
            }
        }

        public static string PDF2SWF(string pdf2swf, string arg)
        {
            
      
                System.Diagnostics.Process proc = new System.Diagnostics.Process();
                proc.StartInfo.FileName = pdf2swf;
                proc.StartInfo.Arguments = arg;
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                proc.StartInfo.CreateNoWindow = true;
                proc.Start();
                string output = proc.StandardOutput.ReadToEnd();
                proc.WaitForExit();
                proc.Close();
                return output.ToLower();
           
        }



    }



    
}
