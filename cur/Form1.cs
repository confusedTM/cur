using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace cur
{
    public partial class Form1 : Form
    {

        private int n;
        private TextBox[,] matr;
        private TextBox[] vectorB;
        private TextBox[] vectorX;
        public Form1()
        {
            InitializeComponent();
        }

        private string DefaultText = String.Format("{0:f}", 0.0);

        // создаёт текстбокс
        private TextBox InitTextBox(bool readOnly)
        {
            TextBox textBox = new TextBox();
            textBox.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            textBox.Text = DefaultText;
            textBox.ReadOnly = readOnly;
            if (!readOnly)
            {
                textBox.CausesValidation = true;
                textBox.Validating += ValidateTextBox;
            }
            return textBox;
        }

        // проверяет правильность текста в текстбоксе
        private void ValidateTextBox(object sender, CancelEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            double result;
            e.Cancel = !double.TryParse(textBox.Text, out result);
        }

        // создаёт вдумерный массив текстбоксов, вставляя в layout
        private TextBox[,] InitTextBoxMatrix(TableLayoutPanel layoutPanel, int count, bool readOnly)
        {
            layoutPanel.SuspendLayout();

            layoutPanel.Controls.Clear();

            layoutPanel.ColumnStyles.Clear();
            layoutPanel.ColumnCount = count;

            layoutPanel.RowStyles.Clear();
            layoutPanel.RowCount = count;

            TextBox[,] result = new TextBox[count, count];
            float cellSize = 1f / count * 100f;

            for (int col = 0; col < count; col++)
            {
                layoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, cellSize));
                for (int row = 0; row < count; row++)
                {
                    layoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, cellSize));

                    TextBox textBox = InitTextBox(readOnly);

                    layoutPanel.Controls.Add(textBox, col, row);
                    result[col, row] = textBox;
                }
            }

            layoutPanel.ResumeLayout(true);

            return result;
        }

        // создаёт одномерный массив текстарий, вставляя в layout
        private TextBox[] InitTextBoxArray(TableLayoutPanel layoutPanel, int count, bool readOnly)
        {
            layoutPanel.SuspendLayout();

            layoutPanel.Controls.Clear();

            layoutPanel.ColumnStyles.Clear();
            layoutPanel.ColumnCount = 1;

            layoutPanel.RowStyles.Clear();
            layoutPanel.RowCount = count;

            TextBox[] result = new TextBox[count];
            float cellSize = 1f / count * 100f;

            layoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));

            for (int row = 0; row < count; row++)
            {
                layoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, cellSize));

                TextBox textBox = InitTextBox(readOnly);

                layoutPanel.Controls.Add(textBox, 0, row);
                result[row] = textBox;
            }

            layoutPanel.ResumeLayout(true);

            return result;
        }

        private void InitMatrix()
        {
            matr = InitTextBoxMatrix(layoutMatrix, n, false);
        }

        private void InitVectorX()
        {
            vectorX = InitTextBoxArray(layoutVectorX, n, true);
        }

        private void InitVectorB()
        {
            vectorB = InitTextBoxArray(layoutVectorB, n, false);
        }

        public int N
        {
            get { return n; }
            set
            {
                if (value != n && value > 0)
                {
                    n = value;
                    InitMatrix();
                    InitVectorX();
                    InitVectorB();
                }
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            N = (int)numericUpDown1.Value;
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            N = (int)numericUpDown1.Value;
        }

        public double[][] Matrix
        {
            get
            {
                // собираем введённую пользователем матрицу A
                double[][] matrix = new double[n][];
                for (int i = 0; i < n; i++)
                {
                    matrix[i] = new double[n];
                    for (int j = 0; j < n; j++)
                        matrix[i][j] = double.Parse(matr[j, i].Text);
                }
                return matrix;
            }
            set
            {
                // записываем в текстбоксы матрицу A
                for (int i = 0; i < n; i++)
                    for (int j = 0; j < n; j++)
                        matr[j,i].Text = value[i][j].ToString("f");
            }
        }

        public double[] VectorB
        {
            get
            {
                // собираем введённый пользователем вектор B
                double[] vector_b = new double[n];
                for (int j = 0; j < n; j++)
                    vector_b[j] = double.Parse(vectorB[j].Text);
                return vector_b;
            }
            set
            {
                // записываем в текстбоксы вектор B
                for (int j = 0; j < n; j++)
                    vectorB[j].Text = value[j].ToString("f");
            }
        }

        public double[] VectorX
        {
            set
            {
                // показываем вычисленный результат X
                for (int j = 0; j < n; j++)
                    vectorX[n-j-1].Text = value[j].ToString("f");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            double[][] mat = Matrix;
            double[] b = VectorB;

            double[][] maxmat = (double[][])Matrix.Clone();
            double[] maxb = (double[])VectorB.Clone();
            double max = 0;

            for (int i = 0; i < n; i++)
            {
                max += mat[i][i];
            }

            bool[] chosen = new bool[n];
            LinkedList<double[]> permutationMat = new LinkedList<double[]>();
            LinkedList<double> permutationB = new LinkedList<double>();
            search(chosen, permutationMat, permutationB, mat, b, max, maxmat, maxb);
            Matrix = maxmat;
            VectorB = maxb;
        }

        private void search(bool[] chosen, LinkedList<double[]> permutationMat, LinkedList<double> permutationB, double[][] mat, double[] b, double MaxSumOfElementsOfMainDiagonal, double[][] maxmat, double[] maxb)
        {
            if (permutationMat.Count == n)
            {
                // обработка перестановки
                double g = CountSumOfElementsOfMainDiagonal(permutationMat);
                if (g > MaxSumOfElementsOfMainDiagonal)
                {
                    MaxSumOfElementsOfMainDiagonal = g;
                    for (int i = 0; i < n; i++)
                    {
                        maxmat[i] = permutationMat.ElementAt(i);
                        maxb[i] = permutationB.ElementAt(i);
                    }
                }
            }
            else
            {
                // генератор перестановок
                for (int i = 0; i < n; i++)
                {
                    if (chosen[i]) continue;
                    chosen[i] = true;
                    permutationMat.AddLast(mat[i]);
                    permutationB.AddLast(b[i]);
                    search(chosen, permutationMat, permutationB, mat, b, MaxSumOfElementsOfMainDiagonal, maxmat, maxb);
                    chosen[i] = false;
                    permutationMat.RemoveLast();
                    permutationB.RemoveLast();
                }
            }
        }
        // считатет сумму элементов главной диагонали
        private double CountSumOfElementsOfMainDiagonal(LinkedList<double[]> permutation)
        {
            double sl = 0;

            for (int i = 0; i < n; i++)
            {
                sl += permutation.ElementAt(i)[i];
            }

            return sl;
        }


        private void button1_Click(object sender, EventArgs e)
        {
            VectorX = Gauss(VectorB, Matrix);
        }

        //метод Гаусса
        private double[] Gauss(double[] free, double[][] matrix)
        {

            double[][] MatrixCopy = (double[][])matrix.Clone();   //копирование матрицы
            double[] FreeCopy = (double[])free.Clone();     //копирование вектора свободных членов

            for (int i = 0; i < n - 1; i++)
            {       //приведение к верхнему треугольному виду
                for (int j = i + 1; j < n; j++)
                {
                    double koef = MatrixCopy[j][i] / MatrixCopy[i][i];
                    for (int k = i; k < n; k++)
                    {
                        MatrixCopy[j][k] -= MatrixCopy[i][k] * koef;
                    }
                    FreeCopy[j] -= FreeCopy[i] * koef;        //изменение вектора свободных членов
                }
            }

            int count = 1;
            double[] x = new double[n];
            x[0] = FreeCopy[n - 1] / MatrixCopy[n - 1][n - 1]; // вычисление n-ой неизвестной
            //обртаный ход
            for (int i = 1, k = n - 2; i < n && k >= 0; i++, k--)
            {       
                for (int j = n - 1; j != k; j--, count++)
                {
                    x[i] = (FreeCopy[k] + MatrixCopy[k][j] * (-1) * x[i - count]) / MatrixCopy[k][k];
                }
                count = 1;
            }

            return x;
        }
    }
}
