using System;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Jannesen.VisualStudioExtension.TypedTSql.Library;

namespace Jannesen.VisualStudioExtension.TypedTSql.Commands
{
    internal sealed class CustomMenuCommand: IDisposable
    {
        public  static  readonly            Guid                CommandSet                  = new Guid("60132958-8fc4-4928-8033-bc96d8172d8d");
        public  const                       int                 DatabaseExplorerCommandId   = 0x0100;

        private readonly                    VSPackage           _package;
        private                             MenuCommand         _menuShowDatabaseExplorer;

        public                                                  CustomMenuCommand(VSPackage package)
        {
            this._package = package;

            _menuShowDatabaseExplorer = new MenuCommand(this.ShowCatalogExplorer, new CommandID(CommandSet, DatabaseExplorerCommandId));
            ((IServiceProvider)_package).GetService<OleMenuCommandService>(typeof(IMenuCommandService)).AddCommand(_menuShowDatabaseExplorer);
        }
        public                              void                Dispose()
        {
            if (_menuShowDatabaseExplorer != null) {
                ((IServiceProvider)_package).GetService<OleMenuCommandService>(typeof(IMenuCommandService)).RemoveCommand(_menuShowDatabaseExplorer);
            }
        }

        private                             void                ShowCatalogExplorer(object sender, EventArgs e)
        {
            _package.ShowCatalogExplorer();
        }
    }
}
