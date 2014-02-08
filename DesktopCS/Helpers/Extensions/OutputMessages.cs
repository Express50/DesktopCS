﻿using System;
using DesktopCS.Helpers.Parsers;
using DesktopCS.Models;
using DesktopCS.Services.Command;
using NetIRC;

namespace DesktopCS.Helpers.Extensions
{
    public static class OutputMessages
    {
        public static void AddException(this Tab tab, CommandException ex, ParseArgs args)
        {
            tab.AddChat(new ErrorLine(ex.Message, args));
        }

        public static void AddSystemChat(this Tab tab, string message, ParseArgs args)
        {
            tab.AddChat(new SystemMessageLine(message, args));
        }

        public static void AddAction(this Tab tab, User user, string message, ParseArgs args)
        {
            tab.AddChat(user, MIRCHelper.ItalicChar + message + MIRCHelper.ItalicChar, args);
        }

        public static void AddAction(this Tab tab, UserItem user, string message, ParseArgs args)
        {
            tab.AddChat(new MessageLine(user, MIRCHelper.ItalicChar + message + MIRCHelper.ItalicChar, args));
        }

        #region Channel

        public static void AddJoin(this Tab tab, string nick, ParseArgs args)
        {
            tab.AddSystemChat(String.Format("{0} joined the room.", nick), args);
        }

        public static void AddJoin(this Tab tab, ParseArgs args)
        {
            tab.AddSystemChat(String.Format("You joined the room."), args);
        }

        public static void AddLeave(this Tab tab, string nick, string reason, ParseArgs args)
        {
            tab.AddSystemChat(!String.IsNullOrEmpty(reason)
                ? String.Format("{0} left the room ({1}).", nick, reason)
                : String.Format("{0} left the room.", nick), args);
        }

        public static void AddLeave(this Tab tab, string reason, ParseArgs args)
        {
            tab.AddSystemChat(!String.IsNullOrEmpty(reason)
                ? String.Format("You left the room ({0}).", reason)
                : String.Format("You left the room."), args);
        }

        public static void AddTopic(this Tab tab, string channel, string nick, DateTime time, ParseArgs args)
        {
            tab.AddSystemChat(String.Format("Topic for {0} was set by {1} on {2}.", channel, nick, time), args);
        }

        public static void AddTopicChanged(this Tab tab, string nick, ParseArgs args)
        {
            tab.AddSystemChat(String.Format("Topic was changed by {0}.", nick), args);
        }

        public static void AddMode(this Tab tab, string from, string to, UserRank rank, ParseArgs args)
        {
            string rankStr = rank.ToString().ToLower();
            tab.AddSystemChat(String.Format("{0} gave {2} to {1}.", from, to, rankStr), args);
        }

        #endregion
        
        #region User

        public static void AddNickChange(this Tab tab, string oldNick, string newNick, ParseArgs args)
        {
            tab.AddSystemChat(String.Format("{0} is now known as {1}.", oldNick, newNick), args);
        }

        public static void AddNickChange(this Tab tab, string newNick, ParseArgs args)
        {
            tab.AddSystemChat(String.Format("You are now known as {0}.", newNick), args);
        }

        public static void AddQuit(this Tab tab, string nick, string reason, ParseArgs args)
        {
            tab.AddSystemChat(!String.IsNullOrEmpty(reason)
                ? String.Format("{0} quit ({1}).", nick, reason)
                : String.Format("{0} quit.", nick), args);
        }

        public static void AddQuit(this Tab tab, string reason, ParseArgs args)
        {
            tab.AddSystemChat(String.Format("You quit ({0}).", reason), args);
        }

        #endregion

    }
}