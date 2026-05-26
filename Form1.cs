using System;
using System.Drawing;
using System.Windows.Forms;

namespace SudokuGame
{
    public partial class MainForm : Form
    {
        private GameLogic game;
        private Button[,] cells;
        private Label statusLabel;
        private Label difficultyLabel;
        private int selectedRow = -1, selectedCol = -1;
        private const int cellSize = 50;

        public MainForm()
        {
            InitializeComponent();
            SetupUI();
            game = new GameLogic();
            UpdateBoardUI();
            UpdateStatusMessage("Добро пожаловать! Выберите сложность и начинайте игру");
        }

        private void SetupUI()
        {
            this.Text = "Судоку";
            this.Size = new Size(550, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.KeyPreview = true;
            this.KeyDown += MainForm_KeyDown;

            Panel gridPanel = new Panel
            {
                Location = new Point(25, 60),
                Size = new Size(9 * cellSize, 9 * cellSize),
                BackColor = Color.Black
            };

            cells = new Button[9, 9];
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    Button btn = new Button
                    {
                        Size = new Size(cellSize, cellSize),
                        Location = new Point(j * cellSize, i * cellSize),
                        Font = new Font("Arial", 16, FontStyle.Bold),
                        FlatStyle = FlatStyle.Flat,
                        FlatAppearance = { BorderSize = 1, BorderColor = Color.DarkGray },
                        BackColor = GetBlockColor(i, j), // ← разный фон для разных квадратов 3x3
                        Tag = new Point(i, j)
                    };

                    btn.Click += Cell_Click;
                    cells[i, j] = btn;
                    gridPanel.Controls.Add(btn);
                }
            }

            this.Controls.Add(gridPanel);

            // Статус
            statusLabel = new Label
            {
                Location = new Point(25, 9 * cellSize + 70),
                Size = new Size(500, 40),
                Font = new Font("Arial", 10),
                ForeColor = Color.DarkBlue
            };
            this.Controls.Add(statusLabel);

            // Кнопки
            Button newGameBtn = new Button { Text = "Новая игра", Location = new Point(25, 9 * cellSize + 120), Size = new Size(100, 35), BackColor = Color.LightGreen };
            newGameBtn.Click += (s, e) => { game.NewGame(); UpdateBoardUI(); ClearSelection(); UpdateStatusMessage("Новая игра! Удачи!"); };

            Button saveBtn = new Button { Text = "Сохранить", Location = new Point(140, 9 * cellSize + 120), Size = new Size(100, 35), BackColor = Color.LightYellow };
            saveBtn.Click += SaveGame;

            Button loadBtn = new Button { Text = "Загрузить", Location = new Point(255, 9 * cellSize + 120), Size = new Size(100, 35), BackColor = Color.LightYellow };
            loadBtn.Click += LoadGame;

            // Выбор сложности
            Label diffLabel = new Label { Text = "Сложность:", Location = new Point(25, 9 * cellSize + 170), Size = new Size(70, 30), Font = new Font("Arial", 10, FontStyle.Bold) };
            this.Controls.Add(diffLabel);

            ComboBox difficultyCombo = new ComboBox
            {
                Location = new Point(100, 9 * cellSize + 170),
                Size = new Size(100, 30),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            difficultyCombo.Items.AddRange(new string[] { "Easy", "Medium", "Hard" });
            difficultyCombo.SelectedIndex = 1;
            difficultyCombo.SelectedIndexChanged += (s, e) =>
            {
                Difficulty diff = (Difficulty)difficultyCombo.SelectedIndex;
                game.SetDifficulty(diff);
                UpdateBoardUI();
                ClearSelection();
                UpdateStatusMessage($"Сложность изменена на {diff}. Начинайте новую игру!");
            };
            this.Controls.Add(difficultyCombo);

            difficultyLabel = new Label
            {
                Location = new Point(220, 9 * cellSize + 170),
                Size = new Size(200, 30),
                Font = new Font("Arial", 9),
                ForeColor = Color.Gray
            };
            this.Controls.Add(difficultyLabel);

            this.Controls.Add(newGameBtn);
            this.Controls.Add(saveBtn);
            this.Controls.Add(loadBtn);
        }

        // Определяем цвет фона для клетки в зависимости от того, в каком она квадрате 3x3
        private Color GetBlockColor(int row, int col)
        {
            int blockRow = row / 3;
            int blockCol = col / 3;

            // Шахматная раскраска квадратов 3x3
            if ((blockRow + blockCol) % 2 == 0)
                return Color.FromArgb(240, 248, 255); // AliceBlue (очень светлый голубой)
            else
                return Color.FromArgb(255, 250, 240); // FloralWhite (кремовый)
        }

        private void Cell_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            Point pos = (Point)btn.Tag;
            selectedRow = pos.X;
            selectedCol = pos.Y;

