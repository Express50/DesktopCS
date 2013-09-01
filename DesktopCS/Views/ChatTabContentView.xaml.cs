﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using DesktopCS.ViewModels;

namespace DesktopCS.Views
{
    /// <summary>
    /// Interaction logic for TabUserControl.xaml
    /// </summary>
    public partial class ChatTabContentView
    {
        public ChatTabContentView(ChatTabContentViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
            SetBinding(DocumentProperty, new Binding("FlowDocument"));
        }

        public FlowDocument Document
        {
            get { return (FlowDocument)GetValue(DocumentProperty); }
            set { SetValue(DocumentProperty, value); }
        }

        public static readonly DependencyProperty DocumentProperty =
            DependencyProperty.Register("Document", typeof(FlowDocument), typeof(ChatTabContentView), new PropertyMetadata(OnDocumentChanged));

        private static void OnDocumentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (ChatTabContentView)d;
            if (e.NewValue == null)
                control.ChatRichTextBox.Document = new FlowDocument(); //Document is not amused by null :)

            control.ChatRichTextBox.Document = control.Document;
        }

    }
}