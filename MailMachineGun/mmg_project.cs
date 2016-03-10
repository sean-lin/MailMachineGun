using Antlr4.StringTemplate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net.Mail;
using System.Threading.Tasks;

namespace MailMachineGun
{
    public class MGConfig
    {
        public String From;
        public String Name;
        public String Password;
        public String Host;
        public bool SSL;
        public int Port; 
    }

    public class MMGProjectConfig
    {
        public String Subject {
            get;
            set;
        }
        public String Body
        {
            get;
            set;
        }
    }

    class ConfigFileConvertor
    {
        static public T load<T>(String path)
        {
            var data = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<T>(data);
        }

        static public void save(Object obj, String path)
        {
            var data = JsonConvert.SerializeObject(obj);
            File.WriteAllText(path, data);
        }
    }

    public class MessageEntry {
        public String Name { get; set; }
        public String Email { get; set; }

        public Dictionary<String, String> Fields;
    }
    class MMGProject
    {
        private MGConfig MailConfig;
        private MMGProjectConfig ProjectConfig;

        public MMGProject(MGConfig MailCfg, MMGProjectConfig PrjCfg)
        {
            MailConfig = MailCfg;
            ProjectConfig = PrjCfg;
        }

        public async Task<bool> sendMessage(MessageEntry Message)
        {
            SmtpClient client = new SmtpClient();
            var from = new MailAddress(MailConfig.From, MailConfig.Name);
            var to = new MailAddress(Message.Email, Message.Name);

            MailMessage mail = new MailMessage(from, to);
            mail.Subject = RenderTemplate(ProjectConfig.Subject, Message.Fields);
            mail.SubjectEncoding = System.Text.Encoding.UTF8;
            mail.Body = RenderTemplate(ProjectConfig.Body, Message.Fields);
            mail.BodyEncoding = System.Text.Encoding.UTF8;

            client.Port = MailConfig.Port;
            client.EnableSsl = MailConfig.SSL;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;
            client.Host = MailConfig.Host;

            client.Credentials = new NetworkCredential(MailConfig.From, MailConfig.Password);

            try
            {
                await client.SendMailAsync(mail);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private String RenderTemplate(String Template, Dictionary<String, String> Message)
        {
            var st = new Template(Template, '$', '$');
            foreach(var i in Message)
            {
                st.Add(i.Key, i.Value);
            }
            return st.Render();
        }
    }
}
