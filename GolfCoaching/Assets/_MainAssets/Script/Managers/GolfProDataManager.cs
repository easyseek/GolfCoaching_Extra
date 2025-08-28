using UnityEngine;
using Enums;
using System.Collections.Generic;
using System;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Linq;
using System.Net;
using System.IO;

public class ProData
{
    public int uid;
    public string name;
    public int hide;
}

public class ProInfoData
{
    public int uid;
    public string name;
    public int gender;
    public string info;
    public string introduce;
    public EFilter[] filters = new EFilter[3];
    public int favoriteCount;
    public int popularity;
    public int views;
    public string recently;
}

public class SelectProData
{
    public int uid;
    public ProInfoData infoData;
    public List<ProVideoData> videoData = new List<ProVideoData>();
    public List<ProImageData> imageData = new List<ProImageData>();
    public ProSwingData swingData = new ProSwingData();
}

public class ProVideoData
{
    public int uid = 0;
    public int id = 0;
    public string name;
    public string path;
    public EPoseDirection direction;
    public ESceneType sceneType;
    public EClub clubFilter;
    public EStance poseFilter;
    public int favoriteCount;
    public int views;
    public string recently;
}

public class ProImageData
{
    public int uid = 0;
    public string name;
    public string path;
    public EImageType imageType;
}

public class ProSwingData
{
    public int uid = 0;
    public Dictionary<EClub, ProSwingStepData> dicFull = new Dictionary<EClub, ProSwingStepData>();
    public Dictionary<EClub, ProSwingStepData> dicQuarter = new Dictionary<EClub, ProSwingStepData>();
    public Dictionary<EClub, ProSwingStepData> dicHalf = new Dictionary<EClub, ProSwingStepData>();
}

public class ProSwingStepData
{
    public int uid = 0;
    public Dictionary<string, int> dicAddress = new Dictionary<string, int>();
    public Dictionary<string, int> dicTakeback = new Dictionary<string, int>();
    public Dictionary<string, int> dicBackswing = new Dictionary<string, int>();
    public Dictionary<string, int> dicTop = new Dictionary<string, int>();
    public Dictionary<string, int> dicDownswing = new Dictionary<string, int>();
    public Dictionary<string, int> dicImpact = new Dictionary<string, int>();
    public Dictionary<string, int> dicFollow = new Dictionary<string, int>();
    public Dictionary<string, int> dicFinish = new Dictionary<string, int>();
}

public class GolfProDataManager : MonoBehaviourSingleton<GolfProDataManager>
{
    private SelectProData selectProData = new SelectProData();
    public SelectProData SelectProData {
        get { return selectProData; }
        set { selectProData = value; }
    }

    private List<ProData> proDataList = new List<ProData>();

    private Dictionary<int, ProInfoData> proInfoDataDic = null;
    private Dictionary<int, List<ProVideoData>> proVideoDataDic = null;
    private Dictionary<int, List<ProImageData>> proImageDataDic = null;
    private Dictionary<int, ProSwingData> proSwingDataDic = null;

    [SerializeField] private float minLoadTime = 2.0f;

    public void LoadProData()
    {
        StartCoroutine(LoadData());
    }

    private IEnumerator LoadData()
    {
        float startTime = Time.time;

        bool bProTable = LoadProTable();
        bool bProInfoTable = LoadProInfoData();
        bool bProVideoTable = LoadProVideoData();
        bool bProImageTable = LoadProImageData();;
        bool bProSwingTable = LoadProSwingData();;

        yield return new WaitUntil(() => (bProTable && bProInfoTable && bProVideoTable && bProImageTable && bProSwingTable));

        float timePassed = Time.time - startTime;
        if (timePassed < minLoadTime)
        {
            yield return new WaitForSeconds(minLoadTime - timePassed);
        }

        GameManager.Instance.SelectedSceneName = "Login";

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("Login");

        while(!asyncLoad.isDone)
        {
            yield return null;
        }
    }

