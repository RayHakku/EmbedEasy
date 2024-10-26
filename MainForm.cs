using System;
using System.Windows.Forms;


public class MainForm : Form {
    private TextBox txtVideoPath;
    private Button btnSelectFile;
    private ComboBox ddSubtitles;
    private Button btnStartProcess;

    public MainForm(){
        this.Text = "Embedeasy";
        this.Size = new System.Drawing.Size(400, 200);
        this.MinimumSize = new System.Drawing.Size(400,200);
        this.MaximumSize = new System.Drawing.Size (500,300);

        InitializeComponets();
    }

    private void InitializeComponets(){
        TableLayoutPanel tableLayoutPanel = new TableLayoutPanel();
        tableLayoutPanel.Dock = DockStyle.Top;
        tableLayoutPanel.ColumnCount = 2;
        tableLayoutPanel.RowCount = 1;
        tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 80F));
        tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
        tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
    
        
        txtVideoPath = new TextBox();
        txtVideoPath.Dock = DockStyle.Fill;
        txtVideoPath.Enabled = false;
        tableLayoutPanel.Controls.Add(txtVideoPath, 0, 0);
    
        
        btnSelectFile = new Button();
        btnSelectFile.Text = "Escolha o arquivo";
        btnSelectFile.Click += SelectFile;
        tableLayoutPanel.Controls.Add(btnSelectFile, 1, 0); 
    
        
        this.Controls.Add(tableLayoutPanel);
    
        ddSubtitles = new ComboBox();
        ddSubtitles.Dock = DockStyle.Bottom;
        this.Controls.Add(ddSubtitles);
        
        btnStartProcess = new Button();
        btnStartProcess.Text = "Adicionar Legenda";
        btnStartProcess.AutoSize = true;
        btnStartProcess.Anchor = AnchorStyles.None;
        this.Controls.Add(btnStartProcess);

        btnStartProcess.Left = (this.ClientSize.Width - btnStartProcess.Width)/2;
        btnStartProcess.Top = (this.ClientSize.Height - btnStartProcess.Height) / 2;
    }

    private void SelectFile(object sender, EventArgs e){

    }
}