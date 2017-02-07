﻿using System;

namespace Estuite.AzureEventStore
{
    public class EventRecord
    {
        public DateTime Created { get; set; }
        public SessionId SessionId { get; set; }
        public int Version { get; set; }
        public string Type { get; set; }
        public string Payload { get; set; }
    }
}