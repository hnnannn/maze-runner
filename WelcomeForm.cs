using System;
using System.Drawing;
using System.Windows.Forms;

class WelcomeForm : Form
{
    public int SelectedDifficulty = 3;
    private Label  titleLabel;
    private Label  taglineLabel;
    private Label  difficultyLabel;
    private Button easyButton;
    private Button mediumButton;
    private Button hardButton;
    private Button playButton;
    private Button exitButton;

    public WelcomeForm()
    {
        this.Text            = "MAZE ESCAPE";
        this.BackColor       = Color.FromArgb(5, 8, 22);
        this.ClientSize      = new Size(460, 500);
        this.FormBorderStyle = FormBorderStyle.FixedSingle;
        this.MaximizeBox     = false;
        this.StartPosition   = FormStartPosition.CenterScreen;
        this.DoubleBuffered  = true;
        titleLabel            = new Label();
        titleLabel.Text       = "MAZE ESCAPE";
        titleLabel.Font       = new Font("Consolas", 28, FontStyle.Bold);
        titleLabel.ForeColor  = Color.FromArgb(0, 210, 220);
        titleLabel.BackColor  = Color.Transparent;
        titleLabel.AutoSize   = true;
        titleLabel.Location   = new Point(85, 80);
        taglineLabel           = new Label();
        taglineLabel.Text      = "Slide   ·   Collect   ·   Escape";
        taglineLabel.Font      = new Font("Consolas", 10, FontStyle.Regular);
        taglineLabel.ForeColor = Color.FromArgb(80, 160, 180);
        taglineLabel.BackColor = Color.Transparent;
        taglineLabel.AutoSize  = true;
        taglineLabel.Location  = new Point(110, 148);
        difficultyLabel           = new Label();
        difficultyLabel.Text      = "SELECT DIFFICULTY";
        difficultyLabel.Font      = new Font("Consolas", 9, FontStyle.Regular);
        difficultyLabel.ForeColor = Color.FromArgb(60, 130, 150);
        difficultyLabel.BackColor = Color.Transparent;
        difficultyLabel.AutoSize  = true;
        difficultyLabel.Location  = new Point(148, 278);
        easyButton         = MakeDifficultyButton("EASY", 80);
        easyButton.Click  += (s, e) => PickDifficulty(5, easyButton);
        mediumButton        = MakeDifficultyButton("MEDIUM", 185);
        mediumButton.Click += (s, e) => PickDifficulty(3, mediumButton);
        hardButton         = MakeDifficultyButton("HARD", 290);
        hardButton.Click  += (s, e) => PickDifficulty(2, hardButton);
        playButton                           = new Button();
        playButton.Text                      = "PLAY GAME";
        playButton.Font                      = new Font("Consolas", 13, FontStyle.Bold);
        playButton.ForeColor                 = Color.Black;
        playButton.BackColor                 = Color.FromArgb(0, 210, 220);
        playButton.FlatStyle                 = FlatStyle.Flat;
        playButton.FlatAppearance.BorderSize = 0;
        playButton.Size                      = new Size(200, 44);
        playButton.Location                  = new Point(130, 380);
        playButton.Cursor                    = Cursors.Hand;
        playButton.Click                    += StartGame;
        exitButton                                = new Button();
        exitButton.Text                           = "EXIT";
        exitButton.Font                           = new Font("Consolas", 11, FontStyle.Bold);
        exitButton.ForeColor                      = Color.FromArgb(200, 50, 50);
        exitButton.BackColor                      = Color.Transparent;
        exitButton.FlatStyle                      = FlatStyle.Flat;
        exitButton.FlatAppearance.BorderColor     = Color.FromArgb(200, 50, 50);
        exitButton.FlatAppearance.BorderSize      = 1;
        exitButton.Size                           = new Size(200, 38);
        exitButton.Location                       = new Point(130, 435);
        exitButton.Cursor                         = Cursors.Hand;
        exitButton.Click                         += (s, e) => Application.Exit();
        this.Controls.Add(titleLabel);
        this.Controls.Add(taglineLabel);
        this.Controls.Add(difficultyLabel);
        this.Controls.Add(easyButton);
        this.Controls.Add(mediumButton);
        this.Controls.Add(hardButton);
        this.Controls.Add(playButton);
        this.Controls.Add(exitButton);
        PickDifficulty(3, mediumButton);
    }
    private Button MakeDifficultyButton(string label, int xPosition)
    {
        Button btn                       = new Button();
        btn.Text                         = label;
        btn.Font                         = new Font("Consolas", 10, FontStyle.Bold);
        btn.ForeColor                    = Color.FromArgb(0, 210, 220);
        btn.BackColor                    = Color.Transparent;
        btn.FlatStyle                    = FlatStyle.Flat;
        btn.FlatAppearance.BorderColor   = Color.FromArgb(0, 150, 160);
        btn.FlatAppearance.BorderSize    = 1;
        btn.Size                         = new Size(95, 34);
        btn.Location                     = new Point(xPosition, 310);
        btn.Cursor                       = Cursors.Hand;
        return btn;
    }
    private void PickDifficulty(int hunterSpeed, Button selected)
    {
        SelectedDifficulty = hunterSpeed;
        easyButton.BackColor                    = Color.Transparent;
        easyButton.ForeColor                    = Color.FromArgb(0, 210, 220);
        easyButton.FlatAppearance.BorderColor   = Color.FromArgb(0, 150, 160);

        mediumButton.BackColor                  = Color.Transparent;
        mediumButton.ForeColor                  = Color.FromArgb(0, 210, 220);
        mediumButton.FlatAppearance.BorderColor = Color.FromArgb(0, 150, 160);

        hardButton.BackColor                    = Color.Transparent;
        hardButton.ForeColor                    = Color.FromArgb(0, 210, 220);
        hardButton.FlatAppearance.BorderColor   = Color.FromArgb(0, 150, 160);
        selected.BackColor                    = Color.FromArgb(0, 55, 65);
        selected.ForeColor                    = Color.FromArgb(0, 240, 255);
        selected.FlatAppearance.BorderColor   = Color.FromArgb(0, 210, 220);
    }
    private void StartGame(object sender, EventArgs e)
    {
        this.Hide();
        GameForm game = new GameForm(SelectedDifficulty);
        game.ShowDialog();
        this.Close();
    }
}
