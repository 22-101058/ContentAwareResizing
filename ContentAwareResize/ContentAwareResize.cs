using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Linq;

namespace ContentAwareResize
{
  // *****************************************
  // DON'T CHANGE CLASS OR FUNCTION NAME
  // YOU CAN ADD FUNCTIONS IF YOU NEED TO
  // *****************************************
  public class ContentAwareResize
  {
    public struct coord
    {
      public int row;
      public int column;
      public coord(int i, int j)
      {
        row = i;
        column = j;
      }
    }
    public struct Seam
    {
      public int mySeam;
      public int childrenSeam;
      public coord childCoord;

      // Constructor with parameters
      public Seam(int mySeam = 0, int childrenSeam = 0 )
      {
        // Initialize fields with parameters
        this.mySeam = mySeam + childrenSeam;
        this.childrenSeam = childrenSeam;
        // If no value is provided for childCoords, create a new list
        this.childCoord = new coord(-1, -1);
      }
    }
    //========================================================================================================
    // Your Code is Here:
    //===================
    /// <summary>
    /// Develop an efficient algorithm to get the minimum vertical seam to be removed
    /// </summary>
    /// <param name="energyMatrix">2D matrix filled with the calculated energy for each pixel in the image</param>
    /// <param name="Width">Image's width</param>
    /// <param name="Height">Image's height</param>
    /// <returns>BY REFERENCE: The min total value (energy) of the selected seam in "minSeamValue" & List of points of the selected min vertical seam in seamPathCoord</returns>

    public Seam Calculate( int i, int j,int[,] energyMatrix, int Width, int Height, ref Seam[,] dp)
    {
      if (j < 0 || j >= Width || i >= Height) return new Seam(Int32.MaxValue);
      if (!dp[i, j].Equals(new Seam())) return dp[i, j];
      
      Seam[] childrenIj = new Seam[3];
      for (int k = -1; k <= 1; k++)
      {
        int nextJ = j + k;
        if (nextJ >= 0 && nextJ < Width) 
        {
          Seam childOfIj = Calculate(i + 1, nextJ, energyMatrix, Width, Height, ref dp);
          childrenIj[k + 1] = childOfIj;
        }
      }

      Seam lowestChildOfIj = new Seam(Int32.MaxValue);
      int childOfIjKey = 0;
      for (int k = 0; k <= 2; k++)
      {
        if (childrenIj[k].mySeam < lowestChildOfIj.mySeam)
        {
          lowestChildOfIj = childrenIj[k];
          childOfIjKey = k - 1;
        }
      }

      Seam thisSeam = new Seam
      {
        mySeam = energyMatrix[i, j] + lowestChildOfIj.mySeam,
        childrenSeam = lowestChildOfIj.mySeam,
        childCoord = new coord(i + 1, childOfIjKey)
      };
      return dp[i, j] = thisSeam;
      //coord chosenChildCoord = new coord
      //{
      //  row = i + 1,
      //  column = childOfIjKey,
      //};
      //Seam chosenChild = new Seam
      //{
      //  //mySeam = dp[i, j] += temp.mySeam,
      //  //childrenSeam = dp[i + 1, tempKey] = temp.mySeam,
      //  mySeam = energyMatrix[i, j],
      //  childrenSeam = lowestChildOfIj.mySeam + lowestChildOfIj.childrenSeam,
      //  childCoords = new List<coord> {chosenChildCoord}
      //};
      //tempSeamPathCoord.Add(chosenChildCoord);
    }