            HighlightCellAndRelated();
            UpdateStatusMessage($"Выбрана клетка [{selectedRow + 1}, {selectedCol + 1}]");
        }

        private void HighlightCellAndRelated()
        {
            int currentNumber = game.Board[selectedRow, selectedCol];

            // Сброс подсветки (возвращаем цвета квадратов)
            for (int i = 0; i < 9; i++)
                for (int j = 0; j < 9; j++)
                    cells[i, j].BackColor = GetBlockColor(i, j);

            // Подсветка строки
            for (int i = 0; i < 9; i++)
                if (i != selectedCol)
                    cells[selectedRow, i].BackColor = Color.LightGray;

            // Подсветка столбца
            for (int i = 0; i < 9; i++)
                if (i != selectedRow)
                    cells[i, selectedCol].BackColor = Color.LightGray;

            // Подсветка квадрата 3x3
            int startRow = (selectedRow / 3) * 3;
            int startCol = (selectedCol / 3) * 3;
            for (int i = startRow; i < startRow + 3; i++)
                for (int j = startCol; j < startCol + 3; j++)
                    if (i != selectedRow || j != selectedCol)
                        cells[i, j].BackColor = Color.LightGray;

            // Подсветка всех таких же цифр
            if (currentNumber != 0)
            {
                for (int i = 0; i < 9; i++)
                    for (int j = 0; j < 9; j++)
                        if (game.Board[i, j] == currentNumber)
                            cells[i, j].BackColor = Color.LightGoldenrodYellow;
            }

            // Выделяем выбранную клетку ярко
            cells[selectedRow, selectedCol].BackColor = Color.LightBlue;
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (selectedRow == -1 || selectedCol == -1) return;

            int num = 0;
            if (e.KeyCode >= Keys.D1 && e.KeyCode <= Keys.D9)
                num = e.KeyCode - Keys.D0;
            else if (e.KeyCode == Keys.NumPad0 || e.KeyCode == Keys.Delete || e.KeyCode == Keys.Back)
                num = 0;
            else if (e.KeyCode >= Keys.NumPad1 && e.KeyCode <= Keys.NumPad9)
                num = e.KeyCode - Keys.NumPad0;
            else
                return;

            if (game.SetNumber(selectedRow, selectedCol, num))
            {
                UpdateBoardUI();
                HighlightCellAndRelated();

                if (game.CheckWin())
                {
                    MessageBox.Show("Поздравляем! Вы решили судоку!", "Победа!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    UpdateStatusMessage("ПОБЕДА! Отличная работа! Нажмите 'Новая игра' чтобы продолжить");
                }
                else
                {
                    UpdateStatusMessage($"Ход принят. Осталось заполнить: {GetEmptyCellsCount()} клеток");
                }
            }
            else
            {
                UpdateStatusMessage("Недопустимый ход или клетка изначальная");
            }
        }

        private void UpdateBoardUI()
        {
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    int val = game.Board[i, j];
                    cells[i, j].Text = val == 0 ? "" : val.ToString();

                    if (game.InitialBoard[i, j] != 0)
                        cells[i, j].ForeColor = Color.Blue;
                    else
                        cells[i, j].ForeColor = Color.Black;

                    if (val != 0 && !game.IsValidMove(i, j, val, false))
                        cells[i, j].ForeColor = Color.Red;
                }
            }

            string diffText = "";
            switch (game.CurrentDifficulty)
            {
                case Difficulty.Easy: diffText = "Easy (много подсказок)"; break;
                case Difficulty.Medium: diffText = "Medium (средне)"; break;
                case Difficulty.Hard: diffText = "Hard (мало подсказок)"; break;
            }
            difficultyLabel.Text = $"Текущая: {diffText}";
        }

        private int GetEmptyCellsCount()
        {
            int count = 0;
            for (int i = 0; i < 9; i++)
                for (int j = 0; j < 9; j++)
                    if (game.Board[i, j] == 0)
                        count++;
            return count;
        }

        private void UpdateStatusMessage(string message)
        {
            statusLabel.Text = message;
        }

        private void ClearSelection()
        {
            selectedRow = -1;
            selectedCol = -1;
            for (int i = 0; i < 9; i++)
                for (int j = 0; j < 9; j++)
                    cells[i, j].BackColor = GetBlockColor(i, j);
        }

        private void SaveGame(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "Sudoku Save|*.sav",
                Title = "Сохранить игру"
            };
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                SaveLoad.SaveGame(sfd.FileName, game.GetSaveData());
                UpdateStatusMessage("Игра сохранена");
            }
        }

        private void LoadGame(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog
            {
                Filter = "Sudoku Save|*.sav",
                Title = "Загрузить игру"
            };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                var data = SaveLoad.LoadGame(ofd.FileName);
                game.LoadSaveData(data);
                UpdateBoardUI();
                ClearSelection();
                UpdateStatusMessage("Игра загружена. Удачи!");
            }
        }
    }
}