    private bool LoadProTable()
    {
        var tableData = CSVReader.ReadCSV(INI.proDataPath);

        foreach (var row in tableData)
        {
            if (row.ContainsKey("Uid") && row.ContainsKey("Name"))
            {
                ProData entry = new ProData();

                entry.uid = Convert.ToInt32(row["Uid"]);
                entry.name = row["Name"].ToString();
                entry.hide = Convert.ToInt32(row["Hide"]);

                //Debug.Log($"[LoadProTable] uid : {entry.uid}, name : {entry.name}, hide : {entry.hide}");

                if (entry.hide != 1)
                    proDataList.Add(entry);
            }
            else
            {
                Debug.LogWarning("TablePro.csv에 필수 컬럼이 누락되었습니다.");
                return false;
            }
        }

        return true;
    }

    private bool LoadProInfoData()
    {
        if (proInfoDataDic == null)
            proInfoDataDic = new Dictionary<int, ProInfoData>();
        else
            proInfoDataDic.Clear();

        foreach (ProData list in proDataList)
        {
            var detailDataList = CSVReader.ReadCSV($"{INI.proInfoPath}{list.uid}");
            
            if (detailDataList == null || detailDataList.Count != 0)
            {
                foreach (var item in detailDataList)
                {
                    ProInfoData detailData = new ProInfoData();

                    try
                    {
                        detailData.uid = list.uid;
                        if (item.ContainsKey("Name")) detailData.name = item["Name"].ToString();
                        if (item.ContainsKey("Gender")) detailData.gender = Convert.ToInt32(item["Gender"]);
                        if (item.ContainsKey("Info")) detailData.info = item["Info"].ToString();
                        if (item.ContainsKey("Introduce")) detailData.introduce = item["Introduce"].ToString();

                        for (int i = 0; i < 3; i++)
                        {
                            if (item.ContainsKey($"Filter{i}"))
                            {
                                detailData.filters[i] = Utillity.Instance.StringToEnum<EFilter>(item[$"Filter{i}"].ToString());
                            }
                        }

                        if (item.ContainsKey("FavoriteCount")) detailData.favoriteCount = Convert.ToInt32(item["FavoriteCount"]);
                        if (item.ContainsKey("Popularity")) detailData.popularity = Convert.ToInt32(item["Popularity"]);
                        if (item.ContainsKey("Views")) detailData.views = Convert.ToInt32(item["Views"]);
                        if (item.ContainsKey("Recently")) detailData.recently = item["Recently"].ToString();


                        //Debug.Log($"[LoadProInfoData] {detailData.name}, {detailData.gender}, {detailData.info}, {detailData.proImage}, {detailData.profileImage}, {detailData.frontVideo}, {detailData.sideVideo}, {detailData.introduce}, {detailData.filters[0]}, {detailData.filters[1]}, {detailData.filters[2]}");

                        proInfoDataDic[list.uid] = detailData;
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogWarning($"프로 상세 CSV 파싱 중 예외 발생: uid {list.uid}, {ex.Message}");
                        return false;
                    }
                }
            }
            else
            {
                Debug.LogWarning($"프로 상세 CSV 파일이 비어있습니다. uid: {list.uid}");
                return false;
            }
        }

        return true;
    }

