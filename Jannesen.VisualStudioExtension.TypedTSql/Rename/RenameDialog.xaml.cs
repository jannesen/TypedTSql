using System;
using System.Windows;
using Microsoft.VisualStudio.PlatformUI;

namespace Jannesen.VisualStudioExtension.TypedTSql.Rename
{
    internal partial class RenameDialog: DialogWindow
    {
        private     bool                    _activated;

        public                              RenameDialog(Renamer renamer)
        {
            DataContext = renamer;
            InitializeComponent();
        }

        protected   override    void        OnActivated(System.EventArgs e) {
            base.OnActivated(e);
            if (!_activated) {
            this.MinWidth  = this.ActualWidth;
            this.MinHeight = this.ActualHeight;
            this.MaxHeight = this.ActualHeight;
                _newName.Focus();
                _newName.SelectAll();
                _activated = true;
            }
        }
        private                 void        _OK_click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            Close();
        }
        private                 void        _Cancel_click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            Close();
        }
    }
}
