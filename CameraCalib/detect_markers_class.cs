using System;
using System.Collections.Generic;
using System.Linq;
using OpenCvSharp;
using System.IO;
using Sparrow;
using Accord.Math;

namespace CameraCalib
{


    public class Marker : List<Point2f>
    {
        public Marker()
        {
            this.id = new();
            this.ssize = new();

        }
        public int id { get; set; }
        public Point2f[] markercoordinate { get; set; }
        public float ssize { get; set; }
        string dict_info { get; set; }
        List<Point> contourPoints { get; set; }

        

        //public float time { get; set; }

    }
    static public class ArucoSerdes
    {
        static public void serialize_marker(Marker marker, ref BinaryWriter output)
        {
            output.Write(marker.id);
            for (int c = 0; c < 4; c++)
            {
                //Console.Write("마커 X, Y 크기");
                //Console.WriteLine(sizeof(float));
                output.Write(marker[c].X);
                output.Write(marker[c].Y);
            }
            //Console.Write("마커 ID 크기");
            //Console.WriteLine(sizeof(int));
            
        }
    }
    public class Dataset
    {
        private string folder_path;
        private int num_cams;
        
        //private List<int> all_frame;
        //private List<List<int>> frame_nums;
        //public Dataset()
        //{
        //    List<int> all_frame;
        //    List<List<int>> frame_nums;
        //}
        public void _Dataset(string folder)
        {
            List<int> all_frame = new();
            List<List<int>> frame_nums = new();

            folder_path = folder;
            num_cams = obtain_num_cams();
            for (int i = 0; i < num_cams; i++)
            {
                frame_nums.Add(new List<int>());
            }
            get_frame_nums(ref all_frame, ref frame_nums);
            
        }
        public void get_frame_nums(ref List<int> fns)
        {
            foreach(int frame_num in fns)
            {
                fns.Add(frame_num);
            }
        }
        public void get_frame_nums(ref List<long> fn)
        {
            List<int> all_frame = new();
            List<List<int>> frame_nums = new();
            for (int i = 0; i < num_cams; i++)
            {
                frame_nums.Add(new List<int>());
            }
            get_frame_nums(ref all_frame, ref frame_nums);
            fn = all_frame.ConvertAll(i =>(long)i);
        }
        public int get_num_cams()
        {
            return num_cams;
        }
        public int obtain_num_cams()
        {
            
            DirectoryInfo di = new DirectoryInfo(folder_path);
            var nums = di.GetDirectories().Length;
            var did = di.GetDirectories();
            List<string> dirs_list = Directory.GetDirectories(folder_path).ToList();
            
            int num_cams = -1;
            for (int i = 0; i < nums; i++)
            {
                if (string_is_uint(dirs_list[i]))
                {
                    int dir_num = Convert.ToInt32(dirs_list[i].Substring(dirs_list[i].Length-1,1));
                    if (dir_num > num_cams)
                        num_cams = dir_num;
                }
            }
            num_cams++;
            return num_cams;
        }
        public bool string_is_uint(string s)
        {
            if (s.Length == 0)
                return false;

            //foreach (char c in s)//문자열 따라서 숫자인지 판별하는 반복문
            if (!Char.IsDigit(s.Last()))
                return false;
            return true;
        }

        public void get_frame_nums(ref List<int> all_frame, ref List<List<int>> frame_nums)
        {
            
            List<int> alf  = new();
            List<List<int>> fns = new();
            for (int i = 0; i < num_cams; i++)
            {
                fns.Add(new List<int>());
            }
            char sp = '\\';
            for (var cam_num = 0; cam_num < num_cams; cam_num++)
            {
                string cam_dir_path = folder_path + "\\" + cam_num.ToString();
                List<string> files_list = Directory.GetFiles(cam_dir_path).ToList();
                //files_list.Remove(files_list.Last());(Calib.xml이 숫자와 비교대상이 아니어서 문제가 생김?)
                var sorted_files_list = files_list.OrderBy<string, string>(a => a, new StringComparer()).ThenBy<string, string>(b => b, new NumberComparer()).ToList(); ;
                //files_list.Sort();
                foreach (string file_name in sorted_files_list)
                {
                    int frame_num = 0;
                    var spstring = file_name.ToString().Split(sp).Last();
                    try
                    {
                        frame_num = Convert.ToInt32(spstring.Substring(0, spstring.IndexOf(".jpg")));
                    }
                    catch (ArgumentOutOfRangeException e)
                    {
                        Console.WriteLine("{cam_num}번 진행중");
                    }
                    //Console.WriteLine(frame_num);
                    //if (spstring.GetType() == 1.GetType())
                    //{
                    
                    fns[cam_num].Add(frame_num);
                    alf.Add(frame_num);
                    //}

                }
                frame_nums[cam_num] = fns[cam_num];
                all_frame = alf;

            }
        }


