﻿using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;

namespace SirHurtAPI
{
    public class SirHurtAPI
    {
        private static bool Injected = false;
        private static bool autoInject = false;
        private static bool multipleRBX = false;
        private static bool isCleaning = false;
        private static bool isCheckingDetachDone = false;
        private static bool firstLaunch = true;
        private static Mutex rbxmutex = null;
        internal static string SHdatPath = "sirhurt.dat";
        private readonly static string DllName = "[SirHurtAPI]";
        internal static bool AlwaysGoodCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors policyErrors)
        {
            return true;
        }
        [DllImport("SirHurtInjector.dll")]
        private static extern int Inject();
        [DllImport("user32.dll", SetLastError = true)]
        internal static extern IntPtr FindWindowA(string lpClassName, string lpWindowName);
        [DllImport("user32.dll", SetLastError = true)]
        internal static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
        internal static uint _injectionResult;
        

        


        public static bool BoostrapSirHurt()
        {
            try
            {
                WebClient wc = new WebClient();
                if(File.Exists("SirHurt.dll"))
                {
                    File.Delete("SirHurt.dll");
                }
                if (File.Exists("injector.exe"))
                {
                    File.Delete("injector.exe");
                }
                if (File.Exists("SirHurt V5.exe"))
                {
                    File.Delete("SirHurt V5.exe");
                }
                wc.DownloadFile("https://sirhurt.net/asshurt/update/v5/GUI.exe","SirHurt V5.exe");
                wc.DownloadFile("https://sirhurt.net/asshurt/update/v5/sirhurt.exe","injector.exe");
                string version = wc.DownloadString("https://sirhurt.net/asshurt/update/v5/fetch_version.php");
                wc.Headers.Add(HttpRequestHeader.UserAgent, "Roblox/WinInet");
                string hash = wc.DownloadString("https://sirhurt.net/asshurt/update/v5/fetch_sirhurt_version.php");
                if (version == "Failed: A update has not yet been released for this ROBLOX build.")
                {
                    MessageBox.Show("An error occured while trying to update to the latest version of SirHurt V5. SirHurt has not yet released a new build for this ROBLOX version. Try again later!");
                    Environment.Exit(0);
                }
                RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("Software\\Asshurt", true);
                if (registryKey != null)
                {
                    registryKey.SetValue("VHASH", CalculateMD5Hash(hash), RegistryValueKind.String);
                }
                wc.DownloadFile(version, "SirHurt.dll");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(DllName + "Failed to download SirHurt: " + ex.Message);
                return false;
            }

        }
        private static string CalculateMD5Hash(string input)
        {
            HashAlgorithm hashAlgorithm = MD5.Create();
            byte[] bytes = Encoding.ASCII.GetBytes(input);
            byte[] array = hashAlgorithm.ComputeHash(bytes);
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < array.Length; i++)
            {
                stringBuilder.Append(array[i].ToString("X2"));
            }
            return stringBuilder.ToString();
        }
        
        
        public static bool LaunchExploit() //Why LaunchExploit? because some ppl are used to make exploit using weareretarded api so yea. ; Why would I change that :3
        {
            bool returnval;
            bool injector = false;
            if (!IsInjected())
            {
                IntPtr intPtr = FindWindowA("WINDOWSCLIENT", "Roblox");
                if (intPtr == IntPtr.Zero)
                {
                    SetInjectStatus(false);
                    return false;
                }
                try
                {
                    Process injectorProcess = Process.Start(Directory.GetCurrentDirectory() + "/injector.exe"); // Crank this shit manually since the old injection method ded
                    injector = injectorProcess != null && !injectorProcess.HasExited;
                    returnval = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(DllName+"An error occured with injecting SirHurt: "+ ex.Message);
                    SetInjectStatus(false);
                    return false;
                }
                if (injector)
                {
                    Console.WriteLine(DllName + "Sucessfully injected SirHurt V5.");
                    SetInjectStatus(true);
                    var a = Registry.CurrentUser.CreateSubKey("SirHurtAPI");
                    SHdatPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\sirhurt\\sirhui\\sirhurt.dat";
                    a.SetValue("SHDatPath", SHdatPath);
                    returnval = true;
                    GetWindowThreadProcessId(intPtr, out _injectionResult);
                    SetInjectStatus(true);
                    isCheckingDetachDone = false;
                    Task.Run(async () =>
                    {
                        await injectionCheckerThreadHandler();
                    });
                    
                }
                else
                {
                    Console.WriteLine(DllName + "Failed to inject SirHurt V5");
                    SetInjectStatus(false);
                    return false;
                }
            }
            else
                return false;
            return returnval;
        }
        public static bool GetAutoInject()
        {
            try
            {
                var a = Registry.CurrentUser.OpenSubKey("SirHurtAPI");
                autoInject = Convert.ToBoolean(a.GetValue("AutoIJ"));
            }
            catch (Exception ex)
            {
                Console.WriteLine(DllName + "Cannot read auto inject status from registry, setting to false");
                SetAutoIJStatus(false);
                Console.WriteLine(ex);
            }
            return autoInject;
        }
        public static bool GetAutoExecute()
        {
            try
            {
                var a = Registry.CurrentUser.OpenSubKey("SirHurtAPI");
                autoInject = Convert.ToBoolean(a.GetValue("autoExecuteStatus"));
            }
            catch (Exception ex)
            {
                Console.WriteLine(DllName + "Cannot read auto inject status from registry, setting to false");
                SetAutoIJStatus(false);
                Console.WriteLine(ex);
            }
            return autoInject;
        }
        private static bool SetMRBX(bool mRBXStatus) // Multi client, althought you gotta provide your own mutex, from the DLL it dont work
        {
            try
            {
                var a = Registry.CurrentUser.CreateSubKey("SirHurtAPI");
                a.SetValue("mRBX", mRBXStatus);
                multipleRBX = mRBXStatus;
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(DllName + "Cannot write mRBX status to registry");
                Console.WriteLine(ex);
                return false;
            }
        }
        public static bool GetMultipleRBX()
        {
            try
            {
                var a = Registry.CurrentUser.OpenSubKey("SirHurtAPI");
                multipleRBX = Convert.ToBoolean(a.GetValue("mRBX"));
            }
            catch (Exception ex)
            {
                Console.WriteLine(DllName + "Cannot read auto inject status from registry, setting to false");
                SetMRBX(false);
                Console.WriteLine(ex);
            }
            return multipleRBX;
        }

        public static bool SetInjectStatus(bool InjectStatus)
        {
            try
            {
                var a = Registry.CurrentUser.CreateSubKey("SirHurtAPI");
                a.SetValue("InjectedValue", InjectStatus);
                Injected = InjectStatus;
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(DllName + "Cannot write inject status to registry");
                Console.WriteLine(ex);
                return false;
            }
        }
        public static bool SetAutoEXStatus(bool AutoEX)
        {
            try
            {
                var a = Registry.CurrentUser.CreateSubKey("SirHurtAPI");
                a.SetValue("AutoEX", AutoEX);
                Injected = AutoEX;
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(DllName + "Cannot write auto ex status to registry");
                Console.WriteLine(ex);
                return false;
            }
        }
        private static bool SetAutoIJStatus(bool AutoInjectStatus)
        {
            try
            {
                var a = Registry.CurrentUser.CreateSubKey("SirHurtAPI");
                a.SetValue("AutoIJ", AutoInjectStatus);
                autoInject = AutoInjectStatus;
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(DllName + "Cannot write auto inject status to registry");
                Console.WriteLine(ex);
                return false;
            }
        }
        public static bool IsInjected()
        {
            try
            {
                var a = Registry.CurrentUser.OpenSubKey("SirHurtAPI");
                Injected = Convert.ToBoolean(a.GetValue("InjectedValue"));
                if (firstLaunch && Injected && !isCheckingDetachDone)
                {
                    firstLaunch = false;
                    Task.Run(async () =>
                    {
                        await injectionCheckerThreadHandler();
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(DllName + "Cannot read inject status from registry, setting to false");
                SetInjectStatus(false);
                Console.WriteLine(ex);
            }
            return Injected;
        }
        public static bool AutoInjectToggle() //Why does everyone asking for this shit function ._.
        {
            MessageBox.Show("Do not use this function if roblox take long to load or you are joining an empty server !", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            if (!GetAutoInject())
            {
                SetAutoIJStatus(true);
                Task.Run(async () =>
                {
                    await AutoIJ();
                }); ;
                Console.WriteLine(DllName + "Enabled auto-inject");
            }
            else
            {
                SetAutoIJStatus(false);
                Console.WriteLine(DllName + "Disabled auto-inject");
            }
            return GetAutoInject();
        }
        private static async Task AutoIJ()
        {
            while (GetAutoInject())
            {
                await Task.Delay(100);
                IntPtr intPtr = FindWindowA("WINDOWSCLIENT", "Roblox");
                if (IsInjected() || intPtr == IntPtr.Zero)
                {
                    Console.WriteLine(DllName + "Injected or ROBLOX isn't running...");
                }
                else
                {
                    Console.WriteLine(DllName + "ROBLOX Found. Injecting...");
                    Thread.Sleep(10000); // Janky fix, but if you inject roblox too early, roblox will shit itself and dies
                    LaunchExploit();
                    if (GetAutoExecute())
                    {
                        foreach (string file in Directory.GetFiles(Directory.GetCurrentDirectory() + "\\Workspace\\autoexecute", "*.lua"))
                        {
                            Thread.Sleep(5000); // Let's not overload the buffer pls
                            Console.WriteLine(DllName + "Auto injecting : " + file);
                            Execute(File.ReadAllText(file), true);
                        }
                    }
                }
            }
        }
        

        private static async Task injectionCheckerThreadHandler()
        {
            while (!isCheckingDetachDone)
            {
                Application.DoEvents();
                await Task.Delay(100);
                IntPtr intPtr = FindWindowA("WINDOWSCLIENT", "Roblox");
                uint num = 0U;
                GetWindowThreadProcessId(intPtr, out num);
                if ((intPtr == IntPtr.Zero && IsInjected()) || (_injectionResult != 0U && num != _injectionResult))
                {
                    Execute("", true);
                    SetInjectStatus(false);
                    
                    isCheckingDetachDone = true;
                }
            }
        }
        private static async void revert() // cleanin' shit up
        {
            isCleaning = true;
            await Task.Delay(100);
            Execute("", true);
            isCleaning = false;
        }

        public static bool Execute(string script, bool Forced) // nice
        {
            if ((IsInjected() || Forced) && !isCleaning)
            {
                try
                {
                    Console.WriteLine(DllName + "Begin to read registry");
                    var Reg_Key = Registry.CurrentUser.OpenSubKey("SirHurtAPI");
                    var KeyValue = Reg_Key.GetValue("SHDatPath").ToString();
                    Console.WriteLine(DllName + "sirhurt.dat path: " + KeyValue);
                    SHdatPath = KeyValue;
                    if (KeyValue == "" || !KeyValue.Contains("sirhurt.dat"))
                    {
                        Console.WriteLine(DllName + "Failed to fetch sirhurt.dat directory, using default one...");
                        Reg_Key = Registry.CurrentUser.CreateSubKey("SirHurtAPI");
                        SHdatPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\sirhurt\\sirhui\\sirhurt.dat";
                        Console.WriteLine(DllName + "Setting sirhurt.dat directory to: " + SHdatPath);
                        Reg_Key.SetValue("SHDatPath", SHdatPath);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(DllName + $"Failed to fetch sirhurt.dat directory, using default one...[T/C]\n{ex}");
                    var Reg_Key = Registry.CurrentUser.CreateSubKey("SirHurtAPI");
                    SHdatPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\sirhurt\\sirhui\\sirhurt.dat";
                    Console.WriteLine(DllName + "Setting sirhurt.dat directory to: " + SHdatPath);
                    Reg_Key.SetValue("SHDatPath", SHdatPath);
                }
                try
                {
                    Directory.CreateDirectory("Workspace");
                    Console.WriteLine(DllName + "Begin to write to " + SHdatPath);
                    File.WriteAllText(SHdatPath, script);
                    if (Forced && script != "")
                    {
                        Console.WriteLine(DllName + "Forced detected, will clear sirhurt.dat in 0.1s");
                        revert();
                    }
                    Console.WriteLine(DllName + "Sucessfully write to sirhurt.dat");
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(DllName + "Cannot write to sirhurt.dat: " + ex);
                    return false;
                }
            }
            else
            {
                if (isCleaning)
                {
                    return false;
                    throw new Exception(DllName + "Cleaning sirhurt.dat");
                }
                return false;
            }
        }
        
        private async static Task rbxTrack()
        {
            while (GetMultipleRBX())
            {
                Process[] pname = Process.GetProcessesByName("RobloxPlayerBeta");
                if (pname.Length == 0)
                {
                    try
                    {
                        rbxmutex.Dispose();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(DllName + $"failed to dispose mutex (this is not a bad thing, its just bruh)\n{ex}");
                    }
                    rbxmutex = new Mutex(true, "ROBLOX_singletonMutex");
                }
                await Task.Delay(100);
            }
        }
       
        public static void oofRBX() // Funny name so i kept it
        {
            Process[] pname = Process.GetProcessesByName("RobloxPlayerBeta");
            foreach (Process proc in pname)
            {
                proc.Kill();
            }
            Console.WriteLine(DllName + "OK");
            SetInjectStatus(false);

        }
        public static bool ExecuteFromFile(bool Forced) // Unsure why this would be needed but hey you can use it if you want :shrug:
        {
            var FileDg = new OpenFileDialog();
            FileDg.Filter = "txt (*.txt)|*.txt|lua (*.lua)|*.lua|All files (*.*)|*.*";
            FileDg.InitialDirectory = Environment.CurrentDirectory;
            FileDg.Title = "SirHurtAPI File Executor";
            if (FileDg.ShowDialog() == DialogResult.OK)
            {
                string file;
                try
                {
                    using (StreamReader reader = new StreamReader(FileDg.OpenFile()))
                    {
                        file = reader.ReadToEnd();
                    }
                    Console.WriteLine(DllName + "Sucessfully read " + Path.GetFullPath(FileDg.FileName));
                }
                catch(Exception ex)
                {
                    Console.WriteLine(DllName+"Failed to read file.\nLog:", ex);
                    return false;
                }
                if (Execute(file, Forced))
                    return true;
                else
                    return false;
            }
            return true;
        }
    }
}