    private bool LoadProVideoData()
    {
        if (proVideoDataDic == null)
            proVideoDataDic = new Dictionary<int, List<ProVideoData>>();
        else
            proVideoDataDic.Clear();

        foreach (ProData list in proDataList)
        {
            var detailDataList = CSVReader.ReadCSV($"{INI.proVideoPath}{list.uid}/{list.uid}");
            
            if (detailDataList == null || detailDataList.Count != 0)
            {
                List<ProVideoData> detailData = new List<ProVideoData>();

                foreach (var item in detailDataList)
                {
                    ProVideoData proVideoData = new ProVideoData();

                    try
                    {
                        proVideoData.uid = list.uid;
                        if (item.ContainsKey("Id")) proVideoData.id = Convert.ToInt32(item["Id"]);
                        if (item.ContainsKey("Name")) proVideoData.name = item["Name"].ToString();
                        if (item.ContainsKey("Path")) proVideoData.path = item["Path"].ToString();
                        if (item.ContainsKey("Direction")) proVideoData.direction = Utillity.Instance.StringToEnum<EPoseDirection>(item["Direction"].ToString());
                        if (item.ContainsKey("SceneType")) proVideoData.sceneType = Utillity.Instance.StringToEnum<ESceneType>(item["SceneType"].ToString());
                        if (item.ContainsKey("ClubFilter")) proVideoData.clubFilter = Utillity.Instance.StringToEnum<EClub>(item["ClubFilter"].ToString()); 
                        if (item.ContainsKey("PoseFilter")) proVideoData.poseFilter = Utillity.Instance.StringToEnum<EStance>(item["PoseFilter"].ToString());
                        if (item.ContainsKey("FavoriteCount")) proVideoData.favoriteCount = Convert.ToInt32(item["FavoriteCount"]);
                        if (item.ContainsKey("Views")) proVideoData.views = Convert.ToInt32(item["Views"]);
                        if (item.ContainsKey("Recently")) proVideoData.recently = item["Recently"].ToString();

                        //Debug.Log($"[LoadProVideoData] {detailData.name}, {detailData.path}, {detailData.sceneType}, {detailData.clubFilter}, {detailData.poseFilter}, {detailData.recommendFilter}, {detailData.priority}");

                        detailData.Add(proVideoData);
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogWarning($"프로 비디오 CSV 파싱 중 예외 발생: uid {list.uid}, {ex.Message}");
                        return false;
                    }
                }

                proVideoDataDic.Add(list.uid, detailData);
            }
            else
            {
                Debug.LogWarning($"프로 비디오 CSV 파일이 비어있습니다. uid: {list.uid}");
                return false;
            }
        }

        return true;
    }

    private bool LoadProImageData()
    {
        if (proImageDataDic == null)
            proImageDataDic = new Dictionary<int, List<ProImageData>>();
        else
            proImageDataDic.Clear();

        foreach (ProData list in proDataList)
        {
            var detailDataList = CSVReader.ReadCSV($"{INI.proImagePath}{list.uid}/{list.uid}");

            if (detailDataList == null || detailDataList.Count != 0)
            {
                List<ProImageData> detailData = new List<ProImageData>();

                foreach (var item in detailDataList)
                {
                    ProImageData proImageData = new ProImageData();

                    try
                    {
                        proImageData.uid = list.uid;
                        if (item.ContainsKey("Name")) proImageData.name = item["Name"].ToString();
                        if (item.ContainsKey("Path")) proImageData.path = item["Path"].ToString();
                        if (item.ContainsKey("ImageType")) proImageData.imageType = Utillity.Instance.StringToEnum<EImageType>(item["ImageType"].ToString());

                        detailData.Add(proImageData);
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogWarning($"프로 이미지 CSV 파싱 중 예외 발생: uid {list.uid}, {ex.Message}");
                        return false;
                    }
                }

                proImageDataDic.Add(list.uid, detailData);
            }
            else
            {
                Debug.LogWarning($"프로 이미지 CSV 파일이 비어있습니다. uid: {list.uid}");
                return false;
            }
        }

        return true;
    }