        public void get_frame(int frame_num, ref List<Mat> frames)
        {
            List<int> all_frame = new();
            List<List<int>> frame_nums = new();
            int _num_cams = obtain_num_cams();
            for (int i = 0; i < _num_cams; i++)
            {
                frame_nums.Add(new List<int>());
            }
            get_frame_nums(ref all_frame, ref frame_nums);

            int num_cams = frame_nums.Count();

            ListExtras.Resize<Mat>(frames, num_cams);
            for (int cam = 0;cam<num_cams;cam++)
            {
                string cam_dir_path = folder_path + "/" + cam.ToString();

                if(frame_nums[cam].Contains(frame_num)==true)
                {
                    frames[cam] = Cv2.ImRead(cam_dir_path + "/" + frame_num.ToString() + ".jpg");
                }
            }
        }
        public Point2f[][] get_markercoordinate(List<Marker> markersets)
        {
            
            Point2f[][] pntsArray = new Point2f[markersets.Count][];
            Point2f[] pnts = new Point2f[4];
            for (int i = 0; i<markersets.Count; i++)
            {
                pntsArray[i] = new Point2f[4];
            }
            foreach (Marker marker in markersets)
            {
                
                for (int k = 0; k < 4; k++)
                {
                    pnts[k] = marker[k];
                }
                
                pnts.CopyTo(pntsArray[markersets.IndexOf(marker)]);
            }
            var retpntsArray = pntsArray;
            
            return retpntsArray;
        }


    }
    

    public static class ListExtras
    {
        //    list: List<T> to resize
        //    size: desired new size
        // element: default value to insert

        public static void Resize<T>(this List<T> list, int size, T element = default(T))
        {
            int count = list.Count;

            if (size < count)
            {
                list.RemoveRange(size, count - size);
            }
            else if (size > count)
            {
                if (size > list.Capacity)   // Optimization
                    list.Capacity = size;

                list.AddRange(Enumerable.Repeat(element, size - count));
            }
        }
    }
    public class StringComparer : IComparer<string>
    {
        public int Compare(string a, string b)
        {
            string[] arr1 = a.Split('\\');
            string[] arr2 = b.Split('\\');
            string str1 = arr1[0];
            string str2 = arr2[0];
            return str1.CompareTo(str2);
        }
    }

    public class NumberComparer : IComparer<string>
    {
        public int Compare(string a, string b)
        {
            string[] arr1 = a.Split('\\');
            string[] arr1_1 = arr1[arr1.Count() - 1].Split('.');
            string[] arr2 = b.Split('\\');
            string[] arr2_1 = arr2[arr1.Count()-1].Split('.');
            int int1 = int.Parse(arr1_1[0]);
            int int2 = int.Parse(arr2_1[0]);
            return int1.CompareTo(int2);
        }
    }

    class TransfBetweenCam
    {
        public int markernum { get; set; }
        public int cameranum { get; set; }
        public double[] markercoordinate { get; set; }

    }
    class AtoB
    {
        private double[] rodriguess_rot { get; set; }
        private double[] translate { get; set; }
        public int markernum { get; set; }
        public int cameranum { get; set; }

    }
    class CtoC
    {
        private double[] rodriguess_rot { get; set; }
        private double[] translate { get; set; }
        public int markernum { get; set; }
        public int cameranum { get; set; }
    }
    class MtoC
    {
        private double[] rodriguess_rot { get; set; }
        private double[] translate { get; set; }
        public int markernum { get; set; }
        public int cameranum { get; set; }
    }
    public static class CalibExtension
    {
        
    }
}
/*
namespace Extension
{
    public static class CalibExtension
    {
        public static void Dataset(string folder)
        {
            folder_path = folder;
            num_cams = obtain_num_cams();
            get_frame_nums();

        }
    }
}
*/