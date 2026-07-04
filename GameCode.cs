using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
interface IPickable
{
    void OnPickup(Player player);
}
interface IRenderable
{
    void Draw(Graphics canvas, int cellSize);
}
abstract class MazeObject
{
    public int Row { get; set; }
    public int Col { get; set; }

    protected MazeObject(int startRow, int startCol)
    {
        Row = startRow;
        Col = startCol;
    }
    protected PointF GetCenterInPixels(int cellSize)
    {
        float centerX = Col * cellSize + cellSize / 2f;
        float centerY = Row * cellSize + cellSize / 2f;
        return new PointF(centerX, centerY);
    }
    protected static void DrawGlowCircle(Graphics canvas, PointF center, float radius, Color color)
    {
        using SolidBrush brush = new SolidBrush(color);
        canvas.FillEllipse(brush, center.X - radius, center.Y - radius, radius * 2, radius * 2);
    }
}
class Player : MazeObject, IRenderable
{
    public int  Health  { get; private set; }
    public int  Score   { get; private set; }
    public bool IsAlive { get { return Health > 0; } }

    public Player(int startRow, int startCol) : base(startRow, startCol)
    {
        Health = 100;
        Score  = 0;
    }
    public bool Move(int dRow, int dCol, int[,] grid)
    {
        int newRow = Row + dRow;
        int newCol = Col + dCol;

        if (newRow < 0)                      return false;
        if (newRow >= grid.GetLength(0))     return false;
        if (newCol < 0)                      return false;
        if (newCol >= grid.GetLength(1))     return false;
        if (grid[newRow, newCol] == 0)       return false;

        Row = newRow;
        Col = newCol;
        return true;
    }

    public void GainHealth(int amount)
    {
        Health = Health + amount;
        if (Health > 100) Health = 100;
    }

    public void Die()
    {
        Health = 0;
    }

    public void AddScore(int points)
    {
        Score = Score + points;
    }

    public void Draw(Graphics canvas, int cellSize)
    {
        PointF center = GetCenterInPixels(cellSize);
        float  radius = cellSize * 0.35f;

        DrawGlowCircle(canvas, center, radius + 7, Color.FromArgb(35,  0, 255, 100));
        DrawGlowCircle(canvas, center, radius + 4, Color.FromArgb(70,  0, 255, 100));

        using SolidBrush bodyBrush = new SolidBrush(Color.FromArgb(0, 220, 80));
        canvas.FillEllipse(bodyBrush, center.X - radius, center.Y - radius, radius * 2, radius * 2);

        using Font       symbolFont = new Font("Consolas", cellSize * 0.38f, FontStyle.Bold);
        using SolidBrush textBrush  = new SolidBrush(Color.Black);
        SizeF textSize = canvas.MeasureString("@", symbolFont);
        canvas.DrawString("@", symbolFont, textBrush,
            center.X - textSize.Width  / 2,
            center.Y - textSize.Height / 2);
    }
}
class Hunter : MazeObject, IRenderable
{
    public int Speed { get; private set; }
    private int tickCounter = 0;

    public Hunter(int startRow, int startCol, int speed) : base(startRow, startCol)
    {
        Speed = speed;
    }
    public bool Update(Player player, int[,] grid)
    {
        tickCounter = tickCounter + 1;
        if (tickCounter < Speed) return false;
        tickCounter = 0;

        List<int[]> path = FindPathToPlayer(grid, player);

        if (path != null && path.Count > 1)
        {
            Row = path[1][0];
            Col = path[1][1];
        }

        return (Row == player.Row && Col == player.Col);
    }
    private List<int[]> FindPathToPlayer(int[,] grid, Player player)
    {
        int totalRows = grid.GetLength(0);
        int totalCols = grid.GetLength(1);

        Queue<int[]>             queue    = new Queue<int[]>();
        Dictionary<string, int[]> cameFrom = new Dictionary<string, int[]>();

        queue.Enqueue(new int[] { Row, Col });
        cameFrom[Row + "," + Col] = null;

        int[] moveRow = new int[] { -1, 1,  0, 0 };
        int[] moveCol = new int[] {  0, 0, -1, 1 };

        while (queue.Count > 0)
        {
            int[] current    = queue.Dequeue();
            int   currentRow = current[0];
            int   currentCol = current[1];

            if (currentRow == player.Row && currentCol == player.Col)
            {
                List<int[]> path = new List<int[]>();
                int[] step = current;

                while (step != null)
                {
                    path.Add(step);
                    cameFrom.TryGetValue(step[0] + "," + step[1], out step);
                }

                path.Reverse();
                return path;
            }

            for (int i = 0; i < 4; i++)
            {
                int nRow = currentRow + moveRow[i];
                int nCol = currentCol + moveCol[i];

                if (nRow < 0 || nRow >= totalRows) continue;
                if (nCol < 0 || nCol >= totalCols) continue;
                if (grid[nRow, nCol] == 0)         continue;

                string key = nRow + "," + nCol;
                if (cameFrom.ContainsKey(key))     continue;

                cameFrom[key] = current;
                queue.Enqueue(new int[] { nRow, nCol });
            }
        }

        return null;
    }

