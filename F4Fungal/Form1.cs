using CoreOSC;
using System.Runtime.InteropServices;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace F4Fungal
{
    public partial class Form1 : Form
    {

        public static UDPSender OSCSender;
        public static Form1 MainFormGlobal;
        bool editMode = false;
        public Form1()
        {
            InitializeComponent();
            MainFormGlobal = this;
            OSCSender = new UDPSender("127.0.0.1", 9000);//9000

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
              //  TTSButton.PerformClick();

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

                if (id == 0)
                {
                    var message0 = new CoreOSC.OscMessage(MainFormGlobal.textBox2.Text.ToString(), true);

                    OSCSender.Send(message0);
                    MainFormGlobal.panel1.BackColor = Color.Green;
                   // MainFormGlobal.textBox2.BackColor = Color.Green;
                    //  MessageBox.Show("wa1");
                    Thread.Sleep(100);

                    var message1 = new CoreOSC.OscMessage(MainFormGlobal.textBox2.Text.ToString(), false);

                    OSCSender.Send(message1);
                   
                   // Thread.Sleep(1000);
                   // MainFormGlobal.textBox2.BackColor = Color.White;
                    // MainFormGlobal.button1.ForeColor = Color.White;
                    //  MessageBox.Show("wa2");

                }
      
              

                
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
          
            string normalKey = textBox1.Text.ToString();

            Keys normKey;
            Enum.TryParse(normalKey, out normKey);
            RegisterHotKey(this.Handle, 0,0, normKey.GetHashCode());
            button1.Enabled = false;

            Settings1.Default.Button = textBox1.Text.ToString();
            Settings1.Default.Parameter = textBox2.Text.ToString();
            Settings1.Default.Save();

            //textBox1.ReadOnly = true;
            textBox2.ReadOnly = true;
            // MessageBox.Show("wa1");
            button3.Enabled = true;

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            textBox1.Text = Settings1.Default.Button;
            textBox2.Text = Settings1.Default.Parameter;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Settings1.Default.Button = textBox1.Text.ToString();
            Settings1.Default.Parameter = textBox2.Text.ToString();
            Settings1.Default.Save();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            UnregisterHotKey(this.Handle,0);
           // textBox1.ReadOnly = false;
            textBox2.ReadOnly = false;

            button1.Enabled=true;
            button3.Enabled = false;
            MainFormGlobal.panel1.BackColor = Color.White;
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (textBox1.Enabled == true)
            {
                Keys modifierKeys = e.Modifiers;

                Keys pressedKey = e.KeyData ^ modifierKeys; //remove modifier keys

                //do stuff with pressed and modifier keys
                var converter = new KeysConverter();

                textBox1.Text = converter.ConvertToString(pressedKey);
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (button1.Enabled == true)
            {
                textBox1.Select();
                textBox1.Enabled = true;
                button4.Enabled = true;
               // editMode = true;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            
            textBox1.Enabled = false;
            button4.Enabled = false;
           // editMode= false;

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}