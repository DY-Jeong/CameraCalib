using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;
using OpenCvSharp.Aruco;
using System.IO;

//Unit, Length:m, Angle:deg, 
//카메라는 n개
//마커는 n개


namespace CameraCalib
{
    class detect_markers
    {
        
        static List<Marker> Detect(Mat mat)
        {
            //마커 이름, 좌표 찾기
            Point2f[][] _cornersource = new Point2f[][] { };
            int[] _corners = new int[] { };
            int[] _ids = new int[] { };
            Point2f[][] _rejectpts = new Point2f[][] { };
            List<Marker> detectedlist = new List<Marker>();
            using var dict = CvAruco.GetPredefinedDictionary(PredefinedDictionaryName.Dict4X4_100);
            var arucoParam = DetectorParameters.Create();
            CvAruco.DetectMarkers(mat, dict,out _cornersource, out _ids, arucoParam, out _rejectpts);
            try
            {
                int i = 0;
                while (true)
                {
                    
                    if(_ids[i]<5)
                    {
                        detectedlist.Add(new Marker());
                        detectedlist[i].id = _ids[i];
                        for (int a = 0; a < 4; a++)
                        {
                            detectedlist[i].Add(new Point2f());
                            detectedlist[i][a] = _cornersource[i][a];
                        }
                    }
                    if (_ids[i] >= 5) // 
                    {
                        Console.WriteLine("17 or 37 or 5detected");
                        //detectedlist.RemoveAt(detectedlist.Count - 1);
                    }
                    i++;

                }
            }
            catch(IndexOutOfRangeException e) 
            {
            }
            catch(ArgumentOutOfRangeException e)
            {
            }
            
            if(detectedlist.Count!=0)
            {
                if (detectedlist[0].Count == 0)
                {
                    detectedlist.RemoveAt(detectedlist.Count - 1);
                }
                else if (detectedlist[^1].Count == 0)
                {
                    detectedlist.RemoveAt(detectedlist.Count - 1);
                }
            }
            return detectedlist;
        }


        static void Main(string[] args)
        {
            using var dict = CvAruco.GetPredefinedDictionary(PredefinedDictionaryName.Dict4X4_100);
            var arucoParam = DetectorParameters.Create();
            try
            {

                string folderpath = "D:\\AFDSS\\CameraSolution\\20220314test(Recal)";
                Dataset dataset = new Dataset();
                dataset._Dataset(folderpath);
               
                var num_cams = dataset.get_num_cams();
                string output_file_name = folderpath + "\\aruco.detections";
                BinaryWriter output_file = new(new FileStream(output_file_name, FileMode.OpenOrCreate));
                Console.Write("num_cams 크기");
                Console.WriteLine(sizeof(long));
                output_file.Write(Convert.ToInt64(num_cams));
                
                const int min_detections_per_marker = 1;

                List<long> frame_nums = new();
                dataset.get_frame_nums(ref frame_nums);

                List<int> marker_ids = new List<int>();
                List<Mat> frames = new List<Mat>();
                frame_nums = frame_nums.Distinct().ToList();
                foreach(int frame_num in frame_nums)
                {
                    Console.WriteLine("frame num:" + frame_num.ToString());
                    var start = DateTime.Now;
                    dataset.get_frame(frame_num, ref frames);
                    Dictionary<int, int> markers_count = new Dictionary<int, int>();
                    List<Marker>[] cam_markers = new List<Marker>[num_cams];
                    for (int cam = 0; cam < num_cams; cam++)
                    {
                        Mat tmp_img = frames[cam];
                        cam_markers[cam] = Detect(tmp_img);
                        for(int m=0; m<cam_markers[cam].Count();m++)
                        {
                            
                            if(markers_count.ContainsKey(cam_markers[cam][m].id))
                            {
                                markers_count[cam_markers[cam][m].id]++;
                            }
                            else
                            {
                                markers_count[cam_markers[cam][m].id] = 1;
                            }
                        }
                    }
                    var end = DateTime.Now;
                    var d = end - start;

                    List<List<Marker>> new_markers = new();
                    ListExtras.Resize(new_markers, num_cams);
                    for(int i=0; i<new_markers.Count;i++)
                    {
                        new_markers[i] = new List<Marker>();
                    }
                   
                    for (int cam=0; cam<num_cams; cam++)
                    {
                        var markers = cam_markers[cam];
                        for (int m = 0; m < markers.Count; m++)
                        {
                            if(markers_count[markers[m].id]>=min_detections_per_marker)
                            {
                                new_markers[cam].Add(new Marker());
                                var tmp = markers[m];
                                new_markers[cam][m]=tmp;
                            }
                        }
                    }

                    for(int cam=0; cam<num_cams;cam++)
                    {
                        Mat tmp_img = frames[cam];
                        var new_markers_size = new_markers[cam].Count;
                        output_file.Write(Convert.ToInt64(new_markers_size));

                        for (int m = 0; m < new_markers_size; m++)
                        {
                            for (int i = 0; i < new_markers[cam].Count; i++)
                            {
                                marker_ids.Add(new_markers[cam][i].id);
                            }
                          
                            ArucoSerdes.serialize_marker(new_markers[cam][m], ref output_file);
                            
                            int j = new_markers[cam].Count;                            
                            Point2f[][] points = new Point2f[j][];
                            //int[] ids = new int[5] { 0, 1, 2, 3, 4 };
                            //new_Markers 반복문

                            //points = dataset.get_markercoordinate(new_markers[cam]); //그릴려면 쓰는코드
                            //CvAruco.DrawDetectedMarkers(tmp_img, points, marker_ids, Scalar.Red);


                            marker_ids.Clear();
                           
                        }
                        //Cv2.ImShow("cam_" + cam.ToString(), tmp_img);//그릴려면 쓰는코드
                        //Cv2.WaitKey(1);
                        
                    }


                }
                Console.Write("감지된 마커 번호 : ");
                foreach(int ids in marker_ids)
                {
                    Console.Write(ids.ToString() + "  ");
                }
            }
            catch(OpenCVException e)
            {

                //Console.WriteLine(e);
            }

        }
    }
}