    public void Draw(Graphics canvas, int cellSize)
    {
        PointF center = GetCenterInPixels(cellSize);
        float  radius = cellSize * 0.35f;

        DrawGlowCircle(canvas, center, radius + 7, Color.FromArgb(35, 255, 30, 30));
        DrawGlowCircle(canvas, center, radius + 4, Color.FromArgb(75, 255, 30, 30));

        using SolidBrush bodyBrush = new SolidBrush(Color.FromArgb(210, 40, 40));
        canvas.FillEllipse(bodyBrush, center.X - radius, center.Y - radius, radius * 2, radius * 2);

        float  offset   = radius * 0.55f;
        using Pen crossPen = new Pen(Color.FromArgb(255, 90, 90), 2.5f);
        canvas.DrawLine(crossPen, center.X - offset, center.Y - offset, center.X + offset, center.Y + offset);
        canvas.DrawLine(crossPen, center.X + offset, center.Y - offset, center.X - offset, center.Y + offset);
    }
}
class Energizer : MazeObject, IPickable, IRenderable
{
    public int  HealAmount { get; private set; }
    public bool IsActive   { get; private set; }

    public Energizer(int startRow, int startCol, int healAmount) : base(startRow, startCol)
    {
        HealAmount = healAmount;
        IsActive   = true;
    }

    public void OnPickup(Player player)
    {
        if (IsActive == false) return;
        player.GainHealth(HealAmount);
        player.AddScore(10);
        IsActive = false;
    }

    public void Draw(Graphics canvas, int cellSize)
    {
        if (IsActive == false) return;

        PointF center = GetCenterInPixels(cellSize);
        float  radius = cellSize * 0.28f;

        DrawGlowCircle(canvas, center, radius + 6, Color.FromArgb(35,  0, 220, 220));
        DrawGlowCircle(canvas, center, radius + 3, Color.FromArgb(80,  0, 220, 220));

        PointF[] diamond = new PointF[]
        {
            new PointF(center.X,          center.Y - radius),
            new PointF(center.X + radius, center.Y         ),
            new PointF(center.X,          center.Y + radius),
            new PointF(center.X - radius, center.Y         ),
        };

        using SolidBrush fillBrush   = new SolidBrush(Color.FromArgb(0, 200, 200));
        using Pen        borderPen   = new Pen(Color.FromArgb(120, 255, 255), 1f);
        canvas.FillPolygon(fillBrush, diamond);
        canvas.DrawPolygon(borderPen, diamond);
    }
}
class MazeGenerator
{
    private int      totalRows;
    private int      totalCols;
    private int[,]   grid;
    private bool[,]  visited;
    private Random   randomizer;

    public MazeGenerator(int rows, int cols)
    {
        totalRows  = rows;
        totalCols  = cols;
        randomizer = new Random();
        grid       = new int[totalRows, totalCols];
        visited    = new bool[totalRows, totalCols];
    }

    public int[,] Generate()
    {
        for (int r = 0; r < totalRows; r++)
            for (int c = 0; c < totalCols; c++)
            {
                grid[r, c]    = 0;
                visited[r, c] = false;
            }

        CarvePath(0, 0);
        grid[totalRows - 1, totalCols - 1] = 1;
        return grid;
    }

    private void CarvePath(int row, int col)
    {
        visited[row, col] = true;
        grid[row, col]    = 1;

        List<int[]> directions = new List<int[]>
        {
            new int[] {  0,  2 },
            new int[] {  0, -2 },
            new int[] {  2,  0 },
            new int[] { -2,  0 }
        };

        ShuffleList(directions);

        foreach (int[] direction in directions)
        {
            int newRow = row + direction[0];
            int newCol = col + direction[1];

            if (newRow < 0 || newRow >= totalRows) continue;
            if (newCol < 0 || newCol >= totalCols) continue;
            if (visited[newRow, newCol])           continue;

            grid[row + direction[0] / 2, col + direction[1] / 2] = 1;
            CarvePath(newRow, newCol);
        }
    }

