using Avalonia.Controls;
using System.Collections.Generic;
using System;
using Avalonia.Input;

namespace MAMEIronXP
{
    public partial class ExitWindow : Window
    {
        public ExitWindow()
        {
            InitializeComponent();
            //_exitWindowStartTime = DateTime.Now;
            List<string> x = new List<string>();
            x.Add("Exit to operating system");
            x.Add("Reboot system");
            x.Add("Shutdown system");
            ExitListBox.ItemsSource = x;
            ExitListBox.SelectedIndex = 0;
            ExitListBox.Focus();
            ExitListBox.KeyDown += ExitListBox_KeyDown;
            this.PointerPressed += ExitWindow_PointerPressed;
        }

        private void ExitListBox_KeyDown(object? sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.D1:
                    e.Handled = true;
                    Environment.Exit(1);
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
    }
}