using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;
using OpenCvSharp.Aruco;
using System.IO;

namespace CameraCalib
{
    public class CamConfig
    {
        public OpenCvSharp.Mat cam_mat { get; set; }
        public OpenCvSharp.Mat dist_coeffs { get; set; }
        public OpenCvSharp.Size image_size { get; set; }

        public void Camconfig()
        {

        }
        public void Camconfig(OpenCvSharp.Mat c_mat, OpenCvSharp.Mat d_coeffs, OpenCvSharp.Size im_size)
        {
            setCamMat(c_mat);
            setDistCoeffs(d_coeffs);
            setImageSize(im_size);
        }
        public void setCamMat(OpenCvSharp.Mat c_mat)
        {
            if(c_mat.Rows != 3 || c_mat.Cols !=3)
            {
                Console.WriteLine("The size of the input camera matrix must be 4x4!");
                return;
            }
            c_mat.ConvertTo(cam_mat, OpenCvSharp.MatType.CV_32FC1);
        }

        public void setDistCoeffs(OpenCvSharp.Mat d_coeffs)
        {
            if(d_coeffs.Rows != 1&& d_coeffs.Cols !=1)
            {
                Console.WriteLine("The size of the input matrix for distorsion coefficients must be 1xN nor Nx1!");
            }
            dist_coeffs = OpenCvSharp.Mat.Zeros(5, 1, OpenCvSharp.MatType.CV_64FC1);
            d_coeffs.ConvertTo(d_coeffs, OpenCvSharp.MatType.CV_64FC1);
            for(int i=0; i<d_coeffs.Total()&& i<5;i++)
            {
                dist_coeffs.At<double>(i) = d_coeffs.At<double>(i);
            }
            //여까지 함
        }
        public void setImageSize(OpenCvSharp.Size im_size)
        {
            image_size = im_size;
        }
        public readonly OpenCvSharp.Mat getCamMat()
        {
            return cam_mat;
        }
        public readonly OpenCvSharp.Mat getDistCoeffs()
        {
            return dist_coeffs;
        }
        public readonly OpenCvSharp.Mat getImageSize()
        {
            return image_size;
        }
        public bool read_from_file(string path)
        {
            OpenCvSharp.
            return true;
        }
        static List<CamConfig> read_cam_configs(string folder_path)
        {
            List<CamConfig> camConfigs = new List<CamConfig>();
            return camConfigs;
        }
    }
    public class initializer
    {
        public void Initializer(double marker_s, ref List<CamConfig> cam_c)
        {

        }
    }
}
