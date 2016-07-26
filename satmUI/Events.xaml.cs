using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System.ComponentModel;
using System.Threading;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Threading;
using System.Runtime.ExceptionServices;


namespace AP.SATM.Eyes
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class Events : MetroWindow, INotifyPropertyChanged
    {
        public Events()
        {
            DataContext = this;
            InitializeComponent();
            satm = new satmClient.CoreClient();
            satm.ChannelFactory.Closing += ChannelFactory_Closed;
            satm.ChannelFactory.Closed += ChannelFactory_Closed;
            satm.ChannelFactory.Faulted += ChannelFactory_Closed;
            satm.ChannelFactory.Opened += ChannelFactory_Opened;
            startTime = new TimeSpan(0, 0, 0);
            endTime = new TimeSpan(23, 59, 59);
            heartbeat = new System.Timers.Timer(1000);
            heartbeat.Elapsed += heartbeat_Elapsed;
            Properties.Settings.Default.Upgrade();
            Application.Current.Exit += Current_Exit;
            monitor = new AP.CCTV.Client();
            //eventsView.key
        }
        #region Colors
        public System.Drawing.Color IllegalColor
        {
            get { return (System.Drawing.Color)GetValue(IllegalColorProperty); }
            set { SetValue(IllegalColorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedColor.  
        // This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IllegalColorProperty =
            DependencyProperty.Register("IllegalColor",
        typeof(System.Drawing.Color), typeof(Events), new UIPropertyMetadata(null));

        public System.Drawing.Color LegalColor
        {
            get { return (System.Drawing.Color)GetValue(LegalColorProperty); }
            set { SetValue(LegalColorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedColor.  
        // This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LegalColorProperty =
            DependencyProperty.Register("LegalColor",
        typeof(System.Drawing.Color), typeof(Events), new UIPropertyMetadata(null));

        public System.Drawing.Color ProcessedColor
        {
            get { return (System.Drawing.Color)GetValue(ProcessedColorProperty); }
            set { SetValue(ProcessedColorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedColor.  
        // This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ProcessedColorProperty =
            DependencyProperty.Register("ProcessedColor",
        typeof(System.Drawing.Color), typeof(Events), new UIPropertyMetadata(null));
        #endregion

        void Current_Exit(object sender, ExitEventArgs e)
        {
            Properties.Settings.Default.Save();
            monitor.CloseAll();
        }

        async void heartbeat_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            ExceptionDispatchInfo error = null;
            if (satm.ChannelFactory.State == CommunicationState.Opened && coreConnected)
            {
                try
                {
                    satm.HeartBeat();
                }
                catch (Exception ex)
                {
                    error = ExceptionDispatchInfo.Capture(ex);
                    coreConnected = false;
                    OnPropertyChanged("coreConnected");
                    connectionStatus.Dispatcher.Invoke(
                          System.Windows.Threading.DispatcherPriority.Normal,
                          new Action(
                            delegate ()
                            {
                                connectionStatus.Text = "увійти";
                            }
                        ));

                }
            }
            if (error != null)
            {
                HostHide(false);
                progress.SetMessage("Втрачено зв’язок с сервером! Подальша робота неможлива!\r\n" + error.SourceException.Message);
                progress.SetCancelable(true);
                for (int i = 1; i < 300; i++)
                {
                    Thread.Sleep(100);
                    if (progress.IsCanceled)
                        break;
                }

                /*while (!progress.IsCanceled)
                {
                    Thread.Sleep(100);
                }*/
            }
        }

        private AP.CCTV.Client monitor;

        private System.Timers.Timer heartbeat;

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public bool coreConnected { get; set; }

        private bool goFlag;

        public TimeSpan startTime { get; set; }

        public TimeSpan endTime { get; set; }

        private List<satmClient.Cores> coresList;

        private List<satmClient.Entries> entriesList;

        private List<satmClient.Events> eventsList;

        private DateTime upTime;

        private DateTime enterTime;

        private DateTime exitTime;

        private DateTime tmpUpTime;

        private DateTime tmpEnterTime;

        private DateTime tmpExitTime;

        private int tmpUpCam;

        private int tmpEnterCam;

        private int tmpExitCam;

        private int upCam;

        private int enterCam;

        private int exitCam;

        private ListSortDirection _sortDirection;

        private GridViewColumnHeader _sortColumn;

        protected void OnPropertyChanged(string property)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

        private satmClient.CoreClient satm;

        private EventStyleSelector eventsStyle;

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            coreConnected = false;
            eventsStyle = new EventStyleSelector();
            eventsView.ItemContainerGenerator.StatusChanged += ItemContainerGenerator_StatusChanged;
            OnPropertyChanged("coreConnected");
            startDate.SelectedDate = DateTime.Now.Date;
            endDate.SelectedDate = DateTime.Now.Date.AddDays(1).AddSeconds(-1);
            X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);
            if (store.Certificates.Find(X509FindType.FindByThumbprint, Properties.Settings.Default.x509, true).Count <= 0)
            {
                signin.IsEnabled = false;
            }

            Properties.Settings.Default.Reload();
            int i = 0;
            int selected = -1;
            foreach (object item in iColor.Items)
            {
                if (item.ToString().Contains(Properties.Settings.Default.illegalColor.ToString().Replace("Color [", String.Empty).Replace("]", String.Empty)))
                {
                    selected = i;
                }
                i++;
            }
            iColor.SelectedIndex = selected;
            selected = -1;
            i = 0;
            foreach (object item in lColor.Items)
            {
                if (item.ToString().Contains(Properties.Settings.Default.legalColor.ToString().Replace("Color [", String.Empty).Replace("]", String.Empty)))
                {
                    selected = i;
                }
                i++;
            }
            lColor.SelectedIndex = selected;
            selected = -1;
            i = 0;
            foreach (object item in pColor.Items)
            {
                if (item.ToString().Contains(Properties.Settings.Default.processedColor.ToString().Replace("Color [", String.Empty).Replace("]", String.Empty)))
                {
                    selected = i;
                }
                i++;
            }
            pColor.SelectedIndex = selected;
        }

        private void ItemContainerGenerator_StatusChanged(object sender, EventArgs e)
        {

        }

        private void ConnectToServer()
        {
            try
            {
                satm = new satmClient.CoreClient();
                satm.ChannelFactory.Closing += ChannelFactory_Closed;
                satm.ChannelFactory.Closed += ChannelFactory_Closed;
                satm.ChannelFactory.Faulted += ChannelFactory_Closed;
                satm.ChannelFactory.Opened += ChannelFactory_Opened;
                satm.ClientCredentials.ClientCertificate.SetCertificate(
                    StoreLocation.CurrentUser,
                    StoreName.My,
                    X509FindType.FindByThumbprint,
                    Properties.Settings.Default.x509);
                coreConnected = satm.SignIn();

            }
            catch (CommunicationException ex)
            {
                HostHide(false);
                try
                {
                    satm.ChannelFactory.Close();
                }
                catch
                {
                    satm.ChannelFactory.Abort();
                }
                progress.SetMessage("Помилка при підключенні до сервера! Подальша робота неможлива!\r\n" + ex.Message);
                progress.SetCancelable(true);
                while (!progress.IsCanceled)
                {
                    Thread.Sleep(100);
                }
            }
            catch (TimeoutException ex)
            {
                HostHide(false);
                progress.SetMessage("Відсутня відповідь від сервера! Подальша робота неможлива!\r\n" + ex.Message);
                progress.SetCancelable(true);
                for (int i = 1; i < 200; i++)
                {
                    Thread.Sleep(100);
                    if (progress.IsCanceled)
                        break;
                }
            }
            catch (FormatException ex)
            {
                HostHide(false);
                try
                {
                    satm.ChannelFactory.Close();
                }
                catch
                {
                    satm.ChannelFactory.Abort();
                }
                progress.SetMessage("Невірно вказаний сертифікат користувача! Подальша робота неможлива!\r\n"
                    + ex.Message + "\r\nПоточний сертифікат: " + Properties.Settings.Default.x509);
                progress.SetCancelable(true);
                for (int i = 1; i < 200; i++)
                {
                    Thread.Sleep(100);
                    if (progress.IsCanceled)
                        break;
                }

            }
            catch (Exception ex)
            {
                progress.SetMessage("Невідома помилка: " + ex.Message + "\r\nПодальша робота неможлива!");
                progress.SetCancelable(true);
                for (int i = 1; i < 200; i++)
                {
                    Thread.Sleep(100);
                    if (progress.IsCanceled)
                        break;
                }

            }
            if (coreConnected)
            {
                try
                {
                    string username = satm.ARM().Replace("CN=", "");
                    if (String.IsNullOrEmpty(username) || String.IsNullOrWhiteSpace(username))
                    {
                        try
                        {
                            coreConnected = satm.SignOut();
                            satm.ChannelFactory.Close();
                        }
                        catch
                        {
                            satm.ChannelFactory.Abort();
                        }
                        progress.SetMessage("Помилка при аутентифікації користувача! " + username + "Зверніться до адміністратора за діючим сертифікатом!");
                        progress.SetCancelable(true);
                        for (int i = 1; i < 200; i++)
                        {
                            Thread.Sleep(100);
                            if (progress.IsCanceled)
                                break;
                        }
                        coreConnected = false;
                        OnPropertyChanged("coreConnected");
                        connectionStatus.Dispatcher.Invoke(
                              System.Windows.Threading.DispatcherPriority.Normal,
                              new Action(
                                delegate ()
                                {
                                    connectionStatus.Text = "увійти";
                                }
                            ));
                    }
                    else
                    {
                        connectionStatus.Dispatcher.Invoke(
                              System.Windows.Threading.DispatcherPriority.Normal,
                              new Action(
                                delegate ()
                                {
                                    connectionStatus.Text = satm.ARM().Replace("CN=", "");
                                }
                            ));

                        OnPropertyChanged("coreConnected");
                    }
                }
                catch (Exception ex)
                {
                    HostHide(false);
                    try
                    {
                        coreConnected = satm.SignOut();
                        satm.ChannelFactory.Close();
                    }
                    catch
                    {
                        satm.ChannelFactory.Abort();
                    }

                    progress.SetMessage("Помилка при отриманні облікових данних!!\r\n" + ex.Message);
                    progress.SetCancelable(true);
                    for (int i = 1; i < 200; i++)
                    {
                        Thread.Sleep(100);
                        if (progress.IsCanceled)
                            break;
                    }
                    coreConnected = false;
                    OnPropertyChanged("coreConnected");
                    connectionStatus.Dispatcher.Invoke(
                          System.Windows.Threading.DispatcherPriority.Normal,
                          new Action(
                            delegate ()
                            {
                                connectionStatus.Text = "увійти";
                            }
                        ));
                }
                //throw new ApplicationException("DEBUG exception throw");

            }
            else
            {
                connectionStatus.Dispatcher.Invoke(
                      System.Windows.Threading.DispatcherPriority.Normal,
                      new Action(
                        delegate ()
                        {
                            connectionStatus.Text = "увійти";
                        }
                    ));
            }
            progress.CloseAsync();
        }

        void ChannelFactory_Opened(object sender, EventArgs e)
        {
            //MessageBox.Show("Start HeartBeat");
            heartbeat.Enabled = true;
        }

        void ChannelFactory_Closed(object sender, EventArgs e)
        {
            //MessageBox.Show("Stop HeartBeat");
            heartbeat.Stop();
            heartbeat.Enabled = false;
        }

        private ProgressDialogController progress;

        private async void SingInClick(object sender, RoutedEventArgs e)
        {
            HostHide(false);
            objectFlayout.IsOpen = false;
            settingsFlayout.IsOpen = false;

            if (!coreConnected)
            {
                progress = await this.ShowProgressAsync("Підключення", "Триває підключення до серверу.\r\nЗачекайте...");
                //progress.SetCancelable(true);
                Thread coreConnect = new Thread(ConnectToServer);
                coreConnect.Start();
            }
            else
            {
                ExceptionDispatchInfo error = null;
                try
                {
                    coresView.Items.Clear();
                    entriesView.Items.Clear();
                    if (eventsList != null)
                        eventsList.Clear();

                    if (satm.InnerChannel.State != System.ServiceModel.CommunicationState.Faulted)
                    {
                        try
                        {
                            coreConnected = satm.SignOut();
                            satm.ChannelFactory.Close();
                        }
                        catch
                        {
                            satm.ChannelFactory.Abort();
                        }
                    }

                    else
                        coreConnected = false;
                    OnPropertyChanged("coreConnected");
                    connectionStatus.Text = "увійти";
                }
                catch (CommunicationException ex)
                {
                    error = ExceptionDispatchInfo.Capture(ex);
                    HostHide(false);
                    connectionStatus.Text = "увійти";
                    coreConnected = false;
                    OnPropertyChanged("coreConnected");

                    //satm.Close();
                    //satm.Abort();
                }
                catch (TimeoutException ex)
                {
                    error = ExceptionDispatchInfo.Capture(ex);
                    HostHide(false);
                    connectionStatus.Text = "увійти";
                    coreConnected = false;
                    OnPropertyChanged("coreConnected");
                    //satm.Close();
                }
                if (error != null)
                {
                    await this.ShowMessageAsync("Помилка!", "Відсутня відповідь від сервера!\r\n" + error.SourceException.Message, MessageDialogStyle.Affirmative);
                }
            }


        }

        private void SettingsFlayoutClick(object sender, RoutedEventArgs e)
        {
            settingsFlayout.IsOpen = !settingsFlayout.IsOpen;
        }

        private void settings_IsOpenChanged(object sender, EventArgs e)
        {
            HostHide(!settingsFlayout.IsOpen);
            /*Properties.Settings.Default.Save();
            Properties.Settings.Default.Upgrade();
            Properties.Settings.Default.Reload();*/

            if (settingsFlayout.IsOpen)
            {
                Properties.Settings.Default.Save();
                objectFlayout.IsOpen = !settingsFlayout.IsOpen;
            }
            else
            {
                Properties.Settings.Default.Reload();
            }
            //filter.IsOpen = !settings.IsOpen;
            /*if (settings.IsOpen)
            {
                HostHide(false);
                filter.IsOpen = !settings.IsOpen;
                Properties.Settings.Default.Reload();
                //certificate.Text = Properties.Settings.Default.x509;
            }
            else
            {
                HostHide(true);
                filter.IsOpen = settings.IsOpen;
                //Properties.Settings.Default.x509 = certificate.Text;
                //Properties.Settings.Default.Save();
            }*/
        }

        private async void ObjectsFlayoutClick(object sender, RoutedEventArgs e)
        {
            if (!coreConnected)
            {
                MessageDialogResult result = await this.ShowMessageAsync("Попередження!", "Перед вибором об’єктів необхідне активне підключення до серверу!\r\nПідключитись до сервера?", MessageDialogStyle.AffirmativeAndNegative);
                if (result == MessageDialogResult.Affirmative)
                {
                    if (signin.IsEnabled)
                    {
                        ButtonAutomationPeer peer = new ButtonAutomationPeer(signin);
                        IInvokeProvider invokeProv = peer.GetPattern(PatternInterface.Invoke)
                                                        as IInvokeProvider;
                        invokeProv.Invoke();
                    }
                    else
                    {
                        await this.ShowMessageAsync("Помилка", "Необхідно обрати діючий сертифікат!", MessageDialogStyle.Affirmative);
                        settingsFlayout.IsOpen = true;
                    }
                }
            }
            else
            {
                objectFlayout.IsOpen = !objectFlayout.IsOpen;
            }
        }

        private void HostHide(bool visible)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                eventsView.Visibility = (goFlag & visible) ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
                hostExitCam.Visibility = (goFlag & visible) ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
                hostEnterCam.Visibility = (goFlag & visible) ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
                hostUpCam.Visibility = (goFlag & visible) ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            }));
        }

        private void cores_IsOpenChanged(object sender, EventArgs e)
        {

            if (objectFlayout.IsOpen)
            {
                settingsFlayout.IsOpen = !objectFlayout.IsOpen;
                try
                {
                    HostHide(false);
                    if (coresView.Items.Count == 0 || entriesView.Items.Count == 0)
                    {
                        coresList = new List<satmClient.Cores>();
                        entriesList = new List<satmClient.Entries>();
                        coresList = satm.GetCores().OrderBy(x => x.Description).ToList();
                        entriesList = satm.GetEntries().OrderBy(x => x.core).ToList();
                        foreach (satmClient.Cores citem in coresList)
                            coresView.Items.Add(citem.Description);
                        foreach (satmClient.Entries eitem in entriesList)
                            entriesView.Items.Add(eitem.core + "\\" + eitem.entryDescription);
                    }
                }
                catch (CommunicationException ex)
                {
                    this.ShowMessageAsync("Помилка!", "Помилка при підключенні до бази конфігурацій!\r\n" + ex.Message, MessageDialogStyle.Affirmative);
                    connectionStatus.Text = "увійти";
                    coreConnected = false;
                    OnPropertyChanged("coreConnected");
                    objectFlayout.IsOpen = false;
                    HostHide(false);
                }
                catch (TimeoutException ex)
                {
                    this.ShowMessageAsync("Помилка!", "Відсутня відповідь від сервера!\r\n" + ex.Message, MessageDialogStyle.Affirmative);
                    connectionStatus.Text = "увійти";
                    coreConnected = false;
                    OnPropertyChanged("coreConnected");
                    objectFlayout.IsOpen = false;
                    HostHide(false);
                }
            }
            else
            {
                HostHide(true);
            }
        }

        private void coresView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            monitor.Connect(coresList.Find(x => x.Description.Contains(coresView.SelectedItem.ToString())).IP, 1, 0x00000001 ^ 0x00000008 ^ 0x00000010 ^ 0x00000020 ^ 0x00000040, 0);
            objectFlayout.IsOpen = !objectFlayout.IsOpen;
        }

        private void RenewEventsClick(object sender, RoutedEventArgs e)
        {
            ocxEnter.Disconnect();
            ocxUp.Disconnect();
            ocxExit.Disconnect();
            objectFlayout.IsOpen = false;
            List<satmClient.Entries> selectedEntries = new List<satmClient.Entries>();
            if (entriesView.SelectedItems.Count == 0 && coresView.SelectedItems.Count == 0)
            {
                this.ShowMessageAsync("Помилка", "Необхідно обрати принаймні один об’єкт!", MessageDialogStyle.Affirmative);
                return;
            }

            if (entriesView.SelectedItems.Count > 0)
            {
                String entry;
                foreach (Object item in entriesView.SelectedItems)
                {
                    entry = item as String;
                    string[] tmp = entry.Split('\\');
                    selectedEntries.Add(new satmClient.Entries { core = tmp[0], entryDescription = tmp[1] });
                }
            }
            else if (coresView.SelectedItems.Count > 0)
            {
                //MessageBox.Show(listBox2.SelectedItems.Count.ToString());
                String s;
                foreach (Object item in coresView.SelectedItems)
                {
                    s = item as String;
                    string core = coresList.Find(y => y.Description.Contains(s)).Name;
                    //MessageBox.Show(core);

                    foreach (satmClient.Entries st in entriesList.FindAll(x => x.core.Contains(core)))
                    {
                        //MessageBox.Show(st.core);
                        selectedEntries.Add(new satmClient.Entries { core = coresList.Find(x => x.Description.Contains(s)).Name, entryDescription = st.entryDescription });
                        //cores.Add(owners.Find(x => x.Description.Contains(s)).Name);
                    }
                }
            }
            try
            {
                if (offlineData.IsChecked == true)
                    eventsList = satm.GetEvents(selectedEntries, startDate.SelectedDate.Value.Date + startTime, endDate.SelectedDate.Value.Date + endTime, false);
                else
                    eventsList = satm.GetEventsQuery(selectedEntries, startDate.SelectedDate.Value.Date + startTime, endDate.SelectedDate.Value.Date + endTime, false);
                //eventsList = satm.GetEvents(selectedEntries, startDate.SelectedDate.Value.Date + startTime, endDate.SelectedDate.Value.Date + endTime, false);
            }
            catch (CommunicationException ex)
            {
                HostHide(false);
                this.ShowMessageAsync("Помилка!", "Помилка при підключенні до бази конфігурацій!\r\n" + ex.Message, MessageDialogStyle.Affirmative);
                connectionStatus.Text = "увійти";
                coreConnected = false;
                OnPropertyChanged("coreConnected");
                objectFlayout.IsOpen = false;
            }
            catch (TimeoutException ex)
            {
                HostHide(false);
                this.ShowMessageAsync("Помилка!", "Відсутня відповідь від сервера!\r\n" + ex.Message, MessageDialogStyle.Affirmative);
                connectionStatus.Text = "увійти";
                coreConnected = false;
                OnPropertyChanged("coreConnected");
                objectFlayout.IsOpen = false;
            }
            eventsView.ItemsSource = eventsList;
            goFlag = true;
            HostHide(true);
        }

        private void startDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            startDate.SelectedDate = startDate.SelectedDate.Value.Date;
        }

        private void endDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            endDate.SelectedDate = endDate.SelectedDate.Value.Date.AddDays(1).AddSeconds(-1);
        }

        private void textStartTime_TextChanged(object sender, TextChangedEventArgs e)
        {
            BindingOperations.GetBindingExpressionBase(textStartTime, TextBox.TextProperty).UpdateSource();
            if (startTime.Days > 0)
            {
                startTime = new TimeSpan(startTime.Hours, startTime.Minutes, startTime.Seconds);
                BindingOperations.GetBindingExpressionBase(textStartTime, TextBox.TextProperty).UpdateTarget();
            }
        }

        private void textEndTime_TextChanged(object sender, TextChangedEventArgs e)
        {
            BindingOperations.GetBindingExpressionBase(textEndTime, TextBox.TextProperty).UpdateSource();
            if (endTime.Days > 0)
            {
                endTime = new TimeSpan(endTime.Hours, endTime.Minutes, endTime.Seconds);
                BindingOperations.GetBindingExpressionBase(textEndTime, TextBox.TextProperty).UpdateTarget();
            }
        }

        private void listEvents_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView ls = (ListView)sender;
            if (ls.SelectedItem is satmClient.Events)
            {
                satmClient.Events selectedEvent = (satmClient.Events)ls.SelectedItem;
                selectedEvent.user = "*";
                upCam = selectedEvent.upCam;
                ocxUp.Tag = upCam.ToString();
                enterCam = selectedEvent.enterCam;
                ocxEnter.Tag = enterCam.ToString();
                exitCam = selectedEvent.exitCam;
                ocxExit.Tag = exitCam.ToString();
                upTime = selectedEvent.upTime;
                enterTime = selectedEvent.enterTime;
                exitTime = selectedEvent.exitTime;
                //DateTime tempDate = DateTime.Now;

                //if (ocxUp.IsConnecte)
                if (ocxUp.IsConnected() == 1 && ocxUp.GetCurIP() == selectedEvent.ip && tmpUpCam > 0)
                {
                    ocxUp.DoReactMonitor("MONITOR||ACTIVATE_CAM|cam<" + selectedEvent.upCam + ">");
                    ocxUp.DoReactMonitor("MONITOR||ARCH_FRAME_TIME|cam<" + upCam + ">,date<" + upTime.ToString("dd-MM-yy") + ">,time<" + upTime.ToString("HH:mm:ss") + ">");
                }
                else
                {
                    tmpUpCam = 0;
                    ocxUp.Disconnect();
                    ocxUp.Connect(selectedEvent.ip, "", "", "", 0);
                }
                if (ocxEnter.IsConnected() == 1 && ocxEnter.GetCurIP() == selectedEvent.ip && tmpEnterCam > 0)
                {
                    ocxEnter.DoReactMonitor("MONITOR||ACTIVATE_CAM|cam<" + selectedEvent.enterCam + ">");
                    ocxEnter.DoReactMonitor("MONITOR||ARCH_FRAME_TIME|cam<" + enterCam + ">,date<" + enterTime.ToString("dd-MM-yy") + ">,time<" + enterTime.ToString("HH:mm:ss") + ">");
                }
                else
                {
                    tmpEnterCam = 0;
                    ocxEnter.Disconnect();
                    ocxEnter.Connect(selectedEvent.ip, "", "", "", 0);
                }
                if (ocxExit.IsConnected() == 1 && ocxExit.GetCurIP() == selectedEvent.ip && tmpExitCam > 0)
                {
                    ocxExit.DoReactMonitor("MONITOR||ACTIVATE_CAM|cam<" + selectedEvent.exitCam + ">");
                    ocxExit.DoReactMonitor("MONITOR||ARCH_FRAME_TIME|cam<" + exitCam + ">,date<" + exitTime.ToString("dd-MM-yy") + ">,time<" + exitTime.ToString("HH:mm:ss") + ">");
                }
                else
                {
                    tmpExitCam = 0;
                    ocxExit.Disconnect();
                    ocxExit.Connect(selectedEvent.ip, "", "", "", 0);
                }
                //MessageBox.Show((DateTime.Now - tempDate).ToString());

                /*Thread renewColor = new Thread(EventsRenewColors);
                renewColor.Start();*/
                //ls.ItemContainerStyleSelector = new EventStyleSelector();
            }
        }

        private void EventsRenewColors()
        {
            eventsView.Dispatcher.Invoke(
                  System.Windows.Threading.DispatcherPriority.Normal,
                  new Action(
                    delegate ()
                    {
                        eventsView.ItemContainerStyleSelector = new EventStyleSelector();
                    }
                ));

        }

        private void ocx_OnVideoFrame(object sender, AxACTIVEXLib._DCamMonitorEvents_OnVideoFrameEvent e)
        {
            if (e.cam_id == upCam)
            {
                tmpUpCam = e.cam_id;
                tmpUpTime = e.date;
            }
            if (e.cam_id == enterCam)
            {
                tmpEnterCam = e.cam_id;
                tmpEnterTime = e.date;
            }
            if (e.cam_id == exitCam)
            {
                tmpExitCam = e.cam_id;
                tmpExitTime = e.date;
            }
        }

        private void certificate_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            CertificateManager mgr = new CertificateManager();
            mgr.ShowDialog();
            //Properties.Settings.Default.Save();
            //Properties.Settings.Default.Upgrade();
            Properties.Settings.Default.Reload();
            settingsFlayout.IsOpen = !settingsFlayout.IsOpen;
            X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);
            if (store.Certificates.Find(X509FindType.FindByThumbprint, Properties.Settings.Default.x509, true).Count <= 0)
            {
                this.ShowMessageAsync("Помилка", "Необхідно обрати діючий сертифікат!", MessageDialogStyle.Affirmative);
                signin.IsEnabled = false;
            }
            else
            {
                signin.IsEnabled = true;
            }
        }

        private void ocxEnter_MouseDownEvent(object sender, AxACTIVEXLib._DCamMonitorEvents_MouseDownEvent e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                if (e.button == 1)
                {
                    Grid.SetColumn(hostEnterCam, 0);
                    Grid.SetColumnSpan(hostEnterCam, 2);
                    Grid.SetRow(hostEnterCam, 0);
                    Grid.SetRowSpan(hostEnterCam, 2);
                    hostUpCam.Visibility = System.Windows.Visibility.Hidden;
                    hostExitCam.Visibility = System.Windows.Visibility.Hidden;
                }
                else
                {
                    Grid.SetColumn(hostEnterCam, 0);
                    Grid.SetColumnSpan(hostEnterCam, 1);
                    Grid.SetRow(hostEnterCam, 1);
                    Grid.SetRowSpan(hostEnterCam, 1);
                    hostUpCam.Visibility = System.Windows.Visibility.Visible;
                    hostExitCam.Visibility = System.Windows.Visibility.Visible;
                }
            }
        }

        private void ocxUp_MouseDownEvent(object sender, AxACTIVEXLib._DCamMonitorEvents_MouseDownEvent e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                if (e.button == 1)
                {
                    Grid.SetColumn(hostUpCam, 0);
                    Grid.SetColumnSpan(hostUpCam, 2);
                    Grid.SetRow(hostUpCam, 0);
                    Grid.SetRowSpan(hostUpCam, 2);
                    hostEnterCam.Visibility = System.Windows.Visibility.Hidden;
                    hostExitCam.Visibility = System.Windows.Visibility.Hidden;
                }
                else
                {
                    Grid.SetColumn(hostUpCam, 1);
                    Grid.SetColumnSpan(hostUpCam, 1);
                    Grid.SetRow(hostUpCam, 0);
                    Grid.SetRowSpan(hostUpCam, 1);
                    hostEnterCam.Visibility = System.Windows.Visibility.Visible;
                    hostExitCam.Visibility = System.Windows.Visibility.Visible;
                }
            }

        }

        private void ocxExit_MouseDownEvent(object sender, AxACTIVEXLib._DCamMonitorEvents_MouseDownEvent e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                if (e.button == 1)
                {
                    Grid.SetColumn(hostExitCam, 0);
                    Grid.SetColumnSpan(hostExitCam, 2);
                    Grid.SetRow(hostExitCam, 0);
                    Grid.SetRowSpan(hostExitCam, 2);
                    hostUpCam.Visibility = System.Windows.Visibility.Hidden;
                    hostEnterCam.Visibility = System.Windows.Visibility.Hidden;
                }
                else
                {
                    Grid.SetColumn(hostExitCam, 1);
                    Grid.SetColumnSpan(hostExitCam, 1);
                    Grid.SetRow(hostExitCam, 1);
                    Grid.SetRowSpan(hostExitCam, 1);
                    hostUpCam.Visibility = System.Windows.Visibility.Visible;
                    hostEnterCam.Visibility = System.Windows.Visibility.Visible;
                }
            }

        }

        private void ocxEnter_SizeChanged(object sender, EventArgs e)
        {
            if (ocxEnter.IsConnected() == 1)
            {
                ocxEnter.DoReactMonitor("MONITOR||ARCH_FRAME_TIME|cam<" + enterCam + ">,date<" + tmpEnterTime.ToString("dd-MM-yy") + ">,time<" + tmpEnterTime.ToString("HH:mm:ss") + ">");
            }
        }

        private void ocxUp_SizeChanged(object sender, EventArgs e)
        {
            if (ocxUp.IsConnected() == 1)
            {
                ocxUp.DoReactMonitor("MONITOR||ARCH_FRAME_TIME|cam<" + upCam + ">,date<" + tmpUpTime.ToString("dd-MM-yy") + ">,time<" + tmpUpTime.ToString("HH:mm:ss") + ">");
            }
        }

        private void ocxExit_SizeChanged(object sender, EventArgs e)
        {
            if (ocxExit.IsConnected() == 1)
            {
                ocxExit.DoReactMonitor("MONITOR||ARCH_FRAME_TIME|cam<" + exitCam + ">,date<" + tmpExitTime.ToString("dd-MM-yy") + ">,time<" + tmpExitTime.ToString("HH:mm:ss") + ">");
            }
        }

        private void ocxExit_OnCamListChange(object sender, AxACTIVEXLib._DCamMonitorEvents_OnCamListChangeEvent e)
        {
            ocxExit.ShowCam(exitCam, 1, 1);
            ocxExit.DoReactMonitor("MONITOR||ARCH_FRAME_TIME|cam<" + exitCam + ">,date<" + exitTime.ToString("dd-MM-yy") + ">,time<" + exitTime.ToString("HH:mm:ss") + ">");
        }

        private void ocxUp_OnCamListChange(object sender, AxACTIVEXLib._DCamMonitorEvents_OnCamListChangeEvent e)
        {
            ocxUp.ShowCam(upCam, 1, 1);
            ocxUp.DoReactMonitor("MONITOR||ARCH_FRAME_TIME|cam<" + upCam + ">,date<" + upTime.ToString("dd-MM-yy") + ">,time<" + upTime.ToString("HH:mm:ss") + ">");
        }

        private void ocxEnter_OnCamListChange(object sender, AxACTIVEXLib._DCamMonitorEvents_OnCamListChangeEvent e)
        {
            ocxEnter.ShowCam(enterCam, 1, 1);
            ocxEnter.DoReactMonitor("MONITOR||ARCH_FRAME_TIME|cam<" + enterCam + ">,date<" + enterTime.ToString("dd-MM-yy") + ">,time<" + enterTime.ToString("HH:mm:ss") + ">");
        }

        private void ocxExit_OnConnectStateChanged(object sender, AxACTIVEXLib._DCamMonitorEvents_OnConnectStateChangedEvent e)
        {
            tmpExitCam = 0;
        }

        private void ocxUp_OnConnectStateChanged(object sender, AxACTIVEXLib._DCamMonitorEvents_OnConnectStateChangedEvent e)
        {
            tmpUpCam = 0;
        }

        private void ocxEnter_OnConnectStateChanged(object sender, AxACTIVEXLib._DCamMonitorEvents_OnConnectStateChangedEvent e)
        {
            tmpEnterCam = 0;
        }

        private void GridViewColumnHeaderClickedHandler(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader column = e.OriginalSource as GridViewColumnHeader;
            if (column == null)
            {
                return;
            }

            if (_sortColumn == column)
            {
                // Toggle sorting direction 
                _sortDirection = _sortDirection == ListSortDirection.Ascending ?
                                                   ListSortDirection.Descending :
                                                   ListSortDirection.Ascending;
            }
            else
            {
                // Remove arrow from previously sorted header 
                if (_sortColumn != null)
                {
                    _sortColumn.Column.HeaderTemplate = null;
                    _sortColumn.Column.Width = _sortColumn.ActualWidth - 20;
                }

                _sortColumn = column;
                _sortDirection = ListSortDirection.Ascending;
                column.Column.Width = column.ActualWidth + 20;
            }

            if (_sortDirection == ListSortDirection.Ascending)
            {
                column.Column.HeaderTemplate = Resources["ArrowUp"] as DataTemplate;
            }
            else
            {
                column.Column.HeaderTemplate = Resources["ArrowDown"] as DataTemplate;
            }

            string header = string.Empty;

            // if binding is used and property name doesn't match header content 
            Binding b = _sortColumn.Column.DisplayMemberBinding as Binding;
            if (b != null)
            {
                header = b.Path.Path;
            }

            ICollectionView resultDataView = CollectionViewSource.GetDefaultView(
                                                       eventsView.ItemsSource);
            resultDataView.SortDescriptions.Clear();
            resultDataView.SortDescriptions.Add(
                                        new SortDescription(header, _sortDirection));

        }

        private void mwindow_Closing(object sender, CancelEventArgs e)
        {
            try
            {
                monitor.CloseAll();
                coreConnected = satm.SignOut();
                satm.ChannelFactory.Close();
            }
            catch
            {
                satm.ChannelFactory.Abort();
            }
            coreConnected = false;
            OnPropertyChanged("coreConnected");
            connectionStatus.Dispatcher.Invoke(
                  System.Windows.Threading.DispatcherPriority.Normal,
                  new Action(
                    delegate ()
                    {
                        connectionStatus.Text = "увійти";
                    }
                ));
        }

        private void Previous_Click(object sender, RoutedEventArgs e)
        {
            if (eventsView.SelectedIndex > 0)
            {
                eventsView.SelectedIndex--;
            }
            else
            {
                eventsView.SelectedIndex = eventsView.Items.Count - 1;
            }
            eventsView.ScrollIntoView(eventsView.SelectedItem);
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            if (eventsView.SelectedIndex < eventsView.Items.Count - 1)
            {
                eventsView.SelectedIndex++;
            }
            else
            {
                eventsView.SelectedIndex = 0;
            }
            eventsView.ScrollIntoView(eventsView.SelectedItem);
        }

        private void iColor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Properties.Settings.Default.illegalColor = IllegalColor;
        }

        private void lColor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Properties.Settings.Default.legalColor = LegalColor;
        }

        private void pColor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Properties.Settings.Default.processedColor = ProcessedColor;
        }

    }

    public class EventStyleSelector : StyleSelector
    {
        public override Style SelectStyle(object item,
            DependencyObject container)
        {
            Style st = new Style();
            st.TargetType = typeof(ListViewItem);
            Setter backGroundSetter = new Setter();
            SolidColorBrush brush = new SolidColorBrush();
            backGroundSetter.Property = ListViewItem.BackgroundProperty;
            if (item is satmClient.Events)
            {
                satmClient.Events selectedEvent = (satmClient.Events)item;
                if (selectedEvent.user == String.Empty)
                {
                    Properties.Settings.Default.Reload();
                    if (selectedEvent.legal)
                    {
                        brush.Color = Color.FromArgb(Properties.Settings.Default.legalColor.A, Properties.Settings.Default.legalColor.R, Properties.Settings.Default.legalColor.G, Properties.Settings.Default.legalColor.B);
                        //backGroundSetter.Value = Color.FromArgb(Properties.Settings.Default.legalColor.A, Properties.Settings.Default.legalColor.R, Properties.Settings.Default.legalColor.G, Properties.Settings.Default.legalColor.B);
                    }
                    else
                    {
                        brush.Color = Color.FromArgb(Properties.Settings.Default.illegalColor.A, Properties.Settings.Default.illegalColor.R, Properties.Settings.Default.illegalColor.G, Properties.Settings.Default.illegalColor.B);
                        //backGroundSetter.Value = Properties.Settings.Default.illegalColor;
                    }
                }
                else
                {
                    brush.Color = Color.FromArgb(Properties.Settings.Default.processedColor.A, Properties.Settings.Default.processedColor.R, Properties.Settings.Default.processedColor.G, Properties.Settings.Default.processedColor.B);
                    //backGroundSetter.Value = Properties.Settings.Default.processedColor;
                }
                backGroundSetter.Value = brush;
            }
            st.Setters.Add(backGroundSetter);
            return st;
        }
    }
}
