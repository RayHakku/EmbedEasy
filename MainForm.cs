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

    private ProgressBar progressBar;

    public MainForm(){
        this.Text = "Embedeasy";
        this.Size = new System.Drawing.Size(450, 250);
        this.MinimumSize = new System.Drawing.Size(450,250);
        this.MaximumSize = new System.Drawing.Size (450,250);

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
                EmbedySubtitle(subtitles,txtVideoPath.Text);
            }
            else
            {
                Console.WriteLine("not subtitle");
            }
        };
        this.Controls.Add(btnStartProcess);

        btnStartProcess.Left = (this.ClientSize.Width - btnStartProcess.Width)/2;
        btnStartProcess.Top = this.ClientSize.Height - btnStartProcess.Height - 1;

        progressBar = new ProgressBar();
        progressBar.Width = (int)(this.ClientSize.Width * 0.8);
        progressBar.Left = (this.ClientSize.Width - progressBar.Width) / 2;
        progressBar.Top = ddSubtitles.Top + ddSubtitles.Height + 10;
        this.Controls.Add(progressBar);
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

    private async void EmbedySubtitle(Subtitles sub, string filePath)
    {
        if (sub != null)
        {
    
            await Task.Run(() =>
            {
                Process cmd = new Process();
                Console.WriteLine("Embedding Start");
    
                cmd.StartInfo.FileName = "ffmpeg";
                cmd.StartInfo.ArgumentList.Add("-i");
                cmd.StartInfo.ArgumentList.Add(filePath);
                cmd.StartInfo.ArgumentList.Add("-vf");
    
                Console.WriteLine(sub.Id);
                // Escapar o caminho do arquivo para o filtro subtitles
                string escapedFilePath = filePath.Replace(@"\", @"\\").Replace(":", @"\:");
                cmd.StartInfo.ArgumentList.Add($"subtitles='{escapedFilePath}':si={sub.Id},eq=saturation=0.8");
    
                cmd.StartInfo.ArgumentList.Add("-c:v");
                cmd.StartInfo.ArgumentList.Add("hevc_amf");
                cmd.StartInfo.ArgumentList.Add("-quality");
                cmd.StartInfo.ArgumentList.Add("balanced");
                cmd.StartInfo.ArgumentList.Add("-c:a");
                cmd.StartInfo.ArgumentList.Add("copy");
                cmd.StartInfo.ArgumentList.Add(Path.Combine(Path.GetDirectoryName(filePath), Path.GetFileNameWithoutExtension(filePath) + "_Legendado.mp4"));
                cmd.StartInfo.UseShellExecute = false;
                cmd.StartInfo.CreateNoWindow = true;
                cmd.StartInfo.RedirectStandardError = true;
                cmd.StartInfo.RedirectStandardOutput = true;
    
                cmd.OutputDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        Console.WriteLine(e.Data);
                        UpdateProgressBar(e.Data);
                    }
                };
    
                cmd.ErrorDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        Console.WriteLine(e.Data);
                        UpdateProgressBar(e.Data);
                    }
                };
    
                cmd.Start();
                cmd.BeginOutputReadLine();
                cmd.BeginErrorReadLine();
    
                // Timeout de 10 minutos (600000 milissegundos)
                bool exited = cmd.WaitForExit(600000);
                if (!exited)
                {
                    // Forçar o encerramento do processo se ele ainda estiver em execução
                    cmd.Kill();
                    Console.WriteLine("Processo ffmpeg forçado a encerrar devido ao timeout.");
                }
    
                // Set progress bar to 100% when process completes
                progressBar.Invoke((MethodInvoker)(() => progressBar.Value = 100));
                
                MessageBox.Show("Embedding Done");
                // Reset all fields after completion
                this.Invoke((MethodInvoker)(() => ResetFields()));
            });
        }
        else
        {
            MessageBox.Show("Null Sub");
        }
    }

    private void ResetFields()
    {
        txtVideoPath.Text = string.Empty;
        ddSubtitles.Items.Clear();
        ddSubtitles.Text = string.Empty;
        progressBar.Value = 0;
    }
    
    private void UpdateProgressBar(string data)
    {
        // Analisar a saída do ffmpeg para extrair informações de progresso
        Regex regex = new Regex(@"time=(\d+:\d+:\d+.\d+)");
        Match match = regex.Match(data);
        if (match.Success)
        {
            string timeStr = match.Groups[1].Value;
            TimeSpan currentTime = TimeSpan.Parse(timeStr);
    
            // Supondo que você tenha a duração total do vídeo
            TimeSpan totalTime = GetVideoDuration(txtVideoPath.Text);
    
            if (totalTime.TotalSeconds > 0)
            {
                int progress = (int)((currentTime.TotalSeconds / totalTime.TotalSeconds) * 100);
                progressBar.Invoke((MethodInvoker)(() => progressBar.Value = progress));
            }
        }
    }
    
    private TimeSpan GetVideoDuration(string filePath)
    {
        Process cmd = new Process();
        cmd.StartInfo.FileName = "ffmpeg";
        cmd.StartInfo.Arguments = $"-i \"{filePath}\"";
        cmd.StartInfo.RedirectStandardError = true;
        cmd.StartInfo.UseShellExecute = false;
        cmd.StartInfo.CreateNoWindow = true;
    
        cmd.Start();
        string output = cmd.StandardError.ReadToEnd();
        cmd.WaitForExit();
    
        Regex regex = new Regex(@"Duration: (\d+:\d+:\d+.\d+)");
        Match match = regex.Match(output);
        if (match.Success)
        {
            string durationStr = match.Groups[1].Value;
            return TimeSpan.Parse(durationStr);
        }
    
        return TimeSpan.Zero;
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
                        if (int.TryParse(subtitleID, out int id))
                        {
                            Subtitles subtitle = new Subtitles
                            {
                                Id = id - 2
                            };
                            subtitles.Add(subtitle);
                            Console.WriteLine($"Added subtitle with ID: {subtitle.Id}");
                        }
                        else
                        {
                            Console.WriteLine($"Failed to parse subtitleID: {subtitleID}");
                        }
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