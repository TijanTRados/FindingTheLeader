using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Messaging;
using System.Threading;
using System.IO;
using System.Drawing.Drawing2D;

namespace QueueTestApp
{
    public partial class Form1 : Form
    {
        static int Id, index, remoteId;
        static bool lider = false;
        static string QueueNameLocal = @".\PRIVATE$\QueueName";
        static string QueueNameRemote = @".\PRIVATE$\QueueName";
        static MessageQueue MyQueueLocal;
        static MessageQueue MyQueueRemote;
        List<string> lines = new List<string>();


        public Form1()
        {
            InitializeComponent();

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
        }

        private string uzmiSReda()
        {
            spavaj();
            refresh();
            System.Messaging.Message myMessage = MyQueueRemote.Receive();
            myMessage.Formatter = new XmlMessageFormatter(new String[] { "System.String,mscorlib" });
            listboxset("Procitano: " + myMessage.Body, 3);
            refresh();
            return myMessage.Body.ToString();
        }
        private void staviURed(string poruka)
        {
            spavaj();
            refresh();
            System.Messaging.Message myMessage = new System.Messaging.Message();
            myMessage.Body = poruka;
            MyQueueLocal.Send(myMessage);
            listboxset("Stavljeno: " + myMessage.Body.ToString(), 2);
            refresh();
        }

        //INIT
        private void roundButton1_Click(object sender, EventArgs e)
        {
            try
            {
                Id = Int32.Parse(textBoxLocalUsername.Text);
                Form1 f = new Form1();
                f.Text = Id.ToString();
            }
            catch
            {
                MessageBox.Show("Unijeti pravilan ID");
                return;
            }

            QueueNameLocal += Id.ToString();

            idLabel.Text = Id.ToString();
            idLabel.Visible = true;

            string[] lines = System.IO.File.ReadAllLines("user.txt");
            if (lines.Length == 0) index = 0;
            else
            {
                index = Int32.Parse(lines[lines.Length - 1].Split(' ')[0]) + 1;
            }

            using (System.IO.StreamWriter file = new System.IO.StreamWriter("user.txt", true))
            {
                file.WriteLine(index + " " + Id.ToString());
            }

            if (MessageQueue.Exists(QueueNameLocal)) MessageQueue.Delete(QueueNameLocal);
            MyQueueLocal = MessageQueue.Create(QueueNameLocal, false);
            textBoxLocalUsername.Visible = false;
            roundButton1.Enabled = false;
            roundButton2.Enabled = true;
        }

        //CONN
        private void roundButton2_Click(object sender, EventArgs e)
        {
            int k = 0;
            using (StreamReader r = new StreamReader("user.txt"))
            {
                string line;
                while ((line = r.ReadLine()) != null)
                {
                    lines.Add(line);
                    k++;
                }
            }

            try
            {
                remoteId = Int32.Parse(lines[index - 1].Split(' ')[1]);
            }
            catch
            {
                remoteId = Int32.Parse(lines[k - 1].Split(' ')[1]);
            }

            label5.Visible = true;
            label6.Text = "rep " + Id.ToString();
            label6.Visible = true;
            label3.Visible = true;
            label4.Text = "rep " + remoteId.ToString();
            label4.Visible = true;

            QueueNameRemote += remoteId.ToString();
            roundButton2.Enabled = false;
            roundButton3.Enabled = true;
            MyQueueRemote = new MessageQueue(QueueNameRemote, false, false, QueueAccessMode.Receive);
        }

        //RUN
        private void roundButton3_Click(object sender, EventArgs e)
        {
            //slaganje listView u listu redaka
            listView1.View = View.List;
            staviURed(Id.ToString());
            //dok lider nije pronađen
            while (!lider)
            {
                refresh();
                string procitano = uzmiSReda();
                refresh();
                //ako poruka počinje sa "Vođa.."
                if (procitano.Split(' ')[0] == "Vođa")
                {
                    lider = true; //postavi zastavicu
                    staviURed(procitano); //proslijedi info
                }
                else
                { //ukoliko vođa još ne postoji
                    if (Int32.Parse(procitano) < Id)
                    { //manji ID ne prosijeđuj
                        continue;
                    }
                    else if (Int32.Parse(procitano) > Id)
                    { //veći ID proslijedi
                        staviURed(procitano);
                    }
                    else
                    { //ako otkriješ da si primio vlasitit ID, ti si VOĐA!
                        lider = true;
                        pictureBox2.Visible = true;
                        listboxset("Ja sam VOĐA", 1);
                        staviURed("Vođa je " + Id.ToString());
                    }
                }
            }
            roundButton3.Enabled = false;
        }


        int pomak = 0;
        delegate void SetTextCallback(string text, int col);

        private void refresh()
        {
            if (this.listView1.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(listboxset);
                this.Invoke(d, new object[] { });
            }
            else
            {
                this.listView1.Refresh();
            }
        }

        private void listboxset(string output, int col)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (this.listView1.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(listboxset);
                this.Invoke(d, new object[] { output });
            }
            else
            {
                ListViewItem novi = new ListViewItem();
                novi.Text = (pomak++).ToString() + " " + tab(pomak - 1) + output;
                switch (col)
                {
                    case 1: novi.ForeColor = Color.Red;
                        novi.Font = new Font("Sagoe UI", 11, FontStyle.Regular);
                        break;
                    case 2: novi.ForeColor = Color.Green;
                        break;
                    case 3:
                        novi.ForeColor = Color.Blue;
                        break;
                }
                this.listView1.Items.Add(novi);
                this.listView1.Refresh();
                listView1.View = View.List;
            }
        }

        public void spavaj()
        {
            Random r = new Random();
            int sleep = r.Next(1, 2000);
            Thread.Sleep(sleep);
        }

        public string tab(int i)
        {
            string tabovi = "";
            for (int j = 0; j < i; j++)
            {
                tabovi = tabovi + "     ";
            }
            return tabovi;
        }
    }
}
