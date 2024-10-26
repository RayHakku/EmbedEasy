using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Text;
using System.IO;


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
        txtVideoPath.Width = (int)(panel.Width * 0.7);
        txtVideoPath.ReadOnly = true;
        
        
        
        btnSelectFile = new Button();
        btnSelectFile.Text = "Escolha o arquivo";
        btnSelectFile.AutoSize = true;
        btnSelectFile.Dock = DockStyle.Right;
        btnSelectFile.Width = (int)(panel.Width * 0.2);
        btnSelectFile.Click += new EventHandler(SelectFile);
        
        panel.Controls.Add(txtVideoPath);
        panel.Controls.Add(btnSelectFile);
        

        this.Controls.Add(panel);
    
        ddSubtitles = new ComboBox();
        this.Controls.Add(ddSubtitles);
        ddSubtitles.Width = (int)(this.ClientSize.Width * 0.8);
        ddSubtitles.Left = (this.ClientSize.Width - ddSubtitles.Width)/2;
        ddSubtitles.Top = panel.Height + 10;
        
        btnStartProcess = new Button();
        btnStartProcess.Text = "Adicionar Legenda";
        btnStartProcess.AutoSize = true;
        btnStartProcess.Anchor = AnchorStyles.None;
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

        if(txtVideoPath.Text != null) {
            GetSubtitles(txtVideoPath.Text);
        }
    }

    private string GetSubtitles(string filePath){
        Process cmd = new Process();

        Console.WriteLine("GetSubtitles");

        cmd.StartInfo.FileName = "ffmpeg";
        cmd.StartInfo.Arguments = $"-i \"{filePath}\"";
        cmd.StartInfo.RedirectStandardError = true;
        cmd.StartInfo.UseShellExecute = false;

        cmd.Start();

         
        string output = cmd.StandardError.ReadToEnd();
        cmd.WaitForExit();

        MessageBox.Show(output);

        return output;
    }
}