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
using System.Net.NetworkInformation;
using System.Windows;
using System.Net.Http;

namespace Precios_Turnos
{
    internal class Seguridad
    {
        internal string[] TIPO_EQUIPO = { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N" };

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
            string? Name = "";
            string? SerialNumber = "";
            foreach (ManagementObject OS in Finder.Get()) Name = OS["Name"].ToString();

            if (Name != null)
            {
                int ind = Name.IndexOf("Harddisk") + 8;
                int HardIndex = Convert.ToInt16(Name.Substring(ind, 1));
                Finder = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive WHERE Index=" + HardIndex);
                foreach (ManagementObject HardDisks in Finder.Get())
                    foreach (ManagementObject HardDisk in HardDisks.GetRelated("Win32_PhysicalMedia"))
                        SerialNumber = HardDisk["SerialNumber"].ToString();

                if (SerialNumber != null)
                    SerialNumber.Replace(" ", string.Empty);
                else
                    SerialNumber = "";
            }
            
            return SerialNumber;
        }

        internal string numeroSeriePlacaBase()
        {
            ManagementObjectSearcher Finder = new ManagementObjectSearcher("Select * from Win32_BaseBoard");
            string? SerialNumber = "";
            foreach (ManagementObject getserial in Finder.Get())
            {
                SerialNumber = getserial["SerialNumber"].ToString();
                if (SerialNumber != null)
                    SerialNumber.Replace(" ", string.Empty);
                else
                    SerialNumber = "";
            }
            return SerialNumber;
        }

        internal DateTime GetNetworkTime()
        {
            try
            {
                const string ntpServer = "time.windows.com";
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
                TimeSpan offsetAmount = TimeZoneInfo.Local.GetUtcOffset(networkDateTime);
                return networkDateTime + offsetAmount;
            }
            catch (Exception)
            {
                return new DateTime(1900, 1, 1);
            }

        }

        internal string GenerarCheckSum(int campoInicio, string comando, bool tieneCheckSum)
        {
            //Si el comando ya tiene checksum no lo toma en cuenta para crear el nuevo checksum
            int leerHasta = 1;
            if (tieneCheckSum) leerHasta = 3;
            int suma = campoInicio;
            //Empieza de la 1° posición de ser un comando viejo
            string checkSum;
            try
            {
                //Suma el caracter del tipo de equipo de ser un modelo nuevo
                for (int i = 0; i < TIPO_EQUIPO.Length; i++)
                    if (TIPO_EQUIPO[i].Equals(comando.Substring(0, 1)))
                        suma = suma + 65 + i;
                //Despues suma cada uno de los caracteres, desde el nro de equipo hasta llegar al checksum
                byte[] data = Encoding.ASCII.GetBytes(comando.Substring(1, (comando.Length - leerHasta)));
                for (int i = 0; i < data.Length; i++)
                    suma += data[i];

                //Pasa el resultado decimal a hexadecimal tomando los dos caracteres menos significativos
                string sumaStr = string.Format("{0:x}", suma);
                sumaStr = sumaStr.Substring(sumaStr.Length - 2, 2);
                string Eh = "3" + sumaStr[0];
                string El = "3" + sumaStr[1];
                checkSum = ConvertirHEXToASCII(Eh) + ConvertirHEXToASCII(El);
            }
            catch (Exception)
            {
                return string.Empty;
            }
            return checkSum;
        }

        /*<AME 13/05/2019 Codigo inicial>
         * Convierte HEX a ASCII 
         */
        internal  string ConvertirHEXToASCII(String hexa)
        {
            int n = Convert.ToInt32(hexa, 16);
            n &= 0xFF;
            char ch = (char)n;
            String campo = "" + ch;

            return campo;
        }

        /*<AME 28/01/2021 Agregado de rangos y teclas directas>
       * Formateo de cadena con ceros a la izquierda 
       */
        internal string CadenaConCeros(string valor, int cantidad)
        {
            return valor.Replace(".", "").PadLeft(cantidad, '0');
        }

        /*<AME 28/01/2021 Agregado de rangos y teclas directas>
         * Formateo de cadena con espacios a la derecha 
         */
        internal string CadenaConEspacios(string valor, int cantidad)
        {
            return valor.PadRight(cantidad, ' ');
        }

        public string DisplayIPAddresses()
        {
            string ipInternet = string.Empty;

            // Get a list of all network interfaces (usually one per network card, dialup, and VPN connection) 
            NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

            foreach (NetworkInterface network in networkInterfaces)
            {
                if (network.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || network.NetworkInterfaceType == NetworkInterfaceType.Ethernet
                    && network.OperationalStatus == OperationalStatus.Up)
                {
                    // Read the IP configuration for each network 
                    IPInterfaceProperties properties = network.GetIPProperties();

                    // Each network interface may have multiple IP addresses 
                    foreach (IPAddressInformation address in properties.UnicastAddresses)
                    {
                        // We're only interested in IPv4 addresses for now 
                        if (address.Address.AddressFamily != AddressFamily.InterNetwork)
                            continue;

                        // Ignore loopback addresses (e.g., 127.0.0.1) 
                        else if (IPAddress.IsLoopback(address.Address))
                            continue;

                        else
                        {

                            ipInternet = address.Address.ToString();
                        }
                    }
                    if (ipInternet.Length > 0)
                        break;
                }
            }

           return ipInternet;
        }
    }
}
