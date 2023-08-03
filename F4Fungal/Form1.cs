using CoreOSC;
using Microsoft.VisualBasic;
using System;
using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace F4Fungal
{
    public partial class Form1 : Form
    {

        public static UDPSender OSCSender;
        public static Form1 MainFormGlobal;
        public Form1()
        {
            InitializeComponent();
            MainFormGlobal = this;
            OSCSender = new UDPSender("127.0.0.1", 9000);//9000
            comboBoxType.SelectedIndex = 0;
            voiceCommandsStored = Settings1.Default.storedCommands;

        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            CatchHotkey(ref m);
        }
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
            }
        }

        enum KeyModifier
        {
            None = 0,
            Alt = 1,
            Control = 2,
            Shift = 4,
            WinKey = 8
        }
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);

        [DllImport("user32.dll")]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        public static void CatchHotkey(ref Message m)
        {
            //link to implementation https://www.fluxbytes.com/csharp/how-to-register-a-global-hotkey-for-your-application-in-c/ 
            //additional links https://stackoverflow.com/questions/2450373/set-global-hotkeys-using-c-sharp

            //  System.Diagnostics.Debug.WriteLine("-------------get key press id: " + m.Result.ToString());
            if (m.Msg == 0x0312)
            {
                /* Note that the three lines below are not needed if you only want to register one hotkey.
                * The below lines are useful in case you want to register multiple keys, which you can use a switch with the id as argument, or if you want to know which key/modifier was pressed for some particular reason. */


                Keys key = (Keys)((int)m.LParam >> 16 & 0xFFFF);                  // The key of the hotkey that was pressed.
                KeyModifier modifier = (KeyModifier)((int)m.LParam & 0xFFFF);       // The modifier of the hotkey that was pressed.
                int id = m.WParam.ToInt32();                                        // The id of the hotkey that was pressed.

                System.Diagnostics.Debug.WriteLine("-------------get key press id: " + key.ToString());


                HotkeyPressed(id);

            }

        }

        private void button1_Click(object sender, EventArgs e)
        {


          
            Settings1.Default.storedCommands = voiceCommandsStored;
            Settings1.Default.Save();


            int index = 0;
           
            foreach (var normalKey in VCButton)
            {
              
                Keys normKey;

                Enum.TryParse(normalKey, out normKey);
                RegisterHotKey(this.Handle, index, 0, normKey.GetHashCode());
                index++;

            }
            buttonStart.Enabled = false;
            buttonStop.Enabled = true;
            buttonAdd.Enabled = false;
            deleteCommandsToggle.Enabled= false;
            buttonClearAll.Enabled = false;


        }

        private void Form1_Load(object sender, EventArgs e)
        {
            buttonCommands();
            if (!Directory.Exists("ExportList"))
            {
                Directory.CreateDirectory("ExportList");
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Settings1.Default.storedCommands = voiceCommandsStored;
            Settings1.Default.Save();
        }

        private void button3_Click(object sender, EventArgs e)
        {     
            for(int i = 0; i < VCButton.Count; i++)
            {
                UnregisterHotKey(this.Handle, i);
            }
            buttonStart.Enabled = true;
            buttonAdd.Enabled = true;
            buttonStop.Enabled = false;
            deleteCommandsToggle.Enabled = true;
            MainFormGlobal.panel1.BackColor = Color.White;
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (textBoxButton.Enabled == true)
            {
                Keys modifierKeys = e.Modifiers;

                Keys pressedKey = e.KeyData ^ modifierKeys; //remove modifier keys

                //do stuff with pressed and modifier keys
                var converter = new KeysConverter();

                textBoxButton.Text = converter.ConvertToString(pressedKey);
            }
        }



        static List<string> VCButton = new List<string>();
        static List<string> VCAddress = new List<string>();
        static List<string> VCType = new List<string>();
        static List<string> VCValue = new List<string>();

        static List<int> VCAutoDisable = new List<int>();


        public static string voiceCommandsStored = "";

    
                
        public static void clearButtonCommands()
        {
            VCAddress.Clear();
            VCButton.Clear();
            VCValue.Clear();
            VCType.Clear();
            VCAutoDisable.Clear();
            //refreshCommandList();
        }
        public static void removeButtonCommandsAt(int index)
        {
            VCAddress.RemoveAt(index);
            VCButton.RemoveAt(index);
            VCValue.RemoveAt(index);
            VCType.RemoveAt(index);
            VCAutoDisable.RemoveAt(index);
            // refreshCommandList();
        }

        public static void voiceCommandsAdd()
        {
            clearButtonCommands();

            int autoDisable = -1;
            if (MainFormGlobal.checkboxAutoDisable.Checked)
            {
                autoDisable = Int32.Parse(MainFormGlobal.textBoxAutoDisable.Text);
            }


            voiceCommandsStored += $"{MainFormGlobal.textBoxButton.Text}:{MainFormGlobal.textBoxParameter.Text}:{MainFormGlobal.comboBoxType.SelectedItem}:{MainFormGlobal.textBoxValue.Text}:{autoDisable};";

            buttonCommands();
          //  refreshCommandList();

        }

        public static void buttonCommands()
        {
            string words = voiceCommandsStored;

            string[] split = words.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string s in split)
            {

                if (s.Trim() != "")
                {

                    string words2 = s;
                    int count = 1;

                    string[] split2 = words2.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string s2 in split2)
                    {


                        if (count == 1)
                        {
                            VCButton.Add(s2);
                            System.Diagnostics.Debug.WriteLine("Phrase Added: " + s2);

                        }
                        if (count == 2)
                        {
                            VCAddress.Add(s2);
                            System.Diagnostics.Debug.WriteLine("address added: " + s2);

                        }
                        if (count == 3)
                        {
                            VCType.Add(s2);
                            System.Diagnostics.Debug.WriteLine("typeadded: " + s2);

                        }
                        if (count == 4)
                        {
                            VCValue.Add(s2);
                            System.Diagnostics.Debug.WriteLine("value added: " + s2);

                        }
                        if (count == 5)
                        {
                            VCAutoDisable.Add(Int32.Parse(s2));
                            System.Diagnostics.Debug.WriteLine("value added: " + s2);

                        }
                        count++;
                    }
                }
            }
            refreshCommandList();
        }
       private static Dictionary<string, bool> addressStatus = new Dictionary<string, bool>();
        public static void HotkeyPressed(int index)
        {    
                string address = VCAddress[index];
                string type = VCType[index];
            var VCMessage = new OscMessage(address, true);
            bool value;


            switch (type)
            {
                case "Bool":
                    if (string.Equals(VCValue[index], "true", StringComparison.InvariantCultureIgnoreCase))
                    {
                        VCMessage = new OscMessage(address, true);
                    }
                    if (string.Equals(VCValue[index], "false", StringComparison.InvariantCultureIgnoreCase))
                    {
                        VCMessage = new OscMessage(address, false);
                    }
                    if (string.Equals(VCValue[index], "{toggle}", StringComparison.InvariantCultureIgnoreCase))
                    {
                        // Toggle logic: If the address exists in the dictionary, invert its current value
                        if (addressStatus.TryGetValue(address, out bool currentValue))
                        {
                            value = !currentValue;
                            addressStatus[address] = value;
                            VCMessage = new OscMessage(address, value);
                        }
                        else
                        {
                            // If the address doesn't exist in the dictionary, assume it's true as per your example
                            value = true;
                            addressStatus[address] = value;
                            VCMessage = new OscMessage(address, value);
                        }
                    }
                    break;


                case "Float":
                    float value1 = float.Parse(VCValue[index]);
                    VCMessage = new OscMessage(address, value1);
                    break;
                case "Int":
                    int value2 = int.Parse(VCValue[index]);
                    VCMessage = new OscMessage(address, value2);
                    break;
                default:
                    VCMessage = new OscMessage(address, true);
                    break;
            }

           OSCSender.Send(VCMessage);

            if(VCAutoDisable[index]!=-1)
            {
                var VCMessage2 = new OscMessage(address, false);

                Thread.Sleep(VCAutoDisable[index]);
                switch (type)
                {
                    case "Bool":
                        if (string.Equals(VCValue[index], "true", StringComparison.InvariantCultureIgnoreCase))
                        {
                            VCMessage2 = new OscMessage(address, false);
                        }
                        if (string.Equals(VCValue[index], "false", StringComparison.InvariantCultureIgnoreCase))
                        {
                            VCMessage2 = new OscMessage(address, true);
                        }

                        break;

                    case "Float":
  
                       // float value1 = float.Parse(VCValue[index]);
                        VCMessage2 = new OscMessage(address, 0);
                        break;
                    case "Int":
                      //  int value2 = int.Parse(VCValue[index]);
                        VCMessage2 = new OscMessage(address, 0);
                        break;
                    default:
                        VCMessage2 = new OscMessage(address,false);
                        break;
                }

                OSCSender.Send(VCMessage2);

            }
        }
        public static void refreshCommandList()
        {
            MainFormGlobal.listBox1.Items.Clear();
            voiceCommandsStored = "";
            for (var index = 0; index < VCAddress.Count; index++)
            {
                commandListHelper($"ID: {index + 1} | Phrase: {VCButton[index]} | Address: {VCAddress[index]} | Data Type: {VCType[index]} | Value: {VCValue[index]} | Auto Disable (ms): {VCAutoDisable[index]}");
                voiceCommandsStored += $"{VCButton[index]}:{VCAddress[index]}:{VCType[index]}:{VCValue[index]}:{VCAutoDisable[index]};";

            }
        }
        public static void commandListHelper(string line)
        {

            MainFormGlobal.listBox1.Items.Add(line);

        }

        private void button5_Click(object sender, EventArgs e)
        {
            voiceCommandsAdd();
        }

        private void listBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (deleteCommandsToggle.Checked == true)
            {
                try
                {
                    removeButtonCommandsAt(listBox1.SelectedIndex);
                    refreshCommandList();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }

            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            if (deleteCommandsToggle.Checked == true)
            {
                clearButtonCommands();
                refreshCommandList();
            }
        }

        private void deleteCommandsToggle_CheckedChanged(object sender, EventArgs e)
        {
            if(deleteCommandsToggle.Checked == true)
            {
                buttonClearAll.Enabled = true;
            }
            else
            {
                buttonClearAll.Enabled = false;
            }
        }



        private void buttonExport_Click(object sender, EventArgs e)
        {
            string folderPath = @"ExportList"; 
            string fileName = "export_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".txt"; 

            string filePath = Path.Combine(folderPath, fileName);

            // Create a text file and write some content to it
            File.WriteAllText(filePath, voiceCommandsStored);

            Process.Start("explorer.exe", "ExportList");
        }

        private void buttonImport_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                voiceCommandsStored = importFile(openFileDialog1.FileName);
                buttonCommands();
               // refreshCommandList();
            }
        }
        public static string importFile(string path)
        {
            try
            {
                string contents = "";
                using (FileStream stream = new FileStream(path, System.IO.FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        contents = reader.ReadToEnd();
                       
                        
                    }
                }
                return contents;

            }
            catch (Exception ex)
            {

               
                MessageBox.Show("[Text File Reader Error: This error occured while attempting to read the text file: " + ex.Message + "]");
                return "";
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("explorer.exe", "https://ko-fi.com/ttsvoicewizard");

        }

        private void checkboxAutoDisable_CheckedChanged(object sender, EventArgs e)
        {
            if(checkboxAutoDisable.Checked)            
           { 
                textBoxAutoDisable.Enabled = true;
            }
            else
            {
                textBoxAutoDisable.Enabled = false;
            }
        }
    }
}