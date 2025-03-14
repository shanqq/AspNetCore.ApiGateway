﻿using Newtonsoft.Json;
using NJsonSchema;
using System.Collections.Generic;

namespace AspNetCore.ApiGateway
{
    public class Orchestration
    {
        [JsonProperty(Order = 1)]
        public string Api { get; set; }

        [JsonProperty(Order = 2)]
        public OrchestationType OrchestrationType { get; set; }

        [JsonIgnore()]
        public List<RouteBase> Routes { get; set; }
    }

    public enum OrchestationType
    {
        Api,
        Hub,
        EventSource
    }

    public class ApiOrchestration : Orchestration
    {
        public ApiOrchestration()
        {
            OrchestrationType = OrchestationType.Api;
        }

        [JsonProperty(Order = 2)]
        public IEnumerable<Route> ApiRoutes { get; set; }        
    }

    public class HubOrchestration : Orchestration
    {
        public HubOrchestration()
        {
            OrchestrationType = OrchestationType.Hub;
        }

        [JsonProperty(Order = 2)]
        public IEnumerable<HubRoute> HubRoutes { get; set; }
    }

    public class EventSourceOrchestration : Orchestration
    {
        public EventSourceOrchestration()
        {
            OrchestrationType = OrchestationType.EventSource;
        }

        [JsonProperty(Order = 2)]
        public IEnumerable<EventSourceRoute> EventSourceRoutes { get; set; }
    }

    public class RouteBase
    {
        [JsonProperty(Order = 1)]
        public string Key { get; set; }
    }

    public class Route : RouteBase
    {
        [JsonProperty(Order = 3)]
        public string Verb { get; set; }

        [JsonProperty(Order = 4)]
        public string DownstreamPath { get; set; }

        [JsonProperty(Order = 5)]
        public JsonSchema RequestJsonSchema { get; set; }

        [JsonProperty(Order = 6)]
        public JsonSchema ResponseJsonSchema { get; set; }
    }

    public class HubRoute : RouteBase
    {
        [JsonProperty(Order = 3)]
        public string InvokeMethod { get; set; }

        [JsonProperty(Order = 4)]
        public string ReceiveMethod { get; set; }        

        [JsonProperty(Order = 5)]
        public string ReceiveGroup { get; set; }

        [JsonProperty(Order = 6)]
        public string BroadcastType { get; set; }

        [JsonProperty(Order = 7)]
        public IEnumerable<string> ReceiveParameterTypes { get; set; }
    }

    public class EventSourceRoute : RouteBase
    {
        [JsonProperty(Order = 3)]
        public string Type { get; set; }

        [JsonProperty(Order = 4)]
        public string ReceiveMethod { get; set; }

        [JsonProperty(Order = 5)]
        public string OperationType { get; set; }

        [JsonProperty(Order = 6)]
        public string StreamName { get; set; }

        [JsonProperty(Order = 7)]
        public string GroupName { get; set; }
    }

}
