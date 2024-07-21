using System.Windows.Forms;

namespace Ariadna.SplashScreen;

public partial class SplashForm : Form
{
    private string m_StatusInfo = string.Empty;

    public string StatusInfo
    {
        set
        {
            m_StatusInfo = value;
            ChangeStatusText();
        }
        get => m_StatusInfo;
    }


    public SplashForm()
    {
        InitializeComponent();
    }


    public void ChangeStatusText()
    {
        try
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(ChangeStatusText));
                return;
            }

            m_StatusInfoLbl.Text = m_StatusInfo;
        }
        catch
        {
            // Nothing to do
        }
    }
}