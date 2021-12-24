using UnityEngine;
using OpenCvSharp;
using OpenCvSharp.Aruco;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class AR1 : MonoBehaviour
{
    [SerializeField] private RawImage _renderer;

    int _width = 1920;
    int _height = 1080;
    int _fps = 30;

    private WebCamTexture _webcamTexture;
    private const PredefinedDictionaryName dictName = PredefinedDictionaryName.Dict6X6_250;
    private Dictionary ar_dict;
    private DetectorParameters detect_param;
    public GameObject cube;
    public Vector3 ObjectPoint;

    [SerializeField]
    public Vector3 calibration;
    [SerializeField]
    public float cali;
    private GameObject PointToCube;

    private void Start()
    {
        //  Webカメラ設定
        WebCamDevice[] devices = WebCamTexture.devices;
        _webcamTexture = new WebCamTexture(devices[0].name, this._width, this._height, this._fps);
        _webcamTexture.Play();

        PointToCube = GameObject.FindWithTag("Cube");
        
        cali = 1f;

    }
    void OnDestroy()
    {
        if (_webcamTexture != null)
        {
            if (_webcamTexture.isPlaying) _webcamTexture.Stop();
            _webcamTexture = null;
        }
    }

    private void Update()
    
        => Armaker(_webcamTexture);

        void Armaker(WebCamTexture tex)
        {

            // カメラをテクスチャに載せて表示させる
            // ARマーカーの設定と描画設定
            ar_dict = CvAruco.GetPredefinedDictionary(dictName);
            detect_param = DetectorParameters.Create();
            Mat cam_frame = OpenCvSharp.Unity.TextureToMat(tex);
            Mat grayMat = new Mat();

            Point2f[][] corners;  //ARマーカのカドの座標
            int[] ids;          //検出されたARマーカのID
            Point2f[][] reject_Points;
            
            Cv2.CvtColor(cam_frame, grayMat, ColorConversionCodes.BGR2GRAY);
            CvAruco.DetectMarkers(cam_frame, grayMat, ar_dict, out corners, out ids, detect_param, out reject_Points);

            if (ids.Length != 0)
            {
                List<Point2f> midllePoints = new List<Point2f>();
                CvAruco.DrawDetectedMarkers(cam_frame, corners, ids, new Scalar(0, 255, 0));


                //ARマーカーの定義 座標とマーカー番号
                var markers = Enumerable.Zip(ids, corners, (i, c) => new { i, c })
                                .ToDictionary(x => x.i, x => x.c)
                                .OrderBy(i => i.Key);


                int cnt = 0;
                foreach (var marker in markers)
                {
                    //マーカー個々の中心座標
                    var average_X = marker.Value.Average(p => p.X);
                    var average_Y = marker.Value.Average(p => p.Y);

                    // マーカーの中心座標を取得
                    midllePoints.Add(new Point2f(average_X, average_Y));

                    cnt++;
                }

                List<Point3f> center = new List<Point3f>();

                int cnt1 = 0;
                foreach (var Points in midllePoints)
                {
                    //  黄色い点マーカーとマーカーの間の中心点を算出
                    var average1_X = midllePoints[0].X + (midllePoints[1].X - midllePoints[0].X) / 2;
                    var average1_Y = midllePoints[0].Y + (midllePoints[1].Y - midllePoints[0].Y) / 2;
                    var average_Z = 0;

                center.Add(new Point3f(average1_X, average1_Y, average_Z));

                float x = center[0].X;
                float y = center[0].Y;
                float z = center[0].Z;

                Vector3 centerP = new Vector3(x, y, z);
                Debug.Log(centerP );


                cube.transform.localPosition = centerP;
               
                PointToCube.transform.localPosition = new Vector3(centerP.x - calibration.x * cali, (centerP.y - calibration.y) * -1f * cali, 0f - calibration.z * cali);

                cnt1++;

                }

                //マーカーの中心座標を描画/確立
                midllePoints.ForEach(mp => cam_frame.Circle(
                        (int)mp.X, (int)mp.Y, 1, new Scalar(0, 0, 255), 3, LineTypes.AntiAlias));


                // マーカー間の中心点を描画/確立
                center.ForEach(tc => cam_frame.Circle(
                     (int)tc.X, (int)tc.Y, 1, new Scalar(0, 255, 255), 5, LineTypes.AntiAlias));


            }
            // RawImageにカメラ画像を描画
            _renderer.texture = OpenCvSharp.Unity.MatToTexture(cam_frame);
        }
    }



