using System.Diagnostics;
using System.Windows.Forms;

class Program
{
    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new MainForm());
    }

    static List<string> ListSubtitles(string videoPath)
    {
        return null;
    }

    static void EmbedSubtitle(string videoPath, string subtitleTrack)
    {
        
    }
}
