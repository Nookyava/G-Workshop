using System;
using System.IO;
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
using System.Windows.Forms;

namespace WorkshopPub
{
    public partial class MainWindow : Window
    {
        string[] addontypes = {
            "ServerContent",
            "gamemode",
            "map",
            "weapon",
            "vehicle",
            "npc",
            "tool",
            "effects",
            "model"
        };

        string[] addontags = {
            "fun",
            "roleplay",
            "scenic",
            "movie",
            "realism",
            "cartoon",
            "water",
            "comic",
            "build"
        };

        string addonpath, addonimagepath;
        bool finalizeenabled = false;

        /* Main Page */
        public MainWindow()
        {
            InitializeComponent();

            addonTypeComboBox.ItemsSource = addontypes;
            addonTagsList.ItemsSource = addontags;

            string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/[G]Workshop/";

            if (!Directory.Exists(path))
            {
                System.IO.Directory.CreateDirectory(path);
            }
        }

        private void closeButton_Click_1(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void nextButton_Click(object sender, RoutedEventArgs e)
        {
            int currentindex = mainTabBase.SelectedIndex;

            if (currentindex + 1 != -1)
            {
                previousButton.IsEnabled = true;
            }

            if (finalizeenabled)
            {
                uploadButton_Click();
            }

            if (currentindex == 1 && !finalizeenabled)
            {
                nextButton.Content = "Upload";
                finalizeenabled = true;   
            }

            mainTabBase.SelectedIndex = currentindex + 1;
        }

        private void previousButton_Click(object sender, RoutedEventArgs e)
        {
            int currentindex = mainTabBase.SelectedIndex;

            if (currentindex - 1 > 0)
            {
                mainTabBase.SelectedIndex = currentindex - 1;
            }
            else
            {
                mainTabBase.SelectedIndex = currentindex - 1;
                previousButton.IsEnabled = false;
            }

            if (finalizeenabled)
            {
                nextButton.Content = "Next";
                finalizeenabled = false;
            }
        }

        private void uploadButton_Click()
        {
            Addon addon = new Addon();
            string addonname = addonNameTextbox.Text;
            string addontype = Convert.ToString(addonTypeComboBox.SelectedItem);
            string[] addontags = addonTagsList.SelectedItems.Cast<string>().ToArray();
            string[] addonignores = { };

            // JSON
            addon.setJsonDetails(addonname, addontype, addontags, addonignores, addonpath);

            // OUTPUT
            if (addon.createJsonFile())
            {
                if (addon.createGmaFile())
                {
                    if (addon.setImagePath(addonimagepath))
                    {
                        addon.uploadToWorkshop();
                    }
                }
            }
        }

        /* Select Addon Folder Page */
        private void selectRootFolderButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            dialog.SelectedPath = addonpath;
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            addonpath = dialog.SelectedPath;
            selectAddonFolderTextBox.Text = addonpath;
        }

        private void selectAddonFolderTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            addonpath = selectAddonFolderTextBox.Text;
        }

        private void selectAddonFolderTextBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (selectAddonFolderTextBox.Text == "Select Addon Folder")
            {
                selectAddonFolderTextBox.Text = "";
            }
        }  

        /* Json Details Page */
        private void addonNameTextbox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (addonNameTextbox.Text == "Addon Name")
            {
                addonNameTextbox.Text = "";
            }
        }

        private void addonTagsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                if (this.addonTagsList.SelectedItems.Count > 2)
                {
                    this.addonTagsList.SelectedItems.RemoveAt(0);
                }
            }
            else if (this.addonTagsList.SelectedItems.Count > 1)
            {
                this.addonTagsList.SelectedItems.RemoveAt(0);
            }
        }

        /* Addon Image Page */
        private void selectAddonImageBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (selectAddonImageBox.Text == "Select Addon Image")
            {
                selectAddonImageBox.Text = "";
            }
        }

        private void addonimagebrowsebutton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "JPEG File (.jpg)|*.jpg";
            dialog.Multiselect = false;
            dialog.ShowDialog();
            addonimagepath = dialog.FileName;
            selectAddonImageBox.Text = addonimagepath;
        }
    }
}
