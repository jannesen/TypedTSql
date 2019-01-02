using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.VisualStudio.PlatformUI;

namespace Jannesen.VisualStudioExtension.TypedTSql.Rename
{
    internal partial class RenameDialog:  DialogWindow
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
