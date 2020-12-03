
using OW.Data.Entity;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace CnMedicineServer.Models
{
    /// <summary>
    /// 中医药治病思路所用数据表的基类。
    /// </summary>
    public abstract class CnMedicineLogicBase
    {
        static ConcurrentDictionary<string, object> _AllDatas = new ConcurrentDictionary<string, object>();
        static ConcurrentDictionary<string, string> _FullPaths = new ConcurrentDictionary<string, string>();

        /// <summary>
        /// 获取缓存的数据或者是创建缓存数据并返回。特別地，這個函數可以并發調用，亦可重入(同一个线程多次调用)。
        /// </summary>
        /// <typeparam name="T">列表元素的类型。</typeparam>
        /// <param name="fullFileName">数据文件的url，如:"~/xxx/xxx.xxx"</param>
        /// <returns></returns>
        public static Task<List<T>> GetOrCreateAsync<T>(string fullFileName) where T : new()
        {
            fullFileName = System.Web.HttpContext.Current.Server.MapPath(fullFileName);    //本机全路径

            fullFileName = _FullPaths.GetOrAdd(fullFileName, fullFileName); //获取单例字符串对象
            if (Monitor.IsEntered(fullFileName)) //若本线程已经在工作
            {
                return Task.Run(() =>
                {
                    lock (fullFileName)
                    {
                        object tmp;
                        while (!_AllDatas.TryGetValue(fullFileName, out tmp))
                        {
                            Monitor.Wait(fullFileName);
                        }
                        return (List<T>)tmp;
                    }
                });

            }
            lock (fullFileName) //细粒度锁定
            {
                if (_AllDatas.TryGetValue(fullFileName, out object tmp))    //若获取到缓存数据
                {
                    List<T> result;
                    if (tmp is List<T>)
                    {
                        result = (List<T>)tmp;
                        return Task.CompletedTask.ContinueWith(task =>
                        {
                            return result;
                        }, TaskContinuationOptions.ExecuteSynchronously);   //尽可能避免切换线程
                    }
                    else
                    {
                        //TO DO 1495
                        throw new InvalidCastException($"文件 '{fullFileName}' 包含的元素类型为{tmp.GetType().ToString()},无法转换为{typeof(List<T>).ToString()}类型。");
                    }
                }
                var path = Path.GetDirectoryName(fullFileName);
                var resultTask = Task.Run(() =>
                {
                    Monitor.Enter(fullFileName);    //细粒度锁定
                    try
                    {
                        List<T> result;
                        var name = Path.GetFileName(fullFileName);
                        using (var tdb = new TextFileContext(path) { IgnoreQuotes = true, })
                        {
                            result = tdb.GetList<T>(name);
                        }
                        _AllDatas.TryAdd(fullFileName, result);
                        return result;
                    }
                    catch (Exception)
                    {
                        _FullPaths.TryRemove(fullFileName, out string ign); //抛弃细粒度锁定标志
                        throw;
                    }
                    finally
                    {
                        Monitor.Pulse(fullFileName);    //略微提高吞吐量
                        Monitor.Exit(fullFileName);
                    }
                });
                return resultTask;
            }
        }

        /// <summary>
        /// 获取缓存的数据或者是创建缓存数据并返回。特別地，這個函數可以并發調用，亦可重入(同一个线程多次调用)。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path">网站内的相对路径,类似~/xxx1/xxx2格式。</param>
        /// <param name="fileName">文件名，包含扩展名。</param>
        /// <returns></returns>
        /// <exception cref="System.Web.HttpException">当前的  HttpContext 是 null。</exception>
        public static Task<List<T>> GetOrCreateAsync<T>(string path, string fileName) where T : new()
        {
            var localPath = System.Web.HttpContext.Current.Server.MapPath(path);
            var fullPath = Path.Combine(localPath, fileName);
            return GetOrCreateAsync<T>(fullPath);
        }
    }

    public class CnMedicineSignsBase
    {
        /*
            编号	问题	症状	问题类型
            1101	经前乳房胀痛	经前乳房胀痛	"选择,多重"
        */

        /// <summary>
        /// 编号。
        /// </summary>
        [TextFieldName("编号")]
        public virtual int Number { get; set; }

        /// <summary>
        /// 问题。
        /// </summary>
        [TextFieldName("问题")]
        public virtual string Question { get; set; }

        /// <summary>
        /// 症状。
        /// </summary>
        [TextFieldName("症状")]
        public virtual string ZhengZhuang { get; set; }

        /// <summary>
        /// 问题类型。
        /// </summary>
        [TextFieldName("问题类型")]
        public virtual QuestionsKind QuestionsKind { get; set; }

        /// <summary>
        /// 说明。
        /// </summary>
        [TextFieldName("说明")]
        public virtual string Description { get; set; }
    }

    /// <summary>
    /// 派生编号的表。
    /// </summary>
    public class GeneratedNumeber : CnMedicineLogicBase
    {
        /*
         * 归类编号	逻辑	阈值	说明
         * 1001	813，814，815，816，817。	0.20 	睡眠
         */

        public GeneratedNumeber()
        {

        }

        /// <summary>
        /// 归类编号。
        /// </summary>
        [TextFieldName("归类编号")]
        public virtual int Number { get; set; }

        /// <summary>
        /// 逻辑。
        /// </summary>
        [TextFieldName("逻辑")]
        public virtual string NumbersString { get; set; }

        private List<int> _Numbers;

        /// <summary>
        /// 编号的数组。
        /// </summary>
        public List<int> Numbers
        {
            get
            {
                lock (this)
                {
                    if (null == _Numbers)
                    {
                        _Numbers = EntityUtility.GetArray(NumbersString).Where(c => !string.IsNullOrWhiteSpace(c)).Select(c => int.Parse(c)).ToList();
                    }
                }
                return _Numbers;
            }
        }

        /// <summary>
        /// 阈值。
        /// </summary>
        [TextFieldName("阈值")]
        public virtual float Thresholds { get; set; }

        /// <summary>
        /// 说明。
        /// </summary>
        [TextFieldName("说明")]
        public virtual string Description { get; set; }

    }

    /// <summary>
    /// 高荣荣医师的分型表基类。
    /// </summary>
    [DataContract]
    public class GrrBianZhengFenXingBase : CnMedicineLogicBase
    {
        /*
         * 分组号	分型号	症状编号	阈值	最低阈值	方剂号	方药		备注
         * 10	101	"1001*21,1101,1109,1118,1111,1114,5406,5203,3508,3402,1119,1115,5353,10009,4414。"	0.6	0.3	0	苍术15黄芪15黄柏15牛膝10百部10白芷15砂仁后下15陈皮15炙甘草5法半夏10茯苓15生姜10		方药1用法：水煎30分钟坐浴熏洗外阴及冲洗阴道，月经结束后连续用药7-14天
         * */

        /// <summary>
        /// 构造函数。
        /// </summary>
        public GrrBianZhengFenXingBase()
        {

        }

        /// <summary>
        /// 分型组号。
        /// </summary>
        [TextFieldName("分组号")]
        public virtual int GroupNumber { get; set; }

        /// <summary>
        /// 分型号。
        /// </summary>
        [TextFieldName("分型号")]
        public virtual int Number { get; set; }

        /// <summary>
        /// 编号。
        /// </summary>
        [TextFieldName("症状编号")]
        public virtual string NumbersString { get; set; }

        private Dictionary<int, float> _Numbers;

        /// <summary>
        /// 键是编号值是其权值。
        /// </summary>
        public Dictionary<int, float> Numbers
        {
            get
            {
                lock (this)
                {
                    if (null == _Numbers)
                    {
                        _Numbers = EntityUtility.GetArrayWithPower(NumbersString).GroupBy(c => c.Item1).ToDictionary(c => c.Key, c => c.Sum(subc => subc.Item2));
                    }
                }
                return _Numbers;
            }
        }

        /// <summary>
        /// 阈值。
        /// </summary>
        [TextFieldName("阈值")]
        [DataMember]
        public virtual float Thresholds { get; set; }

        /// <summary>
        /// 最低阈值。
        /// </summary>
        [DataMember]
        [TextFieldName("最低阈值")]
        public virtual float ThresholdsOfLowest { get; set; }

        /// <summary>
        /// 方剂号。
        /// </summary>
        [DataMember]
        [TextFieldName("方剂号")]
        public virtual int PrescriptionNumber { get; set; } = 0;

        /// <summary>
        /// 药物。
        /// </summary>
        [TextFieldName("方药")]
        public virtual string CnDrugString { get; set; }

        /// <summary>
        /// 对症药。
        /// </summary>
        [TextFieldName("对症药")]
        public virtual string DuiZhengCnDrugString { get; set; }

        private List<Tuple<string, decimal>> _CnDrugs;

        /// <summary>
        /// 所有药物的列表。
        /// </summary>
        [DataMember]
        public List<Tuple<string, decimal>> CnDrugs
        {
            get
            {
                lock (this)
                    if (null == _CnDrugs)
                    {
                        _CnDrugs = new List<Tuple<string, decimal>>();
                        if (null != CnDrugString)
                            _CnDrugs.AddRange(EntityUtility.GetTuples(CnDrugString));
                        if (null != DuiZhengCnDrugString)
                            _CnDrugs.AddRange(EntityUtility.GetTuples(DuiZhengCnDrugString));
                    }
                return _CnDrugs;
            }
            set
            {
                _CnDrugs = value.ToList();
            }
        }

        [DataMember]
        [TextFieldName("分型诊断")]
        public virtual string ZhengDuan { get; set; }

        [DataMember]
        [TextFieldName("病因分析")]
        public virtual string FenXi { get; set; }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    /// <summary>
    /// 高荣荣医师的经络辩证表基类。
    /// </summary>
    [DataContract]
    public class GrrJingLuoBianZhengBase : CnMedicineLogicBase
    {
        /*
            病位	编号	阈值	分组号	优先度	药物
            少阳	1103，1406，1206，5306。	0.25	20	11	柴胡15 茯苓15
         */

        static Lazy<List<JingXingRuFangZhangTongJingLuoBianZheng>> _DefaultCollection = new Lazy<List<JingXingRuFangZhangTongJingLuoBianZheng>>(() =>
        {
            List<JingXingRuFangZhangTongJingLuoBianZheng> result;
            var path = System.Web.HttpContext.Current.Server.MapPath("~/content/经行乳房胀痛");

            using (var tdb = new TextFileContext(path) { IgnoreQuotes = true, })
            {
                result = tdb.GetList<JingXingRuFangZhangTongJingLuoBianZheng>("乳胀-经络辨证表.txt");
            }
            return result;
        }, true);

        /// <summary>
        /// 默认的集合。
        /// </summary>
        public static List<JingXingRuFangZhangTongJingLuoBianZheng> DefaultCollection
        {
            get
            {
                return _DefaultCollection.Value;
            }
        }

        /// <summary>
        /// 分组号。
        /// </summary>
        [TextFieldName("分组号")]
        public virtual int GroupNumber { get; set; }

        /// <summary>
        /// 病位（六经）。
        /// </summary>
        [TextFieldName("病位")]
        public virtual string BingWei { get; set; }

        /// <summary>
        /// 编号1。
        /// </summary>
        [TextFieldName("编号")]
        public virtual string NumersString { get; set; }

        private List<int> _Numbers;

        /// <summary>
        /// 编号的数组。
        /// </summary>
        public List<int> Numbers
        {
            get
            {
                lock (this)
                {
                    if (null == _Numbers)
                    {
                        _Numbers = EntityUtility.GetArray(NumersString).Where(c => !string.IsNullOrWhiteSpace(c)).Select(c => int.Parse(c)).ToList();
                    }
                }
                return _Numbers;
            }
        }

        /// <summary>
        /// 优先度。
        /// </summary>
        [TextFieldName("优先度")]
        public virtual int Priority { get; set; }

        /// <summary>
        /// 药物。
        /// </summary>
        [TextFieldName("药物")]
        public virtual string DuiZhengCnDrugString { get; set; }

        List<Tuple<string, decimal>> _CnDrugs;

        /// <summary>
        /// 所有药物的列表。
        /// </summary>
        public List<Tuple<string, decimal>> CnDrugs
        {
            get
            {
                lock (this)
                    if (null == _CnDrugs)
                    {
                        _CnDrugs = EntityUtility.GetTuples(DuiZhengCnDrugString);
                    }
                return _CnDrugs;
            }
        }

        /// <summary>
        /// 阈值。
        /// </summary>
        [TextFieldName("阈值")]
        [DataMember]
        public virtual float Thresholds { get; set; }

        [DataMember]
        [TextFieldName("经络病因")]
        public virtual string BingYin { get; set; }

    }

    /// <summary>
    /// 药物矫正表基类。
    /// </summary>
    [DataContract]
    public class CnDrugCorrectionBase : CnMedicineLogicBase
    {

        /*
          * 编号	阈值	加药	减药	类型号
          * 10，1510。	1	小茴香12 龙眼肉6 五灵脂10		1
          * 
          */

        [TextFieldName("编号")]
        public virtual string NumbersString { get; set; }

        private List<int> _Numbers;

        /// <summary>
        /// 编号数组。
        /// </summary>
        public List<int> Numbers
        {
            get
            {
                lock (this)
                    if (null == _Numbers)
                    {
                        _Numbers = EntityUtility.GetArray(NumbersString).Where(c => int.TryParse(c, out int i)).Select(c => int.Parse(c)).ToList();
                    }
                return _Numbers;
            }
        }

        /// <summary>
        /// 阈值。
        /// </summary>
        [TextFieldName("阈值")]
        public virtual float Thresholds { get; set; }

        /// <summary>
        /// 加药。
        /// </summary>
        [TextFieldName("加药")]
        public virtual string CnDrugOfAddString { get; set; }

        List<Tuple<string, decimal>> _CnDrugOfAdd;

        /// <summary>
        /// 加药数组。
        /// </summary>
        public List<Tuple<string, decimal>> CnDrugOfAdd
        {
            get
            {
                lock (this)
                    if (null == _CnDrugOfAdd)
                    {
                        _CnDrugOfAdd = EntityUtility.GetTuples(CnDrugOfAddString);
                    }
                return _CnDrugOfAdd;
            }
        }

        /// <summary>
        /// 减药。
        /// </summary>
        [TextFieldName("减药")]
        public virtual string CnDrugOfSubString { get; set; }

        List<string> _CnDrugOfSub;

        /// <summary>
        /// 减药数组。
        /// </summary>
        public List<string> CnDrugOfSub
        {
            get
            {
                lock (this)
                    if (null == _CnDrugOfSub)
                    {
                        _CnDrugOfSub = EntityUtility.GetArray(CnDrugOfSubString);
                    }
                return _CnDrugOfSub;
            }
        }

        /// <summary>
        /// 类型号。
        /// </summary>
        [TextFieldName("类型号")]
        public virtual int TypeNumber { get; set; }
    }

}