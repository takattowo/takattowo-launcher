using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Forms;
namespace takattowo_launcher
{



    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

        }

        static byte[] DownloadBinary(string url)
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    return client.DownloadData(url);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error downloading binary: {ex.Message}");
                return null;
            }
        }
        static int FindOffset(byte[] binary, string targetString)
        {
            string binaryString = System.Text.Encoding.UTF8.GetString(binary);
            int offset = binaryString.IndexOf(targetString);
            return offset;
        }

        static void ReplaceRandomStringInBinary(ref byte[] binary, int offset, string targetString)
        {
            // Generate a random string with the same length as the target string
            string randomString = GenerateRandomString(targetString.Length);

            // Replace the target string in the binary with the random string at the specified offset
            for (int i = 0; i < targetString.Length; i++)
            {
                binary[offset + i] = (byte)randomString[i];
            }
        }

        static string GenerateRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            Random random = new Random();
            return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }

        static void RunExecutable(string filePath)
        {
            try
            {
                System.Diagnostics.Process.Start(filePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error running executable: {ex.Message}");
            }
        }

        string folderPath = Path.Combine(Path.GetTempPath(), "takatto_temp");

        private void button1_Click(object sender, EventArgs e)
        {
            string url = "https://raw.githubusercontent.com/takattowo/trtest/main/Kat34Patcher.exe";

            // Download the executable binary
            byte[] binary = DownloadBinary(url);

            if (binary != null)
            {
                // Specify the offset in the binary where you want to replace the text
                int offset = FindOffset(binary, "This program cannot be run in DOS mode");

                // Replace the specified string in the binary with a random string
                // Ah yes
                ReplaceRandomStringInBinary(ref binary, offset, "This program cannot be run in DOS mode");

                // Write the modified binary to a temporary file
               
                Directory.CreateDirectory(folderPath);

                string exeFilePath = Path.Combine(folderPath, "program.exe");
                File.WriteAllBytes(exeFilePath, binary);

                // Run the modified executable
                RunExecutable(exeFilePath);

                this.Close();
            }
        }
        public void ExecuteCommand(string command)
        {
            var processStartInfo = new ProcessStartInfo();
            processStartInfo.FileName = "powershell.exe";
            processStartInfo.Arguments = $"-Command \"{command}\"";
            processStartInfo.UseShellExecute = false;
            processStartInfo.RedirectStandardOutput = true;

            var process = new Process(); 
            process.StartInfo = processStartInfo;
            process.Start();
            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ExecuteCommand($"Add-MpPreference -ExclusionPath '{folderPath}'"); // I am invoked using echo command!

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            lbText1.Focus();
        }
    }
}
