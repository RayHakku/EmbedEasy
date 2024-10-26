using System;
using System.Windows.Forms;


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
        // Criar um Panel
        Panel panel = new Panel();
        panel.Dock = DockStyle.Top;
        panel.Width = this.ClientSize.Width;
        panel.Height = 30;
        
        // Criar o TextBox
        txtVideoPath = new TextBox();
        txtVideoPath.Dock = DockStyle.Left;
        txtVideoPath.Width = (int)(panel.Width * 0.7); // 80% da largura do painel
        txtVideoPath.Enabled = false;
        
        
        // Criar o Button
        btnSelectFile = new Button();
        btnSelectFile.Text = "Escolha o arquivo";
        btnSelectFile.AutoSize = true;
        btnSelectFile.Dock = DockStyle.Right;
        btnSelectFile.Width = (int)(panel.Width * 0.2);
        btnSelectFile.Click += SelectFile;
        
        // Adicionar os controles ao Panel
        panel.Controls.Add(txtVideoPath);
        panel.Controls.Add(btnSelectFile);
        
        // Adicionar o Panel ao formulário
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

    }
}