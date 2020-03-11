

using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace OW.Data.Entity
{
    /// <summary>
    /// 资源存储的类。
    /// </summary>
    [DataContract]
    public class ResourceStore : ThingEntityBase
    {
        /// <summary>
        /// 构造函数。
        /// </summary>
        public ResourceStore()
        {

        }

        /// <summary>
        /// 构造函数。
        /// </summary>
        /// <param name="id">指定该实体对象的Id。</param>
        public ResourceStore(Guid id) : base(id)
        {

        }

        /// <summary>
        /// 显示的完整资源路径。
        /// </summary>
        [DataMember]
        [NotMapped]
        public string Url { get; set; }

        /// <summary>
        /// 存储的相对路径，不包含网站地址，这样便于搬家。
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// 所附属的实体的Id。
        /// 在实体中传送可忽略。
        /// </summary>
        [Index]
        [DataMember(IsRequired = false)]
        public Guid ThingEntityId { get; set; }

    }
}