    private void ShuffleList(List<int[]> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int    randomIndex = randomizer.Next(i + 1);
            int[]  temp        = list[i];
            list[i]            = list[randomIndex];
            list[randomIndex]  = temp;
        }
    }
}
class GameForm : Form
{
    private const int MAZE_ROWS  = 15;
    private const int MAZE_COLS  = 21;
    private const int CELL_SIZE  = 36;
    private const int HUD_HEIGHT = 60;

    private int[,]        mazeGrid;
    private Player        player;
    private List<Hunter>  hunters;
    private List<Energizer> energizers;
    private int exitRow;
    private int exitCol;

    private int    currentLevel = 1;
    private int    baseDifficulty;        
    private string statusMessage = "";
    private bool   isGameOver    = false;
    private bool   levelWon      = false;

    private System.Windows.Forms.Timer gameTimer;
    private Bitmap   drawingBuffer;
    private Graphics bufferCanvas;
    public GameForm(int difficulty = 3)
    {
        baseDifficulty = difficulty;

        this.Text            = "MAZE ESCAPE";
        this.BackColor       = Color.Black;
        this.ClientSize      = new Size(MAZE_COLS * CELL_SIZE, MAZE_ROWS * CELL_SIZE + HUD_HEIGHT);
        this.FormBorderStyle = FormBorderStyle.FixedSingle;
        this.MaximizeBox     = false;
        this.DoubleBuffered  = true;
        this.KeyPreview      = true;

        drawingBuffer = new Bitmap(this.ClientSize.Width, this.ClientSize.Height);
        bufferCanvas  = Graphics.FromImage(drawingBuffer);

        gameTimer          = new System.Windows.Forms.Timer();
        gameTimer.Interval = 200;
        gameTimer.Tick    += OnTimerTick;

        this.KeyDown += OnKeyDown;
        this.Paint   += OnPaint;

        mazeGrid   = new int[MAZE_ROWS, MAZE_COLS];
        player     = new Player(0, 0);
        hunters    = new List<Hunter>();
        energizers = new List<Energizer>();
        exitRow    = MAZE_ROWS - 1;
        exitCol    = MAZE_COLS - 1;

        SetupLevel();
    }

    private void SetupLevel()
    {
        MazeGenerator generator = new MazeGenerator(MAZE_ROWS, MAZE_COLS);
        mazeGrid = generator.Generate();

        player  = new Player(0, 0);
        exitRow = MAZE_ROWS - 1;
        exitCol = MAZE_COLS - 1;

        isGameOver    = false;
        levelWon      = false;
        statusMessage = "LEVEL " + currentLevel + "  —  REACH THE EXIT";

        Random rng = new Random();
        hunters.Clear();

        int numberOfHunters = currentLevel + 1;
        if (numberOfHunters > 5) numberOfHunters = 5;

        for (int i = 0; i < numberOfHunters; i++)
        {
            int hunterRow, hunterCol;
            int attempts = 0;

            do
            {
                hunterRow = rng.Next(MAZE_ROWS);
                hunterCol = rng.Next(MAZE_COLS);
                attempts++;
                if (attempts > 300) break;
            }
            while (mazeGrid[hunterRow, hunterCol] == 0
                   || Math.Abs(hunterRow) + Math.Abs(hunterCol) < 8);
            int hunterSpeed = baseDifficulty - currentLevel + 2;
            if (hunterSpeed < 1) hunterSpeed = 1;

            hunters.Add(new Hunter(hunterRow, hunterCol, hunterSpeed));
        }
        energizers.Clear();

        for (int i = 0; i < 5; i++)
        {
            int energizerRow, energizerCol;
            int attempts = 0;

            do
            {
                energizerRow = rng.Next(MAZE_ROWS);
                energizerCol = rng.Next(MAZE_COLS);
                attempts++;
                if (attempts > 300) break;
            }
            while (mazeGrid[energizerRow, energizerCol] == 0
                   || (energizerRow == 0 && energizerCol == 0));

            energizers.Add(new Energizer(energizerRow, energizerCol, 25));
        }

        gameTimer.Start();
        this.Invalidate();
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (isGameOver)
        {
            if (e.KeyCode == Keys.R) { currentLevel = 1; SetupLevel(); }
            if (e.KeyCode == Keys.Q) this.Close();
            return;
        }

        if (levelWon)
        {
            if (e.KeyCode == Keys.R) { currentLevel++; SetupLevel(); }
            if (e.KeyCode == Keys.Q) this.Close();
            return;
        }

        int moveRow = 0;
        int moveCol = 0;

        if (e.KeyCode == Keys.W || e.KeyCode == Keys.Up)    moveRow = -1;
        if (e.KeyCode == Keys.S || e.KeyCode == Keys.Down)  moveRow =  1;
        if (e.KeyCode == Keys.A || e.KeyCode == Keys.Left)  moveCol = -1;
        if (e.KeyCode == Keys.D || e.KeyCode == Keys.Right) moveCol =  1;

        if (moveRow == 0 && moveCol == 0) return;

        player.Move(moveRow, moveCol, mazeGrid);
        foreach (Hunter hunter in hunters)
        {
            if (hunter.Row == player.Row && hunter.Col == player.Col)
            {
                player.Die();
                isGameOver    = true;
                statusMessage = "CAUGHT BY HUNTER!  —  Press R to restart";
                gameTimer.Stop();
                this.Invalidate();
                return;
            }
        }
        foreach (Energizer energizer in energizers)
        {
            if (energizer.IsActive
                && energizer.Row == player.Row
                && energizer.Col == player.Col)
            {
                energizer.OnPickup(player);
                statusMessage = "+25 HP  ENERGIZER COLLECTED";
            }
        }
        if (player.Row == exitRow && player.Col == exitCol)
        {
            player.AddScore(100 * currentLevel);
            levelWon      = true;
            statusMessage = "SECTOR CLEARED!  +" + (100 * currentLevel) + " PTS  —  Press R for next level";
            gameTimer.Stop();
            this.Invalidate();
            return;
        }

        this.Invalidate();
    }

