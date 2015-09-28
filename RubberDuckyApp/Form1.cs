using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Forms;

/* To-Do List
 * Look into grabbing existing scripts from wiki & implementing them via menu
 * Firmware options?
 * Other features?
 */

namespace RubberDuckyApp
{
    public partial class Form1 : Form
    {
        private string selectedDrive = ""; // ex C:\
        private string selectedRemovableDrive = ""; // ex E:\
        private string javaEXELocation = ""; 
        private string duckyDirectory = ""; //C:\RubberDucky
        private string javaHome = ""; // C:\RubberDucky\java.exe
        private string encoderLocation = ""; // C:\RubberDucky\encoder.jar
        bool microSDCheck;
        private string[] keyboardStrings = {
            "be", "br", "ca", "ch", "de", "dk",
            "es", "fi", "fr", "gb", "hr", "it",
            "pt", "ru", "si", "sv", "tr", "us"
        };

        const string encoderURL =
            "https://github.com/midnitesnake/USB-Rubber-Ducky/blob/master/Encoder/encoder.jar?raw=true";

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Auto Hide MicroSD Options
            button1.Hide();
            button2.Hide();
            comboBox3.Hide();
            comboBox2.SelectedIndex = 17;

            refreshHDComboBox();
            
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Assign variable to selected drive
            selectedDrive = comboBox1.SelectedItem.ToString();
            duckyDirectory = selectedDrive + "RubberDucky";
            
            // Check selected drive for Rubber Ducky Directory
            checkDirectoryExists();

            // Check RubberDucky Directory for java
            checkDirectoryJava();

            // Check RubberDucky Directory for encoder
            checkDirectoryEncoder();

            // Check RubberDucky Directory for input.txt
            checkDirectoryInputFile();

            // Display MicroSD Options
            displayMicroSD();

            // Display Encoder Options
            refreshEncoderElements();
        }

