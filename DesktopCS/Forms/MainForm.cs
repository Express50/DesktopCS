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
        internal NetIRC.Client Client;
        internal CommandExecutor Executor;

        private delegate void AddLineDelegate(string tabName, string line);
        private delegate void AddLineWithAuthorDelegate(string tabName, User author, string line);
        private delegate BaseTab AddTabDelegate(BaseTab tab);
        private delegate void RemoveTabDelegate(string tabName);
        private delegate void PopulateUserlistDelegate();
        private delegate void UpdateTopicLabelDelegate();
        private AddLineDelegate _addline;
        private AddLineWithAuthorDelegate _addlinewithauthor;
        private AddTabDelegate _addtab;
        private RemoveTabDelegate _removetab;
        private PopulateUserlistDelegate _populateuserlist;
        private UpdateTopicLabelDelegate _updatetopiclabel;

        public MainForm()
        {
            InitializeComponent();

            BackColor = Constants.BACKGROUND_COLOR;
            ForeColor = Constants.TEXT_COLOR;

            MainMenuStrip.BackColor = BackColor;
            MainMenuStrip.ForeColor = ForeColor;

            InputBox.BackColor = Constants.CHAT_BACKGROUND_COLOR;
            InputBox.ForeColor = ForeColor;

            TopicLabel.BackColor = Constants.BACKGROUND_COLOR;
            TopicLabel.ForeColor = Constants.TOPIC_TEXT_COLOR;
            TopicLabel.Text = "";

            Executor = new CommandExecutor();

            Client = new Client();
            Client.Connect("frogbox.es", 6667, false, new User(Properties.Settings.Default.Nickname, Properties.Settings.Default.Color.Substring(1) + "QQ"));
            Client.OnConnect += Client_OnConnect;
            Client.OnChannelJoin += Client_OnChannelJoin;
            Client.OnChannelLeave += Client_OnChannelLeave;

            _addline = new AddLineDelegate(AddLine);
            _addlinewithauthor = new AddLineWithAuthorDelegate(AddLine);
            _addtab = new AddTabDelegate(AddTab);
            _removetab = new RemoveTabDelegate(RemoveTab);
            _populateuserlist = new PopulateUserlistDelegate(PopulateUserlist);
            _updatetopiclabel = new UpdateTopicLabelDelegate(UpdateTopicLabel);
        }

        #region Form Overrides
        protected override void OnClosing(CancelEventArgs e)
        {
            this.Client.Disconnect();

            base.OnClosing(e);

            Application.Exit();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            this.TopicLabel.MaximumSize = new Size(this.ClientRectangle.Width - 6, 0);

            this.Invalidate();
        }
        #endregion

        #region NetIRC Event Handlers
        void Client_OnConnect(Client client)
        {
            JoinChannels();
            Client.OnConnect -= Client_OnConnect;
        }

        void Client_OnChannelJoin(Client client, Channel channel)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new NetIRC.Client.OnChannelJoinHandler(Client_OnChannelJoin), client, channel);
                return;
            }

            ChannelTab tab = new ChannelTab(channel);

            this.AddTab(tab);

            this.AddLine("#" + channel.Name, "You joined the channel #" + channel.Name);

            this.PopulateUserlist();
            this.UpdateTopicLabel();

            this.TabList.SwitchToTab(tab.Name);

            channel.OnMessage += channel_OnMessage;
            channel.OnNotice += channel_OnNotice;
            channel.OnJoin += channel_OnJoin;
            channel.OnLeave += channel_OnLeave;
            channel.OnKick += channel_OnKick;
            channel.OnTopicChange += channel_OnTopicChange;

            System.Timers.Timer colorTimer = new System.Timers.Timer();

            colorTimer.Elapsed += (s, e) =>
            {
                colorTimer.Enabled = false;

                this.PopulateUserlist();
            };
            colorTimer.Interval = 1000;

            colorTimer.Enabled = true;
        }

        void Client_OnChannelLeave(Client client, Channel channel)
        {
            this.RemoveTab("#" + channel.Name);
            this.PopulateUserlist();
            this.UpdateTopicLabel();
        }

        void channel_OnMessage(Channel source, User user, string message)
        {
            if (user == null)
            {
                MessageBox.Show("Null user");
            }

            this.AddLine("#" + source.Name, user, message);
        }

        void channel_OnNotice(Channel source, User user, string notice)
        {
            this.AddLine("#" + source.Name, user, notice);
        }

        void channel_OnJoin(Channel source, User user)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new NetIRC.Channel.OnJoinHandler(channel_OnJoin), source, user);
                return;
            }

            if (user != Client.User)
            {
                user.OnNickNameChange += user_OnNickNameChange;

                user.OnQuit += (u, m) =>
                {
                    foreach (Channel channel in user.Channels)
                    {
                        channel_OnLeave(channel, user);
                    }
                };

                System.Timers.Timer colorTimer = new System.Timers.Timer();

                colorTimer.Elapsed += (s, e) =>
                {
                    colorTimer.Enabled = false;

                    this.AddUserJoin(source, user);

                    this.PopulateUserlist();
                };
                colorTimer.Interval = 1000;

                colorTimer.Enabled = true;
            }
        }

        void channel_OnLeave(Channel source, User user)
        {
            if (user != Client.User)
            {
                this.AddLine("#" + source.Name, user.NickName + " left the room.");
            }

            System.Timers.Timer colorTimer = new System.Timers.Timer();

            colorTimer.Elapsed += (s, e) =>
            {
                colorTimer.Enabled = false;

                this.PopulateUserlist();
            };
            colorTimer.Interval = 1000;

            colorTimer.Enabled = true;
        }

        void channel_OnKick(Channel source, User kicker, User user, string reason)
        {
            if (user == Client.User)
            {
                this.AddLine("#" + source.Name, "You were kicked by " + kicker.NickName + " (" + reason + ")");
                UserList.Nodes.Clear();
            }
            else
            {
                this.AddLine("#" + source.Name, kicker.NickName + " kicked " + user.NickName + " (" + reason + ")");
                this.PopulateUserlist();
            }
        }

        void channel_OnTopicChange(Channel source, ChannelTopic topic)
        {
            UpdateTopicLabel();
        }

        void user_OnNickNameChange(User user, string original)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new NetIRC.User.OnNickNameChangeHandler(user_OnNickNameChange), user, original);
                return;
            }

            this.AddLine((this.TabList.SelectedTab as BaseTab).Name, original + " has changed their nickname to " + user.NickName);
            this.PopulateUserlist();
        }
        #endregion

        private void JoinChannels()
        {
            string[] channels = Properties.Settings.Default.Channels.Split(',');

            foreach (string c in channels)
            {
                if (!string.IsNullOrEmpty(c))
                    Client.JoinChannel(c);
            }
        }

        internal BaseTab AddTab(BaseTab tab)
        {
            if (this.InvokeRequired)
            {
                return (BaseTab)this.Invoke(_addtab, tab);
            }

            if (!TabList.Tabs.ContainsKey(tab.Text))
            {
                TabList.AddTab(tab);

                Size size = this.TabList.ItemSize;
                size.Width = this.TabList.Width - 1;
                this.TabList.ItemSize = size;

                this.TabList.SizeMode = TabSizeMode.Fixed;

                return tab;
            }

            return TabList.Tabs[tab.Text] as BaseTab;
        }

        private void RemoveTab(string tabName)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(_removetab, tabName);
                return;
            }

            if (TabList.Tabs.ContainsKey(tabName))
            {
                TabList.RemoveTab(TabList.Tabs[tabName]);
            }
        }

        private void AddLine(string tabName, string line)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(_addline, tabName, line);
                return;
            }

            if (this.TabList.Tabs.ContainsKey(tabName))
            {
                ChatOutput output = new ChatOutput(this.TabList.Tabs[tabName], this.Client);
                output.AddLine(line);
            }
        }

        private void AddLine(string tabName, User author, string line)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(_addlinewithauthor, tabName, author, line);
                return;
            }

            if (author == null)
            {
                MessageBox.Show("Null user");
            }

            ChatOutput output = new ChatOutput(this.TabList.Tabs[tabName], this.Client);
            output.AddLine(author, line);

            if (this.TabList.SelectedTab.Name != tabName)
            {
                this.TabList.Tabs[tabName].Active = true;
                this.TabList.Invalidate();
            }
        }

        private delegate void AddUserJoinHandler(Channel channel, User user);

        private void AddUserJoin(Channel channel, User user)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new AddUserJoinHandler(AddUserJoin), channel, user);
                return;
            }

            ChatOutput output = new ChatOutput(this.TabList.Tabs["#" + channel.Name], this.Client);
            output.AddUserJoin(user, channel);
        }

        private void PopulateUserlist()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(_populateuserlist);
                return;
            }

            BaseTab selectedTab = TabList.SelectedTab as BaseTab;

            if (selectedTab != null && selectedTab.Type == TabType.Channel)
            {
                ChannelTab channelTab = selectedTab as ChannelTab;

                this.UserList.PopulateFromChannel(channelTab.Channel);
            }

            else
            {
                UserList.Nodes.Clear();
            }
        }

        private void UpdateTopicLabel()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(_updatetopiclabel);
                return;
            }

            if (TabList.SelectedTab != null && (TabList.SelectedTab as BaseTab).Type == TabType.Channel)
            {
                ChannelTab tab = TabList.SelectedTab as ChannelTab;

                if (tab.Channel.Topic.Message != null && tab.Channel.Topic.Author != null)
                {
                    TopicLabel.Text = tab.Channel.Topic.Message + " (by " + tab.Channel.Topic.Author.NickName + ")";
                }

                else
                {
                    TopicLabel.Text = "";
                }
            }

            else
            {
                TopicLabel.Text = "";
            }
        }

        private void ProcessInput(string input)
        {
            input = input.Trim();
            if (input.StartsWith("/"))
            {
                ChatOutput output = new ChatOutput((this.TabList.SelectedTab as BaseTab), this.Client);
                Executor.Execute(this.Client, input, output);
            }

            else
            {
                if ((TabList.SelectedTab as BaseTab).Type == TabType.Channel)
                {
                    Client.Send(new NetIRC.Messages.Send.ChatMessage((TabList.SelectedTab as ChannelTab).Channel, input));
                    AddLine(TabList.SelectedTab.Name, Client.User, input);
                }
            }
        }

        #region Control Event Handlers
        private void UserList_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            ChannelTab channelTab = this.TabList.SelectedTab as ChannelTab;

            UserNode userNode = this.UserList.SelectedNode as UserNode;
            User user = userNode.User;

            PrivateMessageTab tab = new PrivateMessageTab(user);

            AddTab(tab);

            this.TabList.SwitchToTab(tab.Name);
        }

        private void TabList_SelectedIndexChanged(object sender, EventArgs e)
        {
            PopulateUserlist();
            UpdateTopicLabel();
        }

        private void InputBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ProcessInput(InputBox.Text);
                InputBox.ResetText();
            }
        }

        private void TopicLabel_ClientSizeChanged(object sender, System.EventArgs e)
        {
            int offset = this.TopicLabel.Bottom + 3;

            this.UserList.Top = offset + 25;
            this.TabList.Top = offset;

            this.TabList.Height = this.Height - this.InputBox.Height - this.TopicLabel.Height - this.MenuStrip.Height - 49;
            this.UserList.Height = this.TabList.Height - 9;

            this.InputBox.Top = this.TabList.Bottom + 3;

            this.TopicLabel.Location = new Point(this.ClientRectangle.Width - this.TopicLabel.Width, this.TopicLabel.Top);

            this.Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;
            Rectangle userListRect = this.UserList.Bounds;

            userListRect.X -= 1;
            userListRect.Y -= 1;

            userListRect.Width += 1;
            userListRect.Height += 1;

            g.DrawRectangle(new Pen(Constants.TAB_BORDER_COLOR), userListRect);

            Rectangle inputBoxRectangle = this.InputBox.Bounds;

            inputBoxRectangle.X -= 1;
            inputBoxRectangle.Y -= 1;

            inputBoxRectangle.Width += 1;
            inputBoxRectangle.Height += 1;

            g.DrawRectangle(new Pen(Constants.TAB_BORDER_COLOR), inputBoxRectangle);
        }

        private void ToolsMenuStripItem_Click(object sender, EventArgs e)
        {
            OptionsForm options = new OptionsForm();
            options.Show();
        }
        #endregion

    }
}
