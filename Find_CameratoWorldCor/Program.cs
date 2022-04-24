using System;
using System.Collections.Generic;
using OpenCvSharp;
using Accord.Math;
using Accord.Math.Optimization;

namespace Find_CameratoWorldCor
{
    class Program
    {
        double rodA = 0;
        double rodB = 0;
        double rodC = 0;
        double error = 1.7E+307;
        double[] rodrigues = new double[3] { 0, 0, 0 };
        double[] translation = new double[3] { 0, 0, 0 };

        public double fire_x;
        public double fire_y;
        public double fire_z;
        double[,] RotMat = new double[,] { { -2.0545331805994860e-01, 4.4500061082199471e-01,-8.7164407326970872e-01},
                                               { -5.5157418715304285e-01, 6.8306516301716769e-01, 4.7873572995838815e-01},
                                               {8.0842739325471757e-01, 5.7913421539430932e-01, 1.0511284600371257e-01} };

        double[] TransMat = new double[] { 1.4911733590592615e+01, -9.5035013017689636e+00, 1.2700375195799474e+01 };//{ 1.6164281586808674e+01, -1.0425081773393636e+01, 1.3956958445517484e+01 };
        double[,] K1 = new double[,] { { 1299.965335, 0f, 1273.997764 }, { 0f, 1306.701193f, 798.344544f }, { 0, 0, 1 } };
        double[,] K2 = new double[,] { { 1299.965335, 0f, 1273.997764 }, { 0f, 1306.701193f, 798.344544f }, { 0, 0, 1 } };
        double[,] I3 = new double[,] { { 1, 0, 0 }, { 0, 1, 0 }, { 0, 0, 1 } };
        float[] Z = new float[] { 0, 0, 0 };
        
        static double[] D1 = new double[5] { -3.209793E-01, 1.165590E-01, 1.329457E-03, -1.703221E-03, -2.078315E-02 };//{-0.340254, 0.102706, -0.000052, 0.000360,0};// 


        double[,] Proj1;
        double[,] Proj2;

        
        List<double[]> recogedlist = new List<double[]>();
        

        //XYZ
        double[] pt1W = new double[3] { 2.662, 11.166, 0 };
        double[] pt2W = new double[3] { 7.987, 11.166, 0 };
        double[] pt3W = new double[3] { 7.987, 13.312, 0 };
        double[] pt4W = new double[3] { 18.637, 11.166, 0 };
        double[] pt5W = new double[3] { 2.662, 6.7, 0.2 };
        double[] pt6W = new double[3] { 7.987, 6.7, 0.2 };
        double[] pt7W = new double[3] { 13.312, 6.7, 0 };
        double[] pt8W = new double[3] { 18.637, 6.7, 0 };
        double[] pt9W = new double[3] { 2.662, 2.233, 0.2 };
        double[] pt10W = new double[3] { 7.987, 2.233, 0.2 };
        double[] pt11W = new double[3] { 13.312, 2.233, 0 };
        double[] pt12W = new double[3] { 18.637, 2.233, 0 }; 
        //XY
        double[] pt1L = new double[2] { 597, 588 };
        double[] pt2L = new double[2] { 1030, 392};
        double[] pt3L = new double[2] { 1338,297 };
        double[] pt4L = new double[2] { 1534, 241 };
        double[] pt5L = new double[2] { 812, 750 };
        double[] pt6L = new double[2] { 1339, 486 };
        double[] pt7L = new double[2] { 1624,363 };
        double[] pt8L = new double[2] { 1781,298};
        double[] pt9L = new double[2] { 1337, 1136 };
        double[] pt10L = new double[2] { 1811,661 };
        double[] pt11L = new double[2] { 1982,473};
        double[] pt12L = new double[2] { 2062,384 };
        //XY
        double[] pt1R = new double[2] { 1197, 491 };
        double[] pt2R = new double[2] { 1396,535 };
        double[] pt3R = new double[2] { 1716,639 };
        double[] pt4R = new double[2] { 2153,834};
        double[] pt5R = new double[2] {935,526 };
        double[] pt6R = new double[2] {1106,591};
        double[] pt7R = new double[2] {1413,716};
        double[] pt8R = new double[2] { 1951,994 };
        double[] pt9R = new double[2] { 650,600 };
        double[] pt10R = new double[2] { 743,687 };
        double[] pt11R = new double[2] { 932,864 };
        double[] pt12R = new double[2] {1422,1320};

