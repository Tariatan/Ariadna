using System;
using System.Windows.Forms;

namespace Ariadna
{
    public partial class SplashForm : Form
    {
        public string StatusInfo
        {
            set
            {
                mStatusInfo = value;
                ChangeStatusText();
            }
            get
            {
                return mStatusInfo;
            }
        }

        private string mStatusInfo = "";

        public SplashForm()
        {
            InitializeComponent();
        }


        public void ChangeStatusText()
        {
            try
            {
                if (this.InvokeRequired)
                {
                    this.Invoke(new MethodInvoker(this.ChangeStatusText));
                    return;
                }

                m_StatusInfoLbl.Text = mStatusInfo;
            }
            catch (Exception)
            {
            }
        }
    }
}
