using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Threading.Tasks;

namespace Client
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public static string dataSend = null;
        public static bool connected = false;
        public static string ip;
        public static int port;
        static listen l;

        private void Form1_Load(object sender, EventArgs e)
        {
            btnRelease.Enabled = false;
            btnDisconnect.Enabled = false;
            lblResponse.Text = "";

            abilitatore();
        }
        public void abilitatore()
        {
            if (connected == false)
            {
                txtID.Enabled = false;
                btnSetVM.Enabled = false;
                btnStart.Enabled = false;
                btnStop.Enabled = false;
                btnReboot.Enabled = false;
                btnRelease.Enabled = false;
            }
            else if (connected == true)
            {
                txtID.Enabled = true;
                btnSetVM.Enabled = true;
                btnStart.Enabled = true;
                btnStop.Enabled = true;
                btnReboot.Enabled = true;
                btnRelease.Enabled = true;
            }
        }

        class listen
        {
            static IPAddress ipAddress = IPAddress.Parse(ip);
            static IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);
            static Socket sender = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            public void connect()
            {
                try
                {
                    string data = "";
                    try
                    {
                        sender.Connect(remoteEP);
                    }
                    catch (ArgumentNullException ane)
                    {
                        Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
                    }
                    catch (SocketException se)
                    {
                        Console.WriteLine("SocketException : {0}", se.ToString());
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Unexpected exception : {0}", e.ToString());
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
            public void stop()
            {
                try
                {
                    sender.Shutdown(SocketShutdown.Both);
                    sender.Close();
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
            public string sendMessage(string message)
            {
                byte[] msg = Encoding.ASCII.GetBytes(message);
                int bytesSent = sender.Send(msg);

                byte[] bytes = new byte[1024];
                int bytesRec = sender.Receive(bytes);
                string mess = Encoding.ASCII.GetString(bytes, 0, bytesRec);
                return mess;
            }
        }

        private void getVMStatus(object o)
        {
            dataSend = "pct status " + txtID.Text.ToString();
            string tmp = l.sendMessage(dataSend);
            string[] status = tmp.Split(' ');

            try
            {
                if (tmp.Contains("running"))
                {
                    btnStart.Invoke((MethodInvoker)delegate ()
                    {
                        btnStart.Enabled = false;
                    });
                    btnStop.Invoke((MethodInvoker)delegate ()
                    {
                        btnStop.Enabled = true;
                    });
                    btnReboot.Invoke((MethodInvoker)delegate ()
                    {
                        btnReboot.Enabled = true;
                    });
                    lblStatus.Invoke((MethodInvoker)delegate ()
                    {
                        lblStatus.Text = "running";
                    });
                    btnCustom.Invoke((MethodInvoker)delegate ()
                    {
                        btnCustom.Enabled = true;
                    });
                    txtCustomCommand.Invoke((MethodInvoker)delegate ()
                    {
                        txtCustomCommand.Enabled = true;
                    });
                }
                else if (tmp.Contains("stopped"))
                {
                    btnStart.Invoke((MethodInvoker)delegate ()
                    {
                        btnStart.Enabled = true;
                    });
                    btnStop.Invoke((MethodInvoker)delegate ()
                    {
                        btnStop.Enabled = false;
                    });
                    btnReboot.Invoke((MethodInvoker)delegate ()
                    {
                        btnReboot.Enabled = false;
                    });
                    lblStatus.Invoke((MethodInvoker)delegate ()
                    {
                        lblStatus.Text = "stopped";
                    });
                    btnCustom.Invoke((MethodInvoker)delegate ()
                    {
                        btnCustom.Enabled = false;
                    });
                    txtCustomCommand.Invoke((MethodInvoker)delegate ()
                    {
                        txtCustomCommand.Enabled = false;
                    });
                }
                else if (tmp == "NULL")
                {
                    Console.WriteLine("AAAAAAAAAA");
                }
            }
            catch (Exception a)
            {
                Console.WriteLine(a.ToString());
            }
            
        }

        private void btnSetVM_Click(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(txtID.Text))
            {
                txtID.Enabled = false;
                btnSetVM.Enabled = false;
                dataSend = "pct status " + txtID.Text.ToString();
                string tmp = l.sendMessage(dataSend);

                string[] status = tmp.Split(' ');   //Lo stato della VM è in status[1]
                lblStatus.Text = status[1];

                btnRelease.Enabled = true;

                getVMStatus(sender);
            }
            else
                MessageBox.Show("Devi inserire ID della VM");
        }
        private void btnRelease_Click(object sender, EventArgs e)
        {
            txtID.Enabled = true;
            btnSetVM.Enabled = true;
            btnRelease.Enabled = false;
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            string[] tmp = txtIP.Text.Split(':');
            ip = tmp[0];
            port = Convert.ToInt32(tmp[1]);

            l = new listen();
            Thread t = new Thread(new ThreadStart(l.connect));
            t.Start();
            connected = true;
            abilitatore();

            btnConnect.Enabled = false;
            txtIP.Enabled = false;
            btnDisconnect.Enabled = true;



            int seconds = 15 * 1000;
            var timer = new System.Threading.Timer(getVMStatus, null, 0, seconds);
        }

        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            l.stop();
            connected = false;
            abilitatore();

            btnConnect.Enabled = true;
            txtIP.Enabled = true;
            btnDisconnect.Enabled = false;
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            string s = "pct start " + txtID.Text.ToString();
            l.sendMessage(s);
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            string s = "pct stop " + txtID.Text.ToString();
            l.sendMessage(s);
        }

        private void btnReboot_Click(object sender, EventArgs e)
        {
            dataSend = "pct reboot " + txtID.Text.ToString();
            l.sendMessage(dataSend);
        }

        private void btnCustom_Click(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(txtCustomCommand.Text))
            {
                lblResponse.Text = "";
                dataSend = "pct exec " + txtID.Text + " " + txtCustomCommand.Text.ToString();
                string s = l.sendMessage(dataSend);
                lblResponse.Text = s;
            }
            else
                MessageBox.Show("Il comando non può essere vuoto");
        }
    }
}
