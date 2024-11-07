using System.Diagnostics;
using System.Text.RegularExpressions;

public class MainForm : Form 
{
    private const int FORM_WIDTH = 500;
    private const int FORM_HEIGHT = 300;
    private const int PADDING = 15;

    private TableLayoutPanel _mainLayout;
    private TextBox _txtVideoPath;
    private Button _btnSelectFile;
    private ComboBox _cboSubtitles;
    private Button _btnStartProcess;
    private ProgressBar _progressBar;
    private Label _lblStatus;

    public MainForm()
    {
        Text = "EmbedEasy - Video Subtitle Embedder";
        Size = new Size(FORM_WIDTH, FORM_HEIGHT);
        MinimumSize = new Size(FORM_WIDTH, FORM_HEIGHT);
        StartPosition = FormStartPosition.CenterScreen;

        InitializeComponents();
    }

    private void InitializeComponents()
    {
        _mainLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(PADDING),
            RowCount = 5,
            ColumnCount = 1,
            RowStyles = {
                new RowStyle(SizeType.Absolute, 30),
                new RowStyle(SizeType.Absolute, 65),
                new RowStyle(SizeType.Absolute, 35),
                new RowStyle(SizeType.Percent, 100),
                new RowStyle(SizeType.AutoSize)
            }
        };

        // File selection panel
        var fileSelectionPanel = CreateFileSelectionPanel();
        _mainLayout.Controls.Add(fileSelectionPanel, 0, 0);

        // Subtitle selection
        var subtitlePanel = CreateSubtitleSelectionPanel();
        _mainLayout.Controls.Add(subtitlePanel, 0, 1);

        // Progress section
        var progressPanel = CreateProgressPanel();
        _mainLayout.Controls.Add(progressPanel, 0, 2);

        // Status label
        _lblStatus = new Label
        {
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            ForeColor = Color.Gray
        };
        _mainLayout.Controls.Add(_lblStatus, 0, 3);

        // Action button
        _btnStartProcess = new Button
        {
            Text = "Embed Subtitle",
            Dock = DockStyle.Bottom,
            Padding = new Padding(10, 5, 10, 5),
            Height = 35,
            Enabled = false
        };
        _btnStartProcess.Click += OnStartProcessClick;
        _mainLayout.Controls.Add(_btnStartProcess, 0, 4);

        Controls.Add(_mainLayout);
    }

    private Panel CreateFileSelectionPanel()
    {
        var panel = new Panel { Height = 35, Dock = DockStyle.Top };
        
        _txtVideoPath = new TextBox
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            PlaceholderText = "Select a video file...",
            Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top,
            Height = 23  // Standard Windows Forms TextBox height
        };
    
        _btnSelectFile = new Button
        {
            Text = "Browse...",
            Dock = DockStyle.Right,
            Width = 100,
            Height = 23,  // Match TextBox height
            Padding = new Padding(0),
            Margin = new Padding(3, 0, 0, 0),  // Add small margin between TextBox and Button
            AutoSize = false  // Prevent auto-sizing
        };
        _btnSelectFile.Click += OnSelectFile;
    
        panel.Controls.Add(_btnSelectFile);
        panel.Controls.Add(_txtVideoPath);
        return panel;
    }

    private Panel CreateSubtitleSelectionPanel()
    {
        var panel = new Panel 
        { 
            Height = 65, // Increased height to accommodate both controls
            Dock = DockStyle.Top,
            Padding = new Padding(0, 10, 0, 0)
        };
    
        var label = new Label
        {
            Text = "Available Subtitles:",
            Dock = DockStyle.Top,
            Height = 20, // Explicit height for the label
            Margin = new Padding(0, 0, 0, 5) // Add margin between label and ComboBox
        };
    
        _cboSubtitles = new ComboBox
        {
            Dock = DockStyle.Top, // Changed to Top instead of Bottom
            DropDownStyle = ComboBoxStyle.DropDownList,
            DisplayMember = "Title",
            Height = 25 // Explicit height for the ComboBox
        };
        _cboSubtitles.SelectedIndexChanged += OnSubtitleSelectionChanged;
    
        panel.Controls.Add(_cboSubtitles); // Add ComboBox first
        panel.Controls.Add(label); // Then add label so it appears above
        return panel;
    }

    private Panel CreateProgressPanel()
    {
        var panel = new Panel
        {
            Height = 35,
            Dock = DockStyle.Top,
            Padding = new Padding(0, 10, 0, 0)
        };

        _progressBar = new ProgressBar
        {
            Dock = DockStyle.Fill,
            Style = ProgressBarStyle.Continuous
        };

        panel.Controls.Add(_progressBar);
        return panel;
    }

    private void OnSubtitleSelectionChanged(object sender, EventArgs e)
    {
        _btnStartProcess.Enabled = _cboSubtitles.SelectedItem != null;
    }

    private void OnStartProcessClick(object sender, EventArgs e)
    {
        if (_cboSubtitles.SelectedItem is Subtitles subtitles)
        {
            _btnStartProcess.Enabled = false;
            _btnSelectFile.Enabled = false;
            _cboSubtitles.Enabled = false;
            _lblStatus.Text = "Processing...";
            EmbedSubtitle(subtitles, _txtVideoPath.Text);
        }
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