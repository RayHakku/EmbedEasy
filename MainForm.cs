using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

public class MainForm : Form {

    
    private TextBox txtVideoPath;
    private Button btnSelectFile;
    private ComboBox ddSubtitles;
    private Button btnStartProcess;

    public MainForm(){
        this.Text = "Embedeasy";
        this.Size = new System.Drawing.Size(450, 150);
        this.MinimumSize = new System.Drawing.Size(450,150);
        this.MaximumSize = new System.Drawing.Size (450,150);

        InitializeComponets();
    }

    private void InitializeComponets(){
        Panel panel = new Panel();
        panel.Dock = DockStyle.Top;
        panel.Width = this.ClientSize.Width;
        panel.Height = 30;
        
        txtVideoPath = new TextBox();
        txtVideoPath.Dock = DockStyle.Left;
        txtVideoPath.Width = (int)(this.ClientSize.Width * 0.7);
        txtVideoPath.ReadOnly = true;
        
        
        
        btnSelectFile = new Button();
        btnSelectFile.Text = "Escolha o arquivo";
        btnSelectFile.AutoSize = true;
        btnSelectFile.Dock = DockStyle.Right;
        btnSelectFile.Width = (int)(this.ClientSize.Width * 0.2);
        btnSelectFile.Click += new EventHandler(SelectFile);
        
        panel.Controls.Add(txtVideoPath);
        panel.Controls.Add(btnSelectFile);
        

        this.Controls.Add(panel);
    
        ddSubtitles = new ComboBox();
        this.Controls.Add(ddSubtitles);
        ddSubtitles.Width = (int)(this.ClientSize.Width * 0.8);
        ddSubtitles.DisplayMember = "Title";
        ddSubtitles.Left = (this.ClientSize.Width - ddSubtitles.Width)/2;
        ddSubtitles.Top = panel.Height + 10;
        
        btnStartProcess = new Button();
        btnStartProcess.Text = "Adicionar Legenda";
        btnStartProcess.AutoSize = true;
        btnStartProcess.Anchor = AnchorStyles.None;
        btnStartProcess.Click += (sender, e) => {
            if (ddSubtitles.SelectedItem is Subtitles subtitles)
            {
                EmbedySubtitle(subtitles);
            }
            else
            {
                Console.WriteLine("not subtitle");
            }
        };
        this.Controls.Add(btnStartProcess);

        btnStartProcess.Left = (this.ClientSize.Width - btnStartProcess.Width)/2;
        btnStartProcess.Top = this.ClientSize.Height - btnStartProcess.Height - 1;
    }

    private void SelectFile(object sender, EventArgs e){
        OpenFileDialog openFileDialog = new OpenFileDialog();
        openFileDialog.Filter = "Video (*.mkv)|*.mkv";

        if(openFileDialog.ShowDialog() == DialogResult.OK){
            string filePath = openFileDialog.FileName;

            txtVideoPath.Text = filePath;
        }

        if(!string.IsNullOrEmpty(txtVideoPath.Text)) {
            // Function Add Subtitles List to ddSubittles.\
            ListSubtitles(GetSubtitles(txtVideoPath.Text));
            
        }
    }

    private void EmbedySubtitle(Subtitles sub){
        if (sub != null)
        {
            Subtitles subt = sub as Subtitles;
            
            MessageBox.Show($"{subt.Title}");
        } else
        {
            MessageBox.Show("Null Sub");
        }
    }

    private string GetSubtitles(string filePath){
        Process cmd = new Process();

        Console.WriteLine("GetSubtitles");

        cmd.StartInfo.FileName = "ffmpeg";
        cmd.StartInfo.CreateNoWindow = true;
        cmd.StartInfo.Arguments = $"-i \"{filePath}\"";
        cmd.StartInfo.RedirectStandardError = true;
        cmd.StartInfo.UseShellExecute = false;

        cmd.Start();

         
        string output = cmd.StandardError.ReadToEnd();
        cmd.WaitForExit();

        
        return output;
    }

    private List<Subtitles> ListSubtitles(string output){
        List<Subtitles> subtitles = new List<Subtitles>();
        string[] videoInfo = output.Split('\n');

        Console.WriteLine("ListSubtitles");

        foreach (string legendas in videoInfo ) {
            if(legendas.Contains("Subtitle"))
            {
                
               // Console.WriteLine($"Primeiro IF:{legendas}");
                Regex regexSub = new Regex(@"Stream #(\d+:\d+)(\(\w+\))?: Subtitle: (\w+)( \(\w+\))?");
                if( regexSub.IsMatch(legendas) ) {
                    Match match = regexSub.Match(legendas);
                    if (match.Success)
                    {
                        //Console.WriteLine($"Segundo if:{legendas}");
                        string subtitleID = RemoveBeforePunctuation(match.Groups[1].Value, ':');
                        Subtitles subtitle = new Subtitles
                        {
                            Id = Convert.ToInt32(subtitleID)
                        };
                        subtitles.Add(subtitle);
                    }
                }
            }
            else if (legendas.Contains("title"))
            {
                Console.WriteLine($"Else if 1:{legendas}");
                string[] parts = legendas.Split(new string[] { "title           : " }, StringSplitOptions.None);
                if (parts.Length > 1)
                {
                    string title = parts[1].Trim();
                    if (subtitles.Count > 0)
                    {
                        subtitles.Last().Title = title;
                    }
                }
            }
        }
        
        ddSubtitles.Items.Clear(); // Clear the items before adding new ones
        
        foreach (var item in subtitles)
        {
            Console.WriteLine($"Id:{item.Id}, Title:{item.Title}");
            ddSubtitles.Items.Add(new Subtitles{Id = item.Id, Title = item.Title});
        }
        
        return subtitles;
    }

    public static string RemoveBeforePunctuation(string input, char punctuation)
    {
        int index = input.IndexOf(punctuation);
        if (index >= 0)
        {
            return input.Substring(index + 1).Trim();
        }
        return input;
    }
}