using Avalonia.Controls;
using System.Collections.Generic;
using System;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia;

namespace MAMEIronXP
{
    public partial class ExitWindow : Window
    {
        public ExitWindow()
        {
            InitializeComponent();
            ExitListBox.ItemsSource = new List<string>(["Exit to operating system", "Reboot system", "Shutdown system"]);
            ExitListBox.SelectedIndex = 0;
            ExitListBox.KeyDown += ExitListBox_KeyDown;
            this.PointerPressed += ExitWindow_PointerPressed;
        }

        private void ExitListBox_KeyDown(object? sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.D1:
                    e.Handled = true;
                    switch (ExitListBox.SelectedIndex)
                    {
                        case 0:
                            Environment.Exit(1);
                            break;
                        case 1:
                            //Reboot
                            if (OperatingSystem.IsWindows())
                            {
                                System.Diagnostics.Process.Start("shutdown.exe", "/r /t 0");
                            }
                            else
                            {
                                System.Diagnostics.Process.Start("shutdown", "-r now");
                            }
                            break;
                        case 2:
                            //Shutdown
                            if (OperatingSystem.IsWindows())
                            {
                                System.Diagnostics.Process.Start("shutdown.exe", "/s /t 0");   
                            }
                            else
                            {
                                System.Diagnostics.Process.Start("shutdown", "-h now");
                            }
                            break;
                    }                    
                    break;
                case Key.Escape:
                    this.Hide();
                    break;
            }
        }

        private void ExitWindow_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            var x = e.GetCurrentPoint(this).Properties;

            //The Tankstick "exit" button at the top right is mapped to a Middle Mouse-click.
            if (x.IsMiddleButtonPressed)
            {
                this.Hide();
            }
        }
        private void ExitWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ExitListBox.Focus();
        }
    }
}