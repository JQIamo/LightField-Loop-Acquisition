using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.AddIn;
using System.AddIn.Pipeline;

using PrincetonInstruments.LightField.AddIns;


namespace LoopAcquisition
{
    ///////////////////////////////////////////////////////////////////////////
    //  This sample sets an exposure time, disables date and time as part of the 
    // file name and sets a specific name. Note pressing the button multiple times
    // will continually overwrite the file. (This sample also overrides the file
    // already exists dialog)
    ///////////////////////////////////////////////////////////////////////////
    [AddIn("Loop Acquisition",
    Version = "0.2.3",
    Publisher = "UMD Ultracold Strontium")]
    public class AddinLoopAcquisition : AddInBase, ILightFieldAddIn
    {

        private LoopAcquisition.Loop_Acquisition.LoopAcquisitionUI control_;
        ///////////////////////////////////////////////////////////////////////
        public UISupport UISupport { get { return UISupport.ExperimentSetting; } }
        ///////////////////////////////////////////////////////////////////////
        public void Activate(ILightFieldApplication app)
        {
            // Capture Interface
            LightFieldApplication = app;

            // Build your controls
            control_ = new LoopAcquisition.Loop_Acquisition.LoopAcquisitionUI(LightFieldApplication);
            ExperimentSettingElement = control_;

            // Initialize The Base with the controls dispatcher
            Initialize(control_.Dispatcher, "Loop Acquisition");
        }
        ///////////////////////////////////////////////////////////////////////
        public void Deactivate() { }
        ///////////////////////////////////////////////////////////////////////
        public override string UIExperimentSettingTitle { get { return "Loop Acquisition"; } }
    }

}

