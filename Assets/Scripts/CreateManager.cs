using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using UnityEngine.SceneManagement;
using UnityGoogleDrive;
using UnityEngine.UIElements;


public class CreateManager : MonoBehaviour
{
    private float _repeatSpan;    //繰り返す間隔
    private float _timeElapsed;　 //経過時間
    float speed = 3.0f;
    private GameObject obj;
    public List<GameObject> animals;//どうぶつ取得配列
    public bool isFall;
    int file_length;
    List<string> statefileIds = new List<string>();
    public float pivotHeight = 3;//生成位置の基準
    public Camera mainCamera;//カメラ取得用変数
    public GameObject cameracontroller;
    private const string folderId = "1kuN14vh4dfLBFFqew22rCdBZ1vRc2mou";
    
    void Init()
    {
        //StartCoroutine(AboutGoogleDrive());
        StartCoroutine(CompareGoogleDriveImages());
    }

    void Start()
    {
        //var fileNameList = fileNames.ToList();
        _repeatSpan = 5;    //実行間隔を５に設定
        _timeElapsed = 0;   //経過時間をリセット

        Init();
    }

    void Update()
    {
        if (CheckGameOver(animals))
        {
            SceneManager.LoadScene("GameOver");
        }

        if (CheckMove(Animal.isMoves))
        {
            return;//移動中なら処理はここまで
        }
        _timeElapsed += Time.deltaTime;

        if (_timeElapsed >= _repeatSpan)
        {
            if (animals.Count != 0)
            {
                ScoreScript.Score = GetAnimalsHighestPosition(animals);
            }
            StartCoroutine(CreateGoogleDriveImages());
            _timeElapsed = 0;   //経過時間をリセットする
        }

        RotateAnimal();
        MoveAnimal();
        FallAnimal();
    }


    IEnumerator AboutGoogleDrive()
    {
        yield return GoogleDriveAbout.Get().Send();
    }

    IEnumerator CompareGoogleDriveImages()
    {
        var reqfiles = GoogleDriveFiles.List();
        reqfiles.Fields = new List<string> { "files(id, name, size, mimeType, createdTime)" };
        reqfiles.Q = $"\'{folderId}\' in parents and trashed = false";

        yield return reqfiles.Send();

        reqfiles.Send().OnDone +=
            (filelist) =>
            {
                foreach (var file in filelist.Files)
                {
                    statefileIds.Add(file.Id);
                }
            };

    }

    IEnumerator CreateGoogleDriveImages()
    {
        var reqfiles = GoogleDriveFiles.List();
        reqfiles.Fields = new List<string> { "files(id, name, size, mimeType, createdTime)" };
        reqfiles.Q = $"\'{folderId}\' in parents and trashed = false";

        yield return reqfiles.Send();


        reqfiles.Send().OnDone +=
            (filelist) =>
            {
                foreach (var file in filelist.Files)
                {
                    if (statefileIds.Contains(file.Id))
                    {
                        //Debug.Log(file.Id);
                    }
                    else
                    {
                        var DLrequest = GoogleDriveFiles.Download(fileId: file.Id);
                        DLrequest.Send().OnDone += (DLFile) =>
                        {
                            byte[] bytes = DLFile.Content;
                            Texture2D texture = new Texture2D(2, 2);
                            texture.LoadImage(bytes);
                            
                            Rect rect = new Rect(0, 0, texture.width, texture.height);
                            Vector2 pivot = new Vector2(0.5f, 0.5f); // 中央をピボットとする
                            float pixelsPerUnit = 1000.0f;

                            Sprite img = Sprite.Create(texture, rect, pivot, pixelsPerUnit);

                            if (img == null)
                            {
                                return;
                            }
                            Create(img);
                            statefileIds.Add(file.Id);
                        };
                    }

                }
            };

    }

    void Create(Sprite img)
    {
        if (CameraController.isCollision)
        {
            Debug.Log("collision_start");
            cameracontroller.transform.Translate(0, 3.0f, 0);
            mainCamera.transform.Translate(0, 3.0f, 0);
            pivotHeight += 3.0f;
            Debug.Log("collision_fin");
        }
        isFall = false;
        obj = new GameObject();
        obj.AddComponent<SpriteRenderer>();
        obj.GetComponent<SpriteRenderer>().sprite = img;
        obj.AddComponent<PolygonCollider2D>();
        obj.AddComponent<Rigidbody2D>();
        obj.GetComponent<Rigidbody2D>().isKinematic = true;
        obj.AddComponent<Animal>();
        obj.transform.position = new Vector3(0.0f, pivotHeight, 0.0f);
        animals.Add(obj);
    }

    int GetAnimalsHighestPosition(List<GameObject> animals)
    {
        List<int> animalHeights = new List<int>();
        foreach (GameObject animal in animals)
        {
            var height = animal.transform.localPosition.y * 100f;
            var score = (int)Math.Round(height) + 500;
            animalHeights.Add(score);
        }
        return animalHeights.Max();
    }

    public void RotateAnimal()
    {
        if (!isFall)
        {
            if (Input.GetKey(KeyCode.Space))
            {
                obj.transform.Rotate(0, 0, 1f);
            }
        }
    }

    public void MoveAnimal()
    {
        if (!isFall)
        {
            if (Input.GetKey("left"))
            {
                obj.transform.position = new Vector2(obj.transform.position.x - 0.01f, pivotHeight);
            }
            else if (Input.GetKey("right"))
            {
                obj.transform.position = new Vector2(obj.transform.position.x + 0.01f, pivotHeight);
            }
        }
    }

    public void FallAnimal()
    {
        if (!isFall)
        {
            if (Input.GetKey(KeyCode.Return))
            {
                obj.GetComponent<Rigidbody2D>().isKinematic = false;
                isFall = true;
                Debug.Log("test");
            }
        }
    }


    /// <summary>
    /// 移動中かチェック
    /// </summary>
    /// <param name="isMoves"></param>
    /// <returns></returns>
    bool CheckMove(List<Moving> isMoves)
    {
        if (isMoves == null)
        {
            return false;
        }
        foreach (Moving b in isMoves)
        {
            if (b.isMove)
            {
                //Debug.Log("移動中(*'ω'*)");
                return true;
            }
        }
        return false;
    }

    bool CheckGameOver(List<GameObject> animals)
    {
        foreach (GameObject b in animals)
        {
            if (b.transform.position.y < -5)
            {
                return true;
            }
        }
        return false;
    }
}
