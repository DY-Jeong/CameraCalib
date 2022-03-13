using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;
using OpenCvSharp.Extensions;

namespace CameraCalib
{
    //public struct Camconfig
    //{
    //    public OpenCvSharp.Mat cam_mat { get; private set; }
    //    public OpenCvSharp.Mat dist_coeffs { get; private set; }
    //    public OpenCvSharp.Size image_size { get; private set; }
    //}
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
            //Console.Write("마커 ID 크기");
            //Console.WriteLine(sizeof(int));
            output.Write(marker.id);
            for (int c = 0; c < 4; c++)
            {
                //Console.Write("마커 X, Y 크기");
                //Console.WriteLine(sizeof(float));
                output.Write(marker[c].X);
                output.Write(marker[c].Y);
            }
        }
        static public void deserialize_marker(ref Marker marker, ref BinaryReader input)
        {
            //Console.Write("마커 ID 크기");
            //Console.WriteLine(sizeof(int));
            marker.id = Convert.ToInt32(input.ReadUInt64());
            marker.Resize(4);
            Point2f pnt = new();
            Marker mkr = new();
            for (int c = 0; c < 4; c++)
            {
                //Console.Write("마커 X, Y 크기");
                //Console.WriteLine(sizeof(float));
                pnt.X = input.ReadUInt64();
                pnt.Y = input.ReadUInt64();
                mkr.Add(pnt);
            }
            marker = mkr;
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
    
    public class CamConfig
    {
        public static Mat cam_mat;
        public static Mat dist_coeffs;
        public static Size image_size;
        
        public CamConfig()
        {
            cam_mat = new();
            dist_coeffs = new();
            image_size = new();
            //C# 변수는 메서드 안에서 해당 메서드의 로컬변수로 선언되거나, 혹은 클래스 안에서 클래스 내의 멤버들이 사용하는 전역적 변수(이를 필드(Field)라고 부름)로 선언될 수 있다. 로컬변수는 해당 메서드내에서만 사용되며, 메서드 호출이 끝나면 소멸된다. 반면 필드는 클래스의 객체가 살아있는 한 계속 존속하며 또한 다른 메서드들에서 필드를 참조할 수 있다. (주: 만약 필드가 정적 필드(static field)이면 클래스 Type이 처음으로 런타임에 의해 로드될 때 해당 Type 객체(타입 메타정보를 갖는 객체)에 생성되어 프로그램이 종료될 때까지 유지된다).
            //로컬변수는 기본값을 할당받지 못하기 때문에 반드시 사용 전에 값을 할당해야 하는 반면, 필드는 값을 할당하지 않으면, 해당 타입의 기본값이 자동으로 할당된다.예를 들어, int 타입의 필드인 경우 기본값 0 이 할당된다.
        }

        public void Camonfig(Mat c_mat, Mat d_coeffs, Size im_size)
        {
            setCamMat(c_mat);
            setDistCoeffs(d_coeffs);
            setImageSize(im_size);
        }
        public void setCamMat(Mat c_mat)
        {
            if(c_mat.Rows != 3 || c_mat.Cols !=3)
            {
                Console.WriteLine("The size of the input camera matrix must be 4x4!");
                return;
            }
            c_mat.ConvertTo(cam_mat, MatType.CV_32FC1);
        }

        public void setDistCoeffs(Mat d_coeffs)
        {
            if(d_coeffs.Rows != 1&& d_coeffs.Cols !=1)
            {
                Console.WriteLine("The size of the input matrix for distorsion coefficients must be 1xN nor Nx1!");
            }
            dist_coeffs = Mat.Zeros(5, 1, MatType.CV_64FC1);
            d_coeffs.ConvertTo(d_coeffs, MatType.CV_64FC1);
            for(int i=0; i<d_coeffs.Total()&& i<5;i++)
            {
                dist_coeffs.At<double>(i) = d_coeffs.At<double>(i);
            }
            //여까지 함
        }
        public void setImageSize(Size im_size)
        {
            image_size = im_size;
        }
        public Mat getCamMat()
        {
            return cam_mat;
        }
        public Mat getDistCoeffs()
        {
            return dist_coeffs;
        }
        public Size getImageSize()
        {
            return image_size;
        }
        public bool read_from_file(string path)
        {
            FileStorage file = new FileStorage();
            file.Open(path, FileStorage.Modes.Read);
            
            if(!file.IsOpened())
            {
                return false;
            }

            //if (!file["image_height"].IsNone)
            //{
            //    return false;
            //}
            //else
            //{
            //    //코드 정상인지 확인필요
            //    image_size.Height=Convert.ToInt32(file["image_height"]);
            //}

            if (!file["image_width"].IsNone&& file["image_height"].IsNone)
            {
                return false;
            }
            else
            {
                //코드 정상인지 확인필요
                image_size = file["image_width"].ReadSize();
            }

            if (!file["camera_matrix"].IsNone)
            {
                return false;
            }
            else
            {
                //코드 정상인지 확인필요
                cam_mat = file["camera_matrix"].ReadMat();
            }

            if (!file["distortion_coefficients"].IsNone)
            {
                return false;
            }
            else
            {
                //코드 정상인지 확인필요
                dist_coeffs = file["distortion_coefficients"].ReadMat();
            }

            return true;

        }
        static List<CamConfig> read_cam_configs(string folder_path)
        {
            List<CamConfig> camConfigs = new List<CamConfig>();
            var dirs_list = Directory.GetDirectories(folder_path).ToList(); 
            List<string> possible_extensions = new List<string> { "xml", "yml", "yaml" };
            foreach(string dir_name in dirs_list)
            {
                foreach(string ext in possible_extensions)
                {
                    string file_path = folder_path + "/" + dir_name + "/" + "calib." + ext;
                    CamConfig cc = new CamConfig();
                    if(cc.read_from_file(file_path))
                    {
                        camConfigs.Add(cc);
                    }
                }
            }
            return camConfigs;
        }
    }


    public class initializer
    {
        public void Initializer(double marker_s, ref List<CamConfig> cam_cs, ref HashSet<int> excluded_cs)
        {
            excluded_cams = excluded_cs;
            marker_size = marker_s;
            cam_configs = cam_cs;
        }
        public void Initializer(ref List<List<List<Marker>>> dts, double marker_s, ref List<CamConfig> cam_cs, HashSet<int> excluded_cs)
        {
            excluded_cams = excluded_cs;
            marker_size = marker_s;
            cam_configs = cam_cs;
            detections = dts;
            obtain_pose_estimations();
            init_transforms();

        }
        public static List<List<List<Marker>>> read_detections_file(string path, List<int> subseqs)
        {
            BinaryReader detections_file = new(new FileStream(path, FileMode.Open), Encoding.UTF8);
            //if(!detections_file.)

            List<List<List<Marker>>> all_markers = new List<List<List<Marker>>>();
            Int64 num_cams;
            num_cams = detections_file.ReadInt64();
            if(num_cams.Equals(new Int64()))
            {
                for(int frame_num=0; ;frame_num++)
                {
                    List<List<Marker>> frame_markers = new List<List<Marker>>();
                    bool end_of_data = false;

                    for (int cam = 0; cam < num_cams; cam++)
                    {
                        Int64 num_cam_markers = new();
                        num_cam_markers=detections_file.ReadInt64();
                        if(!num_cam_markers.Equals(new Int64()))
                        {
                            end_of_data = true;
                            break;
                        }
                        frame_markers[cam].Resize(Convert.ToInt32(num_cam_markers));

                        for(int m=0; m<num_cam_markers;m++)
                        {
                            Marker mkr = new Marker();
                            ArucoSerdes.deserialize_marker(ref mkr, ref detections_file);
                            frame_markers[cam][m] = mkr;
                        }
                    }
                    if(end_of_data)
                    {
                        break;
                    }
                    all_markers.Add(frame_markers);
                }

            }
            if(subseqs.Count!=0)
            {
                int prev_last_frame = -1;
                for(int i=0; i+1<subseqs.Count;i+=2)
                {
                    int first_frame = subseqs[i];
                    for(int f = prev_last_frame+1;f<first_frame;f++)
                    {
                        for(int c=0; c<num_cams;c++)
                        {
                            all_markers[f][c].Clear();
                        }
                    }
                    prev_last_frame = subseqs[i + 1];
                }
            }
            return all_markers;

        }
        public class Config
        {
            public bool init_cams = true;
            public bool init_markers = true;
            public bool init_relative_poses = true;
        }
        public HashSet<int> get_marker_ids()
        {
            return marker_ids;
        }
        public HashSet<int> get_cam_ids()
        {
            return cam_ids;
        }
        public int get_root_cam()
        {
            return root_cam;
        }
        public int get_root_marker()
        {
            return root_marker;
        }
        public Dictionary<int,OpenCvSharp.Mat> get_transforms_to_root_cam()
        {
            return transforms_to_root_cam;
        }
        public Dictionary<int, OpenCvSharp.Mat> get_transforms_to_root_marker()
        {
            return transforms_to_root_marker;
        }
        public void set_transforms_to_root_cam(ref Dictionary<int,OpenCvSharp.Mat> ttrc)
        {
            transforms_to_root_cam = ttrc;
        }
        public void set_transforms_to_root_marker(ref Dictionary<int,OpenCvSharp.Mat> ttrm)
        {
            transforms_to_root_marker = ttrm;
        }
        public void set_detections(ref List<List<List<Marker>>> dts)
        {
            detections = dts;
        }
        public void obtain_pose_estimations()
        {
            frame_cam_markers.Clear();
            frame_poses_cam.Clear();
            frame_poses_marker.Clear();
            for (int frame_num = 0; frame_num < detections.Count; frame_num++)
            {
                Dictionary<int, Dictionary<int, List<KeyValuePair<Mat, double>>>> pose_estimations_marker = new();
                Dictionary<int, Dictionary<int, List<KeyValuePair<Mat, double>>>> pose_estimations_cam = new();
                //Find all of the solutions for all of markers in each camera

                //first check if the frame has more than one detections
                int num_detections = 0;
                for (int cam = 0; cam < detections[frame_num].Count; cam++)
                {
                    if (!excluded_cams.Contains(cam)) //(excluded_cams.count(cam)==0
                        num_detections += detections[frame_num][cam].Count;
                }


                if (!(num_detections >= min_detections))
                { continue; }

                //loop over all possible cameras
                for (int cam = 0; cam < detections[frame_num].Count; cam++)
                {
                    if (!excluded_cams.Contains(cam))
                    {
                        int num_cam_markers = detections[frame_num][cam].Count;

                        if (num_cam_markers < 1)
                        { continue; }

                        cam_ids.Add(cam);

                        var cam_markers = frame_cam_markers[frame_num][cam];

                        for (int m = 0; m < num_cam_markers; m++)
                        {
                            Marker marker = detections[frame_num][cam][m];

                            marker_ids.Add(marker.id);

                            cam_markers.Add(marker);

                            List<KeyValuePair<Mat, double>> solutions = new();
                            Mat rvec = new();
                            Mat tvec = new();

                            Cv2.SolvePnP(marker_size, marker.m, cam_configs[cam].getCamMat(), cam_configs[cam].getDistCoeffs(), rvec, tvec, false, SolvePnPFlags);
                            

                            solutions[0].Key.ConvertTo(solutions[0].Key, MatType.CV_64FC1);
                            pose_estimations_cam[marker.id][cam].Add(solutions[0]);
                            pose_estimations_marker[cam][marker.id].Add(solutions[0]);


                            if (solutions[1].Value / solutions[0].Value < threshold)
                            {
                                solutions[1].Key.ConvertTo(solutions[1].Key, MatType.CV_64FC1);
                                pose_estimations_cam[marker.id][cam].Add(solutions[1]);
                                pose_estimations_marker[cam][marker.id].Add(solutions[1]);
                            }
                        }
                    }
                }

                frame_poses_cam[frame_num] = pose_estimations_cam;
                frame_poses_marker[frame_num] = pose_estimations_marker;
            }
        }


        public void init_object_transforms()
        {
            if (!config.init_relative_poses)
                return;
            foreach (var it in frame_poses_cam)
            {
                int frame = it.Key;
                List<Tuple<Mat,Mat,Mat, double>> transformation_set = new();
                Dictionary<int, Dictionary<int, List<KeyValuePair<Mat, double>>>> tmpValit = new();
                
                fill_transformation_set(ref tmpValit, ref transforms_to_root_cam, ref transforms_to_root_marker, ref transformation_set);
                frame_poses_cam.Remove(it.Key);
                frame_poses_cam.Add(it.Key, tmpValit);
                
                double min_err = 0.0;
                int best_transform_index = find_best_transformation(marker_size, ref transformation_set,ref min_err);
                if (best_transform_index >= 0)
                { object_transforms[frame] = transformation_set[best_transform_index].Item1; }
            }
        }
        public Dictionary<int, OpenCvSharp.Mat> get_object_transforms()
        {
            return object_transforms;
        }
        public double get_marker_size()
        {
            return marker_size;
        }
        public Dictionary<int, Dictionary<int, List<Marker>>> get_frame_cam_markers()
        {
            return frame_cam_markers;
        }
        public List<CamConfig> get_cam_configs()
        {
            return cam_configs;
        }
        public struct Node
        {
            public int id { get; set; }
            public double distance { get; set; }
            public int parent { get; set; }
            public static bool operator <(Node n, Node n_)
            {
                if (n.id < n_.id)
                {
                    return true;

                }
                return false;
            }
            public static bool operator >(Node n, Node n_)
            {
                if (n.id > n_.id)
                {
                    return true;

                }
                return false;
            }
        }


        private Config config;
        private List<List<List<Marker>>> detections;
        private enum transform_type 
        { camera, marker};
        private Dictionary<int, Dictionary<int, List<Tuple<Mat, Mat, Mat, double>>>> transformation_sets_cam, transformation_sets_marker;
        private int root_cam, root_marker, min_detections = 2;
        private double marker_size, threshold = 2.0;
        private HashSet<int> marker_ids, cam_ids, excluded_cams;
        private Dictionary<int, Dictionary<int, Dictionary<int, List<KeyValuePair<Mat, double>>>>> frame_poses_cam, frame_poses_marker;
        private Dictionary<int, Dictionary<int, List<Marker>>> frame_cam_markers;
        private List<CamConfig> cam_configs;
        private Dictionary<int, Mat> transforms_to_root_cam, transforms_to_root_marker, object_transforms;
        private void fill_transformation_set(ref Dictionary<int, Dictionary<int, List<KeyValuePair<Mat, double>>>> pose_estimations, ref Dictionary<int, Mat> transforms_to_root_cam, ref Dictionary<int, Mat> transforms_to_root_marker, ref List<Tuple<Mat, Mat, Mat, double>> transformation_set)
        {
            //for(var marker_it= pose_estimations.)
            foreach (var marker_it in pose_estimations)
            {
                int marker_id = marker_it.Key;
                var marker_cams = marker_it.Value;
                foreach(var cam_it in marker_cams)
                {
                    int cam_id = cam_it.Key;
                    var pose_ests = cam_it.Value;
                    Mat T_mr, T_rm, T_cr, T_rc, T_tmp = new Mat() ;
                    
                    if (transforms_to_root_marker[marker_id] != transforms_to_root_marker.Values.Last())
                    {
                        T_mr = transforms_to_root_marker[marker_id];
                        T_rm = T_mr.Inv();
                    }
                    else
                    {
                        T_mr = Mat.Eye(4, 4, MatType.CV_64FC1);
                        T_rm = Mat.Eye(4, 4, MatType.CV_64FC1);
                    }

                    if(transforms_to_root_cam[cam_id]!=transforms_to_root_cam.Values.Last())
                    {
                        T_cr = transforms_to_root_cam[cam_id];
                        T_rc = T_cr.Inv();

                    }
                    else
                    {
                        T_rc = Mat.Eye(4, 4, MatType.CV_64FC1);
                        T_cr = Mat.Eye(4, 4, MatType.CV_64FC1);
                    }

                    for(Int64 i =0; i<pose_ests.Count;i++)
                    {
                        Mat T_mc = new Mat();
                        pose_ests[Convert.ToInt32(i)].Key.ConvertTo(T_mc, MatType.CV_64FC1);
                        Mat T_cm = T_mc.Inv();
                        double error = pose_ests[Convert.ToInt32(i)].Value;
                        var tmpTuple = new Tuple<Mat, Mat, Mat, double>(T_cr*T_mc*T_rm, T_mr*T_cm, T_rc, error);
                        transformation_set.Add(tmpTuple);
                    }
                }
            }

        }
        private void fill_transformation_sets(transform_type tt, ref Dictionary<int, Dictionary<int, List<KeyValuePair<Mat,double>>>>  pose_estimations, ref Dictionary<int, Dictionary<int, List<Tuple<Mat, Mat, Mat, double>>>> transformation_sets )
        {
            foreach(var it in pose_estimations)
            {
                var objects = it.Value;
                if(objects.Count>1)
                {
                    foreach(var it1 in objects)
                    {
                        int id1 = it1.Key;
                        var poses_1 = it1.Value;
                        for(Int64 i =0; i<poses_1.Count;i++)
                        {
                            double error1 = poses_1[Convert.ToInt32(i)].Value;
                            var first = true;
                            foreach(var it2 in objects )
                            {
                                if(first==true)
                                {
                                    first = false;
                                    continue;
                                }
                                int id2 = it2.Key;
                                var poses_2 = it2.Value;
                                for(Int64 j=0; j<poses_2.Count;j++)
                                {
                                    double error2 = poses_2[Convert.ToInt32(j)].Value;
                                    switch(tt)
                                    {
                                        case transform_type.camera:
                                            var addvalc = new Tuple<Mat, Mat, Mat, double>(poses_2[Convert.ToInt32(j)].Key * poses_1[Convert.ToInt32(i)].Key.Inv(), poses_1[Convert.ToInt32(i)].Key, poses_2[Convert.ToInt32(j)].Key.Inv(), error1 * error2);
                                            transformation_sets[id1][id2].Add(addvalc);
                                            break;
                                        case transform_type.marker:
                                            var addvalm = new Tuple<Mat, Mat, Mat, double>(poses_2[Convert.ToInt32(j)].Key.Inv() * poses_1[Convert.ToInt32(i)].Key, poses_1[Convert.ToInt32(i)].Key.Inv(), poses_2[Convert.ToInt32(j)].Key, error1 * error2);
                                            transformation_sets[id1][id2].Add(addvalm);
                                            break;


                                    }
                                }

                            }
                        }
                    }
                }
            }
        }
        private int find_best_transformation_min(double marker_size, ref List<Tuple<Mat, Mat,Mat, double>> solutions, ref double min_error)
        {
            var bestmin = new KeyValuePair<int, double>(-1, double.MaxValue);
            for(Int64 i=0; i<solutions.Count;i++)
            {
                double v = solutions[Convert.ToInt32(i)].Item4;
                if (v < bestmin.Value)
                {
                    bestmin = new KeyValuePair<int, double>(Convert.ToInt32(i), v);
                }
            }
            min_error = bestmin.Value;
            return bestmin.Key;

        }
        private int find_best_transformation(double marker_size, ref List<Tuple<Mat,Mat,Mat, double>> solutions, ref double weight)
        {
            double half_msize = marker_size / 2;
            Mat points = new Mat(4, 4, MatType.CV_64FC1);
            points.At<double>(0, 0) = -half_msize;
            points.At<double>(1, 0) = half_msize;
            points.At<double>(2, 0) = 0;
            points.At<double>(3, 0) = 1;
            points.At<double>(0, 1) = half_msize;
            points.At<double>(1, 1) = half_msize;
            points.At<double>(2, 1) = 0;
            points.At<double>(3, 1) = 1;
            points.At<double>(0, 2) = half_msize;
            points.At<double>(1, 2) = -half_msize;
            points.At<double>(2, 2) = 0;
            points.At<double>(3, 2) = 1;
            points.At<double>(0, 3) = -half_msize;
            points.At<double>(1, 3) = -half_msize;
            points.At<double>(2, 3) = 0;
            points.At<double>(3, 3) = 1;

            double min_error = double.MaxValue;
            int min_index = -1;

            for(Int64 i=0; i<solutions.Count;i++)
            {
                Mat T = solutions[Convert.ToInt32(i)].Item1;
                double curr_error = 0;
                for(Int64 j=0; j<solutions.Count;j++)
                {
                    Mat T1_inv = solutions[Convert.ToInt32(i)].Item2;
                    Mat T2_inv = solutions[Convert.ToInt32(i)].Item3;
                    Mat p2 = T2_inv * T * T1_inv * points;
                    Mat diff = points - p2;
                    var rn = new OpenCvSharp.Range(0, 3);
                    diff = diff.RowRange(rn);
                    Mat diff_sq = diff.Mul(diff);
                    Cv2.Reduce(diff_sq, diff_sq, 0, ReduceTypes.Sum, MatType.CV_64FC1);
                    Cv2.Sqrt(diff_sq, diff_sq);
                    curr_error += Cv2.Sum(diff_sq)[0];
                }
                if(curr_error<min_error)
                {
                    min_index = Convert.ToInt32(i);
                    min_error = curr_error;
                    weight = min_error;

                }
            }
            return min_index;
        }
        //여기까지함
        private void find_best_transformations(double marker_size, ref Dictionary<int, Dictionary<int, List<Tuple<Mat, Mat, Mat, double>>>> transformation_sets, ref Dictionary<int, Dictionary<int, KeyValuePair<Mat, double>>> best_transformations)
        { 
            foreach(var it1 in transformation_sets)
            {
                int id1 = it1.Key;
                Console.Write("id1 : ");
                Console.WriteLine(id1);
               
                var it2tmp = it1.Value;
                foreach (var it2 in it2tmp)
                {
                    //if (first == true)
                    //{
                    //    first = false;
                    //    continue;
                    //}
                    int id2 = it2.Key;
                    Console.Write("id2 : ");
                    Console.WriteLine(id2);
                    List<Tuple<Mat, Mat, Mat, double>> solutions = new();
                    solutions = it2.Value;

                    double min_error = .0;
                    int min_index = find_best_transformation(marker_size, ref solutions, ref min_error);
                    double reproj_error = min_error;
                    best_transformations[id1][id2] = new KeyValuePair<Mat, double>(solutions[min_index].Item1, reproj_error);
                }
            }
        }
        //    double get_reprojection_error(double marker_size, aruco::Marker marker, cv::Mat r, cv::Mat t, cv::Mat cam_mat, cv::Mat dist_coeffs);
        private void make_mst(int starting_node, HashSet<int> node_ids, ref Dictionary<int, Dictionary<int, KeyValuePair<Mat, double>>> adjacency, ref Dictionary<int, HashSet<int>> children)
        {
            HashSet<Node> nodes_outside_tree = new HashSet<Node>();
            foreach(var it in node_ids)
            {
                Node n = new Node();
                n.id = it;
                n.parent = -1;
                if(n.id == starting_node)
                {
                    n.distance = 0;
                }
                else
                {
                    n.distance = double.MaxValue;
                }
                nodes_outside_tree.Add(n);
            }

            while(nodes_outside_tree.Count==0) //
            {//find the undetermined node with the smallest distance
                var min_node_it = nodes_outside_tree.First();
                foreach(var node_it in nodes_outside_tree)
                {
                    if(node_it.distance<min_node_it.distance)
                    {
                        min_node_it = node_it;
                    }
                }
                //update the distance for the neighbours of the node with the minimum distance
                foreach (var node_it in nodes_outside_tree)
                {
                    Mat transform = new();
                    double error = double.MinValue;

                    if(min_node_it.id<node_it.id)
                    {
                        if(adjacency[node_it.id]!=adjacency.Values.Last())
                        {
                            if(adjacency[min_node_it.id][node_it.id].Value != adjacency[min_node_it.id].Values.Last().Value)
                            {
                                transform = adjacency[min_node_it.id][node_it.id].Key;
                                error = adjacency[min_node_it.id][node_it.id].Value;
                            }
                        }
                    }
                    else if(adjacency[node_it.id].Values != adjacency.Values.Last().Values)
                    {
                        if (adjacency[node_it.id][min_node_it.id].Value != adjacency[node_it.id].Values.Last().Value)
                        {
                            transform = adjacency[node_it.id][min_node_it.id].Key;
                            error = adjacency[node_it.id][min_node_it.id].Value;
                        }
                    }
                    if(!transform.Empty())
                    {
                        if(error < node_it.distance)
                        {
                            nodes_outside_tree.Remove(node_it);
                            Node ndt = node_it;
                            ndt.distance = error;
                            
                            if(node_it.parent!=-1)
                            {
                                children[node_it.parent].Remove(node_it.id);
                            }
                            children[min_node_it.id].Add(node_it.id);
                            ndt.parent = min_node_it.id;
                            nodes_outside_tree.Add(ndt);
                        }
                    }
                }
                nodes_outside_tree.Remove(min_node_it);
                //주소참조 확인해보기
            }
        }
        private void find_transforms_to_root(int root_node, ref Dictionary<int, HashSet<int>> children, ref Dictionary<int, Dictionary<int, KeyValuePair<Mat, double>>> best_transforms, ref Dictionary<int, Mat> transforms_to_root)
        {
            transforms_to_root[root_node] = Mat.Eye(4, 4, MatType.CV_64FC1);
            Queue<int> q = new Queue<int>();
            q.Enqueue(root_node);
            Console.WriteLine("finding best transforms");
            while(q.Count!=0)//q가 비어있지 않을때까지
            {
                if(children[q.First()]!=children.Last().Value)
                {
                    foreach(var child_it in children[q.First()])
                    {
                        int child_id = child_it;
                        int parent_id = q.First();

                        if (child_id < parent_id) { transforms_to_root_marker[child_id] = best_transforms[child_id][parent_id].Key.Clone(); }
                        else { transforms_to_root_marker[child_id] = best_transforms[child_id][parent_id].Key.Inv(); }
                        if (parent_id != root_node) { transforms_to_root_marker[child_id] = transforms_to_root[parent_id] * transforms_to_root[child_id]; }
                        q.Enqueue(child_id);
                    }
                    q.Dequeue();
                }
            }
        }
        private void init_transforms_cam()
        {
            if (config.init_cams) { return; }
            
            for(int frame_num =0; frame_num<detections.Count;frame_num++)
            {
                var poses_at_frame_num = frame_poses_marker[frame_num];
                fill_transformation_sets(transform_type.marker, ref poses_at_frame_num, ref transformation_sets_marker);
                frame_poses_marker[frame_num] = poses_at_frame_num;
            }
            Dictionary<int, Dictionary<int, KeyValuePair<Mat, double>>> best_transforms_cam = new();
            find_best_transformations(marker_size, ref transformation_sets_cam, ref best_transforms_cam);
            Dictionary<int, HashSet<int>> cam_tree = new();
            root_cam = cam_ids.First();
            make_mst(root_cam, cam_ids, ref best_transforms_cam, ref cam_tree);
            Console.WriteLine("Finding the transformations to the reference camera..");
            find_transforms_to_root(root_cam, ref cam_tree, ref best_transforms_cam, ref transforms_to_root_cam);
        }
        private void init_transforms_marker()
        {
            if (!config.init_markers)
            { return; }
            for (int frame_num = 0; frame_num < detections.Count; frame_num++)
            {
                var poses_at_frame_num = frame_poses_marker[frame_num];
                fill_transformation_sets(transform_type.marker, ref poses_at_frame_num, ref transformation_sets_marker);
                frame_poses_marker[frame_num] = poses_at_frame_num;
            }

            Dictionary<int, Dictionary<int, KeyValuePair<Mat, double>>> best_transforms_marker = new();
            find_best_transformations(marker_size, ref transformation_sets_marker, ref best_transforms_marker);
            Dictionary<int, HashSet<int>> marker_tree = new();
            root_marker = marker_ids.First();
            make_mst(root_marker, marker_ids, ref best_transforms_marker, ref marker_tree);

            Console.WriteLine("Finding the transformations to the reference marker..");
            find_transforms_to_root(root_marker,ref marker_tree, ref best_transforms_marker, ref transforms_to_root_marker);

        }
        private void init_transforms()
        {
            Console.WriteLine("Finding the best transformations..");
            init_transforms_cam();
            init_transforms_marker();
            init_object_transforms();
        }



    }
}
