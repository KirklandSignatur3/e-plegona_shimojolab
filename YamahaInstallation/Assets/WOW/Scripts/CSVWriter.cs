using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CSVWriter
{
    public static string delimiter = ",";
    public static void SaveCSV(string[,] tableData, string filePath)
    {
        try
        {
            int rows = tableData.GetLength(0);
            int cols = tableData.GetLength(1);

            using (StreamWriter sw = new StreamWriter(filePath))
            {
                for (int i = 0; i < rows; i++)
                {
                    string line = "";

                    for (int j = 0; j < cols; j++)
                    {
                        line += tableData[i, j];

                        if (j < cols - 1)
                        {
                            line += delimiter;
                        }
                    }

                    sw.WriteLine(line);
                }
            }
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
    }
}