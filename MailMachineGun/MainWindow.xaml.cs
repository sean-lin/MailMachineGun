using CsvHelper;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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

namespace MailMachineGun
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private MGConfig MailConfig;

        public static readonly DependencyProperty ProjectConfigProperty = DependencyProperty.Register("ProjectConfig", typeof(MMGProjectConfig), typeof(MainWindow));
        public MMGProjectConfig ProjectConfig{
            get { return (MMGProjectConfig)GetValue(ProjectConfigProperty); }
            set { SetValue(ProjectConfigProperty, value); }
        }

        public static readonly DependencyProperty MessagesProperty = DependencyProperty.Register("Messages", typeof(List<MessageEntry>), typeof(MainWindow));
        public List<MessageEntry> Messages
        {
            get { return (List<MessageEntry>)GetValue(MessagesProperty); }
            set { SetValue(MessagesProperty, value); }
        }

        public MainWindow()
        {
            InitializeComponent();
            MailConfig = ConfigFileConvertor.load<MGConfig>("mmgcfg.json");


            if(LoadProjectConfig() == false)
            {
                ProjectConfig = new MMGProjectConfig()
                {
                    Body = "你好， $name$",
                    Subject = "ejoy测试邮件",
                };
            }
        }

        private void menuSave_Click(object sender, RoutedEventArgs e)
        {
            SaveProjectConfig();
        }

        private void menuClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void menuLoad_Click(object sender, RoutedEventArgs e)
        {
            LoadProjectConfig();
        }

        private void menuConfig_Click(object sender, RoutedEventArgs e)
        {
        }

        private void buttonLoad_Click(object sender, RoutedEventArgs e)
        {
            Load();
        }

        private void buttonSend_Click(object sender, RoutedEventArgs e)
        {
            SendMail();
        }

        private async void SendMail()
        {
            var Project = new MMGProject(MailConfig, ProjectConfig);
            progressBarSending.Value = 0;
            progressBarSending.Maximum  = Messages.Count;
            foreach(var entry in Messages)
            {
                await Project.sendMessage(entry);
                progressBarSending.Value++;
            }
            MessageBox.Show("发送完成！", "完成", MessageBoxButton.OK);
        }

        private void Load()
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                LoadFile(openFileDialog.FileName);
            }
        }

        private void LoadFile(String path)
        {
            var newMessages = new List<MessageEntry>();

            var file = new FileInfo(path);
            var csvCfg = new CsvHelper.Configuration.CsvConfiguration();
            csvCfg.DetectColumnCountChanges = true;
            csvCfg.IsHeaderCaseSensitive = false;
            csvCfg.TrimFields = true;
            csvCfg.TrimHeaders = true;
            csvCfg.IgnoreHeaderWhiteSpace = true;

            using (var reader = file.OpenText()) {
                var csv = new CsvReader(reader, csvCfg);
                List<String> headers = null;

                while (csv.Read())
                {
                    var entry = new MessageEntry()
                    {
                        Fields = new Dictionary<string, string>(),
                    };
                    if (headers == null)
                    {
                        headers = csv.FieldHeaders.Select<String, String>(header => header.Trim()).ToList<String>();
                    }

                    foreach (var header in headers)
                    {   
                        String value = csv.GetField<String>(header);
                        if(header.Equals(@"name"))
                        {
                            entry.Name = value;
                        }
                        else if (header.Equals(@"email"))
                        {
                            entry.Email = value; 
                        }
                        entry.Fields.Add(header, value);
                    }
                    if (entry.Email != null && entry.Name != null) {
                        newMessages.Add(entry);
                    }
                }
            }
            Messages = newMessages;
        }

        private bool LoadProjectConfig()
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "MMG files (*.mmg)|*.mmg|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                ProjectConfig = ConfigFileConvertor.load<MMGProjectConfig>(openFileDialog.FileName);
                return true; 
            }
            return false;
        }

        private bool SaveProjectConfig()
        {
            var dialog = new SaveFileDialog();
            dialog.Filter = "MMG files (*.mmg)|*.mmg|All files (*.*)|*.*";
            if (dialog.ShowDialog() == true)
            {
                ConfigFileConvertor.save(ProjectConfig, dialog.FileName);
                return true; 
            }
            return false;
        }
    }
}
