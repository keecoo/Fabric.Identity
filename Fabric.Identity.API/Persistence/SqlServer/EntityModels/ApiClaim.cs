﻿namespace Fabric.Identity.API.Persistence.SqlServer.EntityModels
{
    public class ApiClaim
    {
        public int Id { get; set; }
        public int ApiResourceId { get; set; }
        public string Type { get; set; }

        public virtual ApiResource ApiResource { get; set; }
    }
}
