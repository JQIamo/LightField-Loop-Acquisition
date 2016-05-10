using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Threading;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using PrincetonInstruments.LightField.AddIns;

namespace LoopAcquisition.Loop_Acquisition
{
    /// <summary>
    /// Interaction logic for LoopAcquisition.xaml
    /// </summary>
    public partial class LoopAcquisitionUI : UserControl
    {
        ILightFieldApplication app_;
        EventHandler<ExperimentCompletedEventArgs> acquireCompletedEventHandler_;
        private System.ComponentModel.BackgroundWorker CamController_;
        
        public LoopAcquisitionUI(ILightFieldApplication application)
        {
            app_ = application;
            CamController_ = new System.ComponentModel.BackgroundWorker();
            CamController_.WorkerReportsProgress = false;
            CamController_.WorkerSupportsCancellation = true;
            CamController_.DoWork += new DoWorkEventHandler(CamController_DoWork);
            CamController_.RunWorkerCompleted += new RunWorkerCompletedEventHandler(CamController_RunWorkerCompleted);
            InitializeComponent();
        }
        ///////////////////////////////////////////////////////////////////////
        // Confirm that AddIn and experiment can acquire data
        ///////////////////////////////////////////////////////////////////////
        bool ValidateAcquisition()
        {
            IDevice camera = null;
            foreach (IDevice device in app_.Experiment.ExperimentDevices)
            {
                if (device.Type == DeviceType.Camera)
                    camera = device;
            }
            ///////////////////////////////////////////////////////////////////////
            if (camera == null)
            {
                MessageBox.Show("This sample requires a camera!");
                return false;
            }
            ///////////////////////////////////////////////////////////////////////
            if (!app_.Experiment.IsReadyToRun)
            {
                MessageBox.Show("The system is not ready for acquisition, is there an error?");
                return false;
            }
            return true;
        }
        //Validate without excessive error messages in the loop.
        bool ValidateLoopAcquisition()
        {
            IDevice camera = null;
            foreach (IDevice device in app_.Experiment.ExperimentDevices)
            {
                if (device.Type == DeviceType.Camera)
                    camera = device;
            }
            ///////////////////////////////////////////////////////////////////////
            if (camera == null)
            {
                return false;
            }
            ///////////////////////////////////////////////////////////////////////
            if (!app_.Experiment.IsReadyToRun)
            {
                return false;
            }
            if (app_.Experiment.IsRunning)
            {
                return false;
            }
            String fileDir = ((String)app_.Experiment.GetValue(ExperimentSettings.FileNameGenerationDirectory));
            String fileNam = ((String)app_.Experiment.GetValue(ExperimentSettings.FileNameGenerationBaseFileName));
            String filePath = System.IO.Path.Combine(fileDir, fileNam);
            filePath += ".spe";
            if (File.Exists(filePath))
            {
                return false;
            }
            return true;
        }
        ///////////////////////////////////////////////////////////////////////
        // Override some typical settings and acquire an spe file with a 
        // specific name. 
        ///////////////////////////////////////////////////////////////////////
        private void AcqButton_Click(object sender, RoutedEventArgs e)
        {
            
            // Are we in a state where we can do this?
            if (!ValidateAcquisition())
                return;

            // Get the experiment object
            IExperiment experiment = app_.Experiment;
            if (experiment != null)
            {
                // Don't Attach Date/Time
                experiment.SetValue(ExperimentSettings.FileNameGenerationAttachDate, false);
                experiment.SetValue(ExperimentSettings.FileNameGenerationAttachTime, false);
                experiment.SetValue(ExperimentSettings.FileNameGenerationAttachIncrement, false);

                // Save file as LabviewTemp.Spe to the default directory
                experiment.SetValue(ExperimentSettings.FileNameGenerationBaseFileName, "LabviewTemp");

                // Connect the event handler
                acquireCompletedEventHandler_ = new EventHandler<ExperimentCompletedEventArgs>(exp_AcquisitionComplete);
                experiment.ExperimentCompleted += acquireCompletedEventHandler_;

                // Begin the acquisition 
                experiment.Acquire();
            }
        }
        ///////////////////////////////////////////////////////////////////////
        // Acquire Completed Handler
        // This just fires a message saying that the data is acquired.
        ///////////////////////////////////////////////////////////////////////
        void exp_AcquisitionComplete(object sender, ExperimentCompletedEventArgs e)
        {
            ((IExperiment)sender).ExperimentCompleted -= acquireCompletedEventHandler_;
            MessageBox.Show("Acquire Completed");
        }

        //start continuous acquisition.
        private void ContAcqButton_Click(object sender, RoutedEventArgs e)
        {
            if (CamController_.IsBusy != true)
            {
                CamController_.RunWorkerAsync();
            }
        }

        //end continuous acquisition.
        private void BreakButton_Click(object sender, RoutedEventArgs e)
        {
            if (CamController_.WorkerSupportsCancellation == true)
            {
                CamController_.CancelAsync();
            }
            IExperiment experiment = app_.Experiment;
            if(experiment.IsRunning)
            {
                experiment.Stop();
            }
        }

        private void CamController_DoWork(Object sender, DoWorkEventArgs e)
        {
            BackgroundWorker CController = sender as BackgroundWorker;

            if (!ValidateAcquisition())
                return;

            // Get the experiment object
            IExperiment experiment = app_.Experiment;
            if (experiment != null)
            {
                // Don't Attach Date/Time
                experiment.SetValue(ExperimentSettings.FileNameGenerationAttachDate, false);
                experiment.SetValue(ExperimentSettings.FileNameGenerationAttachTime, false);
                experiment.SetValue(ExperimentSettings.FileNameGenerationAttachIncrement, false);

                // Save file as LabviewTemp.spe to the default directory
                experiment.SetValue(ExperimentSettings.FileNameGenerationBaseFileName, "LabviewTemp");

                while (CController.CancellationPending != true)
                {
                    // Begin the acquisition
                    if (ValidateLoopAcquisition())
                    {
                        experiment.Acquire();
                        System.Threading.Thread.Sleep(100);
                    }
                    else
                    {
                        System.Threading.Thread.Sleep(100);
                    }
                }
            }
            return;
        }

        private void CamController_RunWorkerCompleted(Object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled == true)
            {
                MessageBox.Show("Loop Acquisition Cancelled");
            }
        }
    }
}
