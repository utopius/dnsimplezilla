using System;

namespace DNSimplezilla
{
    public class DomainRecord
    {
        public static DomainRecord FromDynamic(dynamic record)
        {
            if (record == null) throw new ArgumentNullException("record");

            var domainRecord = new DomainRecord();
            domainRecord.Name = record.name;
            domainRecord.TimeToLive = record.ttl;
            domainRecord.CreatedAt = record.created_at;
            domainRecord.UpdatedAt = record.updated_at;
            domainRecord.DomainId = record.domain_id;
            domainRecord.Id = record.id;
            domainRecord.Content = record.content;
            domainRecord.RecordType = record.record_type;
            domainRecord.Prio = record.prio;
            return domainRecord;
        }

        public string Name { get; private set; }
        public long TimeToLive { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }
        public long DomainId { get; private set; }
        public long Id { get; private set; }
        public string Content { get; private set; }
        public string RecordType { get; private set; }
        public string Prio { get; private set; }

        public override string ToString()
        {
            return string.Format("Name: {0}, TimeToLive: {1}, CreatedAt: {2}, UpdatedAt: {3}, DomainId: {4}, Id: {5}, Content: {6}, RecordType: {7}, Prio: {8}", Name,
                TimeToLive, CreatedAt, UpdatedAt, DomainId, Id, Content, RecordType, Prio);
        }
    }
}