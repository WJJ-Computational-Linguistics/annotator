﻿#pragma checksum "..\..\..\View\MainWindow.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "36DC5EA03D62700A5C56C1AE38B412E3"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using SemanticRelationsResolver.Annotator.View;
using SemanticRelationsResolver.Annotator.ViewModels;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;
using Xceed.Wpf.AvalonDock;
using Xceed.Wpf.AvalonDock.Controls;
using Xceed.Wpf.AvalonDock.Converters;
using Xceed.Wpf.AvalonDock.Layout;
using Xceed.Wpf.AvalonDock.Themes;


namespace SemanticRelationsResolver.Annotator.View {
    
    
    /// <summary>
    /// MainWindow
    /// </summary>
    public partial class MainWindow : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 20 "..\..\..\View\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Menu MainMenu;
        
        #line default
        #line hidden
        
        
        #line 21 "..\..\..\View\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.MenuItem FileMenu;
        
        #line default
        #line hidden
        
        
        #line 22 "..\..\..\View\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.MenuItem New;
        
        #line default
        #line hidden
        
        
        #line 23 "..\..\..\View\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.MenuItem Treebank;
        
        #line default
        #line hidden
        
        
        #line 25 "..\..\..\View\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.MenuItem Open;
        
        #line default
        #line hidden
        
        
        #line 26 "..\..\..\View\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.MenuItem Close;
        
        #line default
        #line hidden
        
        
        #line 28 "..\..\..\View\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.MenuItem Save;
        
        #line default
        #line hidden
        
        
        #line 29 "..\..\..\View\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.MenuItem SaveAs;
        
        #line default
        #line hidden
        
        
        #line 31 "..\..\..\View\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.MenuItem Exit;
        
        #line default
        #line hidden
        
        
        #line 33 "..\..\..\View\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.MenuItem ViewMenu;
        
        #line default
        #line hidden
        
        
        #line 37 "..\..\..\View\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.MenuItem EditSentence;
        
        #line default
        #line hidden
        
        
        #line 38 "..\..\..\View\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.MenuItem EditWordOrder;
        
        #line default
        #line hidden
        
        
        #line 40 "..\..\..\View\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.MenuItem HelpMenu;
        
        #line default
        #line hidden
        
        
        #line 43 "..\..\..\View\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Primitives.StatusBar StatusBar;
        
        #line default
        #line hidden
        
        
        #line 45 "..\..\..\View\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock CurrentStatus;
        
        #line default
        #line hidden
        
        
        #line 49 "..\..\..\View\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal Xceed.Wpf.AvalonDock.DockingManager ContentDockingManager;
        
        #line default
        #line hidden
        
        
        #line 62 "..\..\..\View\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal Xceed.Wpf.AvalonDock.Layout.LayoutRoot LayoutRoot;
        
        #line default
        #line hidden
        
        
        #line 69 "..\..\..\View\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ListBox DocumentExplorerList;
        
        #line default
        #line hidden
        
        
        #line 75 "..\..\..\View\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal Xceed.Wpf.AvalonDock.Layout.LayoutDocumentPane SentenceEditors;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/Treebank.Annotator;component/view/mainwindow.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\View\MainWindow.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal System.Delegate _CreateDelegate(System.Type delegateType, string handler) {
            return System.Delegate.CreateDelegate(delegateType, this, handler);
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.MainMenu = ((System.Windows.Controls.Menu)(target));
            return;
            case 2:
            this.FileMenu = ((System.Windows.Controls.MenuItem)(target));
            return;
            case 3:
            this.New = ((System.Windows.Controls.MenuItem)(target));
            return;
            case 4:
            this.Treebank = ((System.Windows.Controls.MenuItem)(target));
            return;
            case 5:
            this.Open = ((System.Windows.Controls.MenuItem)(target));
            return;
            case 6:
            this.Close = ((System.Windows.Controls.MenuItem)(target));
            return;
            case 7:
            this.Save = ((System.Windows.Controls.MenuItem)(target));
            return;
            case 8:
            this.SaveAs = ((System.Windows.Controls.MenuItem)(target));
            return;
            case 9:
            this.Exit = ((System.Windows.Controls.MenuItem)(target));
            
            #line 31 "..\..\..\View\MainWindow.xaml"
            this.Exit.Click += new System.Windows.RoutedEventHandler(this.ExitClick);
            
            #line default
            #line hidden
            return;
            case 10:
            this.ViewMenu = ((System.Windows.Controls.MenuItem)(target));
            return;
            case 11:
            this.EditSentence = ((System.Windows.Controls.MenuItem)(target));
            return;
            case 12:
            this.EditWordOrder = ((System.Windows.Controls.MenuItem)(target));
            return;
            case 13:
            this.HelpMenu = ((System.Windows.Controls.MenuItem)(target));
            return;
            case 14:
            this.StatusBar = ((System.Windows.Controls.Primitives.StatusBar)(target));
            return;
            case 15:
            this.CurrentStatus = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 16:
            this.ContentDockingManager = ((Xceed.Wpf.AvalonDock.DockingManager)(target));
            
            #line 51 "..\..\..\View\MainWindow.xaml"
            this.ContentDockingManager.DocumentClosing += new System.EventHandler<Xceed.Wpf.AvalonDock.DocumentClosingEventArgs>(this.ContentDockingManager_OnDocumentClosing);
            
            #line default
            #line hidden
            return;
            case 17:
            this.LayoutRoot = ((Xceed.Wpf.AvalonDock.Layout.LayoutRoot)(target));
            return;
            case 18:
            this.DocumentExplorerList = ((System.Windows.Controls.ListBox)(target));
            return;
            case 19:
            this.SentenceEditors = ((Xceed.Wpf.AvalonDock.Layout.LayoutDocumentPane)(target));
            return;
            case 20:
            
            #line 82 "..\..\..\View\MainWindow.xaml"
            ((System.Windows.Controls.DataGrid)(target)).LoadingRow += new System.EventHandler<System.Windows.Controls.DataGridRowEventArgs>(this.DataGrid_OnLoadingRow);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

