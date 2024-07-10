using System;
using System.Windows.Forms;

namespace Custom_Message
{
    public partial class CustomMessage : Form
    {
        private Timer mytimer = new Timer();
        private int tickCount = 0;

        public CustomMessage(string text, int interval)
        {
            InitializeComponent();
            label_text.Text = text;
            mytimer.Tick += new EventHandler(mytimer_Tick);
            mytimer.Interval = interval;
            Counter = interval;
            mytimer.Start();
        }

        private static int Counter = 10;
        void mytimer_Tick(object sender, EventArgs e)
        {
            tickCount++;
            if (tickCount > Counter)
            {
                base.Opacity -= 0.035000000149011612;
                if (base.Opacity <= 0.0)
                {
                    mytimer.Stop();
                    base.Close();
                }
            }
        }
    }
}
