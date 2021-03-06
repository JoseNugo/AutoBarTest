﻿using System;
using Android.App;
using Android.Widget;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace AutoBarTest
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {

        EditText editIp, editPort, editSend;
        Button btnSend;
        TextView txtViewRecived;
        Timer timer;
        string strMessage = "";
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.activity_main);

            Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

            FloatingActionButton fab = FindViewById<FloatingActionButton>(Resource.Id.fab);
            fab.Click += FabOnClick;

            View contentMain = this.FindViewById<View>(Resource.Id.MainContentLayout);
            editIp = contentMain.FindViewById<EditText>(Resource.Id.editIP);
            editPort = contentMain.FindViewById<EditText>(Resource.Id.editPort);
            editSend = contentMain.FindViewById<EditText>(Resource.Id.editSend);
            btnSend = contentMain.FindViewById<Button>(Resource.Id.btnSend);
            txtViewRecived = contentMain.FindViewById<TextView>(Resource.Id.txtViewRecived);
            btnSend.Click += BtnSend_Click;

            timer = new Timer(SyncTextView, DateTimeOffset.UtcNow, 0, 1000);

        }

        private void SyncTextView(object state = null)
        {
            try
            {
                RunOnUiThread(() =>
                {
                    txtViewRecived.Text = strMessage;
                });
            }
            catch (Exception)
            {

            }
        }

        private void SendSocket(string strIP, string strPort, string strSend)
        {
            byte[] bytes = new byte[1024];

            IPAddress ipAddress = IPAddress.Parse(strIP);// GetIPAddressInterNetwork();
            IPEndPoint remoteEP = new IPEndPoint(ipAddress, int.Parse(strPort));
            Socket socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                strMessage = "Inicio\n";
                socket.Connect(remoteEP);
                socket.ReceiveTimeout = 50000;
                byte[] msg = Encoding.ASCII.GetBytes(strSend);
                int bytesSent = socket.Send(msg);

                while (!strMessage.Contains("0008"))
                {
                    int bytesRec = socket.Receive(bytes);
                    var strR = Encoding.ASCII.GetString(bytes, 0, bytesRec);

                    if (string.IsNullOrEmpty(strR))
                        break;

                    strMessage += (strR + "\n");
                }
                strMessage += "FIM";
            }
            catch (ArgumentNullException ane)
            {
                strMessage = ane.ToString();

            }
            catch (SocketException se)
            {
                strMessage = se.ToString();

            }
            catch (Exception e)
            {
                strMessage = e.ToString();
            }
            finally
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }
        }

        private void BtnSend_Click(object sender, EventArgs e)
        {
            Task.Run(() => { SendSocket(editIp.Text, editPort.Text, editSend.Text); });
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_main, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            int id = item.ItemId;
            if (id == Resource.Id.action_settings)
            {
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        private void FabOnClick(object sender, EventArgs eventArgs)
        {
            View view = (View)sender;
            Snackbar.Make(view, "Replace with your own action", Snackbar.LengthLong)
                .SetAction("Action", (Android.Views.View.IOnClickListener)null).Show();
        }
    }
}

