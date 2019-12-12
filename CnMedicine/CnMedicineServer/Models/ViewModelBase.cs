using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace OW.ViewModel
{
    /// <summary>
    /// 需要分页返回信息的控制信息封装类。
    /// </summary>
    [DataContract]
    public class PagingControlBaseViewModel
    {
        /// <summary>
        /// 返回记录在结果集合中的起始索引，第一个的索引是0，以此类推。
        /// </summary>
        [Display(Name = "起始索引")]
        [Range(0, int.MaxValue)]
        [DataMember]
        public int Index { get; set; }

        /// <summary>
        /// 返回最多多少条数据。
        /// 如果返回所有数据可以使用 int.MaxValue。
        /// </summary>
        [Display(Name = "返回最大数量")]
        [Range(1, int.MaxValue)]
        [DataMember]
        public int Count { get; set; }

        /// <summary>
        /// 追加的控制信息。一般是排序的规则，也可不用。
        /// </summary>
        [Display(Name = "附加信息", Description = "一般是排序的规则")]
        [DataMember]
        public string UserState { get; set; }
    }

    /// <summary>
    /// 返回分页数据的基类。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [DataContract]
    public class PagingResult<T>
    {
        /// <summary>
        /// 构造函数。
        /// </summary>
        public PagingResult()
        {

        }

        /// <summary>
        /// 数据总共的记录数。
        /// </summary>
        [DataMember]
        [Description("数据总共的记录数。")]
        public int MaxCount { get; set; }

        /// <summary>
        /// 返回 PagingControlBaseViewModel.UserState 的内容。便于客户端知道此记录集的附加信息。
        /// </summary>
        [DataMember]
        [Description("返回 PagingControlBaseViewModel.UserState 的内容。便于客户端知道此记录集的附加信息。")]
        public string UserState { get; set; }

        /// <summary>
        /// 返回数据的集合。
        /// </summary>
        [DataMember]
        [Description("返回数据的集合。")]
        public List<T> Datas { get; set; }
    }

}