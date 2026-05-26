using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace SudokuGame
{
    public static class SaveLoad
    {
        public static void SaveGame(string filePath, SaveData data)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            using (FileStream stream = new FileStream(filePath, FileMode.Create))
            {
                formatter.Serialize(stream, data);
            }
        }

        public static SaveData LoadGame(string filePath)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            using (FileStream stream = new FileStream(filePath, FileMode.Open))
            {
                return (SaveData)formatter.Deserialize(stream);
            }
        }
    }
}