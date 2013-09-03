﻿namespace DesktopCS.Models
{
    public class ServerTab : Tab
    {
        public ServerTab(string header) : base(header)
        {
            this.TabItem.IsClosable = false;
        }

        public override void MarkUnread()
        {
            // Ignore the request
        }
    }
}
