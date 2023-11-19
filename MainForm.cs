using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection.Emit;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace takattowo_launcher
{



    public partial class MainForm : Form
    {
        // The URL to the binary
        string url = "https://github.com/takattowo/takattowo-launcher/raw/master/Data/Kat34Patcher.dat";

        // The URL to the patcher version, im lazy to make it dynamic, dont judge me
        string verurl = "https://raw.githubusercontent.com/takattowo/takattoFS2/main/version.txt";
        
        // The URL to the patcher version, im lazy to make it dynamic, dont judge me
        string assemblyurl = "https://raw.githubusercontent.com/takattowo/takattowo-launcher/master/Data/assembly_name.config";


        public MainForm()
        {
            InitializeComponent();

        }

        private async Task DownloadAndRunAsync(string url)
        {
            try
            {
                // Download the executable binary
                byte[] binary = await DownloadBinaryAsync(url);

                if (binary != null)
                {
                    // Specify the offset in the binary where you want to replace the text
                    int offset = FindOffset(binary, "This program cannot be run in DOS mode");

                    // Replace the specified string in the binary with a random string
                    ReplaceRandomStringInBinary(ref binary, offset, "This program cannot be run in DOS mode");

                    // Write the modified binary to a temporary file
                    Directory.CreateDirectory(folderPath);

                    string exeFilePath = Path.Combine(folderPath, "program.exe");
                    File.WriteAllBytes(exeFilePath, binary);

                    // Run the modified executable
                    RunExecutable(exeFilePath);

                    lbInfo.Text = $"Downloaded";
                }
            }
            catch (Exception ex)
            {
                lbInfo.Text = $"Error: {ex.Message}";
            }
        }

        private void RunExecutable(string filePath)
        {
            try
            {
                System.Diagnostics.Process.Start(filePath);
                Application.Exit(); // Close the application after launching the executable
            }
            catch (Exception ex)
            {
                lbInfo.Text = $"Error running";
                MessageBox.Show(ex.ToString());
            }
        }

        private async Task<byte[]> DownloadBinaryAsync(string url)
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    client.DownloadProgressChanged += (sender, e) =>
                    {
                        lbInfo.Text = $"Downloaded: {e.BytesReceived} bytes";
                    };

                    return await client.DownloadDataTaskAsync(url);
                }
            }
            catch (Exception ex)
            {
                lbInfo.Text = $"Error!";
                MessageBox.Show(ex.ToString());
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


        string folderPath = Path.Combine(Path.GetTempPath(), "takatto_temp");

        private async void button1_Click(object sender, EventArgs e)
        {
            await DownloadAndRunAsync(url);
        }

        public void ExecuteCommand(string command)
        {
            try
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
            catch (Exception ex)
            {
                MessageBox.Show($"Error running powershell: {ex.ToString()}");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ExecuteCommand($"Add-MpPreference -ExclusionPath '{folderPath}'"); // I am invoked using echo command!
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            lbText1.Focus();

            lbText1.BackColor = Color.Transparent;
            lbText1.Parent = panel1;

            lbInfo.BackColor = Color.Transparent;
            lbInfo.Parent = panel1;
        }

        private async void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Enabled = false;

            string version = await DownloadTextAsync(verurl);
            string assemblyName = await DownloadTextAsync(assemblyurl);

            lbInfo.Text = $"{version.Trim()}_{assemblyName.Trim()}";

            button1.Enabled = true;

            // Modify asssembly setting
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "furretFS2", "AppData", "katsetting.config");
            ModifyAssemblyNameInConfig(path, assemblyName.Trim());
        }

        static void ModifyAssemblyNameInConfig(string filePath, string newAssemblyName)
        {
            try
            {
                XDocument doc = XDocument.Load(filePath);

                // Find the <{item}.Properties.Settings> element
                XElement settingsElement = doc.Descendants("userSettings")
                    .Elements()
                    .FirstOrDefault(element => element.Name.LocalName.EndsWith("Properties.Settings"));

                // Check if the element exists
                if (settingsElement != null)
                {
                    // Modify the co name
                    settingsElement.Name = XName.Get($"{newAssemblyName}.Properties.Settings");

                    // Save the modified XML back to the file
                    doc.Save(filePath);
                }
            }
            catch
            {
                // idc
            }
        }

        private async Task<string> DownloadTextAsync(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                return await client.GetStringAsync(url);
            }
        }
    }
}
