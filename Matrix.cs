using System;

public static class Matrix
{
    public static double[][] MatrixInverse(double[][] matrix)
    {
        int n = matrix.Length;
        double[][] result = MatrixCreate(n, n);
        int[] perm;
        int toggle;
        double[][] lum = MatrixDecompose(matrix, out perm, out toggle);
        
        if (lum == null)
            throw new Exception("Unable to compute inverse");

        double[] b = new double[n];
        for (int i = 0; i < n; ++i)
        {
            for (int j = 0; j < n; ++j)
            {
                if (i == perm[j])
                    b[j] = 1.0;
                else
                    b[j] = 0.0;
            }

            double[] x = HelperSolve(lum, b);
            
            for (int j = 0; j < n; ++j)
                result[j][i] = x[j];
        }
        return result;
    }

    // Helper methods for matrix decomposition and solving
    private static double[][] MatrixCreate(int rows, int cols)
    {
        double[][] result = new double[rows][];
        for (int i = 0; i < rows; ++i)
            result[i] = new double[cols];
        return result;
    }

    private static double[][] MatrixDecompose(double[][] matrix, out int[] perm, out int toggle)
    {
        // Implementation of Crout's LU decomposition
        int n = matrix.Length;
        double[][] result = MatrixDuplicate(matrix);
        perm = new int[n];
        for (int i = 0; i < n; ++i) perm[i] = i;
        toggle = 1;
        
        for (int j = 0; j < n - 1; ++j)
        {
            double colMax = Math.Abs(result[j][j]);
            int pRow = j;
            
            for (int i = j + 1; i < n; ++i)
            {
                if (result[i][j] > colMax)
                {
                    colMax = result[i][j];
                    pRow = i;
                }
            }

            if (pRow != j)
            {
                double[] rowPtr = result[pRow];
                result[pRow] = result[j];
                result[j] = rowPtr;
                int tmp = perm[pRow];
                perm[pRow] = perm[j];
                perm[j] = tmp;
                toggle = -toggle;
            }

            if (Math.Abs(result[j][j]) < 1.0E-20)
                return null;

            for (int i = j + 1; i < n; ++i)
            {
                result[i][j] /= result[j][j];
                for (int k = j + 1; k < n; ++k)
                    result[i][k] -= result[i][j] * result[j][k];
            }
        }
        return result;
    }

    private static double[] HelperSolve(double[][] luMatrix, double[] b)
    {
        // Implementation of forward/backward substitution
        int n = luMatrix.Length;
        double[] x = new double[n];
        b.CopyTo(x, 0);

        for (int i = 1; i < n; ++i)
        {
            double sum = x[i];
            for (int j = 0; j < i; ++j)
                sum -= luMatrix[i][j] * x[j];
            x[i] = sum;
        }

        x[n - 1] /= luMatrix[n - 1][n - 1];
        for (int i = n - 2; i >= 0; --i)
        {
            double sum = x[i];
            for (int j = i + 1; j < n; ++j)
                sum -= luMatrix[i][j] * x[j];
            x[i] = sum / luMatrix[i][i];
        }

        return x;
    }

    public static double[][] MatrixProduct(double[][] matrixA, double[][] matrixB)
    {
        int aRows = matrixA.Length;
        int aCols = matrixA[0].Length;
        int bCols = matrixB[0].Length;
        double[][] result = MatrixCreate(aRows, bCols);

        for (int i = 0; i < aRows; ++i)
            for (int j = 0; j < bCols; ++j)
                for (int k = 0; k < aCols; ++k)
                    result[i][j] += matrixA[i][k] * matrixB[k][j];

        return result;
    }

    private static double[][] MatrixDuplicate(double[][] matrix)
    {
        double[][] result = MatrixCreate(matrix.Length, matrix[0].Length);
        for (int i = 0; i < matrix.Length; ++i)
            for (int j = 0; j < matrix[i].Length; ++j)
                result[i][j] = matrix[i][j];
        return result;
    }
}