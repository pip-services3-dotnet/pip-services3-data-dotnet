using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using PipServices3.Commons.Data;

namespace PipServices3.Data.Persistence
{
    public enum DummyType : int
    {
        Dummy = 0,
        NotDummy
    }

    [DataContract]
    public class Dummy : IStringIdentifiable
    {
        [DataMember(Name = "id")]
        public string Id { get; set; }

        [DataMember(Name = "key")]
        public string Key { get; set; }

        [DataMember(Name = "content")]
        public string Content { get; set; }

        [DataMember(Name = "create_time_utc")]
        public DateTime CreateTimeUtc { get; set; }

        [DataMember(Name = "inner_dummy")]
        public InnerDummy InnerDummy { get; set; }

        [DataMember(Name = "dummy_type")]
        public DummyType DummyType { get; set; }

        [DataMember(Name = "inner_dummies")]
        public List<InnerDummy> InnerDummies { get; set; } = new List<InnerDummy>();
    }

    [DataContract]
    public class InnerDummy
    {
        [DataMember(Name = "id")]
        public string Id { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "description")]
        public string Description { get; set; }

        [DataMember(Name = "inner_inner_dummies")]
        public List<InnerDummy> InnerInnerDummies { get; set; } = new List<InnerDummy>();
    }
}
