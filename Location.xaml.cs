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
    /// Location.xaml 的交互逻辑
    /// </summary>
    public partial class Location : Window
    {
        static ImpinjReader reader = new ImpinjReader();
        private Settings settings;
        private Tag tags;
        List<string> epcs = new List<string>();
        List<LocationReport> list = new List<LocationReport>();

        public Location()
        {
            InitializeComponent();
        }
        private void Setting_Config(Settings settings)
        {
            try
            {
                // Connect to the reader.
                // Change the ReaderHostname constant in SolutionConstants.cs 
                // to the IP address or hostname of your reader.
                reader.Connect(Host.Text);

                // Assign the LocationReported event handler.
                // This specifies which method to call
                // when a location report is available.
                reader.LocationReported += OnLocationReported;

                // Get the default settings
                // We'll use these as a starting point
                // and then modify the settings we're 
                // interested in.
                settings = reader.QueryDefaultSettings();

                // Put the xArray into location mode
                settings.XArray.Mode = XArrayMode.Location;

                // Enable all three report types
                settings.XArray.Location.EntryReportEnabled = true;
                settings.XArray.Location.UpdateReportEnabled = true;
                settings.XArray.Location.ExitReportEnabled = true;

                // Set xArray placement parameters
              
                // The mounting height of the xArray, in centimeters
                settings.XArray.Placement.HeightCm = Convert.ToUInt16(Heigh.Text);
                // These settings aren't required in a single xArray environment
                // They can be set to zero (which is the default)
                settings.XArray.Placement.FacilityXLocationCm = Convert.ToUInt16(Xlocation.Text);
                settings.XArray.Placement.FacilityYLocationCm = Convert.ToUInt16(Ylocation.Text);
                settings.XArray.Placement.OrientationDegrees = Convert.ToInt16(OrientationDegrees.Text);

                // Set xArray location parameters
                settings.XArray.Location.ComputeWindowSeconds = 1;
                settings.ReaderMode = ReaderMode.DenseReaderM4Two;
                settings.Session = 2;
                settings.XArray.Location.TagAgeIntervalSeconds = 1;

                // Specify how often we want to receive location reports
                settings.XArray.Location.UpdateIntervalSeconds = 5;

                // Set this to true if the maximum transmit power is desired, false if a custom value is desired
                settings.XArray.Location.MaxTxPower = false;

                // If MaxTxPower is set to false, then a custom power can be used. Provide a power in .25 dBm increments
                settings.XArray.Location.TxPowerInDbm = 25.25;

                // Disable antennas targeting areas from which we may not want location reports,
                // in this case we're disabling antennas 10 and 15

                //List<ushort> disabledAntennas = new List<ushort> { 10, 15 };
                //settings.XArray.Location.DisabledAntennaList = disabledAntennas;
              

                // Uncomment this is you want to filter tags
                
                // Setup a tag filter.
                // Only the tags that match this filter will respond.
                // We want to apply the filter to the EPC memory bank.
                settings.Filters.TagFilter1.MemoryBank = MemoryBank.Epc;
                // Start matching at the third word (bit 32), since the 
                // first two words of the EPC memory bank are the
                // CRC and control bits. BitPointers.Epc is a helper
                // enumeration you can use, so you don't have to remember this.
                settings.Filters.TagFilter1.BitPointer = BitPointers.Epc;
                // Only match tags with EPCs that start with "3008"
                settings.Filters.TagFilter1.TagMask = "3008";
                // This filter is 16 bits long (one word).
                settings.Filters.TagFilter1.BitCount = 16;

                // Set the filter mode. Use only the first filter
                settings.Filters.Mode = TagFilterMode.OnlyFilter1;
                

                // Apply the newly modified settings.
                reader.ApplySettings(settings);

                // Start the reader
                reader.Start();

                // Wait for the user to press enter.
                Console.WriteLine("Press enter to exit.");
                Console.ReadLine();

                // Apply the default settings before exiting.
                reader.ApplyDefaultSettings();

                // Disconnect from the reader.
                reader.Disconnect();
            }
            catch (OctaneSdkException ex)
            {
                // Handle Octane SDK errors.
                System.Diagnostics.Trace.WriteLine("An Octane SDK exception has occurred : {0}", ex.Message);
            }
            catch (Exception ex)
            {
                // Handle other .NET errors.
                System.Diagnostics.Trace.WriteLine("An exception has occurred : {0}", ex.Message);
            }

        }

        private void Stop_Click(object sender, RoutedEventArgs e)
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

        private void Start_Click(object sender, RoutedEventArgs e)
        {

            Setting_Config(settings);
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

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            listTags.Items.Clear();
        }


        private void UpdateListBox(List<LocationReport> list)
        {
            foreach (var report in list)
            {
                if (epcs.IndexOf(report.Epc.ToString()) == -1)
                {
                    epcs.Add(report.Epc.ToString());
                }
                listTags.Items.Add(report.ReportType + ", \t" + report.Epc + ", \t" + report.LocationXCm + ", \t" + report.LocationYCm + ", \t" + report.Timestamp + "- " + report.Timestamp.LocalDateTime + ", \t" + report.ConfidenceFactors.ReadCount);
            }
        }

        private void OnLocationReported(ImpinjReader reader, LocationReport report)
        {
            List<LocationReport> list = new List<LocationReport>();
            list.Add(report);
            Action action = delegate()
            {
                UpdateListBox(list);
            };
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, action);
        }


    }
}