    public void CalculateSeamsCost(int[,] energyMatrix, int Width, int Height, ref int minSeamValue, ref List<coord> seamPathCoord)
    {
         // Write your code here 
         // throw new NotImplementedException(); // comment this line 
         Seam[,] dp = new Seam[Height, Width];
         //for (int i = 0; i < Height; i++)
         //{
         //  for (int j = 0; j < Width; j++)
         //  {
         //    dp[i,j] = 0;
         //  }
         //}
         Seam latestSeam = new Seam(Int32.MaxValue);
         List<coord> tempSeamPathCoord = new List<coord>();
         coord latestCoord = new coord();
         for (int col = 0; col < Width; col++)
         {
           
           Seam thisSeam = Calculate(0, col, energyMatrix, Width, Height, ref dp);
           coord currentCoord = new coord()
           {
             row = 0,
             column = col,
           };
           
           if (thisSeam.mySeam < latestSeam.mySeam)
           {
             latestSeam = thisSeam;
             latestCoord = currentCoord;
           }

         }
         seamPathCoord.Add(latestCoord);
         int i = latestSeam.childCoord.row;
         int j = latestSeam.childCoord.column;
         while (true)
         {
           Seam x = dp[i, j];
           seamPathCoord.Add(x.childCoord);
           i = x.childCoord.row;
           j = x.childCoord.column;
           if (i == -1 && j == -1) break;
         }
         minSeamValue = latestSeam.mySeam;
    }

    // *****************************************
    // DON'T CHANGE CLASS OR FUNCTION NAME
    // YOU CAN ADD FUNCTIONS IF YOU NEED TO 
    // *****************************************
    #region DON'TCHANGETHISCODE
    public MyColor[,] _imageMatrix;
    public int[,] _energyMatrix;
    public int[,] _verIndexMap;
    public ContentAwareResize(string ImagePath)
    {
      _imageMatrix = ImageOperations.OpenImage(ImagePath);
      _energyMatrix = ImageOperations.CalculateEnergy(_imageMatrix);
      int _height = _energyMatrix.GetLength(0);
      int _width = _energyMatrix.GetLength(1);
    }
    public void CalculateVerIndexMap(int NumberOfSeams, ref int minSeamValueFinal, ref List<coord> seamPathCoord)
    {
      int Width = _imageMatrix.GetLength(1);
      int Height = _imageMatrix.GetLength(0);

      int minSeamValue = -1;
      _verIndexMap = new int[Height, Width];
      for (int i = 0; i < Height; i++)
        for (int j = 0; j < Width; j++)
          _verIndexMap[i, j] = int.MaxValue;

      bool[] RemovedSeams = new bool[Width];
      for (int j = 0; j < Width; j++)
        RemovedSeams[j] = false;

      for (int s = 1; s <= NumberOfSeams; s++)
      {
        CalculateSeamsCost(_energyMatrix, Width, Height, ref minSeamValue, ref seamPathCoord);
        minSeamValueFinal = minSeamValue;

        //Search for Min Seam # s
        int Min = minSeamValue;

        //Mark all pixels of the current min Seam in the VerIndexMap
        if (seamPathCoord.Count != Height)
          throw new Exception("You selected WRONG SEAM");
        for (int i = Height - 1; i >= 0; i--)
        {
          if (_verIndexMap[seamPathCoord[i].row, seamPathCoord[i].column] != int.MaxValue)
          {
            string msg = "overalpped seams between seam # " + s + " and seam # " + _verIndexMap[seamPathCoord[i].row, seamPathCoord[i].column];
            throw new Exception(msg);
          }
          _verIndexMap[seamPathCoord[i].row, seamPathCoord[i].column] = s;
          //remove this seam from energy matrix by setting it to max value
          _energyMatrix[seamPathCoord[i].row, seamPathCoord[i].column] = 100000;
        }

        //re-calculate Seams Cost in the next iteration again
      }
    }
    public void RemoveColumns(int NumberOfCols)
    {
      int Width = _imageMatrix.GetLength(1);
      int Height = _imageMatrix.GetLength(0);
      _energyMatrix = ImageOperations.CalculateEnergy(_imageMatrix);

      int minSeamValue = 0;
      List<coord> seamPathCoord = null;
      //CalculateSeamsCost(_energyMatrix,Width,Height,ref minSeamValue, ref seamPathCoord);
      CalculateVerIndexMap(NumberOfCols, ref minSeamValue, ref seamPathCoord);

      MyColor[,] OldImage = _imageMatrix;
      _imageMatrix = new MyColor[Height, Width - NumberOfCols];
      for (int i = 0; i < Height; i++)
      {
        int cnt = 0;
        for (int j = 0; j < Width; j++)
        {
          if (_verIndexMap[i, j] == int.MaxValue)
            _imageMatrix[i, cnt++] = OldImage[i, j];
        }
      }

    }
    #endregion
  }
}