        private void displayMicroSD() // Auto
        {
            comboBox3.Show();

            if (comboBox1.SelectedIndex != -1)
            refreshMicroSDComboBox();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Make .bin Button
            label5.Text = "Creating .bin";
            string inputFile = "input.txt";
            string outputFile = "inject.bin";
            string languageProperty = keyboardStrings[comboBox2.SelectedIndex];
            string encodeCommand = "cmd /c \"cd " + duckyDirectory + " & java -jar encoder.jar -i " + 
                inputFile + " -o " + outputFile + " -l " + languageProperty + "\""; // cmd /c "cd c"\RubberDucky & java -jar encoder.jar -i input.txt -o inject.bin -l en"
        
                if (File.Exists(duckyDirectory + "\\" + outputFile))
                    File.Delete(duckyDirectory + "\\" + outputFile);

                Process process = new Process();
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                startInfo.FileName = "cmd.exe";
                startInfo.Arguments = encodeCommand;
                process.StartInfo = startInfo;
                process.Start();
            
            // .bin created in label
            label5.Text = ".bin Created In: " + duckyDirectory;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // Copy .bin Button
            refreshMicroSDComboBox();
            if (comboBox3.SelectedIndex != -1) // Check title array for removable disks
                    microSDCheck = true;

            if ( microSDCheck)
                File.Copy(duckyDirectory+"\\inject.bin",selectedRemovableDrive+"\\inject.bin", true);

            // .bin copied to label
            label5.Text = ".bin Copied To: " + comboBox3.SelectedItem;
        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox3.SelectedIndex != -1)
                selectedRemovableDrive = comboBox3.SelectedItem.ToString();
        }

        private void refreshHDComboBox()
        {
            // Assign drives to combobox1
            comboBox1.Items.Clear();
            // Credit to: http://stackoverflow.com/questions/623182/c-sharp-dropbox-of-drives
            foreach (var Drives in Environment.GetLogicalDrives())
            {
                DriveInfo DriveInf = new DriveInfo(Drives);

                if (DriveInf.IsReady)
                {
                    comboBox1.Items.Add(DriveInf.Name);
                }
            }

            if (comboBox1.Items.Count > 0)
                comboBox1.SelectedIndex = 0;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            // Refresh Button
            refreshHDComboBox();
            refreshEncoderElements();
            refreshMicroSDComboBox();

            // Drives Refreshed label
            label5.Text = "Drives Refreshed.";
        }

        private void button5_Click(object sender, EventArgs e)
        {
            // Save Payload Button
            File.WriteAllText(duckyDirectory + "\\input.txt", textBox1.Text);

            // Payload Saved Label
            label5.Text = "Payload Saved.";
        }

        private void refreshMicroSDComboBox()
        {
            // Assign removable drives to combobox3
            comboBox3.Items.Clear();
            foreach (var Drives in Environment.GetLogicalDrives())
            {
                DriveInfo driveInf = new DriveInfo(Drives);

                if (driveInf.IsReady)
                    if (driveInf.DriveType.ToString().Contains("Removable"))
                        comboBox3.Items.Add(driveInf.Name);
            }

            // Remove copies due to loop
            object[] distinctItems = (from Object o in comboBox3.Items select o).Distinct().ToArray();
            comboBox3.Items.Clear();
            comboBox3.Items.AddRange(distinctItems);

            if (comboBox3.Items.Count > 0)
                comboBox3.SelectedIndex = 0;

            if (comboBox3.Items.Count == 0)
                MessageBox.Show(
                    "No removable storage device was found.\n" +
                    "Please insert the MicroSD card that will be used for your USB Rubber Ducky.\n" +
                    "Click \"Refresh\" when ready.");
        }

        private void refreshEncoderElements()
        {
            if (comboBox1.SelectedIndex == -1)
            {
                button1.Hide();
                button2.Hide();
            }
            else if (comboBox1.SelectedIndex != -1)
            {
                button1.Show();
                button2.Show();
            }
        }

        private void checkDirectoryExists()
        {
            if (!Directory.Exists(duckyDirectory))
            {
                DialogResult result = MessageBox.Show("The directory: \"" + duckyDirectory + "\"" + " does not exist.\nWould you like to create it for this application?",
                    "NOTICE!",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.None,
                    MessageBoxDefaultButton.Button3);

                if (result == DialogResult.Yes) // If user said yes
                {
                    Directory.CreateDirectory(duckyDirectory);
                    MessageBox.Show(duckyDirectory + "has been created!");
                }
            }
        }

        private void checkDirectoryJava()
        {
            string java32 = selectedDrive + "Program Files (x86)\\Java";
            string java64 = selectedDrive + "Program Files\\Java";
            javaHome = duckyDirectory + "\\java.exe";

            if (!File.Exists(javaHome)) // If java.exe is NOT in \RubberDucky
            {
                if (Directory.Exists(java32)) // Then Java32 is installed
                {
                string[] java32subdirectories = Directory.GetDirectories(java32);
                javaEXELocation = java32subdirectories.Last() + "\\bin\\java.exe"; // Selects most current version
                File.Copy(javaEXELocation, javaHome);
                }
                else if (Directory.Exists(java64)) // Then Java64 is installed
                {
                    string[] java64subdirectories = Directory.GetDirectories(java64);
                    javaEXELocation = java64subdirectories.Last() + "\\bin\\java.exe"; // Selects most current version
                    File.Copy(javaEXELocation, javaHome);
                }
                else
                {
                    MessageBox.Show("No java.exe file was found.\n\n" +
                                    "If you do not have Java installed, please download it from Oracle and re-run this program.\n\n" +
                                    "If you installed Java to a non-default directory, " +
                                    "please copy your java.exe file to:\n" + duckyDirectory + " and re-run this program.",
                        "ERROR: No Java File Found!",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
                }
            }
        }

        private void checkDirectoryEncoder()
        {
            encoderLocation = duckyDirectory + "\\encoder.jar";
            if (!File.Exists(encoderLocation))
            {
                // Download encoder.jar if not found
                // Credits to: http://stackoverflow.com/questions/32223706/download-zipball-from-github-in-c-sharp
                using (var client = new WebClient())
                {
                    client.Headers.Add("user-agent", "Anything");
                    client.DownloadFile(encoderURL, encoderLocation);
                }
            }
        }

        private void checkDirectoryInputFile()
        {
            if (!File.Exists(duckyDirectory + "\\input.txt"))
                File.CreateText(duckyDirectory + "\\input.txt");
        }


    }
}
