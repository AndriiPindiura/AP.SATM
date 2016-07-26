using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AP.SATM.Immynity
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MakecertGUI : Window
    {
        private Process makecert;
        private Process pvk2pfx;
        private string action;
        private X509Certificate2 ca;
        private FileInfo caPvk;
        public MakecertGUI()
        {
            InitializeComponent();
            makecert = new Process();
            pvk2pfx = new Process();
            makecert.StartInfo.FileName = "makecert.exe";
            pvk2pfx.StartInfo.FileName = "pvk2pfx.exe";
        }

        private void caCN_TextChanged(object sender, TextChangedEventArgs e)
        {
            caBtn.IsEnabled = String.IsNullOrEmpty(caCN.Text) | String.IsNullOrWhiteSpace(caCN.Text);
        }

        private void caBtn_Click(object sender, RoutedEventArgs e)
        {
            action = "newCA";
            SaveFileDialog saveCA = new SaveFileDialog();
            saveCA.FileOk += saveCA_FileOk;
            saveCA.Filter = "Certificates|*.cer;";
            saveCA.ShowDialog();
        }

        void saveCA_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveFileDialog saveCA = (SaveFileDialog)sender;
            switch (action)
            {
                case "newCA": try
                                {
                                    if (File.Exists(saveCA.FileName.Replace(".cer", ".pvk")))
                                    {
                                        File.Delete(saveCA.FileName.Replace(".cer", ".pvk"));
                                    }
                                }
                                catch { }
                                makecert.StartInfo.Arguments = "/n \"CN=" + caCN.Text + "\" /r /sv \"" + saveCA.FileName.Replace(".cer", ".pvk") + "\" \"" + saveCA.FileName + "\"";
                                makecert.Start(); 
                                break;
                case "newCer": try
                                {
                                    if (File.Exists(saveCA.FileName.Replace(".cer", ".pvk")))
                                    {
                                        File.Delete(saveCA.FileName.Replace(".cer", ".pvk"));
                                    }
                                }
                                catch { }
                                makecert.StartInfo.Arguments += " \"" + saveCA.FileName + "\" /sv \"" + saveCA.FileName.Replace(".cer", ".pvk") + "\"";
                                logger.Text = makecert.StartInfo.Arguments;
                                makecert.Start();
                                makecert.WaitForExit();
                                if (makecert.ExitCode == 0)
                                {
                                    if (File.Exists(saveCA.FileName.Replace(".cer", ".pfx")))
                                    {
                                        File.Delete(saveCA.FileName.Replace(".cer", ".pfx"));
                                    }
                                    pvk2pfx.StartInfo.Arguments = " /pvk \"" + saveCA.FileName.Replace(".cer", ".pvk") + "\" /spc \"" + saveCA.FileName + "\" /pfx \"" + saveCA.FileName.Replace(".cer", ".pfx") + "\"";
                                    pvk2pfx.Start();
                                    pvk2pfx.WaitForExit();
                                }
                                ClearAll();
                                break;
            }
        }

        private void ClearAll()
        {
            makecert.StartInfo.Arguments = String.Empty;
            pvk2pfx.StartInfo.Arguments = String.Empty;
            ca = null;
            caPvk = null;
            action = String.Empty;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            dateFrom.SelectedDate = DateTime.Now;
            dateTo.SelectedDate = DateTime.Now;
        }

        private void certCN_TextChanged(object sender, TextChangedEventArgs e)
        {
            certBtn.IsEnabled = !(String.IsNullOrEmpty(certCN.Text) | String.IsNullOrWhiteSpace(certCN.Text) | ca==null | caPvk==null);
        }

        private void caCN_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            action = "caOpenCer";
            ca = null;
            caPvk = null;
            OpenFileDialog openCA = new OpenFileDialog();
            openCA.FileOk += openCA_FileOk;
            openCA.Filter = "Certificates|*.cer;";
            openCA.Multiselect = false;
            makecert.StartInfo.Arguments = "";
            openCA.ShowDialog();
        }

        void openCA_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {

            OpenFileDialog openCA = (OpenFileDialog)sender;
            switch (action)
            {
                case "caOpenCer":
                    ca = new X509Certificate2(openCA.FileName);
                    caCN.Text = ca.SubjectName.Name.Replace("CN=", String.Empty);
                    makecert.StartInfo.Arguments = " /ic \"" + openCA.FileName + "\"";
                    action = "caOpenPvk";
                    openCA.Filter = "Certificates Private Key|" + System.IO.Path.GetFileName(openCA.FileName).Replace(".cer", ".pvk") + ";";
                    openCA.FileName = "";
                    openCA.ShowDialog();
                    break;
                case "caOpenPvk" :
                    caPvk = new FileInfo(openCA.FileName);
                    makecert.StartInfo.Arguments += " /iv \"" + openCA.FileName + "\"";
                    openCA.FileName = "";
                    break;
            }
            
        }

        private void certBtn_Click(object sender, RoutedEventArgs e)
        {
            makecert.StartInfo.Arguments += " /n \"CN=" + certCN.Text + "\" /sky exchange /pe /b " + dateFrom.SelectedDate.Value.Date.ToString("MM/dd/yyyy") + " /e " + dateTo.SelectedDate.Value.Date.ToString("MM/dd/yyyy");
            action = "newCer";
            SaveFileDialog saveCer = new SaveFileDialog();
            saveCer.FileOk += saveCA_FileOk;
            saveCer.Filter = "Certificates|*.cer;";
            saveCer.ShowDialog();
        }
    }
}
