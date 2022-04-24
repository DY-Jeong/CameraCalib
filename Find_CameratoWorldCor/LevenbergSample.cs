using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Accord;
using Accord.Math;
using Accord.Math.Optimization;

namespace Find_CameratoWorldCor
{
    class LevenbergSample
    {
        void LevenbergMarqtest()
        {
            // Example from https://en.wikipedia.org/wiki/Gauss%E2%80%93Newton_algorithm

            // In this example, the Gauss–Newton algorithm will be used to fit a model to 
            // some data by minimizing the sum of squares of errors between the data and 
            // model's predictions.

            // In a biology experiment studying the relation between substrate concentration [S]
            // and reaction rate in an enzyme-mediated reaction, the data in the following table
            // were obtained:

            double[][] inputs = Jagged.ColumnVector(new[] { 0.03, 0.1947, 0.425, 0.626, 1.253, 2.500, 3.740 });
            double[] outputs = new[] { 0.05, 0.127, 0.094, 0.2122, 0.2729, 0.2665, 0.3317 };

            // It is desired to find a curve (model function) of the form
            // 
            //   rate = \frac{V_{max}[S]}{K_M+[S]}
            // 
            // that fits best the data in the least squares sense, with the parameters V_max
            // and K_M to be determined. Let's start by writing model equation below:

            LeastSquaresFunction function = (double[] parameters, double[] input) =>
            {
                return (parameters[0] * input[0]) / (parameters[1] + input[0]);
            };

            // Now, we can either write the gradient function of the model by hand or let
            // the model compute it automatically using Newton's finite differences method:

            LeastSquaresGradientFunction gradient = (double[] parameters, double[] input, double[] result) =>
            {
                result[0] = -((-input[0]) / (parameters[1] + input[0]));
                result[1] = -((parameters[0] * input[0]) / Math.Pow(parameters[1] + input[0], 2));
            };

            // Create a new Levenberg-Marquardt algorithm
            var gn = new LevenbergMarquardt(parameters: 2)
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

    }
}
