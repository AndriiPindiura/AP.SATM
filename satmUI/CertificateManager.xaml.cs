using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Security.Cryptography.X509Certificates;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AP.SATM.Eyes
{
    /// <summary>
    /// Interaction logic for CertificateManager.xaml
    /// </summary>
    public partial class CertificateManager : MetroWindow
    {
        public CertificateManager()
        {
            InitializeComponent();
        }
        private bool flag;

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);
            Certificates.ItemsSource = store.Certificates;
            Certificates.Items.Refresh();
            //MessageBox.Show(store.Certificates.Count.ToString());
            /*foreach (X509Certificate2 cert in store.Certificates)
            {
                
            }*/
        }

        private void Certificates_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ListView ls = (ListView)sender;
            if (ls.SelectedItem is X509Certificate2)
            {
                X509Certificate2 cert = (X509Certificate2)ls.SelectedItem;
                Properties.Settings.Default.x509 = cert.Thumbprint.ToString();
                Properties.Settings settings = Properties.Settings.Default;
                Properties.Settings.Default.Save();
                flag = true;
                this.Close();
            }
        }

        private async void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!flag)
            {
                e.Cancel = true;
                MessageDialogResult result = await this.ShowMessageAsync("Попередження!", "Ви впевнені, що бажаєте продовжити без вибору діючого сертифікату?", MessageDialogStyle.AffirmativeAndNegative);
                if (result == MessageDialogResult.Affirmative)
                {
                    this.Closing -= MetroWindow_Closing;
                    this.Close();
                }
            }
        }
    }

    public class CertificateStyleSelector : StyleSelector
    {
        public override Style SelectStyle(object item,
            DependencyObject container)
        {
            Style st = new Style();
            st.TargetType = typeof(ListViewItem);
            Setter backGroundSetter = new Setter();
            backGroundSetter.Property = ListViewItem.BackgroundProperty;
            /*if (item is X509Certificate2)
            {
                satmClient.Events selectedEvent = (satmClient.Events)item;
                if (selectedEvent.user == String.Empty)
                    if (selectedEvent.legal)
                        backGroundSetter.Value = Brushes.DarkOliveGreen;
                    else
                        backGroundSetter.Value = Brushes.IndianRed;
                else
                    backGroundSetter.Value = Brushes.MidnightBlue;
            }*/
            st.Setters.Add(backGroundSetter);
            return st;
        }
    }
}
