using System;
using System.Text;
using System.Management;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace ayTools
{
    public class systemTools
    {
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern uint GetWindowsDirectory(StringBuilder lpBuffer, uint uSize);

        private string[] blacklist = { "To be filled by O.E.M.", "Serial", "serial", "none", "None", "00000000" };

        private bool blc(string wmi)
        {
            foreach (string search in blacklist)
            {
                if (wmi.IndexOf(search) > -1)
                {
                    return true;
                }
            }
            return false;
        }

        private string WindowsDirectory()
        {
            uint size = 0;
            size = GetWindowsDirectory(null, size);

            StringBuilder sb = new StringBuilder((int)size);
            GetWindowsDirectory(sb, size);

            return sb.ToString().Trim();
        }

        public string SystemID()
        {
            StringBuilder result = new StringBuilder();
            string cpu = String.Empty;
            string mb = String.Empty;
            string bios = String.Empty;
            string hdd = String.Empty;
            string driveC = String.Empty;
            string ram = String.Empty;
            string uuid = String.Empty;
            string os = String.Empty;
            string sysdrive = System.IO.Path.GetPathRoot(WindowsDirectory());
            // CPU
            try
            {
                ManagementObjectSearcher searcher =
                    new ManagementObjectSearcher("root\\CIMV2",
                    "SELECT * FROM Win32_Processor");
                foreach (ManagementObject queryObj in searcher.Get())
                {
                    if (queryObj["ProcessorId"] != null && !blc(queryObj["ProcessorId"].ToString().Trim()))
                       cpu = sha256_hash(queryObj["ProcessorId"].ToString().Trim()) + "-";
                }
            }
            catch
            {
                cpu = "00000000-";
                //result[0] = ("An error occurred while querying for WMI data: " + ex.Message + Environment.NewLine);
            } // CPU
            // MB
            try
            {
                ManagementObjectSearcher searcher =
                    new ManagementObjectSearcher("root\\CIMV2",
                    "SELECT * FROM Win32_BaseBoard");
                foreach (ManagementObject queryObj in searcher.Get())
                {
                    if (queryObj["SerialNumber"] != null && !blc(queryObj["SerialNumber"].ToString().Trim()))
                        mb = sha256_hash(queryObj["SerialNumber"].ToString().Trim()) + "-";
                }
            }
            catch
            {
                mb = "00000000-";
                //result[0] = ("An error occurred while querying for WMI data: " + ex.Message + Environment.NewLine);
            } // MB
            // BIOS
            try
            {
                ManagementObjectSearcher searcher =
                    new ManagementObjectSearcher("root\\CIMV2",
                    "SELECT * FROM Win32_BIOS");

                foreach (ManagementObject queryObj in searcher.Get())
                {
                    if (queryObj["SerialNumber"] != null && !blc(queryObj["SerialNumber"].ToString().Trim()))
                        bios = sha256_hash(queryObj["SerialNumber"].ToString().Trim()) + "-";
                }
            }
            catch
            {
                bios = "00000000-";
                //result[0] = ("An error occurred while querying for WMI data: " + ex.Message + Environment.NewLine);
            } // BIOS

            // HDD
            try
            {
                ManagementObjectSearcher searcher =
                    new ManagementObjectSearcher("root\\CIMV2",
                    "SELECT * FROM Win32_DiskDrive");

                foreach (ManagementObject queryObj in searcher.Get())
                {
                    if (queryObj["SerialNumber"] != null && !blc(queryObj["SerialNumber"].ToString().Trim()))
                    {
                        hdd = sha256_hash(queryObj["SerialNumber"].ToString().Trim()) + "-";
                        break;
                    }
                }
            }
            catch
            {
                hdd = "00000000-";
                //result[0] = ("An error occurred while querying for WMI data: " + ex.Message + Environment.NewLine);
            } // HDD
            // SystemDrive
            try
            {
                ManagementObjectSearcher searcher =
                    new ManagementObjectSearcher("root\\CIMV2",
                    "SELECT * FROM Win32_LogicalDisk");

                foreach (ManagementObject queryObj in searcher.Get())
                {
                    if (queryObj["DeviceID"] != null && sysdrive.IndexOf(queryObj["DeviceID"].ToString().Trim()) > -1)
                    {
                        if (queryObj["VolumeSerialNumber"] != null && !blc(queryObj["VolumeSerialNumber"].ToString().Trim()))
                            driveC = sha256_hash(queryObj["VolumeSerialNumber"].ToString().Trim()) + "-";
                    }
                }
            }
            catch
            {
                driveC = "00000000-";
                //result[0] = ("An error occurred while querying for WMI data: " + ex.Message + Environment.NewLine);
            } //SystemDrive

            // RAM
            try
            {
                ManagementObjectSearcher searcher =
                    new ManagementObjectSearcher("root\\CIMV2",
                    "SELECT * FROM Win32_PhysicalMemory");

                foreach (ManagementObject queryObj in searcher.Get())
                {
                    if (queryObj["SerialNumber"] != null && !blc(queryObj["SerialNumber"].ToString().Trim()))
                        ram = sha256_hash(queryObj["SerialNumber"].ToString().Trim()) + "-";
                }
            }
            catch
            {
                ram = "00000000-";
                //result[0] = ("An error occurred while querying for WMI data: " + ex.Message + Environment.NewLine);
            } // RAM

            // UUID
            try
            {
                ManagementObjectSearcher searcher =
                    new ManagementObjectSearcher("root\\CIMV2",
                    "SELECT * FROM Win32_ComputerSystemProduct");

                foreach (ManagementObject queryObj in searcher.Get())
                {
                    if (queryObj["UUID"] != null)
                        uuid = sha256_hash(queryObj["UUID"].ToString().Trim()) + "-";

                }
            }
            catch
            {
                uuid = "00000000-";
                //result[0] = ("An error occurred while querying for WMI data: " + ex.Message + Environment.NewLine);
            } // UUID

            // OS Key

            try
            {
                ManagementObjectSearcher searcher =
                    new ManagementObjectSearcher("root\\CIMV2",
                    "SELECT * FROM Win32_OperatingSystem");

                foreach (ManagementObject queryObj in searcher.Get())
                {
                    if (queryObj["SerialNumber"] != null)
                        os = sha256_hash(queryObj["SerialNumber"].ToString().Trim());
                }
            }
            catch
            {
                os = "00000000";
                //result[0] = ("An error occurred while querying for WMI data: " + ex.Message + Environment.NewLine);
            } // OS Key
            if (cpu == String.Empty) cpu = "00000000-";
            if (mb == String.Empty) mb = "00000000-";
            if (bios == String.Empty) bios = "00000000-";
            if (hdd == String.Empty) hdd = "00000000-";
            if (driveC == String.Empty) driveC = "00000000-";
            if (ram == String.Empty) ram = "00000000-";
            if (uuid == String.Empty) uuid = "00000000-";
            if (os == String.Empty) os = "00000000-";
            result.Append(cpu + mb + bios + hdd + driveC + ram + uuid + os);
            return result.ToString();
        }

        private String sha256_hash(String value)
        {
            StringBuilder Sb = new StringBuilder();

            using (SHA256 hash = SHA256Managed.Create())
            {
                Encoding enc = Encoding.ASCII;
                Byte[] result = hash.ComputeHash(enc.GetBytes(value));
                for (int i = result.Length - 8; i <= result.Length; i++)
                    //foreach (Byte b in result)
                    Sb.Append(result[i - 1].ToString("x2"));
            }

            return Sb.ToString();
        }

        public String sha256WithSalt(String plainText, String plainSalt)
        {
            StringBuilder Sb = new StringBuilder();
            Byte[] password = Encoding.UTF8.GetBytes(plainText);
            Byte[] salt = Encoding.UTF8.GetBytes(plainSalt);
            Byte[] passwordWithSalt = new Byte[password.Length + salt.Length];
            for (int i = 0; i < plainText.Length; i++)
            {
                passwordWithSalt[i] = password[i];
            }
            for (int i = 0; i < salt.Length; i++)
            {
                passwordWithSalt[password.Length + i] = salt[i];
            }
            using (SHA256 hash = SHA256Managed.Create())
            {
                Byte[] result = hash.ComputeHash(passwordWithSalt);
                foreach (Byte b in result)
                    Sb.Append(b.ToString("x2"));
            }

            return Sb.ToString();
        }
    }
}
