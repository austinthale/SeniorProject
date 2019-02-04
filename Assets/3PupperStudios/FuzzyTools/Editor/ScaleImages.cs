using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace FuzzyTools
{
    public class ResizeImages : EditorWindow
    {
        private static List<Texture2D> _selection = new List<Texture2D>();
        
        
        private const int MinSize = 32;
        private const int MaxSize = 8192;
        private const string ImageRatio = "Image(s) Ratio";
        private const string ImageRes = "New Resolution";
        private const string ImageResY = "New Height Resolution";
        private const string ImageResX = "New Width Resolution";
        private const string ResizeBttn = "Resize Image(s)";
        //private const string MaintainRatio = "Maintain Aspect Ratio";
        private const string AddToList = "Add Selection";
        private const string ClearList = "Clear List";
        private const string SampleTexture = "Sample Texture";

        private static List<Texture2D> _images = new List<Texture2D>();

        private static int _newScale = 2048;
        private static int _xScale = 2048;
        private static int _yScale = 2048;
        private static int _chosenRes = 6;
        private static int _radioSelection = 0;
        private static int _ratioOption = 0;
        private static int _whichResize = 0;
        private static int _sampleTexCount = 0;

        private static bool _keepRatio = false;

        private static Vector2 _scrollPos = Vector2.zero;
        private static Texture2D _sampleTex;

        private static readonly Vector2 MinWindowSize = new Vector2(400, 240);

        //private static readonly GUIStyle WarningFont = new GUIStyle ();

        //private static ImageType _imageType = ImageType.PNG;

        private static readonly string[] RatioOptions = {"1:1", "Other"};
        private static readonly string[] ResizeOptions = {"Rigid Resize", "Flexible Resize"};
        //private static readonly string[] WhichResize = {"Scale Universally", "Scale Individually"};

        private static readonly string[] ImageSizes =
        {
            "32",
            "64",
            "128",
            "256",
            "512",
            "1024",
            "2048",
            "4096",
            "8192"
        };

        private static readonly string[] SupportedFileTypes =
        {
            "PNG",
            "png",
            "JPG",
            "jpg",
            "EXR",
            "exr"
        };

        private static readonly TextureFormat[] SupportedFormats =
        {
            TextureFormat.ARGB32,
            TextureFormat.RGBA32,
            TextureFormat.RGB24,
            TextureFormat.Alpha8,
            TextureFormat.RGFloat,
            TextureFormat.RGBAFloat,
            TextureFormat.RFloat,
            TextureFormat.RGB9e5Float
        };

        [MenuItem("Assets/FuzzyTools/Scale Image(s)")]
        private static void Init()
        {
            _images.Clear();
            AddSelectionToTextures();
            _radioSelection = 0;
            _ratioOption = 0;
            _whichResize = 0;
            var window = GetWindow(typeof(ResizeImages), true, "Scale Image(s)");
            window.minSize = MinWindowSize;
        }


        private void OnGUI()
        {
            var xScale = 0;
            var yScale = 0;
            EditorGUILayout.BeginHorizontal("box");
            GUILayout.Label(ImageRatio);
            _ratioOption = GUILayout.SelectionGrid(_ratioOption, RatioOptions, RatioOptions.Length);

            EditorGUILayout.EndHorizontal();
            if (_ratioOption == 0)
            {
                _radioSelection = GUILayout.SelectionGrid(_radioSelection, ResizeOptions, ResizeOptions.Length,
                    EditorStyles.radioButton);
                if (_radioSelection == 0)
                {
                    _chosenRes = EditorGUILayout.Popup(ImageRes, _chosenRes, ImageSizes);
                    int.TryParse(ImageSizes[_chosenRes], out _newScale);
                    xScale = _chosenRes;
                    yScale = _chosenRes;
                }
                else
                {
                    _newScale = EditorGUILayout.IntSlider(ImageRes, _newScale, MinSize, MaxSize);
                    xScale = _chosenRes;
                    yScale = _chosenRes;
                }
            }
            else
            {
                //EditorGUILayout.BeginHorizontal();

                //_whichResize = GUILayout.SelectionGrid(_whichResize, WhichResize, WhichResize.Length,
                //    EditorStyles.radioButton);
                EditorGUILayout.BeginHorizontal();

                //_keepRatio = GUILayout.Toggle(_keepRatio, MaintainRatio);
                if (_keepRatio && _whichResize == 0 && _images.Count > 0)
                {
                    _sampleTexCount = EditorGUILayout.Popup(SampleTexture, _sampleTexCount, StringArray(_images));
                    _sampleTex = _images[_sampleTexCount];
                }

                EditorGUILayout.EndHorizontal();
                if (_whichResize == 0)
                {
                    //xScale = _xScale;
                    //yScale = _yScale;


                    _yScale = EditorGUILayout.IntSlider(ImageResY, _yScale, MinSize, MaxSize);
                    _xScale = EditorGUILayout.IntSlider(ImageResX, _xScale, MinSize, MaxSize);
                    /*if(_keepRatio)
                    {
                        if(xScale!=_xScale)
                        {
                            KeepRatio(false);
                        }
                        else if (yScale != _yScale)
                        {
                            KeepRatio(true);
                        }
                        
                    }*/

                    xScale = _xScale;
                    yScale = _yScale;
                }
                //EditorGUILayout.EndHorizontal();




                //_xScale = EditorGUILayout.IntSlider(ImageRes, _xScale, MinSize, MaxSize);
                //_yScale = EditorGUILayout.IntSlider(ImageRes, _yScale, MinSize, MaxSize);
                //if(_keepRatio)
            }
            //_keepRatio = EditorGUILayout.Toggle(MaintainRatio, _keepRatio);




            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(AddToList))
            {
                AddSelectionToTextures();
            }

            if (GUILayout.Button(ClearList))
            {
                _images.Clear();
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.TextArea("", GUI.skin.horizontalSlider);
            if (_images.Count > 0)
            {
                //var style = new GUIStyle(GUI.skin.label);
                //style.alignment = TextAnchor.UpperCenter;
                //style.fixedWidth = 70;

                //var result = (Texture2D)EditorGUILayout.ObjectField(texture, typeof(Texture2D), false, GUILayout.Width(70), GUILayout.Height(70));
                EditorGUILayout.BeginVertical("box");
                //var count = 0;


                _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
                for (var i = 0; i < _images.Count; i++)
                {
                    if (_images[i] == null)
                    {
                        _images.Remove(_images[i]);
                        i--;
                        continue;
                    }

                    EditorGUILayout.BeginHorizontal("box");

                    var img = _images[i];
                    var myLabel = img.name + "\n \n       " + img.height + "x" + img.width;
                    GUILayout.Label(myLabel);
                    //EditorGUILayout.LabelField(img.name, img.height.ToString(), style);
                    //EditorGUILayout.BeginVertical();
                    var textDimensions = GUI.skin.label.CalcSize(new GUIContent(""));
                    EditorGUIUtility.labelWidth = textDimensions.x;

                    var texPath = AssetDatabase.GetAssetPath(img);
                    var pos = texPath.LastIndexOf(".") + 1;
                    var fileType = texPath.Substring(pos, texPath.Length - pos);

                    img = (Texture2D) EditorGUILayout.ObjectField("", img, typeof(Texture2D),
                        true); /*(Texture2D) EditorGUILayout.ObjectField(img, typeof(Texture2D), false, GUILayout.Width(70), GUILayout.Height(70));*/
                    //EditorGUILayout.EndVertical();
                    _images[i] = img;
                    EditorGUILayout.EndHorizontal();

                }

                EditorGUILayout.EndVertical();
                EditorGUILayout.EndScrollView();
            }

            EditorGUI.BeginDisabledGroup(_images.Count == 0);
            if (GUILayout.Button(ResizeBttn))
            {
                Resize(xScale, yScale);
            }

            EditorGUI.EndDisabledGroup();
        }

        private static void AddSelectionToTextures()
        {
            var objects = Selection.objects;
            foreach (var obj in objects)
            {
                var image = obj as Texture2D;
                if (image == null) continue;

                var texPath = AssetDatabase.GetAssetPath(obj);
                var pos = texPath.LastIndexOf(".") + 1;
                var fileType = texPath.Substring(pos, texPath.Length - pos);
                if (!SupportedFileTypes.Contains(fileType)) continue;

                _images.AddIfDoesNotContain(image);

            }
        }

        private static string[] StringArray(IEnumerable<Texture2D> input)
        {
            var output = new string[input.Count()];
            var i = 0;
            foreach (var obj in input)
            {
                output[i] = obj.name;
                i++;
            }

            return output;
        }

        /*private static void KeepRatio(bool changingY) ////TODO FINISH THIS
        {
            var newWidth = _yScale;
            var newHeight = _xScale;


            if (_sampleTex == null)
            {
                if (!changingY)
                {
                    _yScale = _xScale;
                }
                else
                {
                    _xScale = _yScale;
                }

                return;
            }

            //if(changingY)
            //{
            var aspect = _sampleTex.width / _sampleTex.height;
            if (aspect == 0) aspect = _sampleTex.height / _sampleTex.width;

            newWidth = (int) (_xScale * aspect);
            newHeight = (int) (newWidth / aspect);


            //if one of the two dimensions exceed the box dimensions
            if (newWidth > _xScale || newHeight > _xScale)
            {
                //depending on which of the two exceeds the box dimensions set it as the box dimension and calculate the other one based on the aspect ratio
                if (newWidth > newHeight)
                {
                    newWidth = _xScale;
                    newHeight = (int) (newWidth / aspect);
                }
                else
                {
                    newHeight = _xScale;
                    newWidth = (int) (newHeight * aspect);
                }
            }
            //}
            else
            {

            }

            _xScale = newWidth;
            _yScale = newHeight;

        }*/

        private static void Resize(int xScale, int yScale)
        {
            for (var i = 0; i < _images.Count; i++)
            {
                var img = _images[i];
                var texPath = AssetDatabase.GetAssetPath(img);
                var pos = texPath.LastIndexOf(".") + 1;
                var fileType = texPath.Substring(pos, texPath.Length - pos);
                if (!SupportedFileTypes.Contains(fileType)) continue;
                var imageType = (ImageType) Enum.Parse(typeof(ImageType), fileType.ToUpper());
                var texSettings = (TextureImporter) AssetImporter.GetAtPath(texPath);
                var originalSettings = texSettings;
                texSettings.isReadable = true;
                texSettings.textureCompression = TextureImporterCompression.Uncompressed;
                var textureType = texSettings.textureType;
                texSettings.textureType = TextureImporterType.Default;
                var texFormat = img.format;
                if (!SupportedFormats.Contains(texFormat)) texFormat = TextureFormat.RGBA32;

                AssetDatabase.ImportAsset(texPath, ImportAssetOptions.ForceUpdate);

                img = img.ScaleTexture(xScale, yScale, texFormat);
                _images[i] = img;
                texPath.Substring(0, texPath.LastIndexOf("/") + 1);
                var bytes = img.GetRawTextureData();

                switch (imageType)
                {
                    case ImageType.EXR:
                        var hdrFormat = new Texture2D(img.width, img.height, TextureFormat.RGBAFloat, false);
                        hdrFormat.SetPixels(img.GetPixels());
                        hdrFormat.Apply();

                        bytes = hdrFormat.EncodeToEXR(Texture2D.EXRFlags.CompressZIP);
                        DestroyImmediate(hdrFormat);
                        break;
                    case ImageType.JPG:
                        bytes = img.EncodeToJPG();
                        break;
                    case ImageType.PNG:
                        bytes = img.EncodeToPNG();
                        break;
                }

                File.WriteAllBytes(texPath, bytes);
                texSettings = originalSettings;
                texSettings.textureType = textureType;
                AssetDatabase.ImportAsset(texPath, ImportAssetOptions.ForceUpdate);
                AssetDatabase.Refresh();

            }

            Init();
        }
    }
}