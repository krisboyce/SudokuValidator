using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace SudokuValidator
{
    class Program
    {
        static void Main(string[] args)
        {
            // :p
            bool showErrors = false;
            try
            {
                if (args.Length != 1)
                    throw new ArgumentException("Invalid arguments: please provide a filename");

                var fileName = args[0];
                if (!File.Exists(fileName))
                    throw new ArgumentException("Invalid arguments: file not found");

                string sudokuText = File.ReadAllText(fileName);

                int[,] parsedSudoku = ParseSudokuSolution(sudokuText);
                if (IsValidSolution(parsedSudoku))
                    Console.WriteLine("Yes, valid");
                else
                    Console.WriteLine("No, invalid");
            }
            catch(FormatException)
            {
                if (showErrors)
                    throw;
                else
                    Console.WriteLine("No, invalid");
            }
        }

        // Trying to be lenient on validation here seeing as there is no real file format spec. 
        // just game rules
        static int[,] ParseSudokuSolution(string solution)
        {
            var lines = solution
                .Split('\n')
                .Select(x => Regex.Replace(x, " |\r|0", ""))
                .Where(x => x.Length > 0)
                .ToList();

            if (lines.Count != 9)
                throw new FormatException("Invalid solution row count. Must be 9 rows in a solution");

            var parsedSolution = new int[9, 9];
            for (int lineIndex = 0; lineIndex < lines.Count; lineIndex++)
            {
                var line = lines[lineIndex];
                if (line.Length != 9)
                    throw new FormatException($"Invalid column count in row {lineIndex}");

                for(int columnIndex = 0; columnIndex < line.Length; columnIndex++)
                {
                    var columnCharacter = line[columnIndex].ToString();
                    if (!int.TryParse(columnCharacter, out int digit))
                        throw new FormatException($"Invalid character {columnCharacter} at row {lineIndex}, column {columnIndex}");

                    parsedSolution[lineIndex, columnIndex] = digit;
                }
            }

            return parsedSolution;
        }

        static bool IsValidSolution(int[,] solution)
        {
            // I used a bitfields because they are fun, 
            // bool array could do the same thing i.e. "rowPermutation[digit] = !rowPermutation[digit]; etc,."
            // Best case time complexity is O(11) -- first two are duplicates   
            // and worst case O(n+9) -- solved

            int[] rowPermutation = new int[9];
            int[] columnPermutation = new int[9];
            int[] squarePermutation = new int[9];
            for(int i = 0; i<9; i++)
            {
                rowPermutation[i] = columnPermutation[i] = squarePermutation[i] = 0b1111111110;
            }

            for (int rowIndex = 0; rowIndex < 9; rowIndex++)
            {
                for (int columnIndex = 0; columnIndex < 9; columnIndex++)
                {
                    int x = columnIndex / 3;
                    int y = rowIndex / 3;
                    int squareIndex = x + y * 3;
                    int bit = 1 << solution[rowIndex, columnIndex];
                    
                    // XOR to flip the bit
                    // If the bit gets set to 1 then duplicate is there and bail
                    rowPermutation[rowIndex] ^= bit;
                    if ((rowPermutation[rowIndex] & bit) != 0)
                        return false;
                    
                    columnPermutation[columnIndex] ^= bit;
                    if ((columnPermutation[columnIndex] & bit) != 0)
                        return false;

                    squarePermutation[squareIndex] ^= bit;
                    if ((squarePermutation[squareIndex] & bit) != 0)
                        return false;
                }
            }

            return true;
        }
    }
}
