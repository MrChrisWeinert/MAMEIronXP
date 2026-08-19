using Avalonia.Controls;
using System.Collections.Generic;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia;

namespace MAMEIronXP
{
    public partial class ExitWindow : Window
    {
        private bool _isRegeneratingCatalog = false;

        public ExitWindow()
        {
            InitializeComponent();
            ExitListBox.ItemsSource = new List<string>(["Exit to operating system", "Reboot system", "Shutdown system", "Regenerate games.json"]);
            ExitListBox.SelectedIndex = 0;
            ExitListBox.KeyDown += ExitListBox_KeyDown;
            this.PointerPressed += ExitWindow_PointerPressed;
        }

        private void ExitListBox_KeyDown(object? sender, KeyEventArgs e)
        {
            if (_isRegeneratingCatalog)
            {
                e.Handled = true;
                return;
            }
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
                        case 3:
                            RegenerateGamesCatalog();
                            break;
                    }
                    break;
                case Key.Escape:
                case Key.V:
                    this.Hide();
                    break;
            }
        }

        /// <summary>
        /// Deletes and rebuilds games.json from MAME's own -listxml output, using the current GameFilter
        /// settings from appsettings.json. This can take a couple of minutes, so it runs off the UI thread
        /// while the menu shows a status message. On success, MAMEIronXP relaunches itself so the newly
        /// generated catalog is picked up.
        /// </summary>
        private async void RegenerateGamesCatalog()
        {
            _isRegeneratingCatalog = true;
            ExitListBox.IsEnabled = false;
            StatusText.Text = "Regenerating games.json from MAME's game list. This can take a few minutes, please wait...";
            StatusText.IsVisible = true;

            AppConfig config = AppConfig.Load();
            try
            {
                await Task.Run(() => GameCatalogService.Regenerate(config));

                StatusText.Text = "games.json regenerated. Restarting...";
                string? exePath = Environment.ProcessPath;
                if (!string.IsNullOrEmpty(exePath))
                {
                    Process.Start(exePath);
                }
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                Logger logger = new Logger(config.LogFile);
                logger.LogInfo($"Error regenerating games.json: {ex}");
                StatusText.Text = $"Error regenerating games.json: {ex.Message}";
                _isRegeneratingCatalog = false;
                ExitListBox.IsEnabled = true;
                ExitListBox.Focus();
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