        double[] pt1 = new double[4] { 597, 588, 1197, 491 };
        double[] pt2 = new double[4] { 1030, 392, 1396, 535 };
        double[] pt3 = new double[4] { 1338, 297, 1716, 639 };
        double[] pt4 = new double[4] { 1534, 241, 2153, 834 };
        double[] pt5 = new double[4] { 812, 750, 935, 526 };
        double[] pt6 = new double[4] { 1339, 486, 1106, 591 };
        double[] pt7 = new double[4] { 1624, 363, 1413, 716 };
        double[] pt8 = new double[4] { 1781, 298, 1951, 994 };
        double[] pt9 = new double[4] { 1337, 1136, 650, 600 };
        double[] pt10 = new double[4] { 1811, 661 , 743, 687 };
        double[] pt11 = new double[4] { 1982, 473 , 932, 864 };
        double[] pt12 = new double[4] { 2062, 384, 1422, 1320 };


        static void Main(string[] args)
        {
            double[] pt1 = new double[4] { 597, 588, 1197, 491 };
            double[] pt2 = new double[4] { 1030, 392, 1396, 535 };
            double[] pt3 = new double[4] { 1338, 297, 1716, 639 };
            double[] pt4 = new double[4] { 1534, 241, 2153, 834 };
            double[] pt5 = new double[4] { 812, 750, 935, 526 };
            double[] pt6 = new double[4] { 1339, 486, 1106, 591 };
            double[] pt7 = new double[4] { 1624, 363, 1413, 716 };
            double[] pt8 = new double[4] { 1781, 298, 1951, 994 };
            double[] pt9 = new double[4] { 1337, 1136, 650, 600 };
            double[] pt10 = new double[4] { 1811, 661, 743, 687 };
            double[] pt11 = new double[4] { 1982, 473, 932, 864 };
            double[] pt12 = new double[4] { 2062, 384, 1422, 1320 };
            //input은 x값 ->이미지좌표(L, R)
            double[][] inputs = Jagged.ColumnVector(new[] { 0.03, 0.1947, 0.425, 0.626, 1.253, 2.500, 3.740 });
            double[][] inputspoint = new double[12][];
            inputspoint[0] = pt1;
            inputspoint[1] = pt2;
            inputspoint[2] = pt3;
            inputspoint[3] = pt4;
            inputspoint[4] = pt5;
            inputspoint[5] = pt6;
            inputspoint[6] = pt7;
            inputspoint[7] = pt8;
            inputspoint[8] = pt9;
            inputspoint[9] = pt10;
            inputspoint[10] = pt11;
            inputspoint[11] = pt12;


            double[] pt1W = new double[3] { 2.662, 11.166, 0 };
            double[] pt2W = new double[3] { 7.987, 11.166, 0 };
            double[] pt3W = new double[3] { 7.987, 13.312, 0 };
            double[] pt4W = new double[3] { 18.637, 11.166, 0 };
            double[] pt5W = new double[3] { 2.662, 6.7, 0.2 };
            double[] pt6W = new double[3] { 7.987, 6.7, 0.2 };
            double[] pt7W = new double[3] { 13.312, 6.7, 0 };
            double[] pt8W = new double[3] { 18.637, 6.7, 0 };
            double[] pt9W = new double[3] { 2.662, 2.233, 0.2 };
            double[] pt10W = new double[3] { 7.987, 2.233, 0.2 };
            double[] pt11W = new double[3] { 13.312, 2.233, 0 };
            double[] pt12W = new double[3] { 18.637, 2.233, 0 };


            //output은 y값 ->오차
            double[] outputs = new[] {0d,0,0,0,0,0,0,0,0,0,0,0 };

            // It is desired to find a curve (model function) of the form
            // 
            //   rate = \frac{V_{max}[S]}{K_M+[S]}
            // 
            // that fits best the data in the least squares sense, with the parameters V_max
            // and K_M to be determined. Let's start by writing model equation below:

            LeastSquaresFunction function = (double[] parameters, double[] input) =>
            {
                double error = 0;
                error+=GetError(inputspoint[0], pt1W, parameters[0], parameters[1], parameters[2], parameters[3], parameters[4], parameters[5]);
                error+=GetError(inputspoint[1], pt2W, parameters[0], parameters[1], parameters[2], parameters[3], parameters[4], parameters[5]);
                error+=GetError(inputspoint[2], pt3W, parameters[0], parameters[1], parameters[2], parameters[3], parameters[4], parameters[5]);
                error+=GetError(inputspoint[3], pt4W, parameters[0], parameters[1], parameters[2], parameters[3], parameters[4], parameters[5]);
                error+=GetError(inputspoint[4], pt5W, parameters[0], parameters[1], parameters[2], parameters[3], parameters[4], parameters[5]);
                error+=GetError(inputspoint[5], pt6W, parameters[0], parameters[1], parameters[2], parameters[3], parameters[4], parameters[5]);
                error+=GetError(inputspoint[6], pt7W, parameters[0], parameters[1], parameters[2], parameters[3], parameters[4], parameters[5]);
                error+=GetError(inputspoint[7], pt8W, parameters[0], parameters[1], parameters[2], parameters[3], parameters[4], parameters[5]);
                error+=GetError(inputspoint[8], pt9W, parameters[0], parameters[1], parameters[2], parameters[3], parameters[4], parameters[5]);
                error+=GetError(inputspoint[9], pt10W, parameters[0], parameters[1], parameters[2], parameters[3], parameters[4], parameters[5]);
                error+=GetError(inputspoint[10], pt11W, parameters[0], parameters[1], parameters[2], parameters[3], parameters[4], parameters[5]);
                error+=GetError(inputspoint[11], pt12W, parameters[0], parameters[1], parameters[2], parameters[3], parameters[4], parameters[5]);
                return error;
                //return (parameters[0] * input[0]) / (parameters[1] + input[0]);
            };

            // Now, we can either write the gradient function of the model by hand or let
            // the model compute it automatically using Newton's finite differences method:
            // 편미분해서 RMS구하기
            LeastSquaresGradientFunction gradient = (double[] parameters, double[] input, double[] result) =>
            {
                result[0] = -((-input[0]) / (parameters[1] + input[0]));
                result[1] = -((parameters[0] * input[0]) / Math.Pow(parameters[1] + input[0], 2));
            };

            // Create a new Levenberg-Marquardt algorithm
            // a, b, c, x, y, z 6개 파라미터
            var gn = new LevenbergMarquardt(parameters: 6)
            {
                Function = function,
                Gradient = gradient,
                Solution = new[] { 0.9, 0.2 } // starting from b1 = 0.9 and b2 = 0.2
            };

            // Find the minimum value:
            gn.Minimize(inputs, outputs);

            // The solution will be at:
            double b1 = gn.Solution[0]; // will be 0.362
            double b2 = gn.Solution[1]; // will be 0.556
        }




