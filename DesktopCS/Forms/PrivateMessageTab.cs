﻿using System;

namespace DesktopCS.Forms
{
    class PrivateMessageTab : BaseTab
    {
        public NetIRC.User Target;

        public PrivateMessageTab(NetIRC.User target) : base(target.NickName)
        {
            this.Target = target;

            this.Type = TabType.PrivateMessage;
        }
    }
}