    private void OnTimerTick(object sender, EventArgs e)
    {
        if (isGameOver || levelWon) return;

        foreach (Hunter hunter in hunters)
        {
            bool caught = hunter.Update(player, mazeGrid);

            if (caught)
            {
                player.Die();
                isGameOver    = true;
                statusMessage = "CAUGHT BY HUNTER!  —  Press R to restart";
                gameTimer.Stop();
                break;
            }
        }

        this.Invalidate();
    }

    private void OnPaint(object sender, PaintEventArgs e)
    {
        DrawEverything(bufferCanvas);
        e.Graphics.DrawImage(drawingBuffer, 0, 0);
    }

    private void DrawEverything(Graphics canvas)
    {
        canvas.SmoothingMode = SmoothingMode.AntiAlias;
        canvas.Clear(Color.Black);

        DrawMazeWalls(canvas);
        DrawExitTile(canvas);

        foreach (Energizer energizer in energizers) energizer.Draw(canvas, CELL_SIZE);
        foreach (Hunter    hunter    in hunters)    hunter.Draw(canvas, CELL_SIZE);

        player.Draw(canvas, CELL_SIZE);
        DrawHUD(canvas);

        if (isGameOver)
            DrawOverlayMessage(canvas, "GAME  OVER",
                "Press R to restart  |  Q to quit",
                Color.FromArgb(255, 60, 60));

        if (levelWon)
            DrawOverlayMessage(canvas, "LEVEL " + currentLevel + " CLEARED",
                "Press R for next level  |  Q to quit",
                Color.FromArgb(60, 255, 120));
    }

    private void DrawMazeWalls(Graphics canvas)
    {
        for (int row = 0; row < MAZE_ROWS; row++)
        {
            for (int col = 0; col < MAZE_COLS; col++)
            {
                Rectangle cellRect = new Rectangle(col * CELL_SIZE, row * CELL_SIZE, CELL_SIZE, CELL_SIZE);

                if (mazeGrid[row, col] == 0)
                {
                    using SolidBrush wallFill   = new SolidBrush(Color.FromArgb(8, 10, 28));
                    using Pen        wallBorder  = new Pen(Color.FromArgb(20, 35, 80), 0.5f);
                    canvas.FillRectangle(wallFill,   cellRect);
                    canvas.DrawRectangle(wallBorder, cellRect);
                }
                else
                {
                    using SolidBrush pathFill = new SolidBrush(Color.FromArgb(12, 12, 22));
                    canvas.FillRectangle(pathFill, cellRect);
                }
            }
        }
    }

