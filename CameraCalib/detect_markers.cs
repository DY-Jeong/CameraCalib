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
                    
                    detectedlist.Add(new Marker());
                    detectedlist[i].id = _ids[i];
                    for (int a = 0; a < 4; a++)
                    {
                        detectedlist[i].Add(new Point2f());
                        detectedlist[i][a] = _cornersource[i][a];
                    }
                    //detectedlist[i] = _cornersource[i].ToList();
                    //detectedlist[i] = _cornersource[i];


                    //if (_ids[i] != 17 & _ids[i] != 37)
                    //{
                    //}
                    /*if (_ids[i] == 17 ^ _ids[i] == 37 ^ _ids[i] == 5) // 
                    {
                        Console.WriteLine("17 or 37 or 5detected");
                        detectedlist.RemoveAt(detectedlist.Count - 1);
                        
                    }*/
                    if (_ids[i] >= 5) // 
                    {
                        Console.WriteLine("17 or 37 or 5detected");
                        detectedlist.RemoveAt(detectedlist.Count - 1);

                    }

                    i++;
                    //continue;
                    //i=0, ID가 17, 37일때 문제발생함

                    ///코드수정
                }
            }
            catch(IndexOutOfRangeException e) 
            {
                //detectedlist.RemoveAt(detectedlist.Count - 1);
                //Console.WriteLine("done");

            }
            catch(ArgumentOutOfRangeException e)
            {

            }
            if (detectedlist[0].Count==0)
            {
                detectedlist.RemoveAt(detectedlist.Count - 1);
            }
            else if(detectedlist[^1].Count == 0)
            {
                detectedlist.RemoveAt(detectedlist.Count - 1);
            }
            //detectedlist.RemoveAt(detectedlist.Count-1);
            return detectedlist;
        }


        static void Main(string[] args)
        {
            using var dict = CvAruco.GetPredefinedDictionary(PredefinedDictionaryName.Dict4X4_100);
            var arucoParam = DetectorParameters.Create();
            try
            {

                string folderpath = "D:\\20220328KICT\\Calibsdata\\Forcalib";
                //args[0];
                Dataset dataset = new Dataset();
                dataset._Dataset(folderpath);
               
                var num_cams = dataset.get_num_cams();
                string output_file_name = folderpath + "\\aruco.detections";
                //List<>
                BinaryWriter output_file = new(new FileStream(output_file_name, FileMode.OpenOrCreate));
                //var char_var = Convert.ToChar(Convert.ToInt64(num_cams));
                Console.Write("num_cams 크기");
                Console.WriteLine(sizeof(long));
                output_file.Write(Convert.ToInt64(num_cams));
                
                //for(int i=0; i<4; i++)
                //{
                //    output_file.Write(0);
                //}
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
                    //new_markers[0] = new List<Marker>();
                    //new_markers[1] = new List<Marker>();
                    //new_markers[2] = new List<Marker>();
                    //new_markers[3] = new List<Marker>();
                    //new_markers[4] = new List<Marker>();
                    //new_markers[5] = new List<Marker>();
                    //new_markers[6] = new List<Marker>();

                    //new_markers[3] = new List<Marker>();
                    //ListExtras.Resize(new_markers[0], 5);
                    //ListExtras.Resize(new_markers[1], 5);
                    //ListExtras.Resize(new_markers[2], 5);
                    //ListExtras.Resize(new_markers[3], 5);
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
                        //Convert.ToChar(Convert.ToInt64(new_markers_size));
                        //Console.Write("new_markers_size 크기");
                        //Console.WriteLine(sizeof(long));
                        output_file.Write(Convert.ToInt64(new_markers_size));
                        //for (int i = 0; i < 4; i++)
                        //{
                        //    output_file.Write(0);
                        //}

                        for (int m = 0; m < new_markers_size; m++)
                        {
                            //marker_ids.Add(new_markers[cam][m].id);
                            for (int i = 0; i < new_markers[cam].Count; i++)
                            {
                                //ids[i] = markers[i].id;
                                marker_ids.Add(new_markers[cam][i].id);
                            }
                          
                            ArucoSerdes.serialize_marker(new_markers[cam][m], ref output_file);
                            
                            int j = new_markers[cam].Count;                            
                            Point2f[][] points = new Point2f[j][];
                            //int[] ids = new int[5] { 0, 1, 2, 3, 4 };
                            //new_Markers 반복문

                            //points = dataset.get_markercoordinate(new_markers[cam]); //그릴려면 쓰는코드
                            //CvAruco.DrawDetectedMarkers(tmp_img, points, marker_ids, Scalar.Red);


                            //marker_ids = marker_ids.Distinct().ToList();


                            marker_ids.Clear();
                            //Array.Resize(ref ids, points.Length);

                            //foreach (List<Marker> markers in new_markers)
                            //{
                            //    //int[] ids = new int[j];
                            //    if (markers.Count==0)
                            //    { continue; }
                            //    //points = dataset.get_markercoordinate(markers);
                            //    //if (l==0)
                            //    //{
                            //    //    points = dataset.get_markercoordinate(markers);
                            //    //    //for (int i = 0; i < markers.Count; i++)
                            //    //    //{
                            //    //    //    //ids[i] = markers[i].id;
                            //    //    //    marker_ids.Add(markers[i].id);
                            //    //    //}

                            //    //}
                            //    //else
                            //    //{
                            //    //    var arr2 = dataset.get_markercoordinate(markers);
                            //    //    int arrOriginsize = points.Length;
                            //    //    Array.Resize<Point2f[]>(ref points, arrOriginsize + arr2.Length);
                            //    //    Array.Copy(arr2, 0, points,arrOriginsize, arr2.Length);
                            //    //    //for (int i = 0; i < markers.Count; i++)
                            //    //    //{
                            //    //    //    //ids.Append(markers[i].id);
                            //    //    //    marker_ids.Add(markers[i].id);
                            //    //    //}

                            //    //}
                            //    l++;


                            //}


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
















            //string folder_PATH = args[1];
            
            //string PATHsensor1 = "Sensorimage1"; //ref camera
            //string PATHsensor2 = "Sensorimage2";
            //string PATHsensor3 = "Sensorimage3";
            //string PATHsensor4 = "Sensorimage4";
            
            // set of marker information
            // marker count : 6
            // marker Ref number : 1
            // marker Ref coordinate : {0,0}, {0,0}, {0,0}, {0,0}
            // marker Size : is it matter?


            //test
            //using var src = ReadPicture(@"C:\Users\jewds\source\repos\CameraCalib\CameraCalib\MVI_2299.MP4_20210722_013547.970.jpg");
            //GetImageCoordinate(src);
            ////test

            //List<marker> detectedlist1, detectedlist2, detectedlist3, detectedlist4 = new List<marker>();

            //using var src1 = ReadPicture(PATHsensor1);
            //detectedlist1 = GetImageCoordinate(src1); //1번 카메라에서 촬영한 마커번호, 마커의 이미지좌표
            //using var src2 = ReadPicture(PATHsensor2);
            //detectedlist2 = GetImageCoordinate(src2); //2번 카메라에서 촬영한 마커번호, 마커의 이미지좌표
            //using var src3 = ReadPicture(PATHsensor3);
            //detectedlist3 = GetImageCoordinate(src3); //3번 카메라에서 촬영한 마커번호, 마커의 이미지좌표
            //using var src4 = ReadPicture(PATHsensor4);
            //detectedlist4 =  GetImageCoordinate(src4); //4번 카메라에서 촬영한 마커번호, 마커의 이미지좌표




        }
    }
}






//for (int k = 0; k <= 4; k++)
//{
//    
//}
//static Mat ReadPicture(string PATH)
//{
//    //요약:
//    //     constructs 2D matrix and fills it with the specified Scalar value.
//    //
//    // 매개 변수:
//    //   size:
//    //     2D array size: Size(cols, rows) . In the Size() constructor, the number of rows
//    //     and the number of columns go in the reverse order.
//    //
//    //   type:
//    //     Array type. Use MatType.CV_8UC1, ..., CV_64FC4 to create 1-4 channel matrices,
//    //     or CV_8UC(n), ..., CV_64FC(n) to create multi-channel (up to CV_CN_MAX channels)
//    //     matrices.
//    Mat src = new Mat(PATH);
//    return src;
//}

//static void Markerpose()
//{

//}

//static void TransformationBetC()
//{

//}

//static void TransformationBetm()
//{

//}

//static void TransformationBetmtoc()
//{

//}

//static double[] Reprojection()
//{
//    double[] val = new double[] { };
//    return val;
//}
//static double[] LMoptimization()
//{
//    double[] val = new double[] { };
//    return val;
//}
//static double[] ReprojectionError()
//{
//    double[] val = new double[] { };
//    return val;
//}