using System.Diagnostics;
using System.Text.RegularExpressions;

public class MainForm : Form 
{
    private TextBox _txtVideoPath;
    private Button _btnSelectFile;
    private ComboBox _cboSubtitles;
    private Button _btnStartProcess;
    private ProgressBar _progressBar;

    public MainForm()
    {
        Text = "EmbedEasy";
        Size = new System.Drawing.Size(450, 250);
        MinimumSize = new System.Drawing.Size(450, 250);
        MaximumSize = new System.Drawing.Size(450, 250);

        InitializeComponents();
    }

    private void InitializeComponents()
    {
        Panel panel = new Panel();
        panel.Dock = DockStyle.Top;
        panel.Width = ClientSize.Width;
        panel.Height = 30;
        
        _txtVideoPath = new TextBox();
        _txtVideoPath.Dock = DockStyle.Left;
        _txtVideoPath.Width = (int)(ClientSize.Width * 0.7);
        _txtVideoPath.ReadOnly = true;
        
        _btnSelectFile = new Button();
        _btnSelectFile.Text = "Escolha o arquivo";
        _btnSelectFile.AutoSize = true;
        _btnSelectFile.Dock = DockStyle.Right;
        _btnSelectFile.Width = (int)(ClientSize.Width * 0.2);
        _btnSelectFile.Click += OnSelectFile;
        
        panel.Controls.Add(_txtVideoPath);
        panel.Controls.Add(_btnSelectFile);
        
        Controls.Add(panel);
    
        _cboSubtitles = new ComboBox();
        Controls.Add(_cboSubtitles);
        _cboSubtitles.Width = (int)(ClientSize.Width * 0.8);
        _cboSubtitles.DisplayMember = "Title";
        _cboSubtitles.Left = (ClientSize.Width - _cboSubtitles.Width) / 2;
        _cboSubtitles.Top = panel.Height + 10;
        
        _btnStartProcess = new Button();
        _btnStartProcess.Text = "Adicionar Legenda";
        _btnStartProcess.AutoSize = true;
        _btnStartProcess.Anchor = AnchorStyles.None;
        _btnStartProcess.Click += (sender, e) => {
            if (_cboSubtitles.SelectedItem is Subtitles subtitles)
            {
                EmbedSubtitle(subtitles, _txtVideoPath.Text);
            }
        };
        Controls.Add(_btnStartProcess);

        _btnStartProcess.Left = (ClientSize.Width - _btnStartProcess.Width) / 2;
        _btnStartProcess.Top = ClientSize.Height - _btnStartProcess.Height - 1;

        _progressBar = new ProgressBar();
        _progressBar.Width = (int)(ClientSize.Width * 0.8);
        _progressBar.Left = (ClientSize.Width - _progressBar.Width) / 2;
        _progressBar.Top = _cboSubtitles.Top + _cboSubtitles.Height + 10;
        Controls.Add(_progressBar);
    }

    private void OnSelectFile(object sender, EventArgs e)
    {
        OpenFileDialog openFileDialog = new OpenFileDialog();
        openFileDialog.Filter = "Video (*.mkv)|*.mkv";

        if(openFileDialog.ShowDialog() == DialogResult.OK)
        {
            string filePath = openFileDialog.FileName;
            _txtVideoPath.Text = filePath;
        }

        if(!string.IsNullOrEmpty(_txtVideoPath.Text))
        {
            LoadSubtitlesList(GetSubtitles(_txtVideoPath.Text));
        }
    }

    private async void EmbedSubtitle(Subtitles subtitle, string filePath)
    {
        if (subtitle != null)
        {
    
            await Task.Run(() =>
            {
                Process cmd = new Process();
                Console.WriteLine("Embedding Start");
    
                cmd.StartInfo.FileName = "ffmpeg";
                cmd.StartInfo.ArgumentList.Add("-i");
                cmd.StartInfo.ArgumentList.Add(filePath);
                cmd.StartInfo.ArgumentList.Add("-vf");
    
                Console.WriteLine(subtitle.Id);
                // Escapar o caminho do arquivo para o filtro subtitles
                string escapedFilePath = filePath.Replace(@"\", @"\\").Replace(":", @"\:");
                cmd.StartInfo.ArgumentList.Add($"subtitles='{escapedFilePath}':si={subtitle.Id},eq=saturation=0.8");
    
                cmd.StartInfo.ArgumentList.Add("-c:v");
                cmd.StartInfo.ArgumentList.Add("hevc_amf");
                cmd.StartInfo.ArgumentList.Add("-quality");
                cmd.StartInfo.ArgumentList.Add("balanced");
                cmd.StartInfo.ArgumentList.Add("-c:a");
                cmd.StartInfo.ArgumentList.Add("copy");
                cmd.StartInfo.ArgumentList.Add(Path.Combine(Path.GetDirectoryName(filePath), Path.GetFileNameWithoutExtension(filePath) + "_Legendado.mkv"));
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
                _progressBar.Invoke((MethodInvoker)(() => _progressBar.Value = 100));
                
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
        _txtVideoPath.Text = string.Empty;
        _cboSubtitles.Items.Clear();
        _cboSubtitles.Text = string.Empty;
        _progressBar.Value = 0;
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
            TimeSpan totalTime = GetVideoDuration(_txtVideoPath.Text);
    
            if (totalTime.TotalSeconds > 0)
            {
                int progress = (int)((currentTime.TotalSeconds / totalTime.TotalSeconds) * 100);
                _progressBar.Invoke((MethodInvoker)(() => _progressBar.Value = progress));
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

    private string GetSubtitles(string filePath)
    {
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

    private List<Subtitles> LoadSubtitlesList(string output)
    {
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
        
        _cboSubtitles.Items.Clear(); // Clear the items before adding new ones
        
        foreach (var item in subtitles)
        {
            Console.WriteLine($"Id:{item.Id}, Title:{item.Title}");
            _cboSubtitles.Items.Add(new Subtitles{Id = item.Id, Title = item.Title});
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