using System;
using System.Windows.Controls;
using System.Windows.Input;
using MahApps.Metro.Controls;
using System.Drawing;
using System.Windows.Forms;

namespace YeelightClient
{
    public partial class MainWindow : MetroWindow
    {
        private DeviceScanner deviceScanner;    
        private DeviceIO deviceIO;

        private System.Windows.Forms.ContextMenu contextMenu1;
        private System.Windows.Forms.MenuItem menuItem1;
        private System.ComponentModel.IContainer components;

        bool yenile = false;

        System.Windows.Forms.NotifyIcon ni = new System.Windows.Forms.NotifyIcon();

        public MainWindow()
        {
            InitializeComponent();

            this.deviceIO = new DeviceIO();

            this.deviceScanner = new DeviceScanner();
            this.deviceScanner.StartListening();

            //Send Discovery Message
            this.deviceScanner.SendDiscoveryMessage();

            //Bind the list to the ListView
            lstBulbs.ItemsSource = this.deviceScanner.DiscoveredDevices;




            this.components = new System.ComponentModel.Container();
            this.contextMenu1 = new System.Windows.Forms.ContextMenu();
            this.menuItem1 = new System.Windows.Forms.MenuItem();

            // Initialize contextMenu1
            /*
            this.contextMenu1.MenuItems.AddRange(
                        new System.Windows.Forms.MenuItem[] { this.menuItem1 });
                        */
            // Initialize menuItem1
            /*
            this.menuItem1.Index = 0;
            this.menuItem1.Text = "E&xit";
            this.menuItem1.Click += new System.EventHandler(this.menuItem1_Click);
            */
            ni.Icon = YeelightClient.Properties.Resources.icon;
            ni.Visible = true;
            ni.ContextMenu = this.contextMenu1;
            ni.Text = this.Title;

            ni.DoubleClick +=
                delegate (object sender, EventArgs args)
                {
                    Show();
                    this.Activate();
                    //this.WindowState = WindowState.Normal;
                    this.WindowState = System.Windows.WindowState.Normal;
                    this.Visibility = System.Windows.Visibility.Visible;
                    listeYenile();
                };
            menuItemYenile();
            ni.MouseDown +=
                delegate (object sender, System.Windows.Forms.MouseEventArgs e)
                {
                    if (e.Button == MouseButtons.Right)
                    {
                        menuItemYenile();
                        listeYenile();
                    }
                };
        }

        private void lstBulbs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!yenile)
            {
                var device = this.deviceScanner.DiscoveredDevices[lstBulbs.SelectedIndex];
                panelBulbControl.IsEnabled = false;

                if (this.deviceIO.Connect(device))
                {
                    //Apply current device values to controls
                    //btnToggle.IsChecked = device.State;
                    btnToggle.IsOn = device.State;
                    sliderBrightness.Value = device.Brightness;

                    //Change panel state -> allow modification
                    panelBulbControl.IsEnabled = true;
                }
            }
        }

        private void ni_Select(object sender, EventArgs e)
        {
            System.Windows.Forms.MenuItem menuitems = sender as System.Windows.Forms.MenuItem;
            ampulKontrol(menuitems.Index);
        }

        private void ampulKontrol (int index)
        {
            var device = this.deviceScanner.DiscoveredDevices[index];

            if (this.deviceIO.Connect(device))
            {
                this.deviceIO.Toggle();
            }
            menuItemYenile();
        }

        private void btnToggle_IsCheckedChanged(object sender, EventArgs e)
        {
            if (panelBulbControl.IsEnabled)
            {
                this.deviceIO.Toggle();
            }
        }

        //LostCapture event is used, we don't want to spam the bulb at each change, just the final one
        private void sliderBrightness_LostMouseCapture(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (panelBulbControl.IsEnabled)
            {
                var value = Convert.ToInt32(sliderBrightness.Value);
                var smooth = Convert.ToInt32(sliderSmooth.Value);
                this.deviceIO.SetBrightness(value, smooth);
            }
        }

        //LostCapture event is used, we don't want to spam the bulb at each change, just the final one
        private void ColorCanvas_LostMouseCapture(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (panelBulbControl.IsEnabled)
            {            
                var smooth = Convert.ToInt32(sliderSmooth.Value);
                this.deviceIO.SetColor(colorCanvas.R, colorCanvas.G, colorCanvas.B, smooth);
            }
        }
        
        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState != System.Windows.WindowState.Minimized)
            {
                Show();
                //ni.Visible = false;
                this.ShowInTaskbar = true;
            }
            else
            {
                Hide();
                //ni.Visible = true;
                this.ShowInTaskbar = false;
                menuItemYenile();
            }


            //base.OnStateChanged(e);
        }

        private void menuItem1_Click(object Sender, EventArgs e)
        {
            // Close the form, which closes the application.
            this.Close();
        }

        private void menuItemYenile()
        {
            contextMenu1.MenuItems.Clear();

            for (int i = 0; i < this.deviceScanner.DiscoveredDevices.Count; i++)
            {
                contextMenu1.MenuItems.Add(new System.Windows.Forms.MenuItem(this.deviceScanner.DiscoveredDevices[i].Ip + " - " + this.deviceScanner.DiscoveredDevices[i].State, new System.EventHandler(this.ni_Select)));
            }
            contextMenu1.MenuItems.Add(
                new System.Windows.Forms.MenuItem(
                    "Exit",
                    new System.EventHandler(this.menuItem1_Click)
                    )
                );
        }

        private void listeYenile()
        {
            yenile = true;
            lstBulbs.ItemsSource = null;
            lstBulbs.Items.Clear();

            this.deviceScanner.StartListening();
            //Send Discovery Message
            this.deviceScanner.SendDiscoveryMessage();
            //Bind the list to the ListView
            lstBulbs.ItemsSource = this.deviceScanner.DiscoveredDevices;
            yenile = false;
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            menuItemYenile();
            listeYenile();
        }

        private void Temizle_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            lstBulbs.ItemsSource = null;
            lstBulbs.Items.Clear();
        }
    }
}
