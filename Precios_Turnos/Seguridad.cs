using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.IO;
using System.Management;
using System.Security.Policy;

namespace Precios_Turnos
{
    internal class Seguridad
    {
        internal string EncryptString(string key, string plainText)
        {
            try
            {
                byte[] iv = new byte[16];
                byte[] array;

                using (Aes aes = Aes.Create())
                {
                    aes.Key = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(key));
                    aes.IV = iv;

                    ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, encryptor, CryptoStreamMode.Write))
                        {
                            using (StreamWriter streamWriter = new StreamWriter((Stream)cryptoStream))
                            {
                                streamWriter.Write(plainText);
                            }

                            array = memoryStream.ToArray();
                        }
                    }
                }

                return Convert.ToBase64String(array);
            }
            catch (Exception)
            {
                return string.Empty;
            }
            
        }

        internal string DecryptString(string key, string cipherText)
        {
            try
            {
                byte[] iv = new byte[16];
                byte[] buffer = Convert.FromBase64String(cipherText);

                using (Aes aes = Aes.Create())
                {
                    aes.Key = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(key));
                    aes.IV = iv;
                    ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                    using (MemoryStream memoryStream = new MemoryStream(buffer))
                    {
                        using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, decryptor, CryptoStreamMode.Read))
                        {
                            using (StreamReader streamReader = new StreamReader((Stream)cryptoStream))
                            {
                                return streamReader.ReadToEnd();
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                return string.Empty;
            }
           
        }
        
        internal string numeroSerieHD()
        {
            ManagementObjectSearcher Finder = new ManagementObjectSearcher("Select * from Win32_OperatingSystem");
            string Name = "";
            string SerialNumber = "";
            foreach (ManagementObject OS in Finder.Get()) Name = OS["Name"].ToString();

            int ind = Name.IndexOf("Harddisk") + 8;
            int HardIndex = Convert.ToInt16(Name.Substring(ind, 1));
            Finder = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive WHERE Index=" + HardIndex);
            foreach (ManagementObject HardDisks in Finder.Get())
                foreach (ManagementObject HardDisk in HardDisks.GetRelated("Win32_PhysicalMedia"))
                    SerialNumber = HardDisk["SerialNumber"].ToString();

            return SerialNumber.Replace(" ", string.Empty);
        }

        internal string numeroSeriePlacaBase()
        {
            ManagementObjectSearcher Finder = new ManagementObjectSearcher("Select * from Win32_BaseBoard");
            string SerialNumber = "";
            foreach (ManagementObject getserial in Finder.Get())
            {
                SerialNumber = getserial["SerialNumber"].ToString();
            }
            return SerialNumber.Replace(" ", string.Empty);
        }

        internal DateTime GetNetworkTime()
        {
            try
            {
                const string ntpServer = "pool.ntp.org";
                var ntpData = new byte[48];
                ntpData[0] = 0x1B; //LeapIndicator = 0 (no warning), VersionNum = 3 (IPv4 only), Mode = 3 (Client Mode)
                var addresses = Dns.GetHostEntry(ntpServer).AddressList;
                var ipEndPoint = new IPEndPoint(addresses[0], 123);
                var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                socket.Connect(ipEndPoint);
                socket.Send(ntpData);
                socket.Receive(ntpData);
                socket.Close();
                ulong intPart = (ulong)ntpData[40] << 24 | (ulong)ntpData[41] << 16 | (ulong)ntpData[42] << 8 | (ulong)ntpData[43];
                ulong fractPart = (ulong)ntpData[44] << 24 | (ulong)ntpData[45] << 16 | (ulong)ntpData[46] << 8 | (ulong)ntpData[47];
                var milliseconds = (intPart * 1000) + ((fractPart * 1000) / 0x100000000L);
                var networkDateTime = (new DateTime(1900, 1, 1)).AddMilliseconds((long)milliseconds);
                return networkDateTime;
            }
            catch (Exception)
            {
                return DateTime.Now;
            }

        }
        
        internal bool ValidaConexionWEB()
        {
            bool Estado = false;
            System.Uri Url = new System.Uri("https://www.google.com/");

            System.Net.WebRequest WebRequest;
            WebRequest = System.Net.WebRequest.Create(Url);
            System.Net.WebResponse objetoResp;

            try
            {
                objetoResp = WebRequest.GetResponse();
                Estado = true;
                objetoResp.Close();
            }
            catch (Exception)
            {
            }
            WebRequest = null;
            return Estado;
        }
    }
}
