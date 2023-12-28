using Avalonia.Controls;
using System.Collections.Generic;
using System;
using Avalonia.Input;
using Avalonia.Interactivity;

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
                            //These are going to be a pain to get working cross-platform.
                            break;
                        case 2:
                            //Shutdown
                            //These are going to be a pain to get working cross-platform.
                            break;
                    }                    
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