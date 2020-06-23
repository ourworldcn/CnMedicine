
#pragma warning disable CS3021 // 由于程序集没有 CLSCompliant 特性，因此类型或成员不需要 CLSCompliant 特性
using CnMedicineServer.Models;
using OW.Data.Entity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace CnMedicineServer.Bll
{
    public class GaoRongrongAnalysisDataBase : CnMedicineAnalysisDataBase
    {
        public GaoRongrongAnalysisDataBase()
        {

        }

        public override void SetAnswers(IEnumerable<SurveysAnswer> answers, DbContext dbContext)
        {
            base.SetAnswers(answers, dbContext);

        }

        Dictionary<int, string> _TypeNumeber;

        /// <summary>
        /// 获取编号的类型码。
        /// </summary>
        public Dictionary<int, string> TypeNumeber
        {
            get
            {
                lock (SyncLocker)
                    if (null == _TypeNumeber)
                    {
                        _TypeNumeber = new Dictionary<int, string>();
                        foreach (var item in Numbers)
                        {
                            if (AnswerTemplates.TryGetValue(item, out SurveysAnswerTemplate surveysAnswer))
                            {
                                var tuples = EntityUtility.GetTuples(surveysAnswer.UserState);
                                if (tuples.Any(c => "类型号A" == c.Item1 && 1 == c.Item2))
                                    _TypeNumeber.Add(item, "A");
                                else if (tuples.Any(c => "类型号B" == c.Item1 && 1 == c.Item2))
                                    _TypeNumeber.Add(item, "B");
                                else if (tuples.Any(c => "类型号C" == c.Item1 && 1 == c.Item2))
                                    _TypeNumeber.Add(item, "C");
                            }
                        }
                    }
                return _TypeNumeber;
            }
        }

        /// <summary>
        /// 因某种原因加入的编号集合。
        /// </summary>
        protected List<int> AddingNumbers { get; } = new List<int>();

        /// <summary>
        /// 获取指定编号的类型号。
        /// </summary>
        /// <param name="number"></param>
        /// <returns>A参与计算且阈值为0.7；B不参与计算；C参与计算且没有阈值,未指定则返回<see cref="string.Empty"/>。</returns>
        public string GetTypeNumber(int number)
        {
            if (TypeNumeber.TryGetValue(number, out string typeNumber))
                return typeNumber.ToUpper();
            else
                return string.Empty;
        }

    }

    public abstract class GaoRonrongAlgorithmBase : CnMedicineAlgorithmBase
    {
        public GaoRonrongAlgorithmBase()
        {

        }

        /// <summary>
        /// 對一組症狀多個分型的匹配值如果與最大匹配值的差小於或等於此值則取最大平均匹配值的分型。
        /// 默認值:0.1。
        /// </summary>
        public float MaxDiffOfFenXing { get; set; } = 0.1f;

        private Tuple<string, Type> _BianZhengFenXingsInfo;

        /// <summary>
        /// 获取或设置<see cref="BianZhengFenXings"/>属性的默认获取方法。
        /// </summary>
        /// <exception cref="InvalidOperationException">只能设置一次。</exception>
        public Tuple<string, Type> BianZhengFenXingsInfo
        {
            get => _BianZhengFenXingsInfo;
            set
            {
                if (null != _BianZhengFenXingsInfo)
                    throw new InvalidOperationException($"{nameof(BianZhengFenXingsInfo)}属性只能设置一次。");
                _BianZhengFenXingsInfo = value;
            }
        }

        /// <summary>
        /// 高荣荣医师的分型的集合数据。
        /// </summary>
        public virtual IEnumerable<GrrBianZhengFenXingBase> BianZhengFenXings
        {
            get => CnMedicineLogicBase.GetOrCreateAsync<GrrBianZhengFenXingBase>(_BianZhengFenXingsInfo.Item1).Result;
        }

        private Tuple<string, Type> _JingLuoBianZhengs;

        /// <summary>
        /// 获取或设置<see cref="JingLuoBianZhengs"/>属性的默认获取方法。
        /// </summary>
        /// <exception cref="InvalidOperationException">只能设置一次。</exception>
        public Tuple<string, Type> JingLuoBianZhengsInfo
        {
            get => _JingLuoBianZhengs;
            set
            {
                if (null != _JingLuoBianZhengs)
                    throw new InvalidOperationException($"{nameof(JingLuoBianZhengs)}属性只能设置一次。");
                _JingLuoBianZhengs = value;
            }
        }

        /// <summary>
        /// 高荣荣医师的经络辩证的集合数据。
        /// </summary>
        public virtual IEnumerable<GrrJingLuoBianZhengBase> JingLuoBianZhengs
        {
            get => CnMedicineLogicBase.GetOrCreateAsync<GrrJingLuoBianZhengBase>(JingLuoBianZhengsInfo.Item1).Result;
        }

        private Dictionary<GrrBianZhengFenXingBase, float> _Fenxing2Matching;

        /// <summary>
        /// 键是分型数据，值匹配度。
        /// </summary>
        public virtual Dictionary<GrrBianZhengFenXingBase, float> Fenxing2Matching
        {
            get
            {
                lock (SyncLocker)
                    if (null == _Fenxing2Matching)
                    {
                        Dictionary<GrrBianZhengFenXingBase, float> result = new Dictionary<GrrBianZhengFenXingBase, float>();
                        foreach (var item in BianZhengFenXings)
                        {
                            var count = item.Numbers.Join(AllNumbers, c => c.Key, c => c, (kv, num) => kv)
                                .GroupBy(c => c.Key, (key, kvs) => kvs.Sum(c => c.Value)).Sum(); //AllNumbers中重複出現將有額外含義
                            result.Add(item, count / item.Numbers.Count);
                        }
                        _Fenxing2Matching = result;
                    }
                return _Fenxing2Matching;
            }
        }

        List<GrrBianZhengFenXingBase> _FenXing;

        /// <summary>
        /// 命中的分型。
        /// </summary>
        public List<GrrBianZhengFenXingBase> FenXing
        {
            get
            {
                lock (SyncLocker)
                    if (null == _FenXing)
                    {
                        _FenXing = new List<GrrBianZhengFenXingBase>();
                        var collAlternative = (from tmp in Fenxing2Matching    //备选
                                               where tmp.Key.ThresholdsOfLowest <= tmp.Value //符合最低匹配度要求的才入选
                                               select tmp).ToArray();
                        var avgDic = (from tmp in collAlternative
                                      group tmp by tmp.Key.GroupNumber into g
                                      select new { GroupNumber = g.Key, Avg = g.Average(c => c.Value) });    //求平均匹配度
                        var coll = from tmp in collAlternative
                                   join avg in avgDic on tmp.Key.GroupNumber equals avg.GroupNumber
                                   orderby avg.Avg descending, tmp.Value descending //平均匹配度 匹配度降序
                                   select new { tmp.Key, Matching = tmp.Value, AvgMatching = avg.Avg };
                        var result = coll.ToArray();
                        if (result.Any())   //若有任何匹配
                        {
                            var maxMatching = result.Max(c => c.Matching);  //最大匹配值
                            var singleResult = result.FirstOrDefault(c => maxMatching - c.Matching <= MaxDiffOfFenXing);    //獲取最佳匹配

                            if (null != singleResult)
                                _FenXing.Add(singleResult.Key);
                            AdditionalNumbers.AddRange(_FenXing.Select(c => c.Number));    //辩证的编号加入编号集合
                        }
                    }
                return _FenXing;
            }
        }

        List<GrrJingLuoBianZhengBase> _JingLuos;

        /// <summary>
        /// 获取命中的经络辩证集合。
        /// </summary>
        public List<GrrJingLuoBianZhengBase> JingLuos
        {
            get
            {
                lock (SyncLocker)
                    if (null == _JingLuos)
                    {
                        var coll = JingLuoBianZhengs
                            .Select(c => new { Value = c, Matching = (float)c.Numbers.Intersect(AllNumbers).Count() / c.Numbers.Count })   //计算中间数值
                            .Where(c => c.Matching >= c.Value.Thresholds)    //匹配项
                            .GroupBy(c => c.Value.GroupNumber, (key, seq) => seq.OrderByDescending(c => c.Matching).ThenByDescending(c => c.Value.Priority).First().Value);  //每组中匹配度最高的项
                        _JingLuos = coll.ToList();
                    }
                return _JingLuos;
            }
        }

        List<Tuple<string, decimal>> _FenXingCnMedicine;

        /// <summary>
        /// 获取分型药物加减后的结果。
        /// </summary>
        public List<Tuple<string, decimal>> FenXingCnMedicine
        {
            get
            {
                lock (SyncLocker)
                    if (null == _FenXingCnMedicine)
                    {
                        var adds = (from tmp in CnDrugCorrections
                                    where tmp.TypeNumber == 1 && (float)AllNumbers.Intersect(tmp.Numbers).Count() / tmp.Numbers.Count >= tmp.Thresholds
                                    select tmp).SelectMany(c => c.CnDrugOfAdd); //增加项
                        var igns = (from tmp in CnDrugCorrections
                                    where tmp.TypeNumber == 1 && (float)AllNumbers.Intersect(tmp.Numbers).Count() / tmp.Numbers.Count >= tmp.Thresholds
                                    select tmp).SelectMany(c => c.CnDrugOfSub);   //忽略的项
                        var subc = (from tmp in CnDrugCorrections
                                    where tmp.TypeNumber == 2 && (float)AllNumbers.Intersect(tmp.Numbers).Count() / tmp.Numbers.Count >= tmp.Thresholds
                                    select tmp).SelectMany(c => c.CnDrugOfAdd);   //减少项
                        var coll1 = FenXing.SelectMany(c => c.CnDrugs).Union(adds) //增加项
                            .Where(c => !igns.Contains(c.Item1));   //忽略药物
                        var result = coll1.GroupJoin(subc, c => c.Item1, c => c.Item1, (tp, c) => c.Any() ? c.First() : tp);
                        _FenXingCnMedicine = result.ToList();
                    }
                return _FenXingCnMedicine;
            }
        }

        List<Tuple<string, decimal>> _ResultCnMedicine;

        /// <summary>
        /// 最终药物结果列表。
        /// </summary>
        public override List<Tuple<string, decimal>> Results
        {
            get
            {
                var coll = FenXingCnMedicine.Union(JingLuos.SelectMany(c => c.CnDrugs)).GroupBy(c => c.Item1);
                var tmp = coll.Select(c => Tuple.Create(c.Key, c.Max(subc => subc.Item2))).ToList();
                return tmp.RandomSequence();
            }
        }

    }
}
#pragma warning restore CS3021 // 由于程序集没有 CLSCompliant 特性，因此类型或成员不需要 CLSCompliant 特性