    private void DrawExitTile(Graphics canvas)
    {
        float x    = exitCol * CELL_SIZE + CELL_SIZE * 0.1f;
        float y    = exitRow * CELL_SIZE + CELL_SIZE * 0.1f;
        float size = CELL_SIZE * 0.8f;

        using SolidBrush outerGlow = new SolidBrush(Color.FromArgb(30,  255, 180, 0));
        using SolidBrush innerGlow = new SolidBrush(Color.FromArgb(70,  255, 200, 0));
        using SolidBrush core      = new SolidBrush(Color.FromArgb(255, 180, 0));

        canvas.FillRectangle(outerGlow, x - 4, y - 4, size + 8, size + 8);
        canvas.FillRectangle(innerGlow, x - 2, y - 2, size + 4, size + 4);
        canvas.FillRectangle(core,      x,     y,     size,     size);

        using Font       font  = new Font("Consolas", CELL_SIZE * 0.26f, FontStyle.Bold);
        using SolidBrush label = new SolidBrush(Color.Black);
        SizeF textSize = canvas.MeasureString("EXIT", font);
        canvas.DrawString("EXIT", font, label,
            exitCol * CELL_SIZE + CELL_SIZE / 2f - textSize.Width  / 2,
            exitRow * CELL_SIZE + CELL_SIZE / 2f - textSize.Height / 2);
    }

    private void DrawHUD(Graphics canvas)
    {
        int hudY = MAZE_ROWS * CELL_SIZE;

        using SolidBrush hudBackground = new SolidBrush(Color.FromArgb(10, 10, 22));
        canvas.FillRectangle(hudBackground, 0, hudY, this.ClientSize.Width, HUD_HEIGHT);

        using Pen topLine = new Pen(Color.FromArgb(0, 160, 255), 1.5f);
        canvas.DrawLine(topLine, 0, hudY, this.ClientSize.Width, hudY);

        int barX      = 16;
        int barY      = hudY + 12;
        int barWidth  = 150;
        int barHeight = 13;

        using SolidBrush barBg   = new SolidBrush(Color.FromArgb(30, 30, 50));
        canvas.FillRectangle(barBg, barX, barY, barWidth, barHeight);

        int filledWidth = (int)(barWidth * (player.Health / 100f));
        using SolidBrush barFill = new SolidBrush(Color.FromArgb(0, 200, 80));
        canvas.FillRectangle(barFill, barX, barY, filledWidth, barHeight);

        using Pen barBorder = new Pen(Color.FromArgb(0, 160, 90), 1f);
        canvas.DrawRectangle(barBorder, barX, barY, barWidth, barHeight);

        using Font       hudFont    = new Font("Consolas", 10, FontStyle.Bold);
        using SolidBrush greenText  = new SolidBrush(Color.FromArgb(0,   200, 100));
        using SolidBrush yellowText = new SolidBrush(Color.FromArgb(220, 210, 0));
        using SolidBrush blueText   = new SolidBrush(Color.FromArgb(0,   170, 255));

        canvas.DrawString("HP " + player.Health + "%", hudFont, greenText,  barX,                    barY + barHeight + 2);
        canvas.DrawString("SCORE " + player.Score,     hudFont, yellowText, barX + barWidth + 20,    hudY + 12);
        canvas.DrawString("LEVEL " + currentLevel,     hudFont, blueText,   barX + barWidth + 150,   hudY + 12);

        using Font       statusFont  = new Font("Consolas", 9, FontStyle.Regular);
        using SolidBrush statusBrush = new SolidBrush(Color.FromArgb(150, 150, 210));
        canvas.DrawString(statusMessage, statusFont, statusBrush, barX, hudY + barHeight + 20);
    }

    private void DrawOverlayMessage(Graphics canvas, string title, string subtitle, Color titleColor)
    {
        using SolidBrush dimmer = new SolidBrush(Color.FromArgb(200, 0, 0, 0));
        canvas.FillRectangle(dimmer, 0, 0, this.ClientSize.Width, this.ClientSize.Height);

        using Font       titleFont  = new Font("Consolas", 30, FontStyle.Bold);
        using SolidBrush titleBrush = new SolidBrush(titleColor);
        SizeF titleSize = canvas.MeasureString(title, titleFont);
        float titleX    = this.ClientSize.Width  / 2f - titleSize.Width  / 2;
        float titleY    = this.ClientSize.Height / 2f - titleSize.Height;
        canvas.DrawString(title, titleFont, titleBrush, titleX, titleY);

        using Font       subFont  = new Font("Consolas", 12, FontStyle.Regular);
        using SolidBrush subBrush = new SolidBrush(Color.FromArgb(180, 180, 180));
        SizeF subSize = canvas.MeasureString(subtitle, subFont);
        canvas.DrawString(subtitle, subFont, subBrush,
            this.ClientSize.Width / 2f - subSize.Width / 2,
            titleY + titleSize.Height + 10);
    }
}
static class Program
{
    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new WelcomeForm());
    }
}