        static double[] SetRodrigues(double a, double b, double c)
        {
            double[] rod = new double[3];
            double theta = Math.Sqrt(a * a + b * b + c * c);
            rod[0] = a / theta;
            rod[1] = b / theta;
            rod[2] = c / theta;
            return rod;
        }

        static double GetError(double[] ptc, double[] pto, double a, double b, double c, double x, double y, double z)
        {
            double[] ptced = GetStereovisionCor(ptc, a, b, c, x, y, z);
            return (Math.Abs(ptced[0] - pto[0])+ Math.Abs(ptced[1] - pto[1])+ Math.Abs(ptced[2] - pto[2]));
        }
        static double[] GetStereovisionCor(double[] pt, double a, double b, double c, double x, double y, double z)
        {
            /*  double[,] Proj1;
              double[,] Proj2;*/
            
            double[] translation = new double[3] { x, y, z };
            double[] rodrigues = SetRodrigues(a, b, c);
            List<Vec4d> pnt4d = new List<Vec4d>();
            double[,] RotMat = new double[,] { { -2.0545331805994860e-01, 4.4500061082199471e-01,-8.7164407326970872e-01},
                                               { -5.5157418715304285e-01, 6.8306516301716769e-01, 4.7873572995838815e-01},
                                               {8.0842739325471757e-01, 5.7913421539430932e-01, 1.0511284600371257e-01} };
            double[] TransMat = new double[] { 1.4911733590592615e+01, -9.5035013017689636e+00, 1.2700375195799474e+01 };//{ 1.6164281586808674e+01, -1.0425081773393636e+01, 1.3956958445517484e+01 };
            double[,] K1 = new double[,] { { 1299.965335, 0f, 1273.997764 }, { 0f, 1306.701193f, 798.344544f }, { 0, 0, 1 } };
            double[,] K2 = new double[,] { { 1299.965335, 0f, 1273.997764 }, { 0f, 1306.701193f, 798.344544f }, { 0, 0, 1 } };
            double[,] I3 = new double[,] { { 1, 0, 0 }, { 0, 1, 0 }, { 0, 0, 1 } };
            float[] Z = new float[] { 0, 0, 0 };
            double[] ptl = new double[2] { pt[0], pt[1] };
            double[] ptr = new double[2] { pt[2], pt[3] };
            double[,] Proj1a = Matrix.Dot(K1, I3);
            double[] Proj1b = Matrix.Dot(K1, Z);
            double[,] Proj1 = new double[3, 4];
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    Proj1[i, j] = Proj1a[i, j];
            for (int i = 0; i < 3; i++)
                Proj1[i, 3] = Proj1b[i];

            var Proj2a = Matrix.Dot(K2, RotMat);
            var Proj2b = Matrix.Dot(K2, TransMat);

            double[,] Proj2 = new double[3, 4];
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    Proj2[i, j] = Proj2a[i, j];
            for (int i = 0; i < 3; i++)
                Proj2[i, 3] = Proj2b[i];

           /* double[,] RotMat1 = RotbyXaxis(-0.66357418); //radian
            double[,] RotMat2 = RotbyYaxis(0.82938046);
            double[,] RotMat3 = RotbyXaxis(-1.5708);
            double[] TransMattoW = new double[] { 0.041441, -0.019641, 4.411598 };*/


            Cv2.TriangulatePoints(InputArray.Create(Proj1), InputArray.Create(Proj2), InputArray.Create(ptl), InputArray.Create(ptr), OutputArray.Create(pnt4d));
            double[] firepoint3d = new double[] { pnt4d[0].Item0 / pnt4d[0].Item3, pnt4d[0].Item1 / pnt4d[0].Item3, pnt4d[0].Item2 / pnt4d[0].Item3 };
            double[,] rotate3by3 = new double[,] { };
            double[,] jacobian = new double[,] { };
            Cv2.Rodrigues(rodrigues, out rotate3by3, out jacobian);
            firepoint3d = Matrix.Dot(rotate3by3, firepoint3d);

            /*firepoint3d = Matrix.Dot(RotMat2, firepoint3d);
            firepoint3d = Matrix.Dot(RotMat3, firepoint3d);*/
            firepoint3d[0] = firepoint3d[0] + translation[0];
            firepoint3d[1] = firepoint3d[1] + translation[1];
            firepoint3d[2] = firepoint3d[2] + translation[2];

            List<double> xyz = new List<double>();
            xyz.Add(firepoint3d[0]);
            xyz.Add(firepoint3d[1]);
            xyz.Add(firepoint3d[2]);
            return firepoint3d;
        }

        static private double[,] RotbyXaxis(double a)
        {
            double[,] result = new double[,] { { 1,0,0},
                                               {0,Math.Cos(a), -Math.Sin(a)},
                                               {0,Math.Sin(a), Math.Cos(a)}};
            return result;
        }
        static private double[,] RotbyYaxis(double a)
        {
            double[,] result = new double[,] { { Math.Cos(a),0,Math.Sin(a)},
                                               {0,1, 0},
                                               {-Math.Sin(a),0, Math.Cos(a)}};
            return result;
        }
        static private double[,] RotbyZaxis(double a)
        {
            double[,] result = new double[,] { { Math.Cos(a),-Math.Sin(a),0},
                                               {Math.Sin(a),Math.Cos(a), 0},
                                               {0,0, 1}};
            return result;
        }

    }
}
