﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NetIRC;

namespace DesktopCS.Forms
{
    public partial class MainForm : Form
    {
        private string RTF;
        private NetIRC.Client Client;

        private delegate void AddLineDelegate(int tabIndex, string line);
        private delegate CTabPage AddTabDelegate(string title, TabType type);
        private AddLineDelegate _addline;
        private AddTabDelegate _addtab;

        public MainForm()
        {
            InitializeComponent();

            RTF = "{\\rtf{\\colortbl;\\red55\\green78\\blue63;\\red186\\green191\\blue187;}}";

            BackColor = Constants.BACKGROUND_COLOR;
            ForeColor = Constants.TEXT_COLOR;

            MainMenuStrip.BackColor = BackColor;
            MainMenuStrip.ForeColor = ForeColor;

            Userlist.SelectedNode = null;
            Userlist.BackColor = Constants.CHAT_BACKGROUND_COLOR;
            Userlist.ForeColor = ForeColor;

            InputBox.BackColor = Constants.CHAT_BACKGROUND_COLOR;
            InputBox.ForeColor = ForeColor;

            Client = new Client();
            Client.Connect("frogbox.es", 6667, false, new User("express3"));
            Client.OnConnect += Client_OnConnect;
            Client.OnChannelJoin += Client_OnChannelJoin;

            _addline = new AddLineDelegate(AddLine);
            _addtab = new AddTabDelegate(AddTab);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            this.Client.Disconnect();

            base.OnClosing(e);
        }

        void Client_OnConnect(Client client)
        {
            Client.JoinChannel("test");
            Client.OnConnect -= Client_OnConnect;
        }

        void Client_OnChannelJoin(Client client, Channel channel)
        {
            this.Invoke(_addtab, channel.Name, TabType.Channel);
            this.Invoke(_addline, 0, "You joined the channel " + channel.Name);
        }

        private CTabPage AddTab(string title, TabType type)
        {
            CTabPage Tab = new CTabPage(title, type);

            //Prepare RichTextBox
            RichTextBox TextBox = new RichTextBox();
            TextBox.Name = "TextBox";
            TextBox.Dock = DockStyle.Fill;
            TextBox.BorderStyle = BorderStyle.None;
            TextBox.BackColor = Constants.CHAT_BACKGROUND_COLOR;
            TextBox.ForeColor = Constants.TEXT_COLOR;
            TextBox.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            TextBox.ReadOnly = true;

            Tab.Controls.Add(TextBox);
            cTabControl.TabPages.Add(Tab);

            return Tab;
        }

        private void RemoveTab(int index)
        {
            cTabControl.TabPages.RemoveAt(index);
        }

        private void AddLine(int tabIndex, string line)
        {
            //initialize the RTF of the RichTextBox in the current tab
            string currRTF;
            if (!String.IsNullOrEmpty((cTabControl.TabPages[tabIndex].Controls["TextBox"] as RichTextBox).Rtf))
            {
                currRTF = (cTabControl.TabPages[tabIndex].Controls["TextBox"] as RichTextBox).Rtf;
            }

            else
            {
                currRTF = RTF;
            }

            line = line.Trim();
            string newRTF = currRTF;

            //append the new line at the end of the current RTF file
            newRTF = newRTF.Insert(newRTF.LastIndexOf('}'), "\\cf1" + DateTime.Now.ToString("[HH:mm] ") + "\\cf2" + line);

            (cTabControl.TabPages[tabIndex].Controls["TextBox"] as RichTextBox).Rtf = newRTF;
        }

        private void AddUser(string username)
        {
            Userlist.Nodes.Add(username);
        }

        private void PopulateUserlist()
        {
            //TODO
        }

        private void Userlist_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            cTabControl.SelectedTab = AddTab(e.Node.Text, TabType.PrivateMessage);
        }

        private void cTabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            if ((cTabControl.SelectedTab as CTabPage).Type == TabType.Channel)
            {
                PopulateUserlist();
            }

            else
            {
                Userlist.Nodes.Clear();
            }
        }
    }
}
