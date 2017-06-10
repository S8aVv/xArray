////////////////////////////////////////////////////////////////////////////////
//
//    Impinj Xarray
//
////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Threading;
using Impinj.OctaneSdk;
using System.IO;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Data;
using System.Windows.Data;


namespace xArray
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Create an instance of the ImpinjReader class.
        private ImpinjReader reader = new ImpinjReader();
        private FeatureSet features;
        private Settings settings;
        private Tag tags; 
        double Frequencies;
        ReportMode report;
        ReaderMode read;
        SearchMode search;
        List<string> epcs = new List<string>();
        int count = 0;
        DispatcherTimer t = new DispatcherTimer();
        private System.Windows.Threading.DispatcherTimer dTimer = new DispatcherTimer();
        
        public MainWindow()
        {
            InitializeComponent();
            dTimer.Tick += new EventHandler(dTimer_Tick);

           // set time:TimeSpan（h, m, s）
                   dTimer.Interval = new TimeSpan(0, 0, 20);

           
        }
        private void dTimer_Tick(object sender, EventArgs e)
        {

            
                reader.Stop();
                dTimer.Stop();
                
          
           
        }
        private void buttonStart_Click(object sender, RoutedEventArgs e)
        {
            //启动 DispatcherTimer对象dTime。
            dTimer.Start(); 
            Setting_Config(features, settings);
            try
            {
                // Don't call the Start method if the
                // reader is already running.
                if (!reader.QueryStatus().IsSingulating)
                {
                    // Start reading.
                    reader.Start();
                }
            }
            catch (OctaneSdkException ex)
            {
                // An Octane SDK exception occurred. Handle it here.
                System.Diagnostics.Trace.
                    WriteLine("An Octane SDK exception has occurred : {0}", ex.Message);
            }
            catch (Exception ex)
            {
                // A general exception occurred. Handle it here.
                System.Diagnostics.Trace.
                    WriteLine("An exception has occurred : {0}", ex.Message);
            }
        }

        private void UpdateListBox(List<Tag> list)
        {
            // Loop through each tag is the list and add it to the Listbox.
            //listTags.Items.Add("EPC, Antenna, Dopppler, PhaseAngle");
            //DataTable dt = new DataTable();
            //DataColumn dc1 = new DataColumn("Epc", Type.GetType("System.String"));
            //DataColumn dc2 = new DataColumn("AntennaPortNumber", Type.GetType("System.String"));
            //DataColumn dc3 = new DataColumn("RfDopplerFrequency", Type.GetType("System.String"));
            //DataColumn dc4 = new DataColumn("PhaseAngleInRadians", Type.GetType("System.String"));
            //DataColumn dc5 = new DataColumn("FirstSeenTime", Type.GetType("System.String"));
            //DataColumn dc6 = new DataColumn("LastSeenTime", Type.GetType("System.String"));
            //DataColumn dc7 = new DataColumn("PeakRssiInDbm", Type.GetType("System.Double"));
            //DataColumn dc8 = new DataColumn("ChannelInMhz", Type.GetType("System.String"));
            //DataColumn dc9 = new DataColumn("TagSeenCount", Type.GetType("System.String"));

            //dt.Columns.Add(dc1);
            //dt.Columns.Add(dc2);
            //dt.Columns.Add(dc3);
            //dt.Columns.Add(dc4);
            //dt.Columns.Add(dc5);
            //dt.Columns.Add(dc6);
            //dt.Columns.Add(dc7);
            //dt.Columns.Add(dc8);
            //dt.Columns.Add(dc9);

           
            //int i = 0;
            foreach (var tag in list)
            {
                //DataRow dr = dt.NewRow();
                //dr["Epc"] = tag.Epc;
                //dr["AntennaPortNumber"] = tag.AntennaPortNumber;
                //dr["RfDopplerFrequency"] = tag.RfDopplerFrequency;
                //dr["PhaseAngleInRadians"] = tag.PhaseAngleInRadians;
                //dr["FirstSeenTime"] = tag.FirstSeenTime;
                //dr["LastSeenTime"] = tag.LastSeenTime;
                //dr["PeakRssiInDbm"] = tag.PeakRssiInDbm;
                //dr["ChannelInMhz"] = tag.ChannelInMhz;
                //dr["TagSeenCount"] = tag.TagSeenCount;
                //dt.Rows.Add(dr);
                if (epcs.IndexOf(tag.Epc.ToString()) == -1)
                {
                    epcs.Add(tag.Epc.ToString());
                }
                listTags.Items.Add(tag.Epc + ", \t" + tag.AntennaPortNumber + ", \t" + tag.RfDopplerFrequency + ", \t" + tag.PhaseAngleInRadians + ", \t" + tag.FirstSeenTime + ", \t" + tag.LastSeenTime + ", \t" + tag.PeakRssiInDbm + ", \t" + tag.ChannelInMhz + ", \t" + tag.TagSeenCount);
            }
            //Console.WriteLine(dt.Rows[0][8]);
            //listTags.DataContext = dt;
            //listTags.ItemsSource = dt.DefaultView;
            
        }

        private void OnTagsReported(ImpinjReader sender, TagReport report)
        {
            // This event handler gets called when a tag report is available.
            // Since it is executed in a different thread, we cannot operate
            // directly on UI elements (the Listbox) in this method.
            // We must execute another method (UpdateListbox) on the main thread
            // using BeginInvoke. We will pass updateListbox a List of tags.
            Action action = delegate()
            {
                UpdateListBox(report.Tags);
            };

            Dispatcher.BeginInvoke(DispatcherPriority.Normal, action);
        }

        private void buttonStop_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Don't call the Stop method if the
                // reader is already stopped.
                if (reader.QueryStatus().IsSingulating)
                {
                    reader.Stop();
                }
            }
            catch (OctaneSdkException ex)
            {
                // An Octane SDK exception occurred. Handle it here.
                System.Diagnostics.Trace.
                    WriteLine("An Octane SDK exception has occurred : {0}", ex.Message);
            }
            catch (Exception ex)
            {
                // A general exception occurred. Handle it here.
                System.Diagnostics.Trace.
                    WriteLine("An exception has occurred : {0}", ex.Message);
            }
        }

        private void buttonClear_Click(object sender, RoutedEventArgs e)
        {
            // Clear all the readings from the Listbox.
            listTags.Items.Clear();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // The application is closing.
            // Stop the reader and disconnect.
            try
            {
                // Don't call the Stop method if the
                // reader is already stopped.
                if (reader.QueryStatus().IsSingulating)
                {
                    reader.Stop();
                }
                // Disconnect from the reader.
                reader.Disconnect();
            }
            catch (OctaneSdkException ex)
            {
                // An Octane SDK exception occurred. Handle it here.
                System.Diagnostics.Trace.
                    WriteLine("An Octane SDK exception has occurred : {0}", ex.Message);
            }
            catch (Exception ex)
            {
                // A general exception occurred. Handle it here.
                System.Diagnostics.Trace.
                    WriteLine("An exception has occurred : {0}", ex.Message);
            }
        }
        private void Setting_Config(FeatureSet features, Settings settings)
        {
             try
            {

                // Connect to the reader.
                // Change the ReaderHostname constant in SolutionConstants.cs 
                // to the IP address or hostname of your reader.
                reader.Connect(Host.Text);

                // Get the reader features to determine if the 
                // reader supports a fixed-frequency table.
                features = reader.QueryFeatureSet();

                if (!features.IsHoppingRegion)
               {

                    // Get the default settings
                    // We'll use these as a starting point
                    // and then modify the settings we're 
                    // interested in.
                    settings = reader.QueryDefaultSettings();


                    // Add antenna number to tag report
                    settings.Report.IncludeAntennaPortNumber = true;

                    // Send a tag report for every tag read.
                    settings.Report.Mode = report;

                    settings.ReaderMode = read;
                    settings.SearchMode = search;
                    settings.Session = Convert.ToUInt16(session.Text);
                    settings.TagPopulationEstimate = 0;

                    // Specify the transmit frequencies to use.
                    // Make sure your reader supports this and
                    // that the frequencies are valid for your region.
                    // readers.(China)
                    settings.TxFrequenciesInMhz.Add(Frequencies);

                    // Start by disabling all of the antennas
                    //settings.Antennas.DisableAll();
                    
                    //string strText = Sector.Text;
                    //string[] strArr = strText.Split(',');
                    //ushort[] usArr = new ushort[strArr.Length];
                    //for(int i = 0; i < strArr.Length; i++)
                    //{
                        //usArr[i] = Convert.ToUInt16(strArr[i]);
                    //}
                    // Enable antennas by specifying an array of antenna IDs
                   // settings.Antennas.EnableById(usArr);
                    // Or set each antenna individually
                    //settings.Antennas.GetAntenna(52).IsEnabled = true;
                    //settings.Antennas.GetAntenna(3).IsEnabled = true;
                    // ...

                    // Set all the antennas to the max transmit power and receive sensitivity
                    settings.Antennas.TxPowerMax = true;
                    settings.Antennas.RxSensitivityMax = true;
                    // Or set all antennas to a specific value in dBm
                    settings.Antennas.TxPowerInDbm =Convert.ToDouble(Power1.Text);
                    //settings.Antennas.RxSensitivityInDbm = -70.0;
                    // Or set each antenna individually
                    //settings.Antennas.GetAntenna(1).MaxTxPower = true;
                    //settings.Antennas.GetAntenna(1).MaxRxSensitivity = true;
                    //settings.Antennas.GetAntenna(2).TxPowerInDbm = 30.0;
                    //settings.Antennas.GetAntenna(2).RxSensitivityInDbm = -70.0;
                    // ...

                    // xArray only
                    // Enable antennas by sector number
                    //settings.Antennas.EnableBySector(new ushort[] { 3, 4, 5 });

                    // xArray only
                    // Enable antennas by ring number
                    //settings.Antennas.EnableByRing(new ushort[] { 6, 7 });

                    // Tell the reader to include the
                    // RF doppler frequency in all tag reports. 
                    settings.Report.IncludeDopplerFrequency = true;

                    // Tell the reader to include the
                    //First Seen Time in all tag reports. 
                    settings.Report.IncludeFirstSeenTime = true;

                    // Tell the reader to include the
                    //Last Seen Time in all tag reports.
                    settings.Report.IncludeLastSeenTime = true;

                    // Tell the reader to include the
                    //Last Seen Time in all tag reports.
                    settings.Report.IncludePeakRssi = true;

                    // Tell the reader to include the
                    // RF Phase Angle in all tag reports. 
                    settings.Report.IncludePhaseAngle = true;

                    settings.Report.IncludeChannel = true;

                    settings.Report.IncludeSeenCount =true;
                    // Tell the reader to include the antenna number
                    // in all tag reports. Other fields can be added
                    // to the reports in the same way by setting the 
                    // appropriate Report.IncludeXXXXXXX property.
                    settings.Report.IncludeAntennaPortNumber = true;

                    // Apply the newly modified settings.
                    reader.ApplySettings(settings);

                    // Assign the TagsReported event handler.
                    // This specifies which method to call
                    // when tags reports are available.
                    // This method will in turn call a delegate 
                    // to update the UI (Listbox).
                    reader.TagsReported += OnTagsReported;
                }
            }
             catch (OctaneSdkException ex)
             {
                 // An Octane SDK exception occurred. Handle it here.
                 System.Diagnostics.Trace.
                     WriteLine("An Octane SDK exception has occurred : {0}", ex.Message);
             }
             catch (Exception ex)
             {
                 // A general exception occurred. Handle it here.
                 System.Diagnostics.Trace.
                     WriteLine("An exception has occurred : {0}", ex.Message);
             }
        }

        private void ButtonOut_Click(object sender, RoutedEventArgs e)
        {
            //for (int i = 0; i < epcs.Count; i++)
           // {
                if (Directory.Exists(@"D:\data\" + /*epcs[i].Substring(0, 4)+*/  DateTime.Now.ToShortDateString().Replace("/", "")) == false)
                    Directory.CreateDirectory(@"D:\data\" + /*epcs[i].Substring(0, 4) + */DateTime.Now.ToShortDateString().Replace("/", ""));
                using (StreamWriter outputStream = new StreamWriter(@"d:\data\"  /*+ epcs[i].Substring(0, 4)*/+ DateTime.Now.ToShortDateString().Replace("/", "") + @"\" /*+ epcs[i].Substring(0, 4)*/ + DateTime.Now.ToLongTimeString().Replace(":", "") + ".csv"))
                {
                    string header1 = "EPC,";
                    string header2 = "AntennaNumber,";
                    string header3 = "Doppler,";
                    string header4 = "PhaseAngle,";
                    string header5 = "FirstSeenTime,";
                    string header6 = "lastSeenTime,";
                    string header7 = "PeakRssi,";
                    string header8 = "Channel,";
                    string header9 = "Count,";

                    outputStream.WriteLine(header1 + header2 + header3 + header4 + header5 + header6 + header7 + header8 + header9);
                    foreach (var item in listTags.Items)
                    {
                        //if (item.ToString().Contains(epcs[i]))
                            outputStream.WriteLine(item.ToString());
                    }
                }
           // }
        }
        
        private void Frequency_Loaded(object sender, RoutedEventArgs e)
        {
            Frequency.Items.Add(920.625);
            Frequency.Items.Add(920.875); 
            Frequency.Items.Add(921.125);
            Frequency.Items.Add(921.375);
            Frequency.Items.Add(921.625);
            Frequency.Items.Add(921.875);
            Frequency.Items.Add(922.125);
            Frequency.Items.Add(922.375);
            Frequency.Items.Add(922.625);
            Frequency.Items.Add(922.875);
            Frequency.Items.Add(923.125);
            Frequency.Items.Add(923.375);
            Frequency.Items.Add(923.625);
            Frequency.Items.Add(923.875);
            Frequency.Items.Add(924.125);
            Frequency.Items.Add(924.375);
        }

        private void Frequency_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            Frequencies = Convert.ToDouble(Frequency.SelectedValue);
        }

        private void Report_Mode_Loaded(object sender, RoutedEventArgs e)
        {
            Report_Mode.Items.Add("WaitForQuery");
            Report_Mode.Items.Add("Individual");
            Report_Mode.Items.Add("IndividualUnbuffered");
            Report_Mode.Items.Add("BatchAfterStop");
        }

        private void Report_Mode_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            switch (Report_Mode.SelectedIndex)
            {
                case 0:
                    report = ReportMode.WaitForQuery;break;
                case 1:
                    report = ReportMode.Individual;break;
                /*case 2:
                    report = ReportMode.IndividualUnbuffered;break;*/
                case 3:
                    report = ReportMode.BatchAfterStop;break;
                default:
                    break;
            }
        }

        private void Search_Mode_Loaded(object sender, RoutedEventArgs e)
        {
            Search_Mode.Items.Add("ReaderSelected");
            Search_Mode.Items.Add("SingleTarget");
            Search_Mode.Items.Add("DualTarget");
            Search_Mode.Items.Add("TagFocus");
            Search_Mode.Items.Add("SingleTargetWithSuppression");
        }

        private void Search_Mode_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            switch (Search_Mode.SelectedIndex)
            {
                case 0:
                    search = SearchMode.ReaderSelected;break;
                case 1:
                    search = SearchMode.SingleTarget;break;
                case 2:
                    search = SearchMode.DualTarget;break;
                case 3:
                    search = SearchMode.TagFocus;break;
                /*case 4:
                    search = SearchMode.SingleTargetWithSuppression;break;*/
                default:
                    break;
            }
        }

        private void Reader_Mode_Loaded(object sender, RoutedEventArgs e)
        {
            Reader_Mode.Items.Add("MaxThroughput");
            Reader_Mode.Items.Add("Hybrid");
            Reader_Mode.Items.Add("DenseReaderM4");
            Reader_Mode.Items.Add("DenseReaderM8");
            Reader_Mode.Items.Add("MaxMiller");
            Reader_Mode.Items.Add("DenseReaderM4Two");
            Reader_Mode.Items.Add("AutoSetDenseReader");
            Reader_Mode.Items.Add("AutoSetDenseReaderDeepScan");
            Reader_Mode.Items.Add("AutoSetStaticFast");
            Reader_Mode.Items.Add("AutoSetStaticDRM");
        }

        private void Reader_Mode_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            switch (Reader_Mode.SelectedIndex)
            {
                case 0:
                    read = ReaderMode.MaxThroughput;break;
                case 1:
                    read = ReaderMode.Hybrid;break;
                case 2:
                    read = ReaderMode.DenseReaderM4;break;
                case 3:
                    read = ReaderMode.DenseReaderM8;break;
                case 4:
                    read = ReaderMode.MaxMiller;break;
                case 5:
                    read = ReaderMode.DenseReaderM4Two;break;
                case 6:
                    read = ReaderMode.AutoSetDenseReader;break;
                case 7:
                    read = ReaderMode.AutoSetDenseReaderDeepScan; break;
                case 8:
                    read = ReaderMode.AutoSetStaticFast;break;
                case 9:
                    read = ReaderMode.AutoSetStaticDRM;break;
                default:
                    break;
            }
        }

        private void location_Click(object sender, RoutedEventArgs e)
        {
            Location form1 = new Location();
            form1.Show();
        }

        
    }
}
