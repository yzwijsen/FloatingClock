using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Globalization;
using Microsoft.Win32;
using System.Reflection;

namespace FloatingClock
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private bool alwaysOnTop = true;
        private bool clock24Hrs = true;
        private bool showSeconds = false;
        private bool autoStart = false;
        private bool windowLock = false;
        private string currentTheme = "Default";
        private string runningDirectory;

        // Default color theme
        Color defaultBgColor = (Color)ColorConverter.ConvertFromString("#FFFFFF");
        Color defaultFgColor = (Color)ColorConverter.ConvertFromString("#000000");
        // Dark color theme
        Color darkBgColor = (Color)ColorConverter.ConvertFromString("#141311");
        Color darkFgColor = (Color)ColorConverter.ConvertFromString("#A2482E");
        // Light color theme
        Color lightBgColor = (Color)ColorConverter.ConvertFromString("#f9bf3b");
        Color lightFgColor = (Color)ColorConverter.ConvertFromString("#171720");
        // Bright color theme
        Color brightBgColor = (Color)ColorConverter.ConvertFromString("#E64A19");
        Color brightFgColor = (Color)ColorConverter.ConvertFromString("#FFFFFF");
        // Teal color theme
        Color tealBgColor = (Color)ColorConverter.ConvertFromString("#00aba9");
        Color tealFgColor = (Color)ColorConverter.ConvertFromString("#FFFFFF");
        // CB color theme
        Color skyBgColor = (Color)ColorConverter.ConvertFromString("#2d89ef");
        Color skyFgColor = (Color)ColorConverter.ConvertFromString("#eff4ff");

        public MainWindow()
        {
            InitializeComponent();

            // upgrade settings file (in case of new build)
            Upgrade_Settings_Check();

            //load user settings
            Load_Settings();

            // add event handlers
            this.Closed += new EventHandler(App_Closed);
            //App.Current.Exit += new ExitEventHandler(App_Closed);
            MouseDown += Window_MouseDown;

            // hide icon from taskbar
            this.ShowInTaskbar = false;

            // set as topmost if option is set
            if (alwaysOnTop == true)
            {
                this.Topmost = true;
            }

            // start the timer and update clock
            Start_ClockTimer();

            // set running directory (for use in autostart regkey)
            runningDirectory = System.AppDomain.CurrentDomain.BaseDirectory;

            //check if autostart regkey is set or not
            Check_Autostart_Regkey();
        }

        private void Upgrade_Settings_Check()
        {
            if (Properties.Settings.Default.CallUpgrade)
            {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.CallUpgrade = false;
                Properties.Settings.Default.Save();
            }
        }

        private void Start_ClockTimer()
        {
            DispatcherTimer timer = new DispatcherTimer(new TimeSpan(0, 0, 1), DispatcherPriority.Normal, delegate
            {

                string timeFormat;
                if (clock24Hrs == true)
                {
                    if (showSeconds == true)
                    {
                        timeFormat = "HH:mm:ss";
                    }
                    else
                    {
                        timeFormat = "HH:mm";
                    }

                }
                else
                {
                    if (showSeconds == true)
                    {
                        timeFormat = "hh:mm:ss tt";
                    }
                    else
                    {
                        timeFormat = "hh:mm tt";
                    }

                }

                this.lblTime.Content = DateTime.Now.ToString(timeFormat, CultureInfo.InvariantCulture);
            }, this.Dispatcher);
        }

        private void Check_Autostart_Regkey()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
                {
                    if (key != null)
                    {
                        object val = key.GetValue("FloatingClock");
                        if (val == null)
                        {
                            autoStart = false;
                            mnuAutoStart.IsChecked = false;
                        }
                        else
                        {
                            autoStart = true;
                            mnuAutoStart.IsChecked = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Issue reading registry (" + ex + ")", "error");
            }
            
        }


        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (windowLock == false && e.ChangedButton == MouseButton.Left) {
                DragMove();
            }
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            if (alwaysOnTop == true)
            {
                this.Topmost = true;
            }
        }

        private void mnuExit_Click(object sender, RoutedEventArgs e)
        {
            //Save_Settings();
            Application.Current.Shutdown();
        }

        private void App_Closed(object sender, EventArgs e)
        {
            Save_Settings();
        }

        private void Save_Settings()
        {
            Properties.Settings.Default.TextSize = lblTime.FontSize;
            Properties.Settings.Default.Opacity = mainWindow.Opacity;
            Properties.Settings.Default.Theme = currentTheme;
            Properties.Settings.Default.AlwaysOnTop = alwaysOnTop;
            Properties.Settings.Default.Clock24Hrs = clock24Hrs;
            Properties.Settings.Default.ShowSeconds = showSeconds;
            Properties.Settings.Default.WindowPosTop = this.Top;
            Properties.Settings.Default.WindowPosLeft = this.Left;
            Properties.Settings.Default.WindowLock = windowLock;
            Properties.Settings.Default.Save();
        }

        private void Load_Settings()
        {
            // load all settings
            lblTime.FontSize = Properties.Settings.Default.TextSize;
            mainWindow.Opacity = Properties.Settings.Default.Opacity;
            currentTheme = Properties.Settings.Default.Theme;
            alwaysOnTop = Properties.Settings.Default.AlwaysOnTop;
            clock24Hrs = Properties.Settings.Default.Clock24Hrs;
            showSeconds = Properties.Settings.Default.ShowSeconds;
            windowLock = Properties.Settings.Default.WindowLock;


            // load previous window position
            this.Top = Properties.Settings.Default.WindowPosTop;
            this.Left = Properties.Settings.Default.WindowPosLeft;

            //calculate which fontsize submenu item should be checked
            mnuTextSize_CheckSelected();

            //calculate which opacity submenu item should be checked
            mnuOpacity_CheckSelected();

            //load theme and calculate which submenu should be checked
            Load_Theme();

            // set windowlock menu state
            if (windowLock == true)
            {
                mnuWindowLock.IsChecked = true;
            } else
            {
                mnuWindowLock.IsChecked = false;
            }

            //set alwaysOnTop state
            if (alwaysOnTop == true)
            {
                mnuAOT.IsChecked = true;
            } else
            {
                mnuAOT.IsChecked = false;
            }
            
            // set 24hr clock state
            if (clock24Hrs == true)
            {
                mnu24Hrs.IsChecked = true;
            } else
            {
                mnu24Hrs.IsChecked = false;
            }
            // set show seconds state
            if (showSeconds == true)
            {
                mnuShowSeconds.IsChecked = true;
            } else
            {
                mnuShowSeconds.IsChecked = false;
            }
        }

        private void mnuTheme_CheckSelected()
        {
            mnuThemeDefault.IsChecked = false;
            mnuThemeDark.IsChecked = false;
            mnuThemeLight.IsChecked = false;
            mnuThemeBright.IsChecked = false;
            mnuThemeTeal.IsChecked = false;
            mnuThemeSky.IsChecked = false;

            if (currentTheme == "Default") { mnuThemeDefault.IsChecked = true; }
            if (currentTheme == "Dark") { mnuThemeDark.IsChecked = true; }
            if (currentTheme == "Light") { mnuThemeLight.IsChecked = true; }
            if (currentTheme == "Bright") { mnuThemeBright.IsChecked = true; }
            if (currentTheme == "Teal") { mnuThemeTeal.IsChecked = true; }
            if (currentTheme == "Sky") { mnuThemeSky.IsChecked = true; }
        }

        private void mnuOpacity_CheckSelected()
        {
            mnuOpacity25.IsChecked = false;
            mnuOpacity50.IsChecked = false;
            mnuOpacity75.IsChecked = false;
            mnuOpacity100.IsChecked = false;

            if (mainWindow.Opacity == 0.25) { mnuOpacity25.IsChecked = true; }
            if (mainWindow.Opacity == 0.50) { mnuOpacity50.IsChecked = true; }
            if (mainWindow.Opacity == 0.75) { mnuOpacity75.IsChecked = true; }
            if (mainWindow.Opacity == 1) { mnuOpacity100.IsChecked = true; }
        }

        private void mnuTextSize_CheckSelected()
        {
            mnuSizeXS.IsChecked = false;
            mnuSizeS.IsChecked = false;
            mnuSizeM.IsChecked = false;
            mnuSizeL.IsChecked = false;
            mnuSizeXL.IsChecked = false;

            switch ((int)lblTime.FontSize)
            {
                case 12:
                    mnuSizeXS.IsChecked = true;
                    break;
                case 16:
                    mnuSizeS.IsChecked = true;
                    break;
                case 20:
                    mnuSizeM.IsChecked = true;
                    break;
                case 24:
                    mnuSizeL.IsChecked = true;
                    break;
                case 36:
                    mnuSizeXL.IsChecked = true;
                    break;
            }
        }

        private void mnuOpacity25_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.Opacity = 0.25;
            mnuOpacity_CheckSelected();
            Save_Settings();
        }

        private void mnuOpacity50_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.Opacity = 0.50;
            mnuOpacity_CheckSelected();
            Save_Settings();
        }

        private void mnuOpacity75_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.Opacity = 0.75;
            mnuOpacity_CheckSelected();
            Save_Settings();
        }

        private void mnuOpacity100_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.Opacity = 1;
            mnuOpacity_CheckSelected();
            Save_Settings();
        }

        private void mnuSizeXS_Click(object sender, RoutedEventArgs e)
        {
            lblTime.FontSize = 12;
            mnuTextSize_CheckSelected();
            Save_Settings();
        }

        private void mnuSizeS_Click(object sender, RoutedEventArgs e)
        {
            lblTime.FontSize = 16;
            mnuTextSize_CheckSelected();
            Save_Settings();
        }

        private void mnuSizeM_Click(object sender, RoutedEventArgs e)
        {
            lblTime.FontSize = 20;
            mnuTextSize_CheckSelected();
            Save_Settings();
        }

        private void mnuSizeL_Click(object sender, RoutedEventArgs e)
        {
            lblTime.FontSize = 24;
            mnuTextSize_CheckSelected();
            Save_Settings();
        }

        private void mnuSizeXL_Click(object sender, RoutedEventArgs e)
        {
            lblTime.FontSize = 36;
            mnuTextSize_CheckSelected();
            Save_Settings();
        }

        private void mnuAOT_Click(object sender, RoutedEventArgs e)
        {
            if (alwaysOnTop == true)
            {
                alwaysOnTop = false;
                mnuAOT.IsChecked = false;
                this.Topmost = false;
            } else
            {
                alwaysOnTop = true;
                mnuAOT.IsChecked = true;
                this.Topmost = true;
            }
            Save_Settings();
        }

        private void mnu24Hrs_Click(object sender, RoutedEventArgs e)
        {
            if (clock24Hrs == true)
            {
                clock24Hrs = false;
                mnu24Hrs.IsChecked = false;
            } else
            {
                clock24Hrs = true;
                mnu24Hrs.IsChecked = true;
            }
            Save_Settings();
        }

        private void Load_Theme()
        {
            switch (currentTheme)
            {
                case "Default":
                    lblTime.Foreground = new System.Windows.Media.SolidColorBrush(defaultFgColor);
                    mainWindow.Background = new System.Windows.Media.SolidColorBrush(defaultBgColor);
                    break;
                case "Dark":
                    lblTime.Foreground = new System.Windows.Media.SolidColorBrush(darkFgColor);
                    mainWindow.Background = new System.Windows.Media.SolidColorBrush(darkBgColor);
                    break;
                case "Light":
                    lblTime.Foreground = new System.Windows.Media.SolidColorBrush(lightFgColor);
                    mainWindow.Background = new System.Windows.Media.SolidColorBrush(lightBgColor);
                    break;
                case "Bright":
                    lblTime.Foreground = new System.Windows.Media.SolidColorBrush(brightFgColor);
                    mainWindow.Background = new System.Windows.Media.SolidColorBrush(brightBgColor);
                    break;
                case "Teal":
                    lblTime.Foreground = new System.Windows.Media.SolidColorBrush(tealFgColor);
                    mainWindow.Background = new System.Windows.Media.SolidColorBrush(tealBgColor);
                    break;
                case "Sky":
                    lblTime.Foreground = new System.Windows.Media.SolidColorBrush(skyFgColor);
                    mainWindow.Background = new System.Windows.Media.SolidColorBrush(skyBgColor);
                    break;
            }

            mnuTheme_CheckSelected();
        }

        private void mnuThemeDefault_Click(object sender, RoutedEventArgs e)
        {
            currentTheme = "Default";
            Load_Theme();
            Save_Settings();
        }

        private void mnuThemeDark_Click(object sender, RoutedEventArgs e)
        {
            currentTheme = "Dark";
            Load_Theme();
            Save_Settings();
        }

        private void mnuThemeLight_Click(object sender, RoutedEventArgs e)
        {
            currentTheme = "Light";
            Load_Theme();
            Save_Settings();
        }

        private void mnuThemeSky_Click(object sender, RoutedEventArgs e)
        {
            currentTheme = "Sky";
            Load_Theme();
            Save_Settings();
        }

        private void mnuThemeTeal_Click(object sender, RoutedEventArgs e)
        {
            currentTheme = "Teal";
            Load_Theme();
            Save_Settings();
        }

        private void mnuThemeBright_Click(object sender, RoutedEventArgs e)
        {
            currentTheme = "Bright";
            Load_Theme();
            Save_Settings();
        }

        private void mnuAutoStart_Click(object sender, RoutedEventArgs e)
        {
            if (autoStart == true)
            {
                try
                {
                    // remove autostart registry key
                    using (RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
                    {
                        if (key != null)
                        {
                            key.DeleteValue("FloatingClock");
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Unable to remove registry key (" + ex + ")", "error");
                }
                
                
            }
            else
            {
                try
                {
                    // add autostart registry key
                    RegistryKey add = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                    add.SetValue("FloatingClock", "\"" + runningDirectory + "FloatingClock.exe" + "\"");
                    //Debug.WriteLine(runningDirectory);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Unable to create registry key (" + ex + ")", "error");
                }
            }

            Check_Autostart_Regkey();
        }

        private void mnuWindowLock_Click(object sender, RoutedEventArgs e)
        {
            if (windowLock == true)
            {
                windowLock = false;
                mnuWindowLock.IsChecked = false;
            } else
            {
                windowLock = true;
                mnuWindowLock.IsChecked = true;
            }
            Save_Settings();
        }

        private void mnuShowSeconds_Click(object sender, RoutedEventArgs e)
        {
            if (showSeconds == true)
            {
                showSeconds = false;
                mnuShowSeconds.IsChecked = false;
            } else
            {
                showSeconds = true;
                mnuShowSeconds.IsChecked = true;
            }
            Save_Settings();
        }

        private void mnuAbout_Click(object sender, RoutedEventArgs e)
        {
            string version = null;
            try
            {
                version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
            catch
            {
                version = "0";
            }
            MessageBox.Show("Floating Clock by Yannick Zwijsen. (version: " + version +")", "About");

        }
    }
}