    private bool LoadProSwingData()
    {
        if (proSwingDataDic == null)
            proSwingDataDic = new Dictionary<int, ProSwingData>();
        else
            proSwingDataDic.Clear();

        foreach (ProData list in proDataList)
        {
            try
            {
                List<Dictionary<string, object>> detailDataList = null;
                detailDataList = CSVReader.ReadCSV($"{INI.proSwingPath}{list.uid}/{list.uid}");

                if (detailDataList == null || detailDataList.Count != 0)
                {
                    ProSwingData detailData = new ProSwingData();
                    detailData.uid = list.uid;

                    foreach (var item in detailDataList) //SWING,CLUB,PATH
                    {
                        int swingIndex = 0; //full0,quarter1,half2 
                        int clbuIndex = 0;  //driver0, wood1, longiron2, midiron3, shortiron4, putter5
                        string dataPath = string.Empty; //csv path
                        if (item.ContainsKey("SWING")) swingIndex = Convert.ToInt32(item["SWING"]);
                        if (item.ContainsKey("CLUB")) clbuIndex = Convert.ToInt32(item["CLUB"]);
                        if (item.ContainsKey("PATH")) dataPath = item["PATH"].ToString();
                        dataPath = dataPath.Replace(".csv","").Replace(".CSV","");
                        //Debug.Log($"{INI.proSwingPath}{list.uid}/{dataPath}");
                        var detailStepDataList = CSVReader.ReadCSV($"{INI.proSwingPath}{list.uid}/{dataPath}");
                        ProSwingStepData proSwingStepData = new ProSwingStepData();

                        foreach (var data in detailStepDataList) //SWING,CLUB,PATH
                        {
                            proSwingStepData.dicAddress.Add(data["NAME"].ToString(), int.Parse(data["ADDRESS"].ToString()));
                            proSwingStepData.dicTakeback.Add(data["NAME"].ToString(), int.Parse(data["TAKEBACK"].ToString()));
                            proSwingStepData.dicBackswing.Add(data["NAME"].ToString(), int.Parse(data["BACKSWING"].ToString()));
                            proSwingStepData.dicTop.Add(data["NAME"].ToString(), int.Parse(data["TOP"].ToString()));
                            proSwingStepData.dicDownswing.Add(data["NAME"].ToString(), int.Parse(data["DOWNSWING"].ToString()));
                            proSwingStepData.dicImpact.Add(data["NAME"].ToString(), int.Parse(data["IMPACT"].ToString()));
                            proSwingStepData.dicFollow.Add(data["NAME"].ToString(), int.Parse(data["FOLLOW"].ToString()));
                            proSwingStepData.dicFinish.Add(data["NAME"].ToString(), int.Parse(data["FINISH"].ToString()));
                        }

                        if (swingIndex == 0)
                            detailData.dicFull.Add((EClub)clbuIndex, proSwingStepData);
                        else if (swingIndex == 1)
                            detailData.dicQuarter.Add((EClub)clbuIndex, proSwingStepData);
                        else // if (swingIndex == 2)
                            detailData.dicHalf.Add((EClub)clbuIndex, proSwingStepData);
                    }

                    proSwingDataDic.Add(list.uid, detailData);
                }
                else
                {
                    Debug.LogWarning($"프로 스윙 데이터 파일이 비어있습니다. uid: {list.uid}");
                    //return false;
                }
            }
            catch
            {
                Debug.LogWarning($"프로 스윙 데이터 파일이 없습니다. uid: {list.uid}");
                //return false;
            }

        }

        return true;
    }

    public void ReloadProVideoData()
    {
        LoadProVideoData();

        SelectProData.videoData = GetProVideoDataList(selectProData.uid);
    }

    public void ReloadProSwingData()
    {
        LoadProSwingData();

        SelectProData.swingData = GetSwingData(SelectProData.uid);
    }

    public ProData GetProData(string value)
    {
        foreach(var data in proDataList)
        {
            if (data.name == value)
                return data;
        }

        return null;
    }

    public ProInfoData GetProInfoData(int uid)
    {
        this.proInfoDataDic.TryGetValue(uid, out ProInfoData temp);
        return temp;
    }

    public bool ContainsKey(int uid)
    {
        return proInfoDataDic.ContainsKey(uid);
    }

    public List<ProData> GetProDataList()
    {
        return proDataList;
    }

    public Dictionary<int, ProInfoData> GetProInfoList()
    {
        return proInfoDataDic;
    }

    public List<ProVideoData> GetProVideoDataList(int uid)
    {
        this.proVideoDataDic.TryGetValue(uid, out List<ProVideoData> temp);
        return temp;
    }

    public Dictionary<int, List<ProVideoData>> GetProVideoDic()
    {
        return proVideoDataDic;
    }

    public List<ProImageData> GetProImageDataList(int uid)
    {
        this.proImageDataDic.TryGetValue(uid, out List<ProImageData> temp);
        return temp;
    }

    public Dictionary<int, List<ProImageData>> GetProImageDic()
    {
        return proImageDataDic;
    }

    public ProImageData GetProImageData(int uid, EImageType type)
    {
        if (this.proImageDataDic.TryGetValue(uid, out var list))
        {
            return list.Single(v => v.imageType == type);
        }
        else
            return null;
    }

    public ProSwingData GetSwingData(int uid)
    {
        this.proSwingDataDic.TryGetValue(uid, out ProSwingData temp);
        return temp;
    }
}
