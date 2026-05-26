using System;

namespace SudokuGame
{
    public enum Difficulty
    {
        Easy,      // 45-50 подсказок
        Medium,    // 35-40 подсказок
        Hard       // 25-30 подсказок
    }

    public class GameLogic
    {
        public int[,] Board { get; private set; }
        public int[,] InitialBoard { get; private set; }
        private Random random = new Random();
        public Difficulty CurrentDifficulty { get; private set; } = Difficulty.Medium;

        public GameLogic()
        {
            Board = new int[9, 9];
            InitialBoard = new int[9, 9];
            NewGame();
        }

        public void SetDifficulty(Difficulty difficulty)
        {
            CurrentDifficulty = difficulty;
            NewGame();
        }

        public void NewGame()
        {
            // Очищаем доску
            for (int i = 0; i < 9; i++)
                for (int j = 0; j < 9; j++)
                    Board[i, j] = 0;

            // Заполняем диагональные блоки 3x3
            for (int block = 0; block < 9; block += 3)
                FillDiagonalBlock(block, block);

            // Решаем судоку
            SolveSudoku();

            // Удаляем клетки в зависимости от сложности
            int cellsToRemove = GetCellsToRemove();
            RemoveCells(cellsToRemove);

            // Сохраняем начальное состояние
            for (int i = 0; i < 9; i++)
                for (int j = 0; j < 9; j++)
                    InitialBoard[i, j] = Board[i, j];
        }

        private int GetCellsToRemove()
        {
            switch (CurrentDifficulty)
            {
                case Difficulty.Easy: return 35 + random.Next(5);   // 35-40 удалить = 41-46 подсказок
                case Difficulty.Medium: return 45 + random.Next(5);   // 45-50 удалить = 31-36 подсказок
                case Difficulty.Hard: return 55 + random.Next(5);   // 55-60 удалить = 21-26 подсказок
                default: return 45;
            }
        }

        private void FillDiagonalBlock(int startRow, int startCol)
        {
            int[] nums = { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            Shuffle(nums);

            int index = 0;
            for (int i = startRow; i < startRow + 3; i++)
                for (int j = startCol; j < startCol + 3; j++)
                    Board[i, j] = nums[index++];
        }

        private void Shuffle(int[] array)
        {
            for (int i = array.Length - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                int temp = array[i];
                array[i] = array[j];
                array[j] = temp;
            }
        }

        private bool SolveSudoku()
        {
            for (int row = 0; row < 9; row++)
            {
                for (int col = 0; col < 9; col++)
                {
                    if (Board[row, col] == 0)
                    {
                        int[] nums = { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
                        Shuffle(nums);

                        foreach (int num in nums)
                        {
                            if (IsValidMove(row, col, num, true))
                            {
                                Board[row, col] = num;
                                if (SolveSudoku())
                                    return true;
                                Board[row, col] = 0;
                            }
                        }
                        return false;
                    }
                }
            }
            return true;
        }

        private void RemoveCells(int countToRemove)
        {
            int removed = 0;
            while (removed < countToRemove)
            {
                int row = random.Next(9);
                int col = random.Next(9);
                if (Board[row, col] != 0)
                {
                    Board[row, col] = 0;
                    removed++;
                }
            }
        }

        public bool IsValidMove(int row, int col, int num, bool ignoreCurrent = false)
        {
            if (num < 1 || num > 9) return false;

            for (int c = 0; c < 9; c++)
                if (Board[row, c] == num && c != col)
                    return false;

            for (int r = 0; r < 9; r++)
                if (Board[r, col] == num && r != row)
                    return false;

            int startRow = (row / 3) * 3;
            int startCol = (col / 3) * 3;
            for (int r = startRow; r < startRow + 3; r++)
                for (int c = startCol; c < startCol + 3; c++)
                    if (Board[r, c] == num && (r != row || c != col))
                        return false;

            return true;
        }

        public bool SetNumber(int row, int col, int num)
        {
            if (InitialBoard[row, col] != 0) return false;

            if (num == 0)
            {
                Board[row, col] = 0;
                return true;
            }

            if (IsValidMove(row, col, num, false))
            {
                Board[row, col] = num;
                return true;
            }
            return false;
        }

        public bool CheckWin()
        {
            for (int i = 0; i < 9; i++)
                for (int j = 0; j < 9; j++)
                    if (Board[i, j] == 0)
                        return false;
            return true;
        }

        public SaveData GetSaveData()
        {
            return new SaveData
            {
                Board = (int[,])Board.Clone(),
                InitialBoard = (int[,])InitialBoard.Clone(),
                Difficulty = CurrentDifficulty
            };
        }

        public void LoadSaveData(SaveData data)
        {
            Board = (int[,])data.Board.Clone();
            InitialBoard = (int[,])data.InitialBoard.Clone();
            CurrentDifficulty = data.Difficulty;
        }
    }

    [Serializable]
    public class SaveData
    {
        public int[,] Board { get; set; }
        public int[,] InitialBoard { get; set; }
        public Difficulty Difficulty { get; set; }
    }
}