using RuntimeComponent = IMainview.IMainWindow; // for those times when the namespace and class name are identical,
                                                    // we can set up an alias for the two items
using System;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;       // IEnumerable<T>
using System.ComponentModel.Composition;    // MEF; reqs ref to System.ComponentModel.Composition
using System.ComponentModel.Composition.Hosting;    // MEF; AggregateCatalog
using System.Linq;
using System.Reflection;
using System.Diagnostics;
using System.Threading;
using Prism;
using Main_ViewModel;
using IMainview;

namespace MainDriver
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        #region backing vars

        [Import(typeof(IMainWindow))]
        // we want MEF to find user controls that satisfy this specification
        public RuntimeComponent m_runtimePart { get; set; }
        IEnumerable<RuntimeComponent> m_runtimeParts = null;   // all the parts that MEF brings in

        MainViewModel vm = null;

        #endregion backing vars

        public MainWindow()
        {
            InitializeComponent();

            vm = new MainViewModel();
            this.DataContext = vm;
            vm.ExceptionRaised += vm_ExceptionRaised;   // something unexpected happened
            vm.HelpAboutRaised += vm_HelpAboutRaised;   // info about the program ver
            LoadMef(); // must be STA call as it will load UI components
            vm.Initialize();
        }

        /// <summary>
        /// user requested close
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        } // public void Dispose()

        /// <summary>
        /// Got here either by user code or by garbage collector. If param false, then gc.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {       // managed resources here
                //                thrd43KWarning.Join();
                while (tabImages.Items.Count > 0)
                {
                    TabItem tab = (TabItem)tabImages.Items[0];
                    RuntimeComponent part = (RuntimeComponent)tab.Content;
                    part.Close();    // calls the Close() method implemented in the code behind of the user control, per the IOcvWindow contract
                    tabImages.Items.Remove(tab);
                    System.Threading.Thread.Sleep(500);
                };

                foreach (TabItem tab in tabImages.Items)
                {
                    (new Thread(() =>
                    {
                        Dispatcher.BeginInvoke(
                            new Action(() =>
                            {
                                RuntimeComponent part = (RuntimeComponent)tab.Content;
                                part.Close();    // calls the Close() method implemented in the code behind of the user control, per the IOcvWindow contract
                                tabImages.Items.Remove(tab);
                                Thread.Sleep(500);
                            }
                            )
                        );
                    })).Start();
                }
                /*
                                while (tabAlgorithms.Items.Count > 0)
                                {
                                    TabItem tab = (TabItem)tabAlgorithms.Items[0];
                                    RuntimeComponent part = (RuntimeComponent)tab.Content;
                                    part.Close();    // calls the Close() method implemented in the code behind of the user control, per the IOcvWindow contract
                                    tabAlgorithms.Items.Remove(tab);
                                    System.Threading.Thread.Sleep(500);
                                };
                */
                vm.ExceptionRaised -= vm_ExceptionRaised;
                vm.HelpAboutRaised -= vm_HelpAboutRaised;
                vm.Dispose();
            }
            // unmanaged resources here
            {
            }
        } // protected virtual void Dispose(bool disposing)

        /// <summary>
        /// user wants to exit app
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mnuFileExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }   // private void mnuFileExit_Click(object sender, RoutedEventArgs e)

        /// <summary>
        ///  Chance to clean up Main Window resources.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainView_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Dispose();
        } // private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)




        #region MEF

        /// <summary>
        /// This is where the user interface (main m_runtimePart) is built at runtime.
        /// MEF looks at each dll in the exec dir. If it matches the contract specified in OCV_Cryoview_IWindow, then the 
        /// dll is loaded into mem as a part. When MEF is finished, we will add each part to the tab control on the main m_runtimePart.
        /// </summary>
        private void LoadMef()
        {
            string status = "";
            LoadMefParts(ref m_runtimeParts, ref status);
            if (!status.Equals("")) { MessageBox.Show(status, "Contact SDG!"); Process.GetCurrentProcess().Kill(); }
            foreach (RuntimeComponent win in m_runtimeParts)
            {
                TabItem tab = new TabItem();
                tab.Header = win.ServiceName;
                tab.Content = win;
                if (win.ServiceName.Equals("Image View"))
                {
                    tabImages.Items.Add(tab);
                }
            }

            var theList = m_runtimeParts.Select((s, i) => new { i, s })
                .Where(t => t.s.ServiceName.Equals("Image View"))       // list of runtime parts that have title of image view
                .Select(t => t.i)                                       // the index of the runtime image view
                .ToList();
            // could test for multiple image views, but almost impossible. Would be apparent in UI.
//            int imagePartIndex = theList[0];
            //            Dispatcher.BeginInvoke((Action)(() => tabMainWindow.SelectedIndex = imagePartIndex));
        } // private void LoadMEF()

        /// <summary>
        /// MEF allows building the app according to what dlls are found in the execution dir. Moves building the app from compile time to runtime.
        /// </summary>
        /// <param name="m_runtimeParts"> the dlls found which match the MEF contract </param>
        /// <param name="status"></param>
        private void LoadMefParts(ref IEnumerable<RuntimeComponent> windows, ref string status)
        {
            try
            {       // let MEF build the app out of discovered parts
                var catalog = new AggregateCatalog();   // var type can not be init'd to null
                // BadFormatImageException occurs as a first chance exception if attempting to load a flat dll
                // as they are w/o a manifest, e.g. hd421md.dll, szlib.dll. It can be safely ignored. 
                // The code will continue to load .net dlls relevant to using the Managed Extension Framework (MEF).
                catalog = new AggregateCatalog(new DirectoryCatalog("."),
                        new AssemblyCatalog(Assembly.GetExecutingAssembly()));
                var container = new CompositionContainer(catalog);
                windows = container.GetExportedValues<RuntimeComponent>();  // ctors of the individual user components for the views called here
            }
            catch (ReflectionTypeLoadException ex)
            {
                string msg = "";
                foreach (Exception exSub in ex.LoaderExceptions)
                {
                    msg += exSub.Message + Environment.NewLine;
                }
                status = msg;
            }
            catch (Exception ex)
            {
                status = ex.ToString();
            }
        }   // private void LoadMefParts(ref IEnumerable<RuntimeComponent> windows, ref string status)

        #endregion MEF

        /// <summary>
        /// Information about the program ver.
        /// </summary>
        /// <param name="msg"></param>
        private void vm_HelpAboutRaised(string msg)
        {
            MainDriver_About dlg = new MainDriver_About();
            dlg.ShowDialog();
        }   // private void m_cryoviewViewModel_HelpAboutRaised(string msg)

        /// <summary>
        /// Not really expecting exceptions to make it back to client. Something went very wrong, e.g. viewmodel initialization.
        /// Really shouldn't continue.
        /// </summary>
        /// <param name="msg"></param>
        private void vm_ExceptionRaised(string msg)
        {
            if (msg.Contains("Fatal Configuration Error"))
            {
                MessageBox.Show(msg + "\r\n SHUTTING DOWN!", "Contact SDG!");
                Process.GetCurrentProcess().Kill();
            }
            MessageBox.Show(msg, "Contact SDG!");
        }   //         private void m_cryoviewViewModel_ExceptionRaised(string msg)

    